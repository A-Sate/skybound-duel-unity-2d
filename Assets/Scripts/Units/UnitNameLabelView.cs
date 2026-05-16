using UnityEngine;

public class UnitNameLabelView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitController unit;

    [Header("Layout")]
    [SerializeField] private float verticalOffset = -0.36f;
    [SerializeField] private int fontSize = 48;
    [SerializeField] private float characterSize = 0.035f;
    [SerializeField] private float zOffset;
    [SerializeField] private int sortingOrder = 45;

    [Header("Colors")]
    [SerializeField] private Color blueNameColor = new Color(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color redNameColor = new Color(1f, 0.35f, 0.3f, 1f);

    private const string LabelRootSuffix = " Name Label";

    private GameObject labelRoot;
    private TextMesh textMesh;
    private MeshRenderer textRenderer;

    private void Awake()
    {
        ResolveReferences();
        EnsureLabelObject();
        RefreshLabel();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        EnsureLabelObject();
        RefreshLabel();
    }

    private void OnDisable()
    {
        DestroyLabelObject();
    }

    private void OnDestroy()
    {
        DestroyLabelObject();
    }

    private void OnValidate()
    {
        fontSize = Mathf.Max(1, fontSize);
        characterSize = Mathf.Max(0.001f, characterSize);
    }

    private void ResolveReferences()
    {
        if (unit == null)
        {
            unit = GetComponent<UnitController>();
        }
    }

    private void EnsureLabelObject()
    {
        if (labelRoot != null)
        {
            return;
        }

        labelRoot = new GameObject($"{name}{LabelRootSuffix}");
        textMesh = labelRoot.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        textRenderer = labelRoot.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingOrder = sortingOrder;
        }
    }

    private void RefreshLabel()
    {
        if (unit == null || labelRoot == null || textMesh == null)
        {
            return;
        }

        labelRoot.transform.position = transform.position + new Vector3(0f, verticalOffset, zOffset);
        labelRoot.transform.rotation = Quaternion.identity;
        labelRoot.transform.localScale = Vector3.one;

        textMesh.text = unit.PlayerName;
        textMesh.fontSize = fontSize;
        textMesh.characterSize = characterSize;
        textMesh.color = unit.Team == UnitTeam.Blue ? blueNameColor : redNameColor;

        if (textRenderer != null)
        {
            textRenderer.sortingOrder = sortingOrder;
        }
    }

    private void DestroyLabelObject()
    {
        if (labelRoot == null)
        {
            return;
        }

        Destroy(labelRoot);
        labelRoot = null;
        textMesh = null;
        textRenderer = null;
    }
}
