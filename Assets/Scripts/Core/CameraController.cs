using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Camera Foundation")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private float orthographicSize = 3f;
    [SerializeField] private float followSpeed = 6f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 0.85f);
    [SerializeField] private float cameraZ = -10f;

    [Header("Camera Bounds")]
    [SerializeField] private bool useCameraBounds = true;
    [SerializeField] private Vector2 minCameraBounds = new Vector2(-12f, -2f);
    [SerializeField] private Vector2 maxCameraBounds = new Vector2(12f, 8f);

    [Header("Follow State")]
    [SerializeField] private Transform currentTarget;

    public float OrthographicSize => orthographicSize;
    public float FollowSpeed => followSpeed;
    public Vector2 Offset => offset;
    public Vector2 MinCameraBounds => minCameraBounds;
    public Vector2 MaxCameraBounds => maxCameraBounds;
    public Transform CurrentTarget => currentTarget;

    private void Awake()
    {
        ResolveReferences();
        ApplyCameraSettings();
    }

    private void Start()
    {
        SnapToCurrentTarget();
    }

    private void LateUpdate()
    {
        ResolveReferences();
        ApplyCameraSettings();

        currentTarget = ResolveFollowTarget();
        if (currentTarget == null)
        {
            return;
        }

        Vector3 desiredPosition = GetDesiredCameraPosition(currentTarget.position);
        float interpolation = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, interpolation);
    }

    private void OnValidate()
    {
        orthographicSize = Mathf.Max(0.1f, orthographicSize);
        followSpeed = Mathf.Max(0f, followSpeed);
        if (minCameraBounds.x > maxCameraBounds.x)
        {
            float previousMinX = minCameraBounds.x;
            minCameraBounds.x = maxCameraBounds.x;
            maxCameraBounds.x = previousMinX;
        }

        if (minCameraBounds.y > maxCameraBounds.y)
        {
            float previousMinY = minCameraBounds.y;
            minCameraBounds.y = maxCameraBounds.y;
            maxCameraBounds.y = previousMinY;
        }
    }

    private void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (turnManager == null)
        {
            turnManager = FindAnyObjectByType<TurnManager>();
        }
    }

    private void ApplyCameraSettings()
    {
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.orthographic = true;
        targetCamera.orthographicSize = orthographicSize;
    }

    private Transform ResolveFollowTarget()
    {
        UnitController activeUnit = turnManager != null ? turnManager.ActiveUnit : null;
        if (activeUnit == null)
        {
            return null;
        }

        ProjectileController activeProjectile = activeUnit.ActiveProjectile;
        if (activeProjectile != null)
        {
            return activeProjectile.transform;
        }

        return activeUnit.transform;
    }

    private Vector3 GetDesiredCameraPosition(Vector3 targetPosition)
    {
        Vector3 desiredPosition = targetPosition + new Vector3(offset.x, offset.y, 0f);
        desiredPosition.z = cameraZ;

        if (useCameraBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minCameraBounds.x, maxCameraBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minCameraBounds.y, maxCameraBounds.y);
        }

        return desiredPosition;
    }

    private void SnapToCurrentTarget()
    {
        currentTarget = ResolveFollowTarget();
        if (currentTarget != null)
        {
            transform.position = GetDesiredCameraPosition(currentTarget.position);
        }
    }
}
