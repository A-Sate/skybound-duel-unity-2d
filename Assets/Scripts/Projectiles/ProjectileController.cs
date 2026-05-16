using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProjectileController : MonoBehaviour
{
    [Header("Foundation References")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WindManager windManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private UnitController ownerUnit;

    [Header("Projectile State")]
    [SerializeField] private Vector2 velocity;
    [SerializeField] private bool launched;
    [SerializeField] private float elapsedLifetime;

    [Header("Projectile Tuning")]
    [SerializeField] private Vector2 gravity = new Vector2(0f, -4.5f);
    [SerializeField] private float maxProjectileSpeed = 8f;
    [SerializeField] private float projectileWindAccelerationScale = 1f;
    [SerializeField] private Vector2 worldBoundsMin = new Vector2(-24f, -8f);
    [SerializeField] private Vector2 worldBoundsMax = new Vector2(24f, 30f);
    [SerializeField] private float projectileLifetime = 10f;
    [SerializeField] private float groundY = -1.25f;
    [SerializeField] private float sideViewPlaneZ;

    [Header("Projectile Visual")]
    [SerializeField] private float projectileSizeToWorldScale = 0.0375f;

    [Header("Unit Hit Detection")]
    [SerializeField] private bool detectUnitHitboxes = true;
    [SerializeField] private float unitHitboxSampleDistance = 0.05f;

    [Header("Impact Visual")]
    [SerializeField] private bool spawnImpactVisual = true;
    [SerializeField] private float impactVisualDuration = 0.45f;
    [SerializeField] private float impactVisualStartSize = 0.25f;
    [SerializeField] private float impactVisualRadiusScale = 0.6f;
    [SerializeField] private float impactVisualMinEndSize = 0.45f;
    [SerializeField] private float impactVisualMaxEndSize = 1.8f;
    [SerializeField, Range(0f, 1f)] private float impactVisualStartAlpha = 0.75f;

    public WeaponData WeaponData => weaponData;
    public Vector2 Velocity => velocity;
    public bool Launched => launched;
    public Vector2 Gravity => gravity;
    public float MaxProjectileSpeed => maxProjectileSpeed;
    public float ProjectileWindAccelerationScale => projectileWindAccelerationScale;
    public Vector2 WorldBoundsMin => worldBoundsMin;
    public Vector2 WorldBoundsMax => worldBoundsMax;
    public float ProjectileLifetime => projectileLifetime;
    public float GroundY => groundY;
    public float ProjectileSizeToWorldScale => projectileSizeToWorldScale;
    public UnitController OwnerUnit => ownerUnit;

    public event Action<ProjectileController> Resolved;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyWeaponPresentation();
    }

    private void Update()
    {
        if (!launched)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        elapsedLifetime += deltaTime;

        Vector2 acceleration = gravity;
        if (windManager != null)
        {
            acceleration += windManager.GetWindAcceleration(projectileWindAccelerationScale);
        }

        velocity += acceleration * deltaTime;
        velocity = ClampSpeed(velocity);

        Vector3 previousPosition = transform.position;
        Vector3 nextPosition = previousPosition + (Vector3)(velocity * deltaTime);
        nextPosition.z = sideViewPlaneZ;
        transform.position = nextPosition;

        CheckResolution(previousPosition, nextPosition);
    }

    public void Configure(WeaponData data, WindManager wind)
    {
        weaponData = data;
        windManager = wind;
        ApplyWeaponPresentation();
    }

    public void Configure(WeaponData data, WindManager wind, UnitController owner)
    {
        Configure(data, wind);
        ownerUnit = owner;
    }

    public void Launch(Vector2 initialVelocity)
    {
        velocity = ClampSpeed(initialVelocity);
        elapsedLifetime = 0f;
        launched = true;
    }

    public void SetSideViewPlane(float z)
    {
        sideViewPlaneZ = z;

        Vector3 position = transform.position;
        position.z = sideViewPlaneZ;
        transform.position = position;
    }

    private void OnValidate()
    {
        maxProjectileSpeed = Mathf.Max(0f, maxProjectileSpeed);
        projectileWindAccelerationScale = Mathf.Max(0f, projectileWindAccelerationScale);
        projectileLifetime = Mathf.Max(0.1f, projectileLifetime);
        projectileSizeToWorldScale = Mathf.Max(0.001f, projectileSizeToWorldScale);
        unitHitboxSampleDistance = Mathf.Max(0.01f, unitHitboxSampleDistance);
        impactVisualDuration = Mathf.Max(0.01f, impactVisualDuration);
        impactVisualStartSize = Mathf.Max(0.01f, impactVisualStartSize);
        impactVisualRadiusScale = Mathf.Max(0f, impactVisualRadiusScale);
        impactVisualMinEndSize = Mathf.Max(0.01f, impactVisualMinEndSize);
        impactVisualMaxEndSize = Mathf.Max(impactVisualMinEndSize, impactVisualMaxEndSize);
    }

    private void ApplyWeaponPresentation()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (weaponData == null || spriteRenderer == null)
        {
            return;
        }

        if (weaponData.ProjectileSprite != null)
        {
            spriteRenderer.sprite = weaponData.ProjectileSprite;
        }

        spriteRenderer.color = weaponData.ProjectileColor;
        float visualScale = Mathf.Max(0.001f, weaponData.ProjectileSize * projectileSizeToWorldScale);
        transform.localScale = Vector3.one * visualScale;
    }

    private void CheckResolution(Vector3 previousPosition, Vector3 currentPosition)
    {
        if (TryResolveUnitHit(previousPosition, currentPosition))
        {
            return;
        }

        Vector3 position = currentPosition;
        Vector2 position2D = new Vector2(position.x, position.y);

        if (position.y <= groundY && velocity.y <= 0f)
        {
            Vector3 impactPosition = position;
            impactPosition.y = groundY;
            impactPosition.z = sideViewPlaneZ;

            Debug.Log($"Projectile impact at {impactPosition}");
            SpawnImpactVisual(impactPosition);
            Resolve();
            return;
        }

        bool outsideBounds =
            position2D.x < worldBoundsMin.x ||
            position2D.x > worldBoundsMax.x ||
            position2D.y < worldBoundsMin.y ||
            position2D.y > worldBoundsMax.y;

        if (outsideBounds || elapsedLifetime >= projectileLifetime)
        {
            Resolve();
        }
    }

    private bool TryResolveUnitHit(Vector3 previousPosition, Vector3 currentPosition)
    {
        if (!detectUnitHitboxes)
        {
            return false;
        }

        UnitHitbox[] hitboxes = FindObjectsByType<UnitHitbox>(FindObjectsInactive.Exclude);
        UnitHitbox closestHitbox = null;
        UnitController closestUnit = null;
        Vector2 closestHitPoint = default;
        float closestDistanceSqr = float.MaxValue;

        Vector2 segmentStart = new Vector2(previousPosition.x, previousPosition.y);
        Vector2 segmentEnd = new Vector2(currentPosition.x, currentPosition.y);

        for (int i = 0; i < hitboxes.Length; i++)
        {
            UnitHitbox hitbox = hitboxes[i];
            if (hitbox == null || !hitbox.isActiveAndEnabled)
            {
                continue;
            }

            UnitController hitUnit = hitbox.Unit;
            if (hitUnit == null || hitUnit == ownerUnit)
            {
                continue;
            }

            if (!hitbox.TryGetFirstHitOnSegment(segmentStart, segmentEnd, unitHitboxSampleDistance, out Vector2 hitPoint))
            {
                continue;
            }

            float distanceSqr = (hitPoint - segmentStart).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestHitbox = hitbox;
                closestUnit = hitUnit;
                closestHitPoint = hitPoint;
            }
        }

        if (closestHitbox == null || closestUnit == null)
        {
            return false;
        }

        Vector3 impactPosition = new Vector3(closestHitPoint.x, closestHitPoint.y, sideViewPlaneZ);
        Debug.Log($"Projectile hit {closestUnit.name} at {impactPosition}");
        closestUnit.ApplyDamage(weaponData != null ? weaponData.Damage : 0);
        SpawnImpactVisual(impactPosition);
        Resolve();
        return true;
    }

    private Vector2 ClampSpeed(Vector2 candidateVelocity)
    {
        if (maxProjectileSpeed <= 0f)
        {
            return candidateVelocity;
        }

        float maxSpeedSqr = maxProjectileSpeed * maxProjectileSpeed;
        if (candidateVelocity.sqrMagnitude <= maxSpeedSqr)
        {
            return candidateVelocity;
        }

        return candidateVelocity.normalized * maxProjectileSpeed;
    }

    private void SpawnImpactVisual(Vector3 impactPosition)
    {
        if (!spawnImpactVisual)
        {
            return;
        }

        GameObject visualObject = new GameObject("Projectile Impact Visual");
        visualObject.transform.position = impactPosition;

        SpriteRenderer visualRenderer = visualObject.AddComponent<SpriteRenderer>();
        visualRenderer.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 10;

        ExplosionVisual explosionVisual = visualObject.AddComponent<ExplosionVisual>();
        float blastRadius = weaponData != null ? weaponData.BlastRadius : impactVisualMinEndSize;
        float endSize = Mathf.Clamp(blastRadius * impactVisualRadiusScale * 2f, impactVisualMinEndSize, impactVisualMaxEndSize);
        Color visualColor = weaponData != null ? weaponData.ProjectileColor : Color.yellow;
        visualColor.a = impactVisualStartAlpha;

        explosionVisual.Configure(impactVisualStartSize, endSize, impactVisualDuration, visualColor);
    }

    private void Resolve()
    {
        if (!launched)
        {
            return;
        }

        launched = false;
        Resolved?.Invoke(this);
        Destroy(gameObject);
    }
}
