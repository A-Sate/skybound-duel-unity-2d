using UnityEngine;

public class UnitHitbox : MonoBehaviour
{
    [Header("Semi-Disc Hitbox")]
    [SerializeField] private UnitController unit;
    [SerializeField] private float width = 1.45f;
    [SerializeField] private float depth = 0.58f;
    [SerializeField] private Vector2 localOffset = new Vector2(0f, 0.28f);

    [Header("Debug")]
    [SerializeField] private bool showSceneGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0.15f, 1f, 0.75f, 0.85f);

    public UnitController Unit
    {
        get
        {
            if (unit == null)
            {
                unit = GetComponent<UnitController>();
            }

            return unit;
        }
    }

    public float Width => width;
    public float Depth => depth;
    public Vector2 LocalOffset => localOffset;

    private void Awake()
    {
        if (unit == null)
        {
            unit = GetComponent<UnitController>();
        }
    }

    private void OnValidate()
    {
        width = Mathf.Max(0.01f, width);
        depth = Mathf.Max(0.01f, depth);
    }

    public bool ContainsWorldPoint(Vector2 worldPoint)
    {
        return ContainsLocalPoint(WorldToLocalPoint(worldPoint));
    }

    public bool TryGetFirstHitOnSegment(Vector2 worldStart, Vector2 worldEnd, float maxStepDistance, out Vector2 worldHitPoint)
    {
        float safeStepDistance = Mathf.Max(0.01f, maxStepDistance);
        float segmentDistance = Vector2.Distance(worldStart, worldEnd);
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(segmentDistance / safeStepDistance));

        Vector2 previousPoint = worldStart;
        if (ContainsWorldPoint(previousPoint))
        {
            worldHitPoint = previousPoint;
            return true;
        }

        for (int i = 1; i <= sampleCount; i++)
        {
            Vector2 samplePoint = Vector2.Lerp(worldStart, worldEnd, i / (float)sampleCount);
            if (ContainsWorldPoint(samplePoint))
            {
                worldHitPoint = RefineFirstHit(previousPoint, samplePoint);
                return true;
            }

            previousPoint = samplePoint;
        }

        worldHitPoint = default;
        return false;
    }

    private bool ContainsLocalPoint(Vector2 localPoint)
    {
        float halfWidth = width * 0.5f;
        float topY = localOffset.y + depth * 0.5f;
        float localX = localPoint.x - localOffset.x;

        if (Mathf.Abs(localX) > halfWidth || localPoint.y > topY)
        {
            return false;
        }

        float normalizedX = localX / halfWidth;
        float arcY = topY - depth * Mathf.Sqrt(Mathf.Clamp01(1f - normalizedX * normalizedX));
        return localPoint.y >= arcY;
    }

    private Vector2 RefineFirstHit(Vector2 outsidePoint, Vector2 insidePoint)
    {
        Vector2 low = outsidePoint;
        Vector2 high = insidePoint;

        for (int i = 0; i < 8; i++)
        {
            Vector2 midpoint = Vector2.Lerp(low, high, 0.5f);
            if (ContainsWorldPoint(midpoint))
            {
                high = midpoint;
            }
            else
            {
                low = midpoint;
            }
        }

        return high;
    }

    private Vector2 WorldToLocalPoint(Vector2 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(new Vector3(worldPoint.x, worldPoint.y, transform.position.z));
        return new Vector2(localPoint.x, localPoint.y);
    }

    private Vector3 LocalToWorldPoint(Vector2 localPoint)
    {
        return transform.TransformPoint(new Vector3(localPoint.x, localPoint.y, 0f));
    }

    private void OnDrawGizmos()
    {
        if (!showSceneGizmo)
        {
            return;
        }

        DrawSemiDiscGizmo();
    }

    private void DrawSemiDiscGizmo()
    {
        float safeWidth = Mathf.Max(0.01f, width);
        float safeDepth = Mathf.Max(0.01f, depth);
        float halfWidth = safeWidth * 0.5f;
        float topY = localOffset.y + safeDepth * 0.5f;

        Gizmos.color = gizmoColor;

        Vector3 previousPoint = LocalToWorldPoint(new Vector2(localOffset.x - halfWidth, topY));
        Vector3 topRight = LocalToWorldPoint(new Vector2(localOffset.x + halfWidth, topY));
        Gizmos.DrawLine(previousPoint, topRight);

        const int arcSegments = 24;
        for (int i = 1; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            float x = Mathf.Lerp(-halfWidth, halfWidth, t);
            float normalizedX = x / halfWidth;
            float y = topY - safeDepth * Mathf.Sqrt(Mathf.Clamp01(1f - normalizedX * normalizedX));
            Vector3 currentPoint = LocalToWorldPoint(new Vector2(localOffset.x + x, y));
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}
