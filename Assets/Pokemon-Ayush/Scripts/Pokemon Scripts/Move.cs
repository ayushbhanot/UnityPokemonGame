using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
 public MovesBase moveBase { get; set; }
    public int PP { get; set; }

public Move(MovesBase pBase)
    {
        moveBase = pBase;
        PP = pBase.PP;
    }

    public Move(MoveSaveData saveData)
    {
        moveBase = MoveDatabase.GetObjectByName(saveData.name);
        PP = saveData.pp;
    }

    public MoveSaveData GetSaveData()
    {
        var saveData = new MoveSaveData()
        {
            name = moveBase.name,
            pp = PP
        };
        return saveData;
    }
}

[Serializable]
public class MoveSaveData
{
    public string name;
    public int pp;
}