using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    public Pokemon GetWildPokemons()
    {
        var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
        wildPokemon.Init();
        return wildPokemon;
    }

}