using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SC_CoinCounter : MonoBehaviour
{
    
    TMP_Text counterText;
    void Start()
    {
        counterText = GetComponent<TMP_Text>();
    }

    
    void Update()
    {
        if (counterText.text != SC_2DCoin.totalCoins.ToString())
        {
            counterText.text = "Coins:" + SC_2DCoin.totalCoins.ToString();
        }
       
    }
    
}