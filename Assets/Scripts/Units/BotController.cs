using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class BotController : MonoBehaviour
{
    private enum BotTurnState
    {
        Idle,
        Waiting,
        Charging,
        Fired
    }

    [Header("References")]
    [SerializeField] private UnitController unit;
    [SerializeField] private TurnManager turnManager;

    [Header("Basic Bot Timing")]
    [SerializeField] private float minActionDelay = 0.65f;
    [SerializeField] private float maxActionDelay = 1.15f;

    [Header("Basic Bot Shot")]
    [SerializeField] private float targetLocalAngle = 45f;
    [SerializeField] private float minDesiredPower = 60f;
    [SerializeField] private float maxDesiredPower = 80f;
    [SerializeField] private bool logBotActions = true;

    [Header("Bot State")]
    [SerializeField] private BotTurnState turnState;
    [SerializeField] private UnitController targetUnit;
    [SerializeField] private float actionDelayRemaining;
    [SerializeField] private float desiredPower;

    public UnitController TargetUnit => targetUnit;
    public float DesiredPower => desiredPower;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnValidate()
    {
        minActionDelay = Mathf.Max(0f, minActionDelay);
        maxActionDelay = Mathf.Max(minActionDelay, maxActionDelay);
        minDesiredPower = Mathf.Clamp(minDesiredPower, 0f, 100f);
        maxDesiredPower = Mathf.Clamp(maxDesiredPower, minDesiredPower, 100f);
    }

    private void Update()
    {
        ResolveReferences();

        if (!CanControlTurn())
        {
            ResetTurnState();
            return;
        }

        if (unit.ActiveProjectile != null)
        {
            turnState = BotTurnState.Fired;
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

        desiredPower = Random.Range(minDesiredPower, maxDesiredPower);
        actionDelayRemaining = Random.Range(minActionDelay, maxActionDelay);
        turnState = BotTurnState.Waiting;

        if (logBotActions)
        {
            Debug.Log($"{unit.DisplayName} bot targeting {targetUnit.DisplayName} with power {desiredPower:0}.");
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

        AimAtTarget(targetUnit);
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

    private void AimAtTarget(UnitController target)
    {
        UnitFacing desiredFacing = target.transform.position.x >= transform.position.x ? UnitFacing.Right : UnitFacing.Left;
        unit.SetFacing(desiredFacing);
        unit.SetLocalAngle(targetLocalAngle);
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

    private void ResetTurnState()
    {
        if (unit != null && unit.IsCharging)
        {
            unit.CancelPowerCharge();
        }

        targetUnit = null;
        actionDelayRemaining = 0f;
        desiredPower = 0f;
        turnState = BotTurnState.Idle;
    }
}
