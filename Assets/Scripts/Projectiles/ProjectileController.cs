using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ProjectileController : MonoBehaviour
{
    [Header("Foundation References")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WindManager windManager;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Projectile State")]
    [SerializeField] private Vector2 velocity;
    [SerializeField] private bool launched;
    [SerializeField] private float elapsedLifetime;

    [Header("Projectile Tuning")]
    [SerializeField] private Vector2 gravity = new Vector2(0f, -9.81f);
    [SerializeField] private Vector2 worldBoundsMin = new Vector2(-24f, -8f);
    [SerializeField] private Vector2 worldBoundsMax = new Vector2(24f, 30f);
    [SerializeField] private float projectileLifetime = 8f;
    [SerializeField] private float groundY = -1.25f;
    [SerializeField] private float sideViewPlaneZ;

    public WeaponData WeaponData => weaponData;
    public Vector2 Velocity => velocity;
    public bool Launched => launched;
    public Vector2 Gravity => gravity;
    public Vector2 WorldBoundsMin => worldBoundsMin;
    public Vector2 WorldBoundsMax => worldBoundsMax;
    public float ProjectileLifetime => projectileLifetime;
    public float GroundY => groundY;

    public event Action<ProjectileController> Resolved;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        ApplyWeaponPresentation();
    }

    private void FixedUpdate()
    {
        if (!launched)
        {
            return;
        }

        elapsedLifetime += Time.fixedDeltaTime;

        Vector2 acceleration = gravity;
        if (windManager != null)
        {
            acceleration += windManager.GetWindAcceleration();
        }

        velocity += acceleration * Time.fixedDeltaTime;

        Vector3 nextPosition = transform.position + (Vector3)(velocity * Time.fixedDeltaTime);
        nextPosition.z = sideViewPlaneZ;
        transform.position = nextPosition;

        CheckResolution();
    }

    public void Configure(WeaponData data, WindManager wind)
    {
        weaponData = data;
        windManager = wind;
        ApplyWeaponPresentation();
    }

    public void Launch(Vector2 initialVelocity)
    {
        velocity = initialVelocity;
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
        transform.localScale = Vector3.one * weaponData.ProjectileSize;
    }

    private void CheckResolution()
    {
        Vector3 position = transform.position;
        Vector2 position2D = new Vector2(position.x, position.y);

        if (position.y <= groundY && velocity.y <= 0f)
        {
            Debug.Log($"Projectile impact at {position}");
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
