using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameplayHudController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Canvas canvas;

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
    private Text turnText;
    private Text weaponText;
    private Text powerText;
    private Text movementText;
    private Text blueLivesText;
    private Text redLivesText;
    private Text passText;
    private Font hudFont;

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
    }

    private void EnsureHud()
    {
        if (canvas == null)
        {
            CreateCanvas();
        }

        if (panelRoot != null)
        {
            return;
        }

        hudFont = ResolveFont();
        panelRoot = CreatePanel(canvas.transform);

        turnText = CreateText("Turn Text", panelRoot, new Vector2(0.025f, 0.55f), new Vector2(0.36f, 0.9f), labelFontSize, TextAnchor.MiddleLeft, textColor);
        weaponText = CreateText("Weapon Text", panelRoot, new Vector2(0.025f, 0.13f), new Vector2(0.36f, 0.48f), labelFontSize, TextAnchor.MiddleLeft, mutedTextColor);
        powerText = CreateText("Power Text", panelRoot, new Vector2(0.39f, 0.55f), new Vector2(0.58f, 0.9f), labelFontSize, TextAnchor.MiddleLeft, textColor);
        movementText = CreateText("Movement Text", panelRoot, new Vector2(0.39f, 0.13f), new Vector2(0.58f, 0.48f), labelFontSize, TextAnchor.MiddleLeft, mutedTextColor);
        blueLivesText = CreateText("Blue Lives Text", panelRoot, new Vector2(0.61f, 0.55f), new Vector2(0.78f, 0.9f), labelFontSize, TextAnchor.MiddleLeft, blueTeamColor);
        redLivesText = CreateText("Red Lives Text", panelRoot, new Vector2(0.61f, 0.13f), new Vector2(0.78f, 0.48f), labelFontSize, TextAnchor.MiddleLeft, redTeamColor);
        passText = CreatePassHint(panelRoot);
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

    private Text CreatePassHint(RectTransform parent)
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

        Text text = AddTextComponent(passLabelObject);
        ConfigureText(text, passFontSize, TextAnchor.MiddleCenter, textColor);
        if (text != null)
        {
            text.text = "PASS (P)";
            text.fontStyle = FontStyle.Bold;
        }

        return text;
    }

    private Text CreateText(string objectName, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        SetStretch(rectTransform, anchorMin, anchorMax);

        Text text = AddTextComponent(textObject);
        ConfigureText(text, fontSize, alignment, color);
        return text;
    }

    private Text AddTextComponent(GameObject textObject)
    {
        if (textObject == null)
        {
            Debug.LogWarning("GameplayHudController could not create HUD text because the target GameObject is missing.");
            return null;
        }

        Text text = textObject.GetComponent<Text>();
        if (text == null)
        {
            text = textObject.AddComponent<Text>();
        }

        if (text == null)
        {
            Debug.LogWarning($"GameplayHudController could not create a UnityEngine.UI.Text component on {textObject.name}.");
        }

        return text;
    }

    private void ConfigureText(Text text, int fontSize, TextAnchor alignment, Color color)
    {
        if (text == null)
        {
            Debug.LogWarning("GameplayHudController skipped HUD text setup because the UnityEngine.UI.Text component is missing.");
            return;
        }

        if (hudFont == null)
        {
            hudFont = ResolveFont();
        }

        text.font = hudFont;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
    }

    private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private Font ResolveFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            return font;
        }

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (font != null)
        {
            return font;
        }

        return Font.CreateDynamicFontFromOSFont("Arial", labelFontSize);
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

    private static void SetTextValue(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
