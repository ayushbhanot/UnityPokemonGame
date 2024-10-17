using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public enum GameState { FreeRoam, Battle, Evolution }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerControl playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] Image fadePanel;

    public static GameControl Instance { get; private set; }
    GameState state;
    private Vector3 preBattlePosition;
    private TrainerController defeatedTrainer;
    private Vector3 safeSpawnPoint = new Vector3(-93, -2, 0);



    private void Awake()
    {
        Instance = this;

        PokemonsDatabase.Init();
        MoveDatabase.Init();
        //ConditionsDB.Init();
    }
    private void Start()
    {
        playerController.OnEncountered += StartBattle;
        playerController.OnTrainerSpotted += HandleTrainerSpotted;
        battleSystem.BattleOver += EndBattle;

        //EvolutionManager.i.OnStartEvolution += () => state = GameState.Evolution;
        //EvolutionManager.i.OnCompleteEvolution += () => state = GameState.FreeRoam;
    }


    void StartBattle()
    {
        preBattlePosition = playerController.transform.position;
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetWildPokemons();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }
    public void StartTrainerBattle(TrainerController trainerController)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainerController.GetComponent<PokemonParty>();
        battleSystem.StartTrainerBattle(playerParty, trainerParty);
        defeatedTrainer = trainerController;
    }

    private void Update()
    {
        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (Input.GetKeyDown(KeyCode.S))
                {
                    if (SavingSystem.i == null)
                    {
                        Debug.LogError("SavingSystem instance is null");
                    }
                    else
                    {
                        SavingSystem.i.Save("saveSlot1");
                    }
                }

            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                SavingSystem.i.Load("saveSlot1");
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }


    }
    /*void EndBattle(bool won)
    {
        var playerPart = playerController.GetComponent<PokemonParty>();
        //StartCoroutine(playerPart.CheckForEvolutions());
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        playerController.gameObject.SetActive(true);
        playerController.isMoving = true;
        playerController.transform.position = preBattlePosition;

        if (won && defeatedTrainer != null)
        {
            defeatedTrainer.DisableFOV();
            defeatedTrainer = null;
        }
        if (!won)
        {
            if (defeatedTrainer != null)
            {
                defeatedTrainer.EnableFOV(); // Assuming you have implemented this
            }
            RespawnPlayerAfterLoss();
        }


        battleSystem.isTrainer = false;
    }*/

    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        playerController.gameObject.SetActive(true);
        playerController.isMoving = true;
        playerController.transform.position = preBattlePosition;

        if (won)
        {
            if (defeatedTrainer != null)
            {
                defeatedTrainer.DisableFOV();
                defeatedTrainer = null; // Reset the reference to the defeated trainer
            }
        }
        else
        {
            // Ensure the player is respawned at a safe location if they lost
            RespawnPlayerAfterLoss();

            // Re-enable the defeated trainer's FOV for a rematch
           
                defeatedTrainer.EnableFOV(); // Make sure this method properly re-enables the FOV
            
        }

        battleSystem.isTrainer = false; // Reset the battle system state
    }

    public void RespawnPlayerAfterLoss()
    {

        playerController.transform.position = safeSpawnPoint;

    }




    private void HandleTrainerSpotted(Transform trainerFovTransform)
    {
        TrainerController trainerController = trainerFovTransform.GetComponentInParent<TrainerController>();
        if (trainerController != null)
        {
            trainerController.MoveTowards(playerController.transform);
            StartCoroutine(StartTrainerBattleAfterDelay(trainerController));

        }
        else
        {
            Debug.LogError("TrainerController component not found in the parent of the detected FOV object.");
        }
    }
    IEnumerator StartTrainerBattleAfterDelay(TrainerController trainerController)
    {
        preBattlePosition = playerController.transform.position;
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeIn());
        StartTrainerBattle(trainerController);
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeOut());
        yield return new WaitForSeconds(0.25f);
        battleSystem.PlayerExitAnimation();
        battleSystem.EnemyExitAnimation();
    }

    IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        Image fadeImage = fadePanel.GetComponent<Image>();
        for (float t = 0.01f; t < 1; t += Time.deltaTime / 2)
        {
            Color newColor = new Color(1, 1, 1, t);
            fadeImage.color = newColor;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        Image fadeImage = fadePanel.GetComponent<Image>();
        for (float t = 1f; t > 0; t -= Time.deltaTime / 2)
        {
            Color newColor = new Color(1, 1, 1, t);
            fadeImage.color = newColor;
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
    }
}