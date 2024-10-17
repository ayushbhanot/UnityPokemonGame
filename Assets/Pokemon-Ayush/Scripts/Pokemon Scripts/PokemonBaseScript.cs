using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonScript : ScriptableObject
{
    [SerializeField] new string name;
    [SerializeField] string description;

    [SerializeField] AnimatorController frontEnd;
    [SerializeField] AnimatorController backEnd;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    [SerializeField] int maxhp;
    [SerializeField] int attackdamage;
    [SerializeField] int defense;
    [SerializeField] int spattack;

    [SerializeField] int catchRate = 255;
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;
    [SerializeField] List<Evolution> evolutions;
   // [SerializeField] float catchPercentage = 100;

    [SerializeField] List<LearnableMoves> learnableMoves;

    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.mediumFast)
        {
            return level * level * level;
        }
        return -1;
    }
    public string Name
    {
        get { return name; }
    }
    public GrowthRate pGrowthRate => growthRate;

    // public float CatchPercentage => catchPercentage;

    public int CatchRate => catchRate;
    public string Description
    {
        get { return description; }
    }

    public List<Evolution> Evolutions => evolutions;

  
    public enum GrowthRate
    {
        Fast, mediumFast
    }

    public int ExpYield => expYield;

    public int MaxHp
    {
        get { return maxhp; }
    }

    public int AttackDamage
    {
        get { return attackdamage; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpaAttack
    {
        get { return spattack; }
    }

    public PokemonType Type1 => type1;
    public PokemonType Type2 => type2;

    public AnimatorController FrontEnd
    {
        get { return frontEnd; }
    }
   

    public AnimatorController BackEnd
    {
        get { return backEnd; }
    }

    public List<LearnableMoves> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMoves
{
    [SerializeField] MovesBase moveBase;
    [SerializeField] int level;

    public MovesBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level;  }
    }
}

[System.Serializable]
public class Evolution
{
    [SerializeField] PokemonScript evolvesInto;
    [SerializeField] int requiredLevel;

    public PokemonScript EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
}

    public enum PokemonType
    {
        None,
        Normal,
        Fire,
        Water,
        Plant,
        Dragon,
        Fighting,
        Poison,
        Electric
    }

public class TypeChart
{
    static float[][] chart =
    {
        //           NOR, FIR, WAT, PlANT, DRAG, FIGH, POIS, ELECT
      /*NOR*/  new float[]  {1f, 1f,  1f,   1f,    1f,   1f,   1f,   1f},
      /*FIR*/  new float[]  {1f, 1f,  0.5f,   2f,    0.5f,   1f,   1f,   1f},
      /*WAT*/  new float[]  {1f, 2f,  1f,   1f,    1f,   1f,   1f,   0.5f},
       /*PLA*/ new float[]  {0.5f, 2f,  1f,   1f,    1f,   0.5f,   1f,   2f},
       /*DRA*/ new float[]  {0.5f, 1f,  2f,   1f,    1f,   1f,   1f,   1f},
      /*FIG*/  new float[]  {2f, 1f,  1f,   1f,    1f,   0.5f,   0.5f,   1f},
      /*POI*/  new float[]  {1f, 1f,  1f,   1f,    2f,   1f,   1f,   1f},
      /*ELE*/  new float[]  {1f, 2f,  0.5f,   1f,    1f,   1f,   1f,   1f},

    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if (attackType == PokemonType.None || defenseType == PokemonType.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];

    }
}

    