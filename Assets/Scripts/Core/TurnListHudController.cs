using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TurnListHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Canvas canvas;

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(240f, 286f);
    [SerializeField] private Vector2 panelOffset = new Vector2(18f, 148f);
    [SerializeField] private int maxVisibleSlots = 8;
    [SerializeField] private float slotHeight = 28f;
    [SerializeField] private float slotSpacing = 5f;
    [SerializeField] private int titleFontSize = 16;
    [SerializeField] private int slotFontSize = 15;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.04f, 0.06f, 0.1f, 0.72f);
    [SerializeField] private Color activeSlotColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private Color futureSlotColor = new Color(0.08f, 0.11f, 0.18f, 0.78f);
    [SerializeField] private Color titleColor = new Color(0.9f, 0.94f, 1f, 1f);
    [SerializeField] private Color blueTeamColor = new Color(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color redTeamColor = new Color(1f, 0.35f, 0.3f, 1f);

    private readonly List<UnitController> orderedUnits = new List<UnitController>();
    private readonly List<TurnSlotView> slotViews = new List<TurnSlotView>();

    private RectTransform panelRoot;
    private RectTransform slotContainer;
    private TextMeshProUGUI titleText;
    private TMP_FontAsset hudFontAsset;
    private bool attemptedResolveTmpFontAsset;
    private bool warnedMissingTmpFontAsset;

    private sealed class TurnSlotView
    {
        public GameObject Root;
        public Image Background;
        public Image TeamMarker;
        public TextMeshProUGUI IndexLabel;
        public TextMeshProUGUI Label;
    }

    private void Awake()
    {
        ResolveReferences();
        EnsureHud();
    }

    private void Start()
    {
        RefreshTurnList();
    }

    private void Update()
    {
        ResolveReferences();
        EnsureHud();
        RefreshTurnList();
    }

    private void OnValidate()
    {
        panelSize.x = Mathf.Max(160f, panelSize.x);
        panelSize.y = Mathf.Max(96f, panelSize.y);
        maxVisibleSlots = Mathf.Clamp(maxVisibleSlots, 1, 16);
        slotHeight = Mathf.Max(18f, slotHeight);
        slotSpacing = Mathf.Max(0f, slotSpacing);
        titleFontSize = Mathf.Max(10, titleFontSize);
        slotFontSize = Mathf.Max(10, slotFontSize);
    }

    public void SetTurnManager(TurnManager manager)
    {
        if (manager != null)
        {
            turnManager = manager;
        }
    }

    private void ResolveReferences()
    {
        if (turnManager == null)
        {
            turnManager = FindAnyObjectByType<TurnManager>();
        }
    }

    private void EnsureHud()
    {
        if (canvas == null)
        {
            CreateCanvas();
        }

        if (panelRoot == null)
        {
            hudFontAsset = ResolveTmpFontAsset();
            panelRoot = CreatePanel(canvas.transform);
            titleText = CreateText("Turn List Title", panelRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -34f), new Vector2(0f, -6f), titleFontSize, TextAlignmentOptions.Center, titleColor);
            titleText.text = "Turn List";
            titleText.fontStyle = FontStyles.Bold;

            slotContainer = CreateSlotContainer(panelRoot);
        }

        EnsureSlotCount(maxVisibleSlots);
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Turn List HUD Canvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 101;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private RectTransform CreatePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("Turn List Panel");
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = panelOffset;
        rectTransform.sizeDelta = panelSize;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = panelColor;

        return rectTransform;
    }

    private RectTransform CreateSlotContainer(RectTransform parent)
    {
        GameObject containerObject = new GameObject("Turn List Slots");
        containerObject.transform.SetParent(parent, false);

        RectTransform rectTransform = containerObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.offsetMin = new Vector2(10f, 10f);
        rectTransform.offsetMax = new Vector2(-10f, -38f);

        return rectTransform;
    }

    private void EnsureSlotCount(int targetCount)
    {
        while (slotViews.Count < targetCount)
        {
            slotViews.Add(CreateSlot(slotViews.Count));
        }

        for (int i = targetCount; i < slotViews.Count; i++)
        {
            if (slotViews[i].Root != null)
            {
                slotViews[i].Root.SetActive(false);
            }
        }
    }

    private TurnSlotView CreateSlot(int slotIndex)
    {
        GameObject slotObject = new GameObject($"Turn Slot {slotIndex + 1}");
        slotObject.transform.SetParent(slotContainer, false);

        RectTransform slotTransform = slotObject.AddComponent<RectTransform>();
        slotTransform.anchorMin = new Vector2(0f, 1f);
        slotTransform.anchorMax = new Vector2(1f, 1f);
        slotTransform.pivot = new Vector2(0.5f, 1f);
        slotTransform.anchoredPosition = new Vector2(0f, -slotIndex * (slotHeight + slotSpacing));
        slotTransform.sizeDelta = new Vector2(0f, slotHeight);

        Image background = slotObject.AddComponent<Image>();
        background.color = futureSlotColor;

        TextMeshProUGUI indexLabel = CreateText("Turn Index", slotTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(26f, 0f), slotFontSize, TextAlignmentOptions.Center, titleColor);

        GameObject markerObject = new GameObject("Team Marker");
        markerObject.transform.SetParent(slotObject.transform, false);
        RectTransform markerTransform = markerObject.AddComponent<RectTransform>();
        markerTransform.anchorMin = new Vector2(0f, 0f);
        markerTransform.anchorMax = new Vector2(0f, 1f);
        markerTransform.pivot = new Vector2(0f, 0.5f);
        markerTransform.offsetMin = new Vector2(30f, 0f);
        markerTransform.offsetMax = new Vector2(36f, 0f);

        Image marker = markerObject.AddComponent<Image>();
        marker.color = Color.white;

        TextMeshProUGUI label = CreateText("Unit Name", slotTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(44f, 0f), new Vector2(-8f, 0f), slotFontSize, TextAlignmentOptions.Left, titleColor);

        return new TurnSlotView
        {
            Root = slotObject,
            Background = background,
            TeamMarker = marker,
            IndexLabel = indexLabel,
            Label = label
        };
    }

    private TextMeshProUGUI CreateText(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        ConfigureText(text, fontSize, alignment, color);
        return text;
    }

    private void ConfigureText(TextMeshProUGUI text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        if (text == null)
        {
            return;
        }

        if (hudFontAsset == null)
        {
            hudFontAsset = ResolveTmpFontAsset();
        }

        if (hudFontAsset != null)
        {
            text.font = hudFontAsset;
        }

        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
    }

    private void RefreshTurnList()
    {
        if (turnManager == null)
        {
            SetTitle("Turn List");
            HideAllSlots();
            return;
        }

        int unitCount = turnManager.GetOrderedPlayableUnits(orderedUnits, maxVisibleSlots);
        SetTitle("Turn List");

        for (int i = 0; i < slotViews.Count; i++)
        {
            TurnSlotView slot = slotViews[i];
            bool shouldShow = i < unitCount;
            if (slot.Root != null)
            {
                slot.Root.SetActive(shouldShow);
            }

            if (!shouldShow)
            {
                continue;
            }

            ApplySlot(slot, orderedUnits[i], i, i == 0);
        }
    }

    private void ApplySlot(TurnSlotView slot, UnitController unit, int slotIndex, bool isActive)
    {
        if (slot == null || unit == null)
        {
            return;
        }

        Color teamColor = GetTeamColor(unit.Team);
        if (slot.Background != null)
        {
            slot.Background.color = isActive ? activeSlotColor : futureSlotColor;
        }

        if (slot.TeamMarker != null)
        {
            slot.TeamMarker.color = teamColor;
        }

        if (slot.IndexLabel != null)
        {
            slot.IndexLabel.text = slotIndex.ToString();
            slot.IndexLabel.color = titleColor;
            slot.IndexLabel.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
        }

        if (slot.Label != null)
        {
            slot.Label.text = unit.DisplayName;
            slot.Label.color = teamColor;
            slot.Label.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    private void HideAllSlots()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            if (slotViews[i].Root != null)
            {
                slotViews[i].Root.SetActive(false);
            }
        }
    }

    private void SetTitle(string value)
    {
        if (titleText != null)
        {
            titleText.text = value;
        }
    }

    private Color GetTeamColor(UnitTeam team)
    {
        return team == UnitTeam.Blue ? blueTeamColor : redTeamColor;
    }

    private TMP_FontAsset ResolveTmpFontAsset()
    {
        if (attemptedResolveTmpFontAsset)
        {
            return hudFontAsset;
        }

        attemptedResolveTmpFontAsset = true;

        try
        {
            TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (fontAsset != null)
            {
                return fontAsset;
            }

            TMP_FontAsset[] availableFontAssets = Resources.LoadAll<TMP_FontAsset>(string.Empty);
            if (availableFontAssets != null && availableFontAssets.Length > 0)
            {
                return availableFontAssets[0];
            }
        }
        catch (System.Exception exception)
        {
            WarnMissingTmpFontAsset($"TurnListHudController TextMeshPro font lookup failed: {exception.Message}");
            return null;
        }

        WarnMissingTmpFontAsset("TurnListHudController could not find a TextMeshPro font asset in Resources. Import TMP Essential Resources so the turn list can use LiberationSans SDF.");
        return null;
    }

    private void WarnMissingTmpFontAsset(string message)
    {
        if (warnedMissingTmpFontAsset)
        {
            return;
        }

        Debug.LogWarning(message);
        warnedMissingTmpFontAsset = true;
    }
}
