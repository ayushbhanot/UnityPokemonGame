using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerUnit : MonoBehaviour
{
    BattleSystem battleSystem;

    [SerializeField] bool isPlayer;
    PokemonScript _base;
    int level;


    public Pokemon pokemon { get; set; }

    Animator animator;
    Vector3 originalPos;
    Color originalColor;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPos = transform.localPosition;
        originalColor = spriteRenderer.color;
    }
   
    public void SetUp(Pokemon pokemon)
    {
        this.pokemon = pokemon;

        Debug.Log("Setting up PlayerUnit with base: " + _base);
        //Pokemon = new Pokemon(_base, level);
//        battleSystem.ShowHUD();
        if (isPlayer)
            GetComponent<Animator>().runtimeAnimatorController = this.pokemon.Base.BackEnd;
        else
            GetComponent<Animator>().runtimeAnimatorController = this.pokemon.Base.FrontEnd;
       // transform.localScale = new Vector3(150, 150, 150);
        spriteRenderer.color = originalColor;
        PlayerEnterAnimation();
    }
    private Sequence criticalHitSequence;

    public void CriticalHitAnimation()
    {
        float totalAnimationDuration = 1.5f;
        float blinkDuration = 0.1f;
        float wiggleAmount = 10f;
        int blinkCount = Mathf.FloorToInt(totalAnimationDuration / (blinkDuration * 3));

        criticalHitSequence = DOTween.Sequence();

        var colorSequence = DOTween.Sequence()
            .Append(spriteRenderer.DOColor(Color.red, blinkDuration))
            .Append(spriteRenderer.DOColor(Color.white, blinkDuration))
            .Append(spriteRenderer.DOColor(originalColor, blinkDuration))
            .SetLoops(blinkCount, LoopType.Restart);
        float wiggleDurationEachSide = 0.15f; 
        int wiggleLoops = Mathf.FloorToInt((totalAnimationDuration / (wiggleDurationEachSide * 2)));

        for (int i = 0; i < wiggleLoops; i++)
        {
            criticalHitSequence.Append(transform.DOLocalMoveX(originalPos.x + wiggleAmount, wiggleDurationEachSide).SetEase(Ease.InOutQuad));
            criticalHitSequence.Append(transform.DOLocalMoveX(originalPos.x - wiggleAmount, wiggleDurationEachSide).SetEase(Ease.InOutQuad));
        }

        criticalHitSequence.Insert(0, colorSequence);
        criticalHitSequence.OnComplete(() => transform.localPosition = originalPos);

        criticalHitSequence.Play();
    }
    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        Color whiteColor = new Color(1, 1, 1, spriteRenderer.color.a);
        sequence.Append(spriteRenderer.DOColor(whiteColor, 0.2f));
        Color targetFadeColor = new Color(1, 1, 1, 0);
        sequence.Append(spriteRenderer.DOColor(targetFadeColor, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        transform.localPosition = new Vector3(originalPos.x, originalPos.y + 50f, originalPos.z);
        spriteRenderer.color = new Color(1, 1, 1, 0);
        sequence.Append(spriteRenderer.DOColor(new Color(1, 1, 1, 1), 0.5f));
       

        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));

        yield return sequence.WaitForCompletion();
    
}
    public void PlayerSwitchAnimation()
    {
        Vector3 endPos = new Vector3(-500f, originalPos.y);
        gameObject.transform.DOLocalMoveX(endPos.x, 1f);
    }



    public void PlayerEnterAnimation()
    {
        gameObject.SetActive(true);
        Vector3 startPos = isPlayer ? new Vector3(-500f, originalPos.y) : new Vector3(500f, originalPos.y);
        transform.localPosition = startPos;

        gameObject.transform.localPosition = isPlayer ? new Vector3(-500f, originalPos.y) : new Vector3(500f, originalPos.y);

        gameObject.transform.DOLocalMoveX(originalPos.x, 1f);
    }
    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayer)
            sequence.Append(gameObject.transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(gameObject.transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

        sequence.Append(gameObject.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(spriteRenderer.DOColor(Color.black, 0.1f));
        sequence.Append(spriteRenderer.DOColor(originalColor, 0.1f));
        sequence.Append(spriteRenderer.DOColor(Color.black, 0.1f));
        sequence.Append(spriteRenderer.DOColor(originalColor, 0.1f));
        sequence.Append(spriteRenderer.DOColor(Color.black, 0.1f));
        sequence.Append(spriteRenderer.DOColor(originalColor, 0.1f));
    }
    public void EnemyEnterAnimation()
    {
        Vector3 startPos = new Vector3(500f, originalPos.y); 
        transform.localPosition = startPos; 
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
        gameObject.transform.DOLocalMoveX(originalPos.x, 1f);
    }
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(gameObject.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(spriteRenderer.DOFade(0f, 0.5f));
    }
}