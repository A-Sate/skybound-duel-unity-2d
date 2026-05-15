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

    public WeaponData WeaponData => weaponData;
    public Vector2 Velocity => velocity;
    public bool Launched => launched;

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

        Vector2 acceleration = Physics2D.gravity;
        if (windManager != null)
        {
            acceleration += windManager.GetWindAcceleration();
        }

        velocity += acceleration * Time.fixedDeltaTime;
        transform.position += (Vector3)(velocity * Time.fixedDeltaTime);
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
        launched = true;
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
}
