using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Animations;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HealthBar healthBar;
    [SerializeField] Color highlightedColor;
    [SerializeField] private Animator animator;
    [SerializeField] private PokemonSpriteScaler spriteScaler;
    Pokemon _pokemon;

    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        levelText.text = "Lvl " + pokemon.Level;
        healthBar.setHealth((float)pokemon.HP / pokemon.MaxHp);
        Debug.Log("Name: " + pokemon.Base.Name);
        Debug.Log("Level: " + pokemon.Level);
        animator.runtimeAnimatorController = pokemon.Base.FrontEnd;
        animator.Play("FrontAnimationStateName", -1, 0f);

        if (spriteScaler != null)
            spriteScaler.ScaleSpriteToTargetSize();
    

}

    public void SetSelected(bool selected)
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        }
        else
            nameText.color = Color.black;
    }
}
