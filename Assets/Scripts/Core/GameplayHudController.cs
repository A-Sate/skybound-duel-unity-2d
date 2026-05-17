using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class GameplayHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Canvas canvas;
    [SerializeField] private TurnListHudController turnListHudController;

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(860f, 104f);
    [SerializeField] private Vector2 panelOffset = new Vector2(0f, 18f);
    [SerializeField] private int labelFontSize = 18;
    [SerializeField] private int passFontSize = 20;

    [Header("Colors")]
    [SerializeField] private Color panelColor = new Color(0.04f, 0.06f, 0.1f, 0.82f);
    [SerializeField] private Color textColor = new Color(0.95f, 0.97f, 1f, 1f);
    [SerializeField] private Color mutedTextColor = new Color(0.7f, 0.76f, 0.86f, 1f);
    [SerializeField] private Color blueTeamColor = new Color(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color redTeamColor = new Color(1f, 0.35f, 0.3f, 1f);
    [SerializeField] private Color passButtonColor = new Color(0.14f, 0.18f, 0.27f, 0.95f);

    private RectTransform panelRoot;
    private TextMeshProUGUI turnText;
    private TextMeshProUGUI weaponText;
    private TextMeshProUGUI powerText;
    private TextMeshProUGUI movementText;
    private TextMeshProUGUI blueLivesText;
    private TextMeshProUGUI redLivesText;
    private TextMeshProUGUI passText;
    private TMP_FontAsset hudFontAsset;
    private bool attemptedResolveTmpFontAsset;
    private bool warnedMissingTmpFontAsset;

    private void Awake()
    {
        ResolveReferences();
        EnsureHud();
    }

    private void Start()
    {
        RefreshHud();
    }

    private void Update()
    {
        ResolveReferences();
        EnsureHud();
        RefreshHud();
    }

    private void OnValidate()
    {
        panelSize.x = Mathf.Max(320f, panelSize.x);
        panelSize.y = Mathf.Max(72f, panelSize.y);
        labelFontSize = Mathf.Max(10, labelFontSize);
        passFontSize = Mathf.Max(10, passFontSize);
    }

    private void ResolveReferences()
    {
        if (turnManager == null)
        {
            turnManager = FindAnyObjectByType<TurnManager>();
        }

        if (turnListHudController == null)
        {
            turnListHudController = GetComponent<TurnListHudController>();
        }
    }

    private void EnsureHud()
    {
        if (canvas == null)
        {
            CreateCanvas();
        }

        if (panelRoot != null)
        {
            EnsureTurnListHud();
            return;
        }

        hudFontAsset = ResolveTmpFontAsset();
        panelRoot = CreatePanel(canvas.transform);

        turnText = CreateText("Turn Text", panelRoot, new Vector2(0.025f, 0.55f), new Vector2(0.36f, 0.9f), labelFontSize, TextAlignmentOptions.Left, textColor);
        weaponText = CreateText("Weapon Text", panelRoot, new Vector2(0.025f, 0.13f), new Vector2(0.36f, 0.48f), labelFontSize, TextAlignmentOptions.Left, mutedTextColor);
        powerText = CreateText("Power Text", panelRoot, new Vector2(0.39f, 0.55f), new Vector2(0.58f, 0.9f), labelFontSize, TextAlignmentOptions.Left, textColor);
        movementText = CreateText("Movement Text", panelRoot, new Vector2(0.39f, 0.13f), new Vector2(0.58f, 0.48f), labelFontSize, TextAlignmentOptions.Left, mutedTextColor);
        blueLivesText = CreateText("Blue Lives Text", panelRoot, new Vector2(0.61f, 0.55f), new Vector2(0.78f, 0.9f), labelFontSize, TextAlignmentOptions.Left, blueTeamColor);
        redLivesText = CreateText("Red Lives Text", panelRoot, new Vector2(0.61f, 0.13f), new Vector2(0.78f, 0.48f), labelFontSize, TextAlignmentOptions.Left, redTeamColor);
        passText = CreatePassHint(panelRoot);
        EnsureTurnListHud();
    }

    private void EnsureTurnListHud()
    {
        if (turnListHudController == null)
        {
            turnListHudController = GetComponent<TurnListHudController>();
        }

        if (turnListHudController == null)
        {
            turnListHudController = gameObject.AddComponent<TurnListHudController>();
        }

        turnListHudController.SetTurnManager(turnManager);
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Gameplay HUD Canvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    private RectTransform CreatePanel(Transform parent)
    {
        GameObject panelObject = new GameObject("Gameplay HUD Panel");
        panelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = panelObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0f);
        rectTransform.anchorMax = new Vector2(0.5f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = panelOffset;
        rectTransform.sizeDelta = panelSize;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = panelColor;

        return rectTransform;
    }

    private TextMeshProUGUI CreatePassHint(RectTransform parent)
    {
        GameObject passObject = new GameObject("Pass Hint");
        passObject.transform.SetParent(parent, false);

        RectTransform passTransform = passObject.AddComponent<RectTransform>();
        SetStretch(passTransform, new Vector2(0.81f, 0.22f), new Vector2(0.975f, 0.78f));

        Image passImage = passObject.AddComponent<Image>();
        passImage.color = passButtonColor;

        GameObject passLabelObject = new GameObject("Pass Hint Text");
        passLabelObject.transform.SetParent(passObject.transform, false);

        RectTransform passLabelTransform = passLabelObject.AddComponent<RectTransform>();
        SetStretch(passLabelTransform, Vector2.zero, Vector2.one);

        TextMeshProUGUI text = AddTextComponent(passLabelObject);
        ConfigureText(text, passFontSize, TextAlignmentOptions.Center, textColor);
        if (text != null)
        {
            text.text = "PASS (P)";
            text.fontStyle = FontStyles.Bold;
        }

        return text;
    }

    private TextMeshProUGUI CreateText(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        SetStretch(rectTransform, anchorMin, anchorMax);

        TextMeshProUGUI text = AddTextComponent(textObject);
        ConfigureText(text, fontSize, alignment, color);
        return text;
    }

    private TextMeshProUGUI AddTextComponent(GameObject textObject)
    {
        if (textObject == null)
        {
            Debug.LogWarning("GameplayHudController could not create HUD text because the target GameObject is missing.");
            return null;
        }

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        if (text == null)
        {
            Debug.LogWarning($"GameplayHudController could not create a TextMeshProUGUI component on {textObject.name}.");
        }

        return text;
    }

    private void ConfigureText(TextMeshProUGUI text, int fontSize, TextAlignmentOptions alignment, Color color)
    {
        if (text == null)
        {
            Debug.LogWarning("GameplayHudController skipped HUD text setup because the TextMeshProUGUI component is missing.");
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
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
    }

    private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
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
            WarnMissingTmpFontAsset($"TextMeshPro font lookup failed: {exception.Message}");
            return null;
        }

        WarnMissingTmpFontAsset("GameplayHudController could not find a TextMeshPro font asset in Resources. Import TMP Essential Resources so the HUD can use LiberationSans SDF.");
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

    private void RefreshHud()
    {
        if (turnManager == null)
        {
            SetNoTurnState();
            return;
        }

        UnitController activeUnit = turnManager.ActiveUnit;
        if (activeUnit == null)
        {
            SetNoTurnState();
            SetLivesText();
            return;
        }

        WeaponData selectedWeapon = activeUnit.SelectedWeapon;
        string selectedWeaponName = selectedWeapon != null ? selectedWeapon.DisplayName : "None";
        string teamName = activeUnit.Team.ToString();

        SetTextValue(turnText, $"Turn: {activeUnit.PlayerName} ({teamName})");
        SetTextValue(weaponText, $"Weapon: {selectedWeaponName}");
        SetTextValue(powerText, activeUnit.IsCharging ? $"Power: {activeUnit.CurrentPower:0}%" : "Power: 0%");
        SetTextValue(movementText, $"Move: {activeUnit.RemainingMovement:0}/{activeUnit.MaxMovement:0}");
        SetLivesText();
        SetTextValue(passText, "PASS (P)");
    }

    private void SetNoTurnState()
    {
        SetTextValue(turnText, "Turn: None");
        SetTextValue(weaponText, "Weapon: None");
        SetTextValue(powerText, "Power: 0%");
        SetTextValue(movementText, "Move: 0/0");
        SetTextValue(blueLivesText, "Blue Lives: 0");
        SetTextValue(redLivesText, "Red Lives: 0");
        SetTextValue(passText, "PASS (P)");
    }

    private void SetLivesText()
    {
        SetTextValue(blueLivesText, $"Blue Lives: {turnManager.BlueTeamLives}");
        SetTextValue(redLivesText, $"Red Lives: {turnManager.RedTeamLives}");
    }

    private static void SetTextValue(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
