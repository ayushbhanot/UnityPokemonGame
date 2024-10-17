using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] Image evolutionUI;
    [SerializeField] Animator pokemonAnimator;
    [SerializeField] BattleDialog battleDialog;
    SpriteRenderer pokemonSpriteRenderer;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    /* public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
     {
         OnStartEvolution?.Invoke();
         evolutionUI.gameObject.SetActive(true);
         Color originalBackgroundColor = evolutionUI.color;
         pokemonAnimator.runtimeAnimatorController = pokemon.Base.FrontEnd as RuntimeAnimatorController;
         yield return battleDialog.TypeDialog($"{pokemon.Base.Name} is evolving...");
         var oldPokemon = pokemon.Base;
         yield return new WaitForSeconds(1.5f);
         pokemonAnimator.gameObject.transform.DOShakePosition(3f, strength: new Vector3(5, 5, 0), vibrato: 20, randomness: 90, snapping: false);
         yield return new WaitForSeconds(1f);
         pokemonSpriteRenderer.DOColor(Color.white, 2f);
         yield return new WaitForSeconds(1.5f);
         evolutionUI.DOColor(Color.white, 1f);


         pokemon.Evolve(evolution);

         pokemonSpriteRenderer = pokemonAnimator.GetComponentInChildren<SpriteRenderer>();
         pokemonAnimator.runtimeAnimatorController = pokemon.Base.FrontEnd as RuntimeAnimatorController;
         pokemonSpriteRenderer.color = Color.white;
         yield return new WaitForSeconds(1.5f);
         evolutionUI.DOColor(originalBackgroundColor, 1f);
         pokemonSpriteRenderer.DOColor(new Color(pokemonSpriteRenderer.color.r, pokemonSpriteRenderer.color.g, pokemonSpriteRenderer.color.b, 1f), 1.5f);

         yield return battleDialog.TypeDialog($"{oldPokemon.Name} has evolved into {pokemon.Base.Name}!!!");
         yield return new WaitForSeconds(3f);

         evolutionUI.gameObject.SetActive(false);
         OnCompleteEvolution?.Invoke();
     }*/
    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.gameObject.SetActive(true);
        Color originalBackgroundColor = evolutionUI.color;

        pokemonAnimator.runtimeAnimatorController = pokemon.Base.FrontEnd as RuntimeAnimatorController;
        pokemonSpriteRenderer = pokemonAnimator.GetComponentInChildren<SpriteRenderer>();

        yield return battleDialog.TypeDialog($"{pokemon.Base.Name} is evolving...");
        var originalPokemon = pokemon.Base;

        Sequence evolutionSequence = DOTween.Sequence();

        evolutionSequence.AppendInterval(1.5f);
        evolutionSequence.Append(pokemonAnimator.gameObject.transform.DOShakePosition(2f, new Vector3(5, 5, 0), 20, 90))
                    .Join(pokemonSpriteRenderer.DOColor(Color.white, 2f));
        evolutionSequence.Append(evolutionUI.DOColor(Color.white, 1f))
        .Join(pokemonSpriteRenderer.DOColor(Color.clear, 1.5f));
        evolutionSequence.AppendInterval(1.5f);
        evolutionSequence.AppendCallback(() =>
        {
            pokemon.Evolve(evolution);
            pokemonAnimator.runtimeAnimatorController = pokemon.Base.FrontEnd as RuntimeAnimatorController;
        });
        
        evolutionSequence.Append(evolutionUI.DOColor(originalBackgroundColor, 2f))
        .Join(pokemonSpriteRenderer.DOColor(new Color(pokemonSpriteRenderer.color.r, pokemonSpriteRenderer.color.g, pokemonSpriteRenderer.color.b, 1f), 3f));

        yield return evolutionSequence.WaitForCompletion();

        yield return battleDialog.TypeDialog($"{originalPokemon.Name} has evolved into {pokemon.Base.Name}!!!");

        yield return new WaitForSeconds(1.5f);
        evolutionUI.gameObject.SetActive(false);

        OnCompleteEvolution?.Invoke();
    }
}
