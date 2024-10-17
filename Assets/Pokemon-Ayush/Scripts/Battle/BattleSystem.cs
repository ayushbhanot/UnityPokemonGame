using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using System.Linq;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Party, ForgetMove, LevelUp, Evolution }

public enum BattleAction { Move, SwitchPokemon, UseItem, Run, }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] PlayerUnit playerUnit;
    [SerializeField] BattleHUD playerHud;
    [SerializeField] BattleHUD opponentHud;
    [SerializeField] PlayerUnit enemyUnit;
    [SerializeField] BattleDialog dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerTrainerImage;
    [SerializeField] Image enemyTrainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] EvolutionManager evolutionManager;


    Vector3 originalPos;
    public Vector3 safeSpawnPoint = new Vector3(-93, -2, 0);
    MovesBase moveToLearn;

    IEnumerator ChooseForgetMove(Pokemon pokemon, MovesBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose move you would like to forget:");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.moveBase).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.ForgetMove;

    }

    public event Action<bool> BattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    PokemonParty playerParty;
    Pokemon wildPokemon;
    int currentMember;
    PokemonParty trainerParty;
    private bool isPartyScreenActive = false;
    public bool isTrainer = false;
    PlayerControl player;
    TrainerController trainer;


    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {

        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;


       // RefreshPartyScreen();
        StartCoroutine(SetUpBattle());
        player = playerParty.GetComponent<PlayerControl>();
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainer = true;
        player = playerParty.GetComponent<PlayerControl>();
        trainer = trainerParty.GetComponent<TrainerController>();
        //RefreshPartyScreen();
        StartCoroutine(SetUpBattle());
    }

    IEnumerator TryToRun()
    {
        state = BattleState.Busy;

        if (isTrainer)
        {
            yield return dialogBox.TypeDialog($"You can't Run from Me!");
            yield return new WaitForSeconds(1f);
            state = BattleState.ActionSelection;
        }
        else
        {
            yield return dialogBox.TypeDialog($"Ran away safely");
            ResetPlayerPokemonsHealth();
            BattleOver(true);
        }
    }
    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;
        dialogBox.EnableActionSelector(false);
        if (isTrainer)
        {
            yield return dialogBox.TypeDialog($"You can't steal a trainer's Pokemon..");
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog("");
            dialogBox.EnableActionSelector(true);
            state = BattleState.ActionSelection;
            yield break;
        }
        yield return dialogBox.TypeDialog($"{player.Name} used a PokeBall!");

        var pokeBallObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeBall = pokeBallObj.GetComponent<SpriteRenderer>();
        Vector3 targetPos = enemyUnit.transform.position; 
        targetPos.y = 1534;
        yield return pokeBall.transform.DOJump(targetPos, 100f, 1, 1f).WaitForCompletion();

        yield return enemyUnit.PlayCaptureAnimation();
        pokeBall.transform.DOMoveY(enemyUnit.transform.position.y - 100, 0.5f).WaitForCompletion();
        enemyUnit.GetComponent<SpriteRenderer>().color = Color.white;

        int shakeCount = TryToCatchPokemon(enemyUnit.pokemon);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeBall.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion(); ;
        }

        if (shakeCount == 4)
        {
            //Caugght Pokemon
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} was Successfuly Caught!!!");
            playerParty.AddPokemon(enemyUnit.pokemon);
            partyScreen.Init();
            partyScreen.SetPartyData(playerParty.Pokemons);
            yield return pokeBall.DOFade(0, 1.5f).WaitForCompletion();
            //RefreshPartyScreen();
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} has been added to your party!");
            Destroy(pokeBall);
            BattleOver(true);
        }
        else
        {
            // pokemon broke out?
            yield return new WaitForSeconds(1f);
            pokeBall.DOFade(0, 0.2f);
            enemyUnit.gameObject.SetActive(true);
            enemyUnit.transform.localScale = new Vector3(150f, 150f, 1f);
            yield return enemyUnit.PlayBreakOutAnimation();
            if (shakeCount < 3)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} Broke Free!!!"); }
            else
            { yield return dialogBox.TypeDialog($"So Close!!!"); }
            Destroy(pokeBall);
            state = BattleState.RunningTurn;
            StartCoroutine(OpponentsMove());
            // state = BattleState.RunningTurn;
            //dialogBox.EnableActionSelector(true);

        }

    }
    public void RefreshPartyScreen()
    {
        Debug.Log("Refreshing party screen.");
        partyScreen.Init();
        partyScreen.SetPartyData(playerParty.Pokemons);
    }



    public IEnumerator SetUpBattle()
    {
        partyScreen.Init();
        ClearHUD();
        if (!isTrainer)
        {
            RefreshPartyScreen();
            playerUnit.SetUp(playerParty.GetPlayerPokemon());
            enemyUnit.SetUp(wildPokemon);
            
            playerHud.SetData(playerUnit.pokemon);
            opponentHud.SetData(enemyUnit.pokemon);
            ShowHUD();
            dialogBox.EnableActionSelector(false);
            enemyUnit.gameObject.SetActive(true);
            enemyUnit.transform.localScale = new Vector3(150f, 150f, 1f);
            partyScreen.Init();
            dialogBox.SetMoveName(playerUnit.pokemon.Moves);

            yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.pokemon.Base.Name} appeared!"));
            yield return new WaitForSeconds(1f);
        }
        else
        {
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerTrainerImage.gameObject.SetActive(true);
            enemyTrainerImage.gameObject.SetActive(true);
            playerTrainerImage.transform.localPosition = new Vector3(-117, -40, 0);
            enemyTrainerImage.transform.localPosition = new Vector3(107, 46, 0);
            playerTrainerImage.sprite = player.Sprite;
            enemyTrainerImage.sprite = trainer.Sprite;
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(dialogBox.TypeDialog($"{trainer.Name} wants to Battle!"));
            yield return new WaitForSeconds(1f);

            enemyUnit.transform.localScale = new Vector3(150f, 150f, 1f);
            var enemyPokemon = trainerParty.GetPlayerPokemon();
            enemyUnit.SetUp(enemyPokemon);
            opponentHud.SetData(enemyPokemon);
            enemyUnit.gameObject.SetActive(true);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Base.Name}!");

            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetPlayerPokemon();
            playerUnit.SetUp(playerPokemon);
            playerHud.SetData(playerPokemon);
            ShowHUD();
            playerUnit.PlayerEnterAnimation();
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMoveName(playerUnit.pokemon.Moves);

            //ActionSelection();
        }
       // partyScreen.Init();
        ActionSelection();
    }

    public void PlayerExitAnimation()
    {
        Vector3 endPos = new Vector3(-500f, originalPos.y, originalPos.z);
        playerTrainerImage.transform.DOLocalMove(endPos, 0.25f).OnComplete(() =>
        {
            playerTrainerImage.gameObject.SetActive(false);
        });
    }

    public void EnemyExitAnimation()
    {
        Vector3 endPos = new Vector3(500f, originalPos.y, originalPos.z);
        enemyTrainerImage.gameObject.transform.DOLocalMove(endPos, 0.25f).OnComplete(() =>
        {
            enemyTrainerImage.gameObject.SetActive(false);
        });
    }

    void ActionSelection() { 
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose a move");
        dialogBox.EnableActionSelector(true);
    }

    void OpenParty()
    {
        partyScreen.Init();
        partyScreen.SetPartyData(playerParty.Pokemons);
        state = BattleState.Party;
        
        partyScreen.gameObject.SetActive(true);
        playerUnit.gameObject.SetActive(false);
        enemyUnit.gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelection)
        { HandleActionSelection(); }
        else if (state == BattleState.MoveSelection)
        { HandleMoveSelection(); }
        else if (state == BattleState.Party)
        {
           // RefreshPartyScreen();
            HandlePartySelection();
        }
        else if (state == BattleState.Busy)
        { }
        else if (state == BattleState.RunningTurn)
        { }
        else if (state == BattleState.ForgetMove)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                if (moveSelectionUI == null)
                {
                    Debug.LogError("moveSelectionUI is not assigned!");
                    return;
                }
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == 4)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} did not learn {moveToLearn.Name}.."));
                }
                else
                {
                    var selectedMove = playerUnit.pokemon.Moves[moveIndex].moveBase;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));

                    playerUnit.pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
                
            moveSelectionUI.ForgetMoveSelection(onMoveSelected);
        }

    }
    public void ResetPlayerPokemonMovesPP()
    {
        foreach (var pokemon in playerParty.Pokemons)
        {
            foreach (var move in pokemon.Moves)
            {
                move.PP = move.moveBase.PP; 
            }
        }
    }

    void HandleActionSelection()
    {
        if (state == BattleState.Busy)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);
        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                StartCoroutine(TryToRun());
            }
            else if (currentAction == 2)
            {
                OpenParty();
            }
            else if (currentAction == 3)
            {
                StartCoroutine(ThrowPokeball());
            }
        }
    }

    
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }
    private IEnumerator CheckAndPerformEvolutions()
    {
        foreach (var pokemon in playerParty.Pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                state = BattleState.Evolution;
                playerUnit.gameObject.SetActive(false);
                yield return evolutionManager.Evolve(pokemon, evolution);
                yield return new WaitForSeconds(1f);
                state = BattleState.RunningTurn;
                RefreshPartyScreen();
                
            }
        }
       
    }


    void CheckForBattleOver()
    {
        
        if (enemyUnit.pokemon.HP <= 0)
        {
            if (isTrainer)
            {
                var nextPokemon = trainerParty.GetPlayerPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(SendNextTrainerPokemon(nextPokemon));
                }
                else
                {
                    RefreshPartyScreen();
                    StartCoroutine(CheckAndPerformEvolutions());
                    StartCoroutine(EndBattle(true));
                }
            }
            else
            {
                RefreshPartyScreen();
                StartCoroutine(CheckAndPerformEvolutions());
                StartCoroutine(EndBattle(true));
            }
        }
        else if (playerUnit.pokemon.HP <= 0)
        {
            var nextPokemon = playerParty.GetPlayerPokemon();
            if (nextPokemon != null)
            {
                OpenParty();
            }
            else
            {
                StartCoroutine(EndBattle(false));
            }
        }
        else
        {
            ActionSelection();
        }
    }
    IEnumerator SendNextTrainerPokemon(Pokemon nextPokemon)
    {
        state = BattleState.Busy;
        yield return new WaitForSeconds(1.5f);
        enemyUnit.SetUp(nextPokemon);
        opponentHud.SetData(nextPokemon);

        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Base.Name}!");
        //  enemyUnit.EnemyEnterAnimation();
        yield return new WaitForSeconds(1f);

        state = BattleState.RunningTurn;
        yield return StartCoroutine(OpponentsMove());
    }
    public void ResetPlayerPokemonsHealth()
    {
        foreach (var pokemon in playerParty.Pokemons)
        {
            pokemon.HP = pokemon.MaxHp;
        }
    }
    public void ResetTrainerPokemonsHealth()
    {
        foreach (var pokemon in trainerParty.Pokemons)
        {
            pokemon.HP = pokemon.MaxHp;
        }
    }


    IEnumerator EndBattle(bool playerWon)
    {
        yield return new WaitUntil(() => state != BattleState.Evolution);
        yield return new WaitUntil(() => state != BattleState.ForgetMove);

        state = BattleState.Busy;
        yield return new WaitForSeconds(1f);

        if (playerWon)
        {
            yield return dialogBox.TypeDialog("You won the battle!");
        }
        else
        {
            yield return dialogBox.TypeDialog("You lost the Battle..");

            // Only reset trainer's Pokémon health if it's a trainer battle
            if (isTrainer && trainerParty != null)
            {
                ResetTrainerPokemonsHealth();
            }
        }

        yield return new WaitForSeconds(2f);
        ResetPlayerPokemonsHealth();
        ResetPlayerPokemonMovesPP();
        BattleOver?.Invoke(playerWon);
    }





    IEnumerator ShowDamageInfo(DamageInfo damageInfo, bool isPlayer)
    {
        if (damageInfo.Critical > 1 || damageInfo.Type > 1)
        {
            if (damageInfo.Critical > 1)
            {
                if (isPlayer)
                {
                    playerUnit.CriticalHitAnimation();
                    yield return dialogBox.TypeDialog("Critical hit!");
                }
                else
                {
                    enemyUnit.CriticalHitAnimation();
                    yield return dialogBox.TypeDialog("Critical hit!");
                }
            }
            if (damageInfo.Type > 1)
            {
                if (isPlayer)
                {
                    playerUnit.CriticalHitAnimation();
                    yield return dialogBox.TypeDialog("It's super effective!");
                }
                else
                {
                    enemyUnit.CriticalHitAnimation();
                    yield return dialogBox.TypeDialog("It's super effective!");
                }
            }
        }
        if (damageInfo.Type < 1)
        {
            if (isPlayer)
            {
                playerUnit.PlayHitAnimation();
            }
            else
            {
                enemyUnit.PlayHitAnimation();
            }
            yield return dialogBox.TypeDialog("It's not very effective..");
        }
        else
        {
            if (isPlayer)
            {
                playerUnit.PlayHitAnimation();
            }
            else
            {
                enemyUnit.PlayHitAnimation();
            }
        }

    }



    public void ClearHUD()
    {
        playerHud.gameObject.SetActive(false);
        opponentHud.gameObject.SetActive(false);
    }

    public void ShowHUD()
    {
        playerHud.gameObject.SetActive(true);
        opponentHud.gameObject.SetActive(true);
    }

    void HandleMoveSelection()
    {
        if (state == BattleState.Party)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove -= 2;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentMove;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.pokemon.Moves.Count - 1);
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            state = BattleState.Busy;
            var selectedMove = playerUnit.pokemon.Moves[currentMove];
            if (selectedMove.PP == 0)
            {
                StartCoroutine(ShowPPLeftMessage());
            }
            else
            {
                dialogBox.EnableMoveSelector(false);
                dialogBox.EnableDialogText(true);
                state = BattleState.ActionSelection;
                StartCoroutine(PlayerMove());
                state = BattleState.Busy;
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;
        Debug.Log(a);

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));
        Debug.Log(b);

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            else
            {
                ++shakeCount;
            }
        }
         return shakeCount;
    }
        

       /*int TryToCatchPokemon(Pokemon pokemon)
        {
            float randomChance = UnityEngine.Random.Range(0f, 100f);
            float difference = Mathf.Abs(pokemon.CatchPercentage - randomChance);

            int shakeCount = 0;
            if (difference <= 10) { shakeCount = 3; }
            else if (difference <= 25) { shakeCount = 2; }
            else if (difference <= 40) { shakeCount = 1; }

            if (randomChance <= pokemon.CatchPercentage) { return 4; }

            return shakeCount;
        }*/


    IEnumerator ShowPPLeftMessage()
    {
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableDialogText(true);

        yield return StartCoroutine(dialogBox.TypeDialog("0 PP Left, Pick a different Move.."));

        yield return new WaitForSeconds(1.5f);

        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    int CalculateCatchRateFromPercentage(float catchPercentage, int maxCatchRate = 255)
    {
       
        float normalizedRate = catchPercentage / 100;


        int catchRate = Mathf.RoundToInt(normalizedRate * maxCatchRate);
        catchRate = Mathf.Clamp(catchRate, 0, maxCatchRate);

        return catchRate;
    }
    void HandlePartySelection()
    {
        // Initialize the party screen
        partyScreen.Init();
        //partyScreen.SetPartyData(playerParty.Pokemons);

        // Update the member selection
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);
        partyScreen.UpdateMemberSelection(currentMember);

        // Handle actions based on input
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("This Pokemon needs to heal.");
                StartCoroutine(DelayedClearMessage());
                return;
            }
            if (selectedMember == playerUnit.pokemon)
            {
                partyScreen.SetMessageText("You are already using this Pokemon..");
                StartCoroutine(DelayedClearMessage());
                return;
            }

            partyScreen.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            enemyUnit.gameObject.SetActive(true);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            enemyUnit.gameObject.SetActive(true);
            ActionSelection();
        }
    }
    IEnumerator DelayedClearMessage()
    {
        yield return new WaitForSeconds(0.5f); 
                                            
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        if (playerUnit.pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.pokemon.Base.Name}!");
            playerUnit.PlayerSwitchAnimation();
            yield return new WaitForSeconds(1.5f);
        }

        playerUnit.SetUp(newPokemon);
        playerHud.SetData(newPokemon);
        dialogBox.SetMoveName(newPokemon.Moves);

        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");

        if (enemyUnit.pokemon.HP <= 0)
        {
            state = BattleState.RunningTurn;
            StartCoroutine(OpponentsMove());
        }
        else
        {
            state = BattleState.ActionSelection;
            ActionSelection();
        }
    }
    IEnumerator PlayerMove()
    {
        if (state == BattleState.Busy)
            yield break;
        var move = playerUnit.pokemon.Moves[currentMove];
        move.PP--;
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} used {move.moveBase.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        var damageInfo = enemyUnit.pokemon.TakeDamage(move, playerUnit.pokemon);
        opponentHud.UpdateHealth();
        yield return ShowDamageInfo(damageInfo, false);



        if (damageInfo.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(HandleExp(enemyUnit));
            
            yield return new WaitForSeconds(0.5f);
            CheckForBattleOver();
            yield return new WaitForSeconds(1f);

        }
        else
        {
            state = BattleState.RunningTurn;
            StartCoroutine(OpponentsMove());
        }
    }

    IEnumerator HandleExp(PlayerUnit faintedUnit)
    {


        int expYield = faintedUnit.pokemon.Base.ExpYield;
        int enemyLevel = enemyUnit.pokemon.Level;
        float trainerbonus = (isTrainer) ? 2f : 1f;

        int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerbonus) / 7);
        playerUnit.pokemon.Exp += expGain;
        yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} gained {expGain} exp points!");
        yield return new WaitForSeconds(0.25f);
        yield return playerHud.SetExpSmooth();

        while (playerUnit.pokemon.CheckForLevelUp())
        {
            state = BattleState.LevelUp;
            playerHud.SetLevel();
            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} grew to level {playerUnit.pokemon.Level}");

            yield return playerHud.SetExpSmooth(true);
            var newMove = playerUnit.pokemon.GetLearnableMove();
            if (newMove != null)
            {
                if (playerUnit.pokemon.Moves.Count < 4)
                {
                    playerUnit.pokemon.LearnMove(newMove);
                    yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} learned {newMove.Base.Name}");
                    dialogBox.SetMoveName(playerUnit.pokemon.Moves);
                }
                else
                {
                   
                    state = BattleState.ForgetMove;
                    yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} wants to learn {newMove.Base.Name}..");
                    yield return new WaitForSeconds(0.5f);
                    yield return dialogBox.TypeDialog($"But {playerUnit.pokemon.Base.Name} cannot learn more than 4 moves");
                    yield return ChooseForgetMove(playerUnit.pokemon, newMove.Base);
                    yield return new WaitUntil(() => state != BattleState.ForgetMove);
                    yield return new WaitForSeconds(1f);
                }
            }
            
            yield return new WaitForSeconds(0.7f);
        }
        yield return new WaitForSeconds(0.25f);

    }
    IEnumerator OpponentsMove()
    {
        if (state == BattleState.Busy)
        {
            yield break;
        }



        var move = enemyUnit.pokemon.GetRandomMove();
        move.PP--;

        yield return dialogBox.TypeDialog($"{enemyUnit.pokemon.Base.Name} used {move.moveBase.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        var damageInfo = playerUnit.pokemon.TakeDamage(move, enemyUnit.pokemon);
        playerHud.UpdateHealth();
        yield return ShowDamageInfo(damageInfo, true);

        if (damageInfo.Fainted)
        {

            yield return dialogBox.TypeDialog($"{playerUnit.pokemon.Base.Name} fainted!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
            CheckForBattleOver();
            

        }
        else
            ActionSelection();   }}
