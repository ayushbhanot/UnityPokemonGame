using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_2DCoin : MonoBehaviour
{
    
    public static int totalCoins = 0;

    void Awake()
    {
        
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D c2d)
    {
        
        if (c2d.CompareTag("Player"))
        {
            
            totalCoins++;
            
            Debug.Log("You currently have " + SC_2DCoin.totalCoins + " Coins.");
            
            Destroy(gameObject);
        }
    }
}