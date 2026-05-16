using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionVisual : MonoBehaviour
{
    [Header("Presentation")]
    [SerializeField] private float duration = 0.45f;
    [SerializeField] private float startSize = 0.25f;
    [SerializeField] private float endSize = 1.2f;
    [SerializeField] private Color startColor = new Color(1f, 0.72f, 0.18f, 0.75f);
    [SerializeField] private Color endColor = new Color(1f, 0.28f, 0.05f, 0f);

    private const int GeneratedSpritePixels = 64;

    private static Sprite generatedCircleSprite;

    private SpriteRenderer spriteRenderer;
    private float elapsedTime;

    public float Duration => duration;
    public float StartSize => startSize;
    public float EndSize => endSize;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetGeneratedCircleSprite();
        ApplyVisual(0f);
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float normalizedTime = duration > 0f ? Mathf.Clamp01(elapsedTime / duration) : 1f;

        ApplyVisual(normalizedTime);

        if (elapsedTime >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void Configure(float configuredStartSize, float configuredEndSize, float configuredDuration, Color configuredColor)
    {
        startSize = Mathf.Max(0.01f, configuredStartSize);
        endSize = Mathf.Max(startSize, configuredEndSize);
        duration = Mathf.Max(0.01f, configuredDuration);

        startColor = configuredColor;
        endColor = new Color(configuredColor.r, configuredColor.g * 0.55f, configuredColor.b * 0.2f, 0f);

        elapsedTime = 0f;
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = GetGeneratedCircleSprite();
        ApplyVisual(0f);
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        startSize = Mathf.Max(0.01f, startSize);
        endSize = Mathf.Max(startSize, endSize);
    }

    private void ApplyVisual(float normalizedTime)
    {
        float currentSize = Mathf.Lerp(startSize, endSize, normalizedTime);
        transform.localScale = Vector3.one * currentSize;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, normalizedTime);
        }
    }

    private static Sprite GetGeneratedCircleSprite()
    {
        if (generatedCircleSprite != null)
        {
            return generatedCircleSprite;
        }

        Texture2D texture = new Texture2D(GeneratedSpritePixels, GeneratedSpritePixels, TextureFormat.RGBA32, false)
        {
            name = "Generated Explosion Circle",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new Vector2((GeneratedSpritePixels - 1) * 0.5f, (GeneratedSpritePixels - 1) * 0.5f);
        float radius = GeneratedSpritePixels * 0.48f;

        for (int y = 0; y < GeneratedSpritePixels; y++)
        {
            for (int x = 0; x < GeneratedSpritePixels; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - ((distance - radius * 0.78f) / (radius * 0.22f)));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        generatedCircleSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, GeneratedSpritePixels, GeneratedSpritePixels),
            new Vector2(0.5f, 0.5f),
            GeneratedSpritePixels);
        generatedCircleSprite.name = "Generated Explosion Circle Sprite";

        return generatedCircleSprite;
    }
}
