using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Turn State")]
    [SerializeField] private List<UnitController> units = new List<UnitController>();
    [SerializeField] private int activeUnitIndex;

    private readonly List<UnitController> subscribedUnits = new List<UnitController>();
    private int passInputBlockedFrame = -1;
    private bool hasLoggedMatchResult;

    public IReadOnlyList<UnitController> Units => units;
    public int ActiveUnitIndex => activeUnitIndex;
    public UnitController ActiveUnit => activeUnitIndex < 0 || activeUnitIndex >= units.Count ? null : units[activeUnitIndex];

    public void Initialize()
    {
        hasLoggedMatchResult = false;

        if (units.Count == 0)
        {
            UnitController[] sceneUnits = FindObjectsByType<UnitController>(FindObjectsInactive.Exclude);
            units.AddRange(sceneUnits);
            units.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        }

        RefreshUnitEventSubscriptions();

        activeUnitIndex = FindNextAliveUnitIndex(Mathf.Clamp(activeUnitIndex, 0, Mathf.Max(0, units.Count - 1)));
        RefreshActiveUnitFlags();

        if (ActiveUnit != null)
        {
            Debug.Log($"TurnManager active unit: {ActiveUnit.name}");
        }

        LogWinningTeamIfResolved();
    }

    public void RegisterUnit(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
        {
            return;
        }

        units.Add(unit);
        SubscribeToUnitEvents(unit);
        if (activeUnitIndex < 0 && !unit.IsKnockedOut)
        {
            activeUnitIndex = 0;
        }

        RefreshActiveUnitFlags();
    }

    public void AdvanceTurn()
    {
        if (units.Count == 0)
        {
            activeUnitIndex = -1;
            return;
        }

        activeUnitIndex = FindNextAliveUnitIndex(activeUnitIndex + 1);
        passInputBlockedFrame = Time.frameCount;
        RefreshActiveUnitFlags();

        if (ActiveUnit != null)
        {
            Debug.Log($"TurnManager active unit: {ActiveUnit.name}");
        }

        LogWinningTeamIfResolved();
    }

    public void RequestPass(UnitController unit)
    {
        if (unit == null || unit != ActiveUnit || unit.IsKnockedOut || unit.IsCharging || unit.ActiveProjectile != null || Time.frameCount == passInputBlockedFrame)
        {
            return;
        }

        Debug.Log($"{unit.name} passed turn");
        AdvanceTurn();
    }

    private void OnDestroy()
    {
        ClearUnitEventSubscriptions();
    }

    private void HandleUnitProjectileResolved(UnitController unit)
    {
        if (unit == null || unit != ActiveUnit)
        {
            return;
        }

        AdvanceTurn();
    }

    private void HandleUnitPassRequested(UnitController unit)
    {
        RequestPass(unit);
    }

    private void RefreshUnitEventSubscriptions()
    {
        ClearUnitEventSubscriptions();

        for (int i = 0; i < units.Count; i++)
        {
            SubscribeToUnitEvents(units[i]);
        }
    }

    private void SubscribeToUnitEvents(UnitController unit)
    {
        if (unit == null || subscribedUnits.Contains(unit))
        {
            return;
        }

        unit.ProjectileResolved += HandleUnitProjectileResolved;
        unit.PassRequested += HandleUnitPassRequested;
        subscribedUnits.Add(unit);
    }

    private void ClearUnitEventSubscriptions()
    {
        for (int i = 0; i < subscribedUnits.Count; i++)
        {
            if (subscribedUnits[i] != null)
            {
                subscribedUnits[i].ProjectileResolved -= HandleUnitProjectileResolved;
                subscribedUnits[i].PassRequested -= HandleUnitPassRequested;
            }
        }

        subscribedUnits.Clear();
    }

    private void RefreshActiveUnitFlags()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].SetActiveTurn(i == activeUnitIndex && !units[i].IsKnockedOut);
            }
        }
    }

    private int FindNextAliveUnitIndex(int startIndex)
    {
        if (units.Count == 0)
        {
            return -1;
        }

        int normalizedStartIndex = ((startIndex % units.Count) + units.Count) % units.Count;
        for (int offset = 0; offset < units.Count; offset++)
        {
            int candidateIndex = (normalizedStartIndex + offset) % units.Count;
            UnitController candidate = units[candidateIndex];
            if (candidate != null && !candidate.IsKnockedOut)
            {
                return candidateIndex;
            }
        }

        return -1;
    }

    private void LogWinningTeamIfResolved()
    {
        if (hasLoggedMatchResult)
        {
            return;
        }

        UnitController survivingUnit = null;

        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];
            if (unit == null || unit.IsKnockedOut)
            {
                continue;
            }

            if (survivingUnit == null)
            {
                survivingUnit = unit;
                continue;
            }

            if (survivingUnit.Team != unit.Team)
            {
                return;
            }
        }

        if (survivingUnit != null)
        {
            Debug.Log($"{survivingUnit.Team} Team wins");
            hasLoggedMatchResult = true;
        }
    }
}
