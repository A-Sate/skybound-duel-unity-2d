using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(UnitController))]
public class BotController : MonoBehaviour
{
    private enum BotTurnState
    {
        Idle,
        Waiting,
        Aiming,
        Charging,
        Fired
    }

    private struct AimSolution
    {
        public bool IsValid;
        public float LocalAngle;
        public float Power;
        public float TargetDistance;
        public Vector2 ClosestPoint;
    }

    [Header("References")]
    [SerializeField] private UnitController unit;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private WindManager windManager;

    [Header("Basic Bot Timing")]
    [SerializeField] private float minActionDelay = 0.65f;
    [SerializeField] private float maxActionDelay = 1.15f;

    [Header("Smooth Aim")]
    [SerializeField] private float aimAngleTolerance = 0.75f;

    [Header("Fallback Shot")]
    [FormerlySerializedAs("targetLocalAngle")]
    [SerializeField] private float fallbackLocalAngle = 45f;
    [FormerlySerializedAs("minDesiredPower")]
    [SerializeField] private float fallbackMinPower = 60f;
    [FormerlySerializedAs("maxDesiredPower")]
    [SerializeField] private float fallbackMaxPower = 80f;

    [Header("Trajectory Sampling")]
    [SerializeField] private bool useSelectedWeaponAngleRange = true;
    [SerializeField] private float minSampledAngle = -15f;
    [SerializeField] private float maxSampledAngle = 90f;
    [SerializeField] private float angleSampleStep = 5f;
    [SerializeField] private float minSampledPower = 20f;
    [SerializeField] private float maxSampledPower = 100f;
    [SerializeField] private float powerSampleStep = 5f;
    [SerializeField] private float simulationStepTime = 0.05f;
    [SerializeField] private float maxSimulationTime = 8f;
    [SerializeField] private Vector2 simulationGravity = new Vector2(0f, ProjectileController.DefaultGravityY);
    [SerializeField] private float simulationMaxProjectileSpeed = ProjectileController.DefaultMaxProjectileSpeed;
    [SerializeField] private float simulationProjectileWindAccelerationScale = ProjectileController.DefaultProjectileWindAccelerationScale;
    [SerializeField] private float simulationGroundY = ProjectileController.DefaultGroundY;

    [Header("Future Difficulty Tuning")]
    [Tooltip("Random local-angle error added after trajectory sampling. This can later map to bot difficulty.")]
    [SerializeField] private float randomAngleError = 2f;
    [Tooltip("Random power error added after trajectory sampling. This can later map to bot difficulty.")]
    [SerializeField] private float randomPowerError = 4f;
    [SerializeField] private bool logBotActions = true;

    [Header("Bot State")]
    [SerializeField] private BotTurnState turnState;
    [SerializeField] private UnitController targetUnit;
    [SerializeField] private float actionDelayRemaining;
    [SerializeField] private float desiredLocalAngle;
    [SerializeField] private float desiredPower;
    [SerializeField] private float predictedTargetDistance;

    public UnitController TargetUnit => targetUnit;
    public float DesiredLocalAngle => desiredLocalAngle;
    public float DesiredPower => desiredPower;
    public float PredictedTargetDistance => predictedTargetDistance;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        minActionDelay = Mathf.Max(0f, minActionDelay);
        maxActionDelay = Mathf.Max(minActionDelay, maxActionDelay);
        aimAngleTolerance = Mathf.Max(0.01f, aimAngleTolerance);
        fallbackMinPower = Mathf.Clamp(fallbackMinPower, 0f, 100f);
        fallbackMaxPower = Mathf.Clamp(fallbackMaxPower, fallbackMinPower, 100f);
        maxSampledAngle = Mathf.Max(minSampledAngle, maxSampledAngle);
        angleSampleStep = Mathf.Max(0.1f, angleSampleStep);
        minSampledPower = Mathf.Clamp(minSampledPower, 0f, 100f);
        maxSampledPower = Mathf.Clamp(maxSampledPower, minSampledPower, 100f);
        powerSampleStep = Mathf.Max(0.1f, powerSampleStep);
        simulationStepTime = Mathf.Max(0.01f, simulationStepTime);
        maxSimulationTime = Mathf.Max(simulationStepTime, maxSimulationTime);
        simulationMaxProjectileSpeed = Mathf.Max(0f, simulationMaxProjectileSpeed);
        simulationProjectileWindAccelerationScale = Mathf.Max(0f, simulationProjectileWindAccelerationScale);
        randomAngleError = Mathf.Max(0f, randomAngleError);
        randomPowerError = Mathf.Max(0f, randomPowerError);
    }

    private void Update()
    {
        ResolveReferences();

        if (!CanControlTurn())
        {
            ResetTurnState();
            return;
        }

        if (IsAnyProjectileActive())
        {
            if (unit.ActiveProjectile != null)
            {
                turnState = BotTurnState.Fired;
            }

            return;
        }

        switch (turnState)
        {
            case BotTurnState.Idle:
                BeginTurnPlan();
                break;

            case BotTurnState.Waiting:
                UpdateWaiting(Time.deltaTime);
                break;

            case BotTurnState.Aiming:
                UpdateAiming(Time.deltaTime);
                break;

            case BotTurnState.Charging:
                UpdateCharging(Time.deltaTime);
                break;
        }
    }

    private void ResolveReferences()
    {
        if (unit == null)
        {
            unit = GetComponent<UnitController>();
        }

        if (turnManager == null)
        {
            turnManager = FindAnyObjectByType<TurnManager>();
        }

        if (windManager == null)
        {
            windManager = FindAnyObjectByType<WindManager>();
        }
    }

    private bool CanControlTurn()
    {
        return unit != null &&
            unit.IsBot &&
            unit.CanAct &&
            turnManager != null &&
            turnManager.ActiveUnit == unit;
    }

    private void BeginTurnPlan()
    {
        targetUnit = FindNearestPlayableEnemy();
        if (targetUnit == null)
        {
            return;
        }

        UnitFacing plannedFacing = GetFacingToTarget(targetUnit);
        unit.SetFacing(plannedFacing);

        bool usedFallback = false;
        AimSolution solution = FindBestTrajectorySolution(targetUnit, plannedFacing);
        if (!solution.IsValid)
        {
            solution = CreateFallbackSolution();
            usedFallback = true;
        }

        desiredLocalAngle = ClampToSelectedWeaponRange(solution.LocalAngle + Random.Range(-randomAngleError, randomAngleError));
        desiredPower = Mathf.Clamp(solution.Power + Random.Range(-randomPowerError, randomPowerError), 0f, 100f);
        predictedTargetDistance = solution.TargetDistance;
        actionDelayRemaining = Random.Range(minActionDelay, maxActionDelay);
        turnState = BotTurnState.Waiting;

        if (logBotActions)
        {
            string source = usedFallback ? "fallback" : $"sampled miss {predictedTargetDistance:0.00}";
            Debug.Log($"{unit.DisplayName} bot targeting {targetUnit.DisplayName}: angle {desiredLocalAngle:0.#}, power {desiredPower:0.#} ({source}).");
        }
    }

    private void UpdateWaiting(float deltaTime)
    {
        if (!IsTargetPlayable(targetUnit))
        {
            ResetTurnState();
            return;
        }

        actionDelayRemaining -= Mathf.Max(0f, deltaTime);
        if (actionDelayRemaining > 0f)
        {
            return;
        }

        unit.SetFacing(GetFacingToTarget(targetUnit));
        turnState = BotTurnState.Aiming;
    }

    private void UpdateAiming(float deltaTime)
    {
        if (!IsTargetPlayable(targetUnit))
        {
            ResetTurnState();
            return;
        }

        unit.SetFacing(GetFacingToTarget(targetUnit));
        if (!unit.TryMoveLocalAngleToward(desiredLocalAngle, deltaTime, aimAngleTolerance))
        {
            return;
        }

        if (unit.TryBeginPowerCharge())
        {
            turnState = BotTurnState.Charging;
        }
    }

    private void UpdateCharging(float deltaTime)
    {
        if (!IsTargetPlayable(targetUnit))
        {
            ResetTurnState();
            return;
        }

        if (!unit.TryContinuePowerCharge(deltaTime))
        {
            ResetTurnState();
            return;
        }

        if (unit.CurrentPower >= desiredPower)
        {
            unit.TryReleasePowerCharge();
            turnState = BotTurnState.Fired;
        }
    }

    private AimSolution FindBestTrajectorySolution(UnitController target, UnitFacing plannedFacing)
    {
        WeaponData weapon = unit != null ? unit.SelectedWeapon : null;
        if (weapon == null || target == null)
        {
            return default;
        }

        Vector2 angleRange = GetSampleAngleRange(weapon);
        if (angleRange.x > angleRange.y)
        {
            return default;
        }

        Vector2 startPosition = unit.GetFiringOriginWorld();
        UnitHitbox targetHitbox = target.GetComponent<UnitHitbox>();
        Vector2 acceleration = simulationGravity;
        if (windManager != null)
        {
            acceleration += windManager.GetWindAcceleration(simulationProjectileWindAccelerationScale);
        }

        AimSolution bestSolution = default;
        bestSolution.TargetDistance = float.MaxValue;

        for (float angle = angleRange.x; angle <= angleRange.y + 0.001f; angle += angleSampleStep)
        {
            float clampedAngle = Mathf.Min(angle, angleRange.y);
            for (float power = minSampledPower; power <= maxSampledPower + 0.001f; power += powerSampleStep)
            {
                float clampedPower = Mathf.Min(power, maxSampledPower);
                AimSolution candidate = SimulateTrajectory(target, targetHitbox, weapon, plannedFacing, clampedAngle, clampedPower, startPosition, acceleration);
                if (!candidate.IsValid || candidate.TargetDistance >= bestSolution.TargetDistance)
                {
                    continue;
                }

                bestSolution = candidate;
            }
        }

        return bestSolution.IsValid ? bestSolution : default;
    }

    private AimSolution SimulateTrajectory(
        UnitController target,
        UnitHitbox targetHitbox,
        WeaponData weapon,
        UnitFacing plannedFacing,
        float localAngle,
        float power,
        Vector2 startPosition,
        Vector2 acceleration)
    {
        Vector2 position = startPosition;
        Vector2 velocity = unit.GetLaunchVelocity(weapon, plannedFacing, localAngle, power);
        if (velocity.sqrMagnitude <= Mathf.Epsilon)
        {
            return default;
        }

        AimSolution solution = new AimSolution
        {
            IsValid = true,
            LocalAngle = localAngle,
            Power = power,
            TargetDistance = GetDistanceToTarget(position, target, targetHitbox),
            ClosestPoint = position
        };

        int stepCount = Mathf.CeilToInt(maxSimulationTime / simulationStepTime);
        for (int i = 0; i < stepCount; i++)
        {
            velocity += acceleration * simulationStepTime;
            velocity = ClampSimulationSpeed(velocity);
            position += velocity * simulationStepTime;

            if (position.y <= simulationGroundY && velocity.y <= 0f)
            {
                position.y = simulationGroundY;
                ScoreTrajectoryPoint(ref solution, position, target, targetHitbox);
                break;
            }

            ScoreTrajectoryPoint(ref solution, position, target, targetHitbox);
        }

        return solution;
    }

    private void ScoreTrajectoryPoint(ref AimSolution solution, Vector2 position, UnitController target, UnitHitbox targetHitbox)
    {
        float distance = GetDistanceToTarget(position, target, targetHitbox);
        if (distance >= solution.TargetDistance)
        {
            return;
        }

        solution.TargetDistance = distance;
        solution.ClosestPoint = position;
    }

    private float GetDistanceToTarget(Vector2 position, UnitController target, UnitHitbox targetHitbox)
    {
        if (targetHitbox != null && targetHitbox.isActiveAndEnabled)
        {
            return targetHitbox.GetDistanceToWorldPoint(position);
        }

        return target != null ? Vector2.Distance(position, target.transform.position) : float.MaxValue;
    }

    private Vector2 GetSampleAngleRange(WeaponData weapon)
    {
        Vector2 weaponRange = weapon.AllowedLocalAngleRange;
        if (useSelectedWeaponAngleRange)
        {
            return weaponRange;
        }

        return new Vector2(
            Mathf.Max(minSampledAngle, weaponRange.x),
            Mathf.Min(maxSampledAngle, weaponRange.y));
    }

    private AimSolution CreateFallbackSolution()
    {
        return new AimSolution
        {
            IsValid = true,
            LocalAngle = ClampToSelectedWeaponRange(fallbackLocalAngle),
            Power = Random.Range(fallbackMinPower, fallbackMaxPower),
            TargetDistance = float.PositiveInfinity
        };
    }

    private UnitFacing GetFacingToTarget(UnitController target)
    {
        return target != null && target.transform.position.x >= transform.position.x ? UnitFacing.Right : UnitFacing.Left;
    }

    private float ClampToSelectedWeaponRange(float localAngle)
    {
        WeaponData selectedWeapon = unit != null ? unit.SelectedWeapon : null;
        if (selectedWeapon == null)
        {
            return localAngle;
        }

        Vector2 weaponRange = selectedWeapon.AllowedLocalAngleRange;
        return Mathf.Clamp(localAngle, weaponRange.x, weaponRange.y);
    }

    private Vector2 ClampSimulationSpeed(Vector2 candidateVelocity)
    {
        if (simulationMaxProjectileSpeed <= 0f)
        {
            return candidateVelocity;
        }

        float maxSpeedSqr = simulationMaxProjectileSpeed * simulationMaxProjectileSpeed;
        if (candidateVelocity.sqrMagnitude <= maxSpeedSqr)
        {
            return candidateVelocity;
        }

        return candidateVelocity.normalized * simulationMaxProjectileSpeed;
    }

    private UnitController FindNearestPlayableEnemy()
    {
        if (turnManager == null)
        {
            return null;
        }

        UnitController nearestEnemy = null;
        float nearestDistanceSqr = float.MaxValue;
        IReadOnlyList<UnitController> units = turnManager.Units;

        for (int i = 0; i < units.Count; i++)
        {
            UnitController candidate = units[i];
            if (!IsTargetPlayable(candidate))
            {
                continue;
            }

            float distanceSqr = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestEnemy = candidate;
            }
        }

        return nearestEnemy;
    }

    private bool IsTargetPlayable(UnitController candidate)
    {
        return candidate != null &&
            candidate != unit &&
            candidate.Team != unit.Team &&
            candidate.IsPlayable;
    }

    private bool IsAnyProjectileActive()
    {
        if (turnManager == null)
        {
            return unit != null && unit.ActiveProjectile != null;
        }

        IReadOnlyList<UnitController> units = turnManager.Units;
        for (int i = 0; i < units.Count; i++)
        {
            UnitController candidate = units[i];
            if (candidate != null && candidate.ActiveProjectile != null)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetTurnState()
    {
        if (unit != null && unit.IsCharging)
        {
            unit.CancelPowerCharge();
        }

        targetUnit = null;
        actionDelayRemaining = 0f;
        desiredLocalAngle = 0f;
        desiredPower = 0f;
        predictedTargetDistance = 0f;
        turnState = BotTurnState.Idle;
    }
}
