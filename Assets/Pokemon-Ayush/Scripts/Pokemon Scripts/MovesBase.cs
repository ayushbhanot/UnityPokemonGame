using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Moves", menuName = "Pokemon/Create a new move")]
public class MovesBase : ScriptableObject
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] int power;
    [SerializeField] PokemonType type;
    [SerializeField] int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public int Power
    {
        get { return power; }
    }

    public PokemonType Type
    {
        get { return type; }
    }

    public int PP
    {
        get { return pp; }
    }

    public bool IsSpecial
    {
        get
        {
            if (Type == PokemonType.Fire || Type == PokemonType.Plant || Type == PokemonType.Water || Type == PokemonType.Dragon
            || Type == PokemonType.Electric || Type == PokemonType.Poison)
            {
                return true;
            }
            else
                return false;
        }
    }
}