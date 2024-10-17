using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MoveSelectionUI : MonoBehaviour
{
    Color customPurple = new Color(162f / 255f, 31f / 255f, 255f / 255f, 1f);

    [SerializeField] List<Text> movetext;

    int currentSelection = 0;
    public void SetMoveData(List<MovesBase> currentMoves, MovesBase newMove)
    {
        for (int i=0; i < currentMoves.Count; ++i)
        {
            movetext[i].text = currentMoves[i].Name;
        }

        movetext[currentMoves.Count].text = newMove.Name;
    }

    public void ForgetMoveSelection(Action<int> onSelected)
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++currentSelection;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --currentSelection;

        currentSelection = Mathf.Clamp(currentSelection, 0, 4);

        UpdateForgetMoveSelection(currentSelection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void UpdateForgetMoveSelection(int selection)
    {
        for (int i = 0; i < 5; i++)
        {
            
            if (i == selection)
            {
                movetext[i].color = Color.blue;
            }
            else if (i <= 3)
                movetext[i].color = Color.black;
            else if (i == 4)
                movetext[i].color = customPurple;
        }
    }
}
