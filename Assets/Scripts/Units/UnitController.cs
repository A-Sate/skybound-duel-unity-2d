using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float localAngleAdjustSpeed = 60f;

    [Header("Power Foundation")]
    [SerializeField] private float currentPower;
    [SerializeField] private bool isCharging;
    [SerializeField] private float powerChargeSpeed = 75f;
    [SerializeField] private WeaponData lockedWeapon;

    [Header("Projectile Foundation")]
    [SerializeField] private WindManager windManager;
    [SerializeField] private ProjectileController activeProjectile;
    [SerializeField] private Vector2 firingOriginOffset = new Vector2(0.55f, 0.25f);
    [SerializeField] private float powerVelocityMultiplier = 0.035f;
    [SerializeField] private float projectileSideViewPlaneZ;

    [Header("Movement Foundation")]
    [SerializeField] private float movement = 220f;
    [SerializeField] private float maxMovement = 220f;
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float slopeRange = 45f;
    [SerializeField] private float movementUnitToWorldUnit = 0.01f;
    [SerializeField] private float sideViewPlaneZ;

    private UnitView view;

    public VehicleData VehicleData => vehicleData;
    public WeaponData WeaponData => weaponData;
    public WeaponData SelectedWeapon => weaponData;
    public UnitTeam Team => team;
    public UnitFacing Facing => facing;
    public bool IsActiveTurn => isActiveTurn;
    public int Hp => hp;
    public int Shield => shield;
    public float LocalAngle => localAngle;
    public float MinLocalAngle => GetAllowedLocalAngleRange().x;
    public float MaxLocalAngle => GetAllowedLocalAngleRange().y;
    public float CurrentPower => currentPower;
    public bool IsCharging => isCharging;
    public float PowerChargeSpeed => powerChargeSpeed;
    public WeaponData LockedWeapon => lockedWeapon;
    public ProjectileController ActiveProjectile => activeProjectile;
    public float BaseLaunchSpeed => weaponData != null ? weaponData.BaseLaunchSpeed : 0f;
    public float PowerVelocityMultiplier => powerVelocityMultiplier;
    public float Movement => movement;
    public float RemainingMovement => movement;
    public float MaxMovement => maxMovement;
    public float MoveSpeed => moveSpeed;
    public float SlopeRange => slopeRange;

    public event Action<UnitController> ProjectileResolved;
    public event Action<UnitController> PassRequested;

    private void Awake()
    {
        view = GetComponent<UnitView>();
        ResolveWindManager();
        ApplyVehicleData();
    }

    private void Start()
    {
        RefreshView();
    }

    private void Update()
    {
        if (!isActiveTurn)
        {
            return;
        }

        HandlePowerChargeInput(Time.deltaTime);
        if (TryRequestPass())
        {
            return;
        }

        if (isCharging)
        {
            return;
        }

        float horizontalInput = ReadHorizontalInput();
        TryMoveHorizontal(horizontalInput, Time.deltaTime);

        float angleInput = ReadLocalAngleInput();
        TryAdjustLocalAngle(angleInput, Time.deltaTime);
    }

    private void OnValidate()
    {
        localAngle = ClampLocalAngle(localAngle);
        localAngleAdjustSpeed = Mathf.Max(0f, localAngleAdjustSpeed);
        currentPower = Mathf.Clamp(currentPower, 0f, 100f);
        powerChargeSpeed = Mathf.Max(0f, powerChargeSpeed);
        powerVelocityMultiplier = Mathf.Max(0f, powerVelocityMultiplier);
        hp = Mathf.Max(0, hp);
        shield = Mathf.Max(0, shield);
        maxMovement = Mathf.Max(0f, maxMovement);
        movement = Mathf.Clamp(movement, 0f, maxMovement);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        slopeRange = Mathf.Clamp(slopeRange, 0f, 90f);
        movementUnitToWorldUnit = Mathf.Max(0f, movementUnitToWorldUnit);
    }

    public void SetActiveTurn(bool active)
    {
        bool becameActive = active && !isActiveTurn;
        isActiveTurn = active;
        if (!active && isCharging)
        {
            CancelPowerCharge();
        }

        if (becameActive)
        {
            ResetMovement();
        }

        RefreshView();
    }

    public void SetFacing(UnitFacing newFacing)
    {
        facing = newFacing;
        RefreshView();
    }

    public void SetLocalAngle(float newLocalAngle)
    {
        localAngle = ClampLocalAngle(newLocalAngle);
    }

    public bool TryAdjustLocalAngle(float direction, float deltaTime)
    {
        if (!isActiveTurn || isCharging || Mathf.Approximately(direction, 0f) || localAngleAdjustSpeed <= 0f || deltaTime <= 0f)
        {
            return false;
        }

        float previousAngle = localAngle;
        SetLocalAngle(localAngle + Mathf.Sign(direction) * localAngleAdjustSpeed * deltaTime);
        return !Mathf.Approximately(previousAngle, localAngle);
    }

    public bool TryMoveHorizontal(float direction, float deltaTime)
    {
        if (!isActiveTurn || isCharging || Mathf.Approximately(direction, 0f))
        {
            return false;
        }

        float signedDirection = Mathf.Sign(direction);
        SetFacing(signedDirection < 0f ? UnitFacing.Left : UnitFacing.Right);

        if (movement <= 0f || moveSpeed <= 0f || movementUnitToWorldUnit <= 0f || deltaTime <= 0f)
        {
            return false;
        }

        float movementCost = Mathf.Min(moveSpeed * deltaTime, movement);
        Vector3 nextPosition = transform.position;
        nextPosition.x += signedDirection * movementCost * movementUnitToWorldUnit;
        nextPosition.z = sideViewPlaneZ;
        transform.position = nextPosition;

        movement -= movementCost;
        return movementCost > 0f;
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
        maxMovement = vehicleData.MaxMovement;
        movement = maxMovement;
        slopeRange = vehicleData.SlopeRange;
    }

    public void ResetMovement()
    {
        movement = maxMovement;
    }

    private void HandlePowerChargeInput(float deltaTime)
    {
        if (activeProjectile != null)
        {
            return;
        }

        bool isSpaceHeld = IsPowerChargeHeld();
        if (isSpaceHeld)
        {
            if (!isCharging)
            {
                BeginPowerCharge();
            }

            ChargePower(deltaTime);
            return;
        }

        if (isCharging)
        {
            EndPowerCharge();
        }
    }

    private bool TryRequestPass()
    {
        if (!isActiveTurn || isCharging || activeProjectile != null || !WasPassPressedThisFrame())
        {
            return false;
        }

        PassRequested?.Invoke(this);
        return true;
    }

    private void BeginPowerCharge()
    {
        lockedWeapon = weaponData;
        currentPower = 0f;
        isCharging = true;
    }

    private void ChargePower(float deltaTime)
    {
        if (!isCharging || deltaTime <= 0f || powerChargeSpeed <= 0f)
        {
            return;
        }

        currentPower = Mathf.Clamp(currentPower + powerChargeSpeed * deltaTime, 0f, 100f);
    }

    private void EndPowerCharge()
    {
        WeaponData weaponToFire = lockedWeapon;
        float finalPower = currentPower;
        SpawnProjectile(weaponToFire, finalPower);
        CancelPowerCharge();
    }

    private void CancelPowerCharge()
    {
        isCharging = false;
        currentPower = 0f;
        lockedWeapon = null;
    }

    private void SpawnProjectile(WeaponData weaponToFire, float finalPower)
    {
        if (weaponToFire == null)
        {
            Debug.LogWarning($"{name} cannot fire because no weapon is selected.");
            return;
        }

        if (activeProjectile != null)
        {
            return;
        }

        ResolveWindManager();

        GameObject projectileObject = new GameObject($"Projectile - {weaponToFire.DisplayName}");
        projectileObject.transform.position = GetFiringOrigin();

        SpriteRenderer projectileRenderer = projectileObject.AddComponent<SpriteRenderer>();
        projectileRenderer.sortingOrder = 30;

        ProjectileController projectile = projectileObject.AddComponent<ProjectileController>();
        projectile.Configure(weaponToFire, windManager, this);
        projectile.SetSideViewPlane(projectileSideViewPlaneZ);
        projectile.Resolved += HandleProjectileResolved;

        Vector2 launchDirection = GetLocalAimDirection();
        float launchSpeed = Mathf.Max(0f, weaponToFire.BaseLaunchSpeed + finalPower * powerVelocityMultiplier);
        projectile.Launch(launchDirection * launchSpeed);

        activeProjectile = projectile;
    }

    private void HandleProjectileResolved(ProjectileController projectile)
    {
        if (activeProjectile == projectile)
        {
            projectile.Resolved -= HandleProjectileResolved;
            activeProjectile = null;
            ProjectileResolved?.Invoke(this);
        }
    }

    private void RefreshView()
    {
        if (view == null)
        {
            view = GetComponent<UnitView>();
        }

        view?.ApplyPresentation(this);
    }

    private void ResolveWindManager()
    {
        if (windManager == null)
        {
            windManager = FindAnyObjectByType<WindManager>();
        }
    }

    private Vector3 GetFiringOrigin()
    {
        float facingSign = facing == UnitFacing.Right ? 1f : -1f;
        Vector3 origin = transform.position + new Vector3(
            Mathf.Abs(firingOriginOffset.x) * facingSign,
            firingOriginOffset.y,
            0f);
        origin.z = projectileSideViewPlaneZ;
        return origin;
    }

    private Vector2 GetLocalAimDirection()
    {
        float facingSign = facing == UnitFacing.Right ? 1f : -1f;
        float angleRadians = localAngle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(angleRadians) * facingSign,
            Mathf.Sin(angleRadians)).normalized;
    }

    private static float ReadHorizontalInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        bool moveLeft = keyboard.aKey.isPressed;
        bool moveRight = keyboard.dKey.isPressed;

        if (moveLeft == moveRight)
        {
            return 0f;
        }

        return moveLeft ? -1f : 1f;
    }

    private static float ReadLocalAngleInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return 0f;
        }

        bool increaseAngle = keyboard.wKey.isPressed;
        bool decreaseAngle = keyboard.sKey.isPressed;

        if (increaseAngle == decreaseAngle)
        {
            return 0f;
        }

        return increaseAngle ? 1f : -1f;
    }

    private static bool IsPowerChargeHeld()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.isPressed;
    }

    private static bool WasPassPressedThisFrame()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.pKey.wasPressedThisFrame;
    }

    private float ClampLocalAngle(float angle)
    {
        Vector2 allowedRange = GetAllowedLocalAngleRange();
        return Mathf.Clamp(angle, allowedRange.x, allowedRange.y);
    }

    private Vector2 GetAllowedLocalAngleRange()
    {
        if (weaponData == null)
        {
            return new Vector2(-90f, 90f);
        }

        Vector2 allowedRange = weaponData.AllowedLocalAngleRange;
        if (allowedRange.x > allowedRange.y)
        {
            return new Vector2(allowedRange.y, allowedRange.x);
        }

        return allowedRange;
    }
}
