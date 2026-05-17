using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

public enum UnitControlType
{
    Human,
    Bot
}

public class UnitController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private VehicleData vehicleData;
    [InspectorName("Selected Weapon")]
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private WeaponData[] availableWeapons = new WeaponData[0];
    [InspectorName("Selected Weapon Index")]
    [SerializeField] private int selectedWeaponIndex;
    [SerializeField] private string unitId;
    [SerializeField] private string playerName;

    [Header("Turn And Team")]
    [SerializeField] private UnitTeam team;
    [SerializeField] private UnitControlType controlType;
    [SerializeField] private UnitFacing facing = UnitFacing.Right;
    [SerializeField] private bool isActiveTurn;

    [Header("Combat Foundation")]
    [FormerlySerializedAs("hp")]
    [SerializeField] private int currentHp = 750;
    [SerializeField] private int maxHp = 750;
    [FormerlySerializedAs("shield")]
    [SerializeField] private int currentShield = 250;
    [SerializeField] private int maxShield = 250;
    [SerializeField] private bool isKnockedOut;
    [SerializeField] private bool isPermanentlyDefeated;
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
    public WeaponData[] AvailableWeapons => availableWeapons;
    public int SelectedWeaponIndex => selectedWeaponIndex;
    public string UnitId => string.IsNullOrWhiteSpace(unitId) ? GetDefaultUnitId() : unitId;
    public string PlayerName => string.IsNullOrWhiteSpace(playerName) ? GetDefaultPlayerName() : playerName;
    public string DisplayName => PlayerName;
    public UnitTeam Team => team;
    public UnitControlType ControlType => controlType;
    public bool IsBot => controlType == UnitControlType.Bot;
    public UnitFacing Facing => facing;
    public bool IsActiveTurn => isActiveTurn;
    public int Hp => currentHp;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int Shield => currentShield;
    public int CurrentShield => currentShield;
    public int MaxShield => maxShield;
    public bool IsKnockedOut => isKnockedOut;
    public bool IsPermanentlyDefeated => isPermanentlyDefeated;
    public bool IsDead => isKnockedOut || isPermanentlyDefeated;
    public bool IsPlayable => !isKnockedOut && !isPermanentlyDefeated;
    public bool CanAct => IsPlayable && isActiveTurn;
    public bool CanCharge => CanAct && !isCharging && activeProjectile == null && weaponData != null;
    public float LocalAngle => localAngle;
    public float MinLocalAngle => GetAllowedLocalAngleRange().x;
    public float MaxLocalAngle => GetAllowedLocalAngleRange().y;
    public float CurrentPower => currentPower;
    public bool IsCharging => isCharging;
    public float PowerChargeSpeed => powerChargeSpeed;
    public WeaponData LockedWeapon => lockedWeapon;
    public ProjectileController ActiveProjectile => activeProjectile;
    public float BaseLaunchSpeed => SelectedWeapon != null ? SelectedWeapon.BaseLaunchSpeed : 0f;
    public float PowerVelocityMultiplier => powerVelocityMultiplier;
    public float Movement => movement;
    public float RemainingMovement => movement;
    public float MaxMovement => maxMovement;
    public float MoveSpeed => moveSpeed;
    public float SlopeRange => slopeRange;

    public event Action<UnitController> ProjectileResolved;
    public event Action<UnitController> PassRequested;
    public event Action<UnitController> KnockedOut;

    private void Awake()
    {
        EnsureDefaultUnitId();
        EnsureDefaultPlayerName();
        ValidateWeaponSelection();
        view = GetComponent<UnitView>();
        ResolveWindManager();
        ApplyVehicleData();
        EnsureBotController();
    }

    private void Start()
    {
        RefreshView();
    }

    private void Update()
    {
        if (isKnockedOut || !isActiveTurn || controlType == UnitControlType.Bot)
        {
            return;
        }

        HandlePowerChargeInput(Time.deltaTime);
        if (TryRequestPass())
        {
            return;
        }

        TrySelectWeaponFromInput();

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
        EnsureDefaultUnitId();
        EnsureDefaultPlayerName();
        ValidateWeaponSelection();
        localAngle = ClampLocalAngle(localAngle);
        localAngleAdjustSpeed = Mathf.Max(0f, localAngleAdjustSpeed);
        currentPower = Mathf.Clamp(currentPower, 0f, 100f);
        powerChargeSpeed = Mathf.Max(0f, powerChargeSpeed);
        powerVelocityMultiplier = Mathf.Max(0f, powerVelocityMultiplier);
        maxHp = Mathf.Max(0, maxHp);
        maxShield = Mathf.Max(0, maxShield);
        currentHp = Mathf.Clamp(currentHp, 0, maxHp);
        currentShield = Mathf.Clamp(currentShield, 0, maxShield);
        maxMovement = Mathf.Max(0f, maxMovement);
        movement = Mathf.Clamp(movement, 0f, maxMovement);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        slopeRange = Mathf.Clamp(slopeRange, 0f, 90f);
        movementUnitToWorldUnit = Mathf.Max(0f, movementUnitToWorldUnit);
    }

    public void SetActiveTurn(bool active)
    {
        active = active && IsPlayable;
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
        if (isKnockedOut || !isActiveTurn || isCharging || Mathf.Approximately(direction, 0f) || localAngleAdjustSpeed <= 0f || deltaTime <= 0f)
        {
            return false;
        }

        float previousAngle = localAngle;
        SetLocalAngle(localAngle + Mathf.Sign(direction) * localAngleAdjustSpeed * deltaTime);
        return !Mathf.Approximately(previousAngle, localAngle);
    }

    public bool TryMoveHorizontal(float direction, float deltaTime)
    {
        if (isKnockedOut || !isActiveTurn || isCharging || Mathf.Approximately(direction, 0f))
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

        InitializeStatsFromVehicleData();
        moveSpeed = vehicleData.MoveSpeed;
        maxMovement = vehicleData.MaxMovement;
        movement = maxMovement;
        slopeRange = vehicleData.SlopeRange;
    }

    public void InitializeStatsFromVehicleData()
    {
        if (vehicleData == null)
        {
            return;
        }

        maxHp = Mathf.Max(0, vehicleData.MaxHp);
        maxShield = Mathf.Max(0, vehicleData.MaxShield);
        currentHp = maxHp;
        currentShield = maxShield;
        isKnockedOut = false;
        isPermanentlyDefeated = false;
    }

    public void ApplyDamage(float amount)
    {
        ApplyDamage(amount, "damage");
    }

    public void ApplyDamage(float amount, string damageLabel)
    {
        if (isKnockedOut)
        {
            return;
        }

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(amount));
        if (finalDamage <= 0)
        {
            Debug.Log($"{name} took {FormatDamageDescription(0, damageLabel)}. Shield: {currentShield}/{maxShield}, HP: {currentHp}/{maxHp}");
            return;
        }

        int absorbedByShield = Mathf.Min(currentShield, finalDamage);
        currentShield -= absorbedByShield;

        int remainingDamage = finalDamage - absorbedByShield;
        currentHp = Mathf.Max(0, currentHp - remainingDamage);

        Debug.Log($"{name} took {FormatDamageDescription(finalDamage, damageLabel)}. Shield: {currentShield}/{maxShield}, HP: {currentHp}/{maxHp}");
        if (currentHp <= 0)
        {
            KnockOut();
        }
    }

    private static string FormatDamageDescription(int amount, string damageLabel)
    {
        string normalizedLabel = string.IsNullOrWhiteSpace(damageLabel) ? "damage" : damageLabel.Trim();
        return normalizedLabel == "damage" ? $"{amount} damage" : $"{amount} {normalizedLabel}";
    }

    public void KnockOut()
    {
        SetKnockedOut(true);
    }

    public void SetKnockedOut(bool knockedOut)
    {
        if (isKnockedOut == knockedOut)
        {
            return;
        }

        isKnockedOut = knockedOut;
        if (isKnockedOut)
        {
            currentHp = 0;
            currentShield = 0;
            isActiveTurn = false;
            CancelPowerCharge();
            Debug.Log($"{name} knocked out");
            RefreshView();
            KnockedOut?.Invoke(this);
            return;
        }

        isPermanentlyDefeated = false;
        RefreshView();
    }

    public void SetPermanentlyDefeated()
    {
        isPermanentlyDefeated = true;
        isKnockedOut = true;
        currentHp = 0;
        currentShield = 0;
        isActiveTurn = false;
        CancelPowerCharge();
        RefreshView();
    }

    public void ResetStatsForRespawn()
    {
        if (vehicleData != null)
        {
            maxHp = Mathf.Max(0, vehicleData.MaxHp);
            maxShield = Mathf.Max(0, vehicleData.MaxShield);
        }

        currentHp = maxHp;
        currentShield = maxShield;
        isKnockedOut = false;
        isPermanentlyDefeated = false;
        isActiveTurn = false;
        CancelPowerCharge();
        ResetMovement();
        RefreshView();
    }

    public void ResetMovement()
    {
        movement = maxMovement;
    }

    public bool TryBeginPowerCharge()
    {
        if (!CanCharge)
        {
            return false;
        }

        BeginPowerCharge();
        return true;
    }

    public bool TryContinuePowerCharge(float deltaTime)
    {
        if (!CanAct || !isCharging || activeProjectile != null)
        {
            return false;
        }

        ChargePower(deltaTime);
        return true;
    }

    public bool TryReleasePowerCharge()
    {
        if (!CanAct || !isCharging)
        {
            return false;
        }

        EndPowerCharge();
        return true;
    }

    private void HandlePowerChargeInput(float deltaTime)
    {
        if (isKnockedOut || activeProjectile != null)
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
        if (isKnockedOut || !isActiveTurn || isCharging || activeProjectile != null || !WasPassPressedThisFrame())
        {
            return false;
        }

        PassRequested?.Invoke(this);
        return true;
    }

    private bool TrySelectWeaponFromInput()
    {
        if (!CanSelectWeapon())
        {
            return false;
        }

        int requestedWeaponIndex = ReadWeaponSlotPressedThisFrame();
        if (requestedWeaponIndex < 0)
        {
            return false;
        }

        return TrySelectWeaponSlot(requestedWeaponIndex);
    }

    public bool TrySelectWeaponSlot(int weaponIndex)
    {
        if (!CanSelectWeapon())
        {
            return false;
        }

        return SetSelectedWeaponIndex(weaponIndex, true);
    }

    private bool CanSelectWeapon()
    {
        return !isKnockedOut && isActiveTurn && !isCharging && activeProjectile == null;
    }

    private bool SetSelectedWeaponIndex(int weaponIndex, bool logSelection)
    {
        if (availableWeapons == null || availableWeapons.Length == 0 || weaponIndex < 0 || weaponIndex >= availableWeapons.Length)
        {
            return false;
        }

        WeaponData selectedWeapon = availableWeapons[weaponIndex];
        if (selectedWeapon == null)
        {
            return false;
        }

        bool changed = selectedWeaponIndex != weaponIndex || weaponData != selectedWeapon;
        selectedWeaponIndex = weaponIndex;
        weaponData = selectedWeapon;
        SetLocalAngle(localAngle);

        if (changed && logSelection)
        {
            Debug.Log($"{PlayerName} selected {weaponData.DisplayName}");
        }

        return changed;
    }

    private void ValidateWeaponSelection()
    {
        if (availableWeapons == null)
        {
            availableWeapons = new WeaponData[0];
        }

        if (availableWeapons.Length == 0)
        {
            selectedWeaponIndex = 0;
            return;
        }

        selectedWeaponIndex = Mathf.Clamp(selectedWeaponIndex, 0, availableWeapons.Length - 1);
        WeaponData selectedWeapon = availableWeapons[selectedWeaponIndex];
        if (selectedWeapon == null)
        {
            int firstAvailableWeaponIndex = FindFirstAvailableWeaponIndex();
            if (firstAvailableWeaponIndex < 0)
            {
                return;
            }

            selectedWeaponIndex = firstAvailableWeaponIndex;
            selectedWeapon = availableWeapons[selectedWeaponIndex];
        }

        weaponData = selectedWeapon;
    }

    private int FindFirstAvailableWeaponIndex()
    {
        if (availableWeapons == null)
        {
            return -1;
        }

        for (int i = 0; i < availableWeapons.Length; i++)
        {
            if (availableWeapons[i] != null)
            {
                return i;
            }
        }

        return -1;
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

    public void CancelPowerCharge()
    {
        isCharging = false;
        currentPower = 0f;
        lockedWeapon = null;
    }

    private void SpawnProjectile(WeaponData weaponToFire, float finalPower)
    {
        if (isKnockedOut)
        {
            return;
        }

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

    private static int ReadWeaponSlotPressedThisFrame()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return -1;
        }

        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
        {
            return 0;
        }

        if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
        {
            return 1;
        }

        if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
        {
            return 2;
        }

        if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame)
        {
            return 3;
        }

        return -1;
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

    private void EnsureDefaultPlayerName()
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = GetDefaultPlayerName();
        }
    }

    private string GetDefaultPlayerName()
    {
        return team == UnitTeam.Blue ? "Player 1" : "Player 2";
    }

    private void EnsureDefaultUnitId()
    {
        if (string.IsNullOrWhiteSpace(unitId))
        {
            unitId = GetDefaultUnitId();
        }
    }

    private string GetDefaultUnitId()
    {
        string teamPrefix = team.ToString().ToLowerInvariant();
        string namePart = string.IsNullOrWhiteSpace(playerName) ? GetDefaultPlayerName() : playerName.Trim();
        return $"{teamPrefix}-{SanitizeUnitIdPart(namePart)}";
    }

    private static string SanitizeUnitIdPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unit";
        }

        char[] chars = value.Trim().ToLowerInvariant().ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            bool isLetterOrDigit = c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
            chars[i] = isLetterOrDigit ? c : '-';
        }

        string sanitized = new string(chars).Trim('-');
        return string.IsNullOrEmpty(sanitized) ? "unit" : sanitized;
    }

    private void EnsureBotController()
    {
        if (controlType == UnitControlType.Bot && GetComponent<BotController>() == null)
        {
            gameObject.AddComponent<BotController>();
        }
    }
}
