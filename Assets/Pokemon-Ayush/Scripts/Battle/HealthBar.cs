using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] GameObject Health;


    /* public void setHealth(float healthNormalized)
     {
         float clampedHealth = Mathf.Clamp01(healthNormalized);
         Debug.Log($"Updating health bar to {clampedHealth * 100}%");
         Health.transform.localScale = new Vector3(clampedHealth, 1f, 1f);
     } */
    public void setHealth(float healthNormalized)
    {
        // Check for NaN and Infinity and clamp the value between 0 and 1
        if (float.IsNaN(healthNormalized) || float.IsInfinity(healthNormalized))
        {
            Debug.LogError("Health normalized value is invalid (NaN or Infinity). Setting to full health.");
            healthNormalized = 1f; // Default to 1 (fully filled health bar) in case of invalid input
            
        }
        else
        {
            // Ensure the value is clamped between 0 and 1
            healthNormalized = Mathf.Clamp(healthNormalized, 0f, 1f);
        }

        Health.transform.localScale = new Vector3(healthNormalized, 1f, 1f);
    }


}
