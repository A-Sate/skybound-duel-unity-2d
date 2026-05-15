using UnityEngine;

[CreateAssetMenu(fileName = "VehicleData", menuName = "Skybound Duel/Vehicle Data")]
public class VehicleData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string vehicleId = "prototype";
    [SerializeField] private string displayName = "Prototype";

    [Header("Stats")]
    [SerializeField] private int maxHp = 750;
    [SerializeField] private int maxShield = 250;
    [SerializeField, Range(0f, 1f)] private float shieldRegenPercent = 0.1f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float maxMovement = 220f;
    [SerializeField] private float slopeRange = 45f;

    [Header("Presentation")]
    [SerializeField] private Sprite vehicleSprite;

    public string VehicleId => vehicleId;
    public string DisplayName => displayName;
    public int MaxHp => maxHp;
    public int MaxShield => maxShield;
    public float ShieldRegenPercent => shieldRegenPercent;
    public float MoveSpeed => moveSpeed;
    public float MaxMovement => maxMovement;
    public float SlopeRange => slopeRange;
    public Sprite VehicleSprite => vehicleSprite;
}
