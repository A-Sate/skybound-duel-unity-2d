using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Skybound Duel/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string weaponId = "atk1";
    [SerializeField] private string displayName = "Standard Shot";

    [Header("Foundation Tuning")]
    [SerializeField] private int damage = 160;
    [SerializeField] private float blastRadius = 1.5f;
    [SerializeField] private float terrainPitPower = 1f;
    [SerializeField] private float projectileSize = 0.3f;
    [FormerlySerializedAs("launchSpeed")]
    [SerializeField] private float baseLaunchSpeed = 2.4f;
    [SerializeField] private Vector2 allowedLocalAngleRange = new Vector2(-15f, 80f);
    [SerializeField] private bool isUltimate;

    [Header("Presentation")]
    [SerializeField] private Sprite projectileSprite;
    [SerializeField] private Color projectileColor = new Color(1f, 0.85f, 0.2f, 1f);

    public string WeaponId => weaponId;
    public string DisplayName => displayName;
    public int Damage => damage;
    public float BlastRadius => blastRadius;
    public float TerrainPitPower => terrainPitPower;
    public float ProjectileSize => projectileSize;
    public float BaseLaunchSpeed => baseLaunchSpeed;
    public float LaunchSpeed => baseLaunchSpeed;
    public Vector2 AllowedLocalAngleRange => allowedLocalAngleRange;
    public bool IsUltimate => isUltimate;
    public Sprite ProjectileSprite => projectileSprite;
    public Color ProjectileColor => projectileColor;
}
