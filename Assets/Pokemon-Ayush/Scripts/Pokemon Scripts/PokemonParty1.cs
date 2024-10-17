using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class PokemonParty : MonoBehaviour
{
    PartyScreen partScreen;
    [SerializeField] List<Pokemon> pokemons;
    private BattleDialog battleDialog;
    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
        set { pokemons = value; }
    }

    private void Start()
    {
       
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    

 public IEnumerator CheckForEvolutions()
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if (evolution != null)
            {
                yield return EvolutionManager.i.Evolve(pokemon, evolution);

            }
        }

        OnUpdated?.Invoke();
    }

    public static PokemonParty Instance { get; private set; }

 /*   private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }*/
    public Pokemon GetPlayerPokemon()
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            Debug.Log("Your party is full");
        }
    }
}
