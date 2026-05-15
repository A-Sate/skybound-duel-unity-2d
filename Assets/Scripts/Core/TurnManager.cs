using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Turn State")]
    [SerializeField] private List<UnitController> units = new List<UnitController>();
    [SerializeField] private int activeUnitIndex;

    public IReadOnlyList<UnitController> Units => units;
    public int ActiveUnitIndex => activeUnitIndex;
    public UnitController ActiveUnit => units.Count == 0 ? null : units[Mathf.Clamp(activeUnitIndex, 0, units.Count - 1)];

    public void Initialize()
    {
        if (units.Count == 0)
        {
            UnitController[] sceneUnits = FindObjectsByType<UnitController>(FindObjectsInactive.Exclude);
            units.AddRange(sceneUnits);
            units.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        }

        activeUnitIndex = units.Count == 0 ? -1 : Mathf.Clamp(activeUnitIndex, 0, units.Count - 1);
        RefreshActiveUnitFlags();

        if (ActiveUnit != null)
        {
            Debug.Log($"TurnManager active unit: {ActiveUnit.name}");
        }
    }

    public void RegisterUnit(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
        {
            return;
        }

        units.Add(unit);
        if (activeUnitIndex < 0)
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

        activeUnitIndex = (activeUnitIndex + 1) % units.Count;
        RefreshActiveUnitFlags();
    }

    private void RefreshActiveUnitFlags()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].SetActiveTurn(i == activeUnitIndex);
            }
        }
    }
}
