using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapBehaviour : MonoBehaviour
{





    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Check if there's at least one coin to remove
            if (SC_2DCoin.totalCoins > 0)
            {
                SC_2DCoin.totalCoins--; // Decrement the total coin count
                Debug.Log("Coin lost! You now have " + SC_2DCoin.totalCoins + " Coins.");
            }
            else
            {
                Debug.Log("No more coins to lose!");
            }

            Debug.LogWarning("Trap has been triggered by the player and will be destroyed.");
            Destroy(gameObject); // Destroy the trap

            
           

        }
    }

}