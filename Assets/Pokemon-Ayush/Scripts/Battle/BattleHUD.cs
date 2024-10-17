using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HealthBar healthBar;
    [SerializeField] GameObject expBar;


    Pokemon _pokemon;
    
    public void SetData(Pokemon pokemon)
    {
        _pokemon = pokemon;

        nameText.text = _pokemon.Base.Name;
        SetLevel();
        healthBar.setHealth((float)pokemon.HP / pokemon.MaxHp);
        SetExp();
        Debug.Log("Name: " + _pokemon.Base.Name);
        Debug.Log("Level: " + _pokemon.Level);

    }
    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _pokemon.Level;
    }
    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset == true)
            expBar.transform.localScale = new Vector3(0, 1, 1);
        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }
    float GetNormalizedExp()
    {
        int currentLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);

        float normalizedExp = (float)(_pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }
    public void UpdateHealth()
    {
        healthBar.setHealth((float)_pokemon.HP / _pokemon.MaxHp);

    }
}
