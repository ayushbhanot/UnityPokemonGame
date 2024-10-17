using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrainerController : MonoBehaviour, ISavable
{
    public float speed = 3f;
    private Transform target;
    [SerializeField] private Image speechBubblePrefab;
    [SerializeField] private string dialogue;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private Vector3 shakeAmount = new Vector3(0.1f, 0.1f, 0);
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private Text dialogueText;
    [SerializeField] Sprite trainerInBattleSprite;
    [SerializeField] string trainerName;
    [SerializeField] GameObject fovCollider;

    bool battleLost = false;

    private Coroutine shakeCoroutine;

    public void MoveTowards(Transform playerTransform)
    {
        target = playerTransform;
        StartCoroutine(Move());
    }
    public void DisableFOV()
    {
        fovCollider.SetActive(false);
        battleLost = true;
    }
    public void EnableFOV()
    {
        fovCollider.SetActive(true);// Re-enable the FOV collider
        battleLost = false; // Reset the battle lost flag if needed
    }

    private IEnumerator Move()
    {
        while (Vector2.Distance(transform.position, target.position) > 1.5f)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            yield return null;
        }
        StartCoroutine(ShowDialogue());
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        if (battleLost)
        {
            DisableFOV();
        }
    }


    private IEnumerator ShowDialogue()
    {
        yield return new WaitForSeconds(1f);
        CreateSpeechBubble();
        
        shakeCoroutine = StartCoroutine(Shake(speechBubblePrefab.rectTransform, shakeDuration, shakeAmount));
        yield return StartCoroutine(TypeDialogue(dialogue));
    }

    void CreateSpeechBubble()
    {
        if (speechBubblePrefab != null)
        {
            speechBubblePrefab.gameObject.SetActive(true);
            dialogueText.text = ""; // Clear the text initially
        }
        else
        {
            Debug.LogError("Speech bubble prefab is not assigned.");
        }
    }

    private IEnumerator Shake(RectTransform targetTransform, float duration, Vector3 amount)
    {
        Vector3 originalPosition = targetTransform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            if (targetTransform == null) // Check if the targetTransform is null
                yield break;

            float x = originalPosition.x + Random.Range(-amount.x, amount.x);
            float y = originalPosition.y + Random.Range(-amount.y, amount.y);

            targetTransform.localPosition = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (targetTransform != null)
            targetTransform.localPosition = originalPosition;
    }

    private IEnumerator TypeDialogue(string dialogueToType)
    {
        foreach (char letter in dialogueToType)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(5f); // Wait for 2 seconds after typing
        speechBubblePrefab.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine); // Stop the shake coroutine if the speech bubble is destroyed
    }
    public string Name { get => trainerName; }

    public Sprite Sprite { get => trainerInBattleSprite;  }
}
