using UnityEngine;

public class PokemonSpriteScaler : MonoBehaviour
{
    public Vector2 targetSize = new Vector2(50f, 50f);

    void Start()
    {
        ScaleSpriteToTargetSize();
    }

    public void ScaleSpriteToTargetSize()
    {
        Debug.Log("Scaling sprite size");
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;
            float scaleFactorX = targetSize.x / spriteWidth;
            float scaleFactorY = targetSize.y / spriteHeight;
            float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);
            transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
    }
}
