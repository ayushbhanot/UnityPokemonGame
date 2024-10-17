using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Pokemon
{
    [SerializeField] private PokemonScript _base;
    [SerializeField] private int level;
    private Dictionary<Stat, int> stats;

    public PokemonScript Base { get { return _base; } }
    public int Exp { get; set; }
    public int Level { get { return level; } }
    public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemon(PokemonScript pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        Init();
    }

    public void Init()
    {
        HP = MaxHp;
        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));
            if (Moves.Count >= 4)
                break;
        }
        Exp = Base.GetExpForLevel(Level);
        RecalculateStats();
    }

    public LearnableMoves GetLearnableMove()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public int Attack { get; private set; }
    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            ++level;
            RecalculateStats();
            return true;
        }
        return false;
    }

    public void LearnMove(LearnableMoves moveToLearn)
    {
        if (Moves.Count < 4)
            Moves.Add(new Move(moveToLearn.Base));
    }

    /*public int Defense { get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; } }
    public int SpAttack { get { return Mathf.FloorToInt((Base.SpaAttack * Level) / 100f) + 5; } }
    public int MaxHp { get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level; } } */

    public int Defense { get; private set; }
    public int SpAttack { get; private set; }
    public int MaxHp { get; private set; }


    public DamageInfo TakeDamage(Move move, Pokemon attacker)
    {
        float critical = 1f;
        if (Random.value * 100f <= 15f)
            critical = 2f;
        float type = TypeChart.GetEffectiveness(move.moveBase.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.moveBase.Type, this.Base.Type2);
        var damageInfo = new DamageInfo()
        {
            Type = type,
            Critical = critical,
            Fainted = false
        };
        float attack = (move.moveBase.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float modifiers = UnityEngine.Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.moveBase.Power * ((float)attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);
        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageInfo.Fainted = true;
        }
        return damageInfo;
    }

    public Move GetRandomMove()
    {
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();
        if (movesWithPP.Count == 0)
            throw new System.InvalidOperationException("No moves left with PP greater than 0");
        int r = UnityEngine.Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public Pokemon(PokemonSaveData saveData)
    {
        _base = PokemonsDatabase.GetObjectByName(saveData.name);
        HP = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;
        Moves = saveData.moves.Select(s => new Move(s)).ToList();
    }

        public void RecalculateStats()
        {
            Attack = Mathf.FloorToInt((Base.AttackDamage * Level) / 100f) + 5;
            Defense = Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5;
            SpAttack = Mathf.FloorToInt((Base.SpaAttack * Level) / 100f) + 5;
        int oldMaxHp = MaxHp;
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
        HP += MaxHp - oldMaxHp;
        Debug.Log($"Stats: {Base.Name}" +
          $"Attack: {Base.AttackDamage}, " +
          $"Defense: {Base.Defense}, " +
          $"SpAttack: {Base.SpaAttack}, " +
          $"MaxHp: {Base.MaxHp}");
        }


    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level);
    }



    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolvesInto;
        RecalculateStats();
    }

    public PokemonSaveData GetSaveData()
    {
        var saveData = new PokemonSaveData()
        {
            name = Base.name,
            hp = HP,
            level = Level,
            exp = Exp,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };
        return saveData;
    }

    public enum Stat
    {
        Attack,
        Defense,
        SpAttack,
        Speed,
    }
}


public class DamageInfo
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float Type { get; set; }
}

[System.Serializable]
public class PokemonSaveData
{
    public int hp;
    public int level;
    public int exp;
    public string name;
    public List<MoveSaveData> moves;
}
