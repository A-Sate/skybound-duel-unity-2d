using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class UnitAimDebugView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UnitController unit;
    [SerializeField] private LineRenderer aimLine;

    [Header("Debug Aim Line")]
    [SerializeField] private bool showAimLine = true;
    [SerializeField] private bool showInactiveAimLine = true;
    [SerializeField] private float aimLineLength = 1.6f;
    [SerializeField] private float activeLineWidth = 0.06f;
    [SerializeField] private float inactiveLineWidth = 0.035f;
    [SerializeField] private Vector2 firingOriginOffset = new Vector2(0.55f, 0.25f);
    [SerializeField] private Color activeLineColor = new Color(1f, 0.95f, 0.2f, 1f);
    [SerializeField] private Color inactiveLineColor = new Color(1f, 0.95f, 0.2f, 0.28f);

    private void Awake()
    {
        ResolveReferences();
        ConfigureLineRenderer();
    }

    private void OnValidate()
    {
        aimLineLength = Mathf.Max(0f, aimLineLength);
        activeLineWidth = Mathf.Max(0f, activeLineWidth);
        inactiveLineWidth = Mathf.Max(0f, inactiveLineWidth);
    }

    private void LateUpdate()
    {
        UpdateAimLine();
    }

    private void ResolveReferences()
    {
        if (unit == null)
        {
            unit = GetComponent<UnitController>();
        }

        if (aimLine == null)
        {
            aimLine = GetComponent<LineRenderer>();
        }
    }

    private void ConfigureLineRenderer()
    {
        if (aimLine == null)
        {
            return;
        }

        aimLine.positionCount = 2;
        aimLine.useWorldSpace = true;
        aimLine.numCapVertices = 2;
        aimLine.numCornerVertices = 2;
        aimLine.textureMode = LineTextureMode.Stretch;
        aimLine.sortingOrder = 25;

        if (aimLine.sharedMaterial == null)
        {
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                aimLine.sharedMaterial = new Material(spriteShader);
            }
        }
    }

    private void UpdateAimLine()
    {
        ResolveReferences();

        if (aimLine == null || unit == null)
        {
            return;
        }

        bool shouldShow = showAimLine && !unit.IsKnockedOut && (unit.IsActiveTurn || showInactiveAimLine);
        aimLine.enabled = shouldShow;
        if (!shouldShow)
        {
            return;
        }

        float facingSign = unit.Facing == UnitFacing.Right ? 1f : -1f;
        float angleRadians = unit.LocalAngle * Mathf.Deg2Rad;
        Vector3 aimDirection = new Vector3(
            Mathf.Cos(angleRadians) * facingSign,
            Mathf.Sin(angleRadians),
            0f).normalized;

        Vector3 origin = transform.position + new Vector3(
            Mathf.Abs(firingOriginOffset.x) * facingSign,
            firingOriginOffset.y,
            0f);

        Color lineColor = unit.IsActiveTurn ? activeLineColor : inactiveLineColor;
        float lineWidth = unit.IsActiveTurn ? activeLineWidth : inactiveLineWidth;

        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.SetPosition(0, origin);
        aimLine.SetPosition(1, origin + aimDirection * aimLineLength);
    }
}
