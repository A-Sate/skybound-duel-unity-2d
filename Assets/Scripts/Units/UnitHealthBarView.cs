using UnityEngine;

public class UnitHealthBarView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitController unit;

    [Header("Layout")]
    [SerializeField] private float width = 1.56f;
    [SerializeField] private float height = 0.1f;
    [SerializeField] private float verticalOffset = -0.18f;
    [SerializeField] private float borderThickness = 0.015f;
    [SerializeField] private float zOffset;
    [SerializeField] private int sortingOrder = 40;

    [Header("Colors")]
    [SerializeField] private Color shieldColor = new Color32(0x52, 0x57, 0x9d, 0xff);
    [SerializeField] private Color hpHighColor = new Color32(0x22, 0xc5, 0x5e, 0xff);
    [SerializeField] private Color hpMidColor = new Color32(0xf5, 0x9e, 0x0b, 0xff);
    [SerializeField] private Color hpLowColor = new Color32(0xef, 0x44, 0x44, 0xff);
    [SerializeField] private Color backgroundColor = new Color32(0x11, 0x18, 0x27, 0xff);
    [SerializeField] private Color borderColor = Color.white;

    private const string BarRootSuffix = " HP Shield Bar";

    private static Sprite generatedRectSprite;

    private GameObject barRoot;
    private SpriteRenderer borderRenderer;
    private SpriteRenderer backgroundRenderer;
    private SpriteRenderer hpRenderer;
    private SpriteRenderer shieldRenderer;

    private void Awake()
    {
        ResolveReferences();
        EnsureBarObjects();
        RefreshBar();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        EnsureBarObjects();
        RefreshBar();
    }

    private void OnDisable()
    {
        DestroyBarObjects();
    }

    private void OnDestroy()
    {
        DestroyBarObjects();
    }

    private void OnValidate()
    {
        width = Mathf.Max(0.05f, width);
        height = Mathf.Max(0.02f, height);
        borderThickness = Mathf.Max(0f, borderThickness);
    }

    private void ResolveReferences()
    {
        if (unit == null)
        {
            unit = GetComponent<UnitController>();
        }
    }

    private void EnsureBarObjects()
    {
        if (barRoot != null)
        {
            return;
        }

        barRoot = new GameObject($"{name}{BarRootSuffix}");
        borderRenderer = CreateSegment("Border", sortingOrder);
        backgroundRenderer = CreateSegment("Background", sortingOrder + 1);
        hpRenderer = CreateSegment("HP", sortingOrder + 2);
        shieldRenderer = CreateSegment("Shield", sortingOrder + 3);
    }

    private SpriteRenderer CreateSegment(string segmentName, int segmentSortingOrder)
    {
        GameObject segmentObject = new GameObject(segmentName);
        segmentObject.transform.SetParent(barRoot.transform, false);

        SpriteRenderer segmentRenderer = segmentObject.AddComponent<SpriteRenderer>();
        segmentRenderer.sprite = GetGeneratedRectSprite();
        segmentRenderer.sortingOrder = segmentSortingOrder;
        return segmentRenderer;
    }

    private void RefreshBar()
    {
        if (unit == null || barRoot == null)
        {
            return;
        }

        barRoot.transform.position = transform.position + new Vector3(0f, verticalOffset, zOffset);
        barRoot.transform.rotation = Quaternion.identity;
        barRoot.transform.localScale = Vector3.one;

        float safeWidth = Mathf.Max(0.05f, width);
        float safeHeight = Mathf.Max(0.02f, height);
        float maxTotalDurability = Mathf.Max(0f, unit.MaxHp + unit.MaxShield);
        float hpPercentOfTotal = maxTotalDurability > 0f ? Mathf.Clamp01(unit.CurrentHp / maxTotalDurability) : 0f;
        float shieldPercentOfTotal = maxTotalDurability > 0f ? Mathf.Clamp01(unit.CurrentShield / maxTotalDurability) : 0f;
        float hpPercent = unit.MaxHp > 0 ? Mathf.Clamp01(unit.CurrentHp / (float)unit.MaxHp) : 0f;

        SetCenteredSegment(borderRenderer, new Vector2(safeWidth + borderThickness * 2f, safeHeight + borderThickness * 2f), borderColor, true);
        SetCenteredSegment(backgroundRenderer, new Vector2(safeWidth, safeHeight), backgroundColor, true);
        SetFillSegment(hpRenderer, 0f, hpPercentOfTotal, safeWidth, safeHeight, GetHpColor(hpPercent), hpPercentOfTotal > 0f, 0f);
        SetFillSegment(shieldRenderer, hpPercentOfTotal, shieldPercentOfTotal, safeWidth, safeHeight, shieldColor, shieldPercentOfTotal > 0f, 0f);
    }

    private void SetCenteredSegment(SpriteRenderer targetRenderer, Vector2 size, Color color, bool visible)
    {
        if (targetRenderer == null)
        {
            return;
        }

        targetRenderer.enabled = visible;
        targetRenderer.color = color;
        targetRenderer.transform.localPosition = Vector3.zero;
        targetRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    private void SetFillSegment(SpriteRenderer targetRenderer, float startPercent, float fillPercent, float segmentWidth, float segmentHeight, Color color, bool visible, float yOffset)
    {
        if (targetRenderer == null)
        {
            return;
        }

        float clampedStartPercent = Mathf.Clamp01(startPercent);
        float clampedFillPercent = Mathf.Clamp01(fillPercent);
        float fillWidth = segmentWidth * Mathf.Min(clampedFillPercent, 1f - clampedStartPercent);
        float fillStartX = -segmentWidth * 0.5f + segmentWidth * clampedStartPercent;
        targetRenderer.enabled = visible && fillWidth > 0f;
        targetRenderer.color = color;
        targetRenderer.transform.localPosition = new Vector3(fillStartX + fillWidth * 0.5f, yOffset, 0f);
        targetRenderer.transform.localScale = new Vector3(fillWidth, segmentHeight, 1f);
    }

    private Color GetHpColor(float hpPercent)
    {
        if (hpPercent < 0.2f)
        {
            return hpLowColor;
        }

        if (hpPercent <= 0.45f)
        {
            return hpMidColor;
        }

        return hpHighColor;
    }

    private void DestroyBarObjects()
    {
        if (barRoot == null)
        {
            return;
        }

        Destroy(barRoot);
        barRoot = null;
        borderRenderer = null;
        backgroundRenderer = null;
        hpRenderer = null;
        shieldRenderer = null;
    }

    private static Sprite GetGeneratedRectSprite()
    {
        if (generatedRectSprite != null)
        {
            return generatedRectSprite;
        }

        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "Generated Health Bar Rect",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        generatedRectSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        generatedRectSprite.name = "Generated Health Bar Rect Sprite";
        return generatedRectSprite;
    }
}
