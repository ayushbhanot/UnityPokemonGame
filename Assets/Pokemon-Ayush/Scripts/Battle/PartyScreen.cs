using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    List<Pokemon> pokemons;//= new List<Pokemon>();
    public GameObject pok1, pok2, pok3, pok4, pok5, pok6;

    PartyMemberUI[] memberSlots;
    [SerializeField] PokemonSpriteScaler spriteScaler;
    void Start()
    {
        Init();
    }

    public void Init()
    { memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
  
    }

    public void SetPartyData(List<Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        Debug.Log("SetPartyData: >>>> " + pokemons.Count.ToString());
        for (int i = 0; i < memberSlots.Length; i++)
        {
            
            if (i < pokemons.Count)
            {
                memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
               
                memberSlots[i].SetData(pokemons[i]);
                memberSlots[i].gameObject.SetActive(true);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        messageText.text = "Choose a Pokemon:";
    }
    


    public void UpdateMemberSelection(int selectedMember)
    {
        
       

        if (selectedMember >= 0 && selectedMember < memberSlots.Length)
        {
            for (int i = 0; i < memberSlots.Length; i++)
            {
                if (i < pokemons.Count)
                {
                    memberSlots[i].SetSelected(i == selectedMember);
                }
            }
        }
        else
        {
            Debug.LogError($"Selected member index ({selectedMember}) is out of range.");
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

}
