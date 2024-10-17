using System;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class PlayerControl : MonoBehaviour, ISavable
{
    public float movementSpeed;
    public LayerMask grassLayer, FovLayer;
    private Rigidbody2D rb;
    private Vector2 input;
    public bool isMoving = true; // Ensure this starts as true
    private float checkTimer;
    private const float ENCOUNTER_CHANCE = 0.3f;
    private const float CHECK_INTERVAL = 0.5f;
    public event Action OnEncountered;
    public event Action<Transform> OnTrainerSpotted;
    [SerializeField] Sprite playerInBattleSprite;
    [SerializeField] string playerName;
    public Vector3 safeSpawnPoint = new Vector3(-93, -3, 0);
    private Animator animator;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    Vector3 originalPos;

    public void Update()
    {
        bool canMove = input != Vector2.zero && isMoving;
        animator.SetBool("isMoving", canMove);
    }
    private void StopMovement()
    {
        isMoving = false;
        rb.velocity = Vector2.zero; // Immediately stop any movement
        input = Vector2.zero; // Reset the input to ensure the player doesn't start moving again on its own
    }
    public void HandleUpdate()
    {
        if (!isMoving) // When isMoving is false, player input is ignored
        {
            rb.velocity = Vector2.zero;
            return;
        }

        input.x = Input.GetAxisRaw("Horizontal");
        input.y = Input.GetAxisRaw("Vertical");

        if (input.x != 0) input.y = 0;

       

        if (input != Vector2.zero)
        {
            OnMoveOver();
        }
    }

    private void FixedUpdate()
    {
        if (input != Vector2.zero)
        {
            animator.SetFloat("moveX", input.x);
            animator.SetFloat("moveY", input.y);
            rb.velocity = new Vector2(input.x * movementSpeed, input.y * movementSpeed);
        }
        else
            rb.velocity = Vector2.zero;
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckForTrainers();
    }

    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };
        //float[] position = new float[] { transform.position.x, transform.position.y };
        return saveData; 
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }
    private void CheckForEncounters()
    {
        checkTimer += Time.deltaTime;
        if (checkTimer >= CHECK_INTERVAL)
        {
            checkTimer = 0f;
            if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null && UnityEngine.Random.Range(0f, 1f) < ENCOUNTER_CHANCE)
            {
                isMoving = false; // Stop movement for encounter
                OnEncountered?.Invoke();
                
            }
        }
    }
    



    private void CheckForTrainers()
    {
        Collider2D trainerCollider = Physics2D.OverlapCircle(transform.position, 0.2f, FovLayer);
        if (trainerCollider != null)
        {
            Debug.Log("Trainer Spotted!");
            StopMovement();
            isMoving = false;
            OnTrainerSpotted?.Invoke(trainerCollider.transform);
        }
    }
    public string Name { get => playerName; }

    public Sprite Sprite { get => playerInBattleSprite; }
}


[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}

