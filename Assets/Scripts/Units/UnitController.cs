using UnityEngine;

public enum UnitTeam
{
    Blue,
    Red
}

public enum UnitFacing
{
    Left = -1,
    Right = 1
}

public class UnitController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private VehicleData vehicleData;
    [SerializeField] private WeaponData weaponData;

    [Header("Turn And Team")]
    [SerializeField] private UnitTeam team;
    [SerializeField] private UnitFacing facing = UnitFacing.Right;
    [SerializeField] private bool isActiveTurn;

    [Header("Combat Foundation")]
    [SerializeField] private int hp = 750;
    [SerializeField] private int shield = 250;
    [SerializeField] private float localAngle = 45f;

    [Header("Movement Foundation")]
    [SerializeField] private float movement = 220f;
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float slopeRange = 45f;

    private UnitView view;

    public VehicleData VehicleData => vehicleData;
    public WeaponData WeaponData => weaponData;
    public UnitTeam Team => team;
    public UnitFacing Facing => facing;
    public bool IsActiveTurn => isActiveTurn;
    public int Hp => hp;
    public int Shield => shield;
    public float LocalAngle => localAngle;
    public float Movement => movement;
    public float MoveSpeed => moveSpeed;
    public float SlopeRange => slopeRange;

    private void Awake()
    {
        view = GetComponent<UnitView>();
        ApplyVehicleData();
    }

    private void Start()
    {
        RefreshView();
    }

    private void OnValidate()
    {
        localAngle = Mathf.Clamp(localAngle, -90f, 90f);
        hp = Mathf.Max(0, hp);
        shield = Mathf.Max(0, shield);
        movement = Mathf.Max(0f, movement);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        slopeRange = Mathf.Clamp(slopeRange, 0f, 90f);
    }

    public void SetActiveTurn(bool active)
    {
        isActiveTurn = active;
        RefreshView();
    }

    public void SetFacing(UnitFacing newFacing)
    {
        facing = newFacing;
        RefreshView();
    }

    public void SetLocalAngle(float newLocalAngle)
    {
        localAngle = Mathf.Clamp(newLocalAngle, -90f, 90f);
    }

    public void ApplyVehicleData()
    {
        if (vehicleData == null)
        {
            return;
        }

        hp = vehicleData.MaxHp;
        shield = vehicleData.MaxShield;
        moveSpeed = vehicleData.MoveSpeed;
        movement = vehicleData.MaxMovement;
        slopeRange = vehicleData.SlopeRange;
    }

    private void RefreshView()
    {
        if (view == null)
        {
            view = GetComponent<UnitView>();
        }

        view?.ApplyPresentation(this);
    }
}
