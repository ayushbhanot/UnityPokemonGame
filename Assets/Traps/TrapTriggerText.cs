using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    public TextMeshProUGUI trapWarningText; 
    public float textDisplayTime = 2f; 

    private void Start()
    {
        trapWarningText.gameObject.SetActive(false); 
    }

    public void TriggerTrapEffect()
    {
        StartCoroutine(DisplayTrapWarning());
    }

    private IEnumerator DisplayTrapWarning()
    {
        trapWarningText.gameObject.SetActive(true); 
        yield return new WaitForSeconds(textDisplayTime); 
        trapWarningText.gameObject.SetActive(false); 
    }
}
