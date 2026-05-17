using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("Team Lives")]
    [SerializeField] private int blueTeamLives = 3;
    [SerializeField] private int redTeamLives = 3;

    [Header("Respawn Foundation")]
    [SerializeField] private Vector3 blueRespawnPosition = new Vector3(-5f, -1.25f, 0f);
    [SerializeField] private Vector3 redRespawnPosition = new Vector3(5f, -1.25f, 0f);

    [Header("Turn State")]
    [SerializeField] private List<UnitController> units = new List<UnitController>();
    [SerializeField] private int activeUnitIndex;

    private readonly List<UnitController> subscribedUnits = new List<UnitController>();
    private readonly List<UnitTeam> teamsInMatch = new List<UnitTeam>();
    private int passInputBlockedFrame = -1;
    private bool hasLoggedMatchResult;

    public IReadOnlyList<UnitController> Units => units;
    public IReadOnlyList<UnitTeam> TeamsInMatch => teamsInMatch;
    public int BlueTeamLives => blueTeamLives;
    public int RedTeamLives => redTeamLives;
    public int ActiveUnitIndex => activeUnitIndex;
    public UnitController ActiveUnit => activeUnitIndex < 0 || activeUnitIndex >= units.Count ? null : units[activeUnitIndex];

    private void OnValidate()
    {
        blueTeamLives = Mathf.Max(0, blueTeamLives);
        redTeamLives = Mathf.Max(0, redTeamLives);
    }

    public void Initialize()
    {
        hasLoggedMatchResult = false;
        passInputBlockedFrame = -1;

        RemoveMissingUnits();

        if (units.Count == 0)
        {
            DiscoverSceneUnits();
        }

        RefreshTeamsInMatch();
        RefreshUnitEventSubscriptions();

        int startIndex = units.Count > 0 ? Mathf.Clamp(activeUnitIndex, 0, units.Count - 1) : -1;
        activeUnitIndex = FindNextPlayableUnitIndex(startIndex);
        RefreshActiveUnitFlags();

        LogActiveUnit();
        LogWinningTeamIfResolved();
    }

    public void RegisterUnit(UnitController unit)
    {
        if (unit == null || units.Contains(unit))
        {
            return;
        }

        units.Add(unit);
        RefreshTeamsInMatch();
        SubscribeToUnitEvents(unit);
        if (activeUnitIndex < 0 && IsUnitPlayable(unit))
        {
            activeUnitIndex = units.Count - 1;
        }

        RefreshActiveUnitFlags();
        LogWinningTeamIfResolved();
    }

    public void AdvanceTurn()
    {
        if (units.Count == 0)
        {
            activeUnitIndex = -1;
            return;
        }

        activeUnitIndex = FindNextPlayableUnitIndex(activeUnitIndex + 1);
        passInputBlockedFrame = Time.frameCount;
        RefreshActiveUnitFlags();

        LogActiveUnit();
        LogWinningTeamIfResolved();
    }

    public void RequestPass(UnitController unit)
    {
        if (unit == null || unit != ActiveUnit || !IsUnitPlayable(unit) || unit.IsCharging || unit.ActiveProjectile != null || Time.frameCount == passInputBlockedFrame)
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
        unit.KnockedOut += HandleUnitKnockedOut;
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
                subscribedUnits[i].KnockedOut -= HandleUnitKnockedOut;
            }
        }

        subscribedUnits.Clear();
    }

    private void HandleUnitKnockedOut(UnitController unit)
    {
        if (unit == null)
        {
            return;
        }

        int livesRemaining = ConsumeTeamLife(unit.Team);
        if (livesRemaining > 0)
        {
            RespawnUnit(unit);
            LogWinningTeamIfResolved();
            return;
        }

        unit.SetPermanentlyDefeated();
        RefreshActiveUnitFlags();
        LogWinningTeamIfResolved();
    }

    public int GetTeamLives(UnitTeam team)
    {
        return team == UnitTeam.Blue ? blueTeamLives : redTeamLives;
    }

    public bool HasPlayableUnits(UnitTeam team)
    {
        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];
            if (unit != null && unit.Team == team && IsUnitPlayable(unit))
            {
                return true;
            }
        }

        return false;
    }

    private int ConsumeTeamLife(UnitTeam team)
    {
        int livesRemaining = Mathf.Max(0, GetTeamLives(team) - 1);
        SetTeamLives(team, livesRemaining);

        Debug.Log($"{team} Team lost 1 life. Lives remaining: {livesRemaining}");
        return livesRemaining;
    }

    private void RespawnUnit(UnitController unit)
    {
        Vector3 respawnPosition = GetRespawnPosition(unit.Team);
        unit.transform.position = respawnPosition;
        unit.ResetStatsForRespawn();

        Debug.Log($"{unit.name} respawned at {respawnPosition}");
    }

    private void RefreshActiveUnitFlags()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].SetActiveTurn(i == activeUnitIndex && IsUnitPlayable(units[i]));
            }
        }
    }

    private int FindNextPlayableUnitIndex(int startIndex)
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
            if (IsUnitPlayable(candidate))
            {
                return candidateIndex;
            }
        }

        return -1;
    }

    private bool IsUnitPlayable(UnitController unit)
    {
        return unit != null && unit.IsPlayable;
    }

    private void LogWinningTeamIfResolved()
    {
        if (hasLoggedMatchResult)
        {
            return;
        }

        UnitTeam survivingTeam = default;
        bool hasSurvivingTeam = false;

        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];
            if (!IsUnitPlayable(unit))
            {
                continue;
            }

            if (!hasSurvivingTeam)
            {
                survivingTeam = unit.Team;
                hasSurvivingTeam = true;
                continue;
            }

            if (survivingTeam != unit.Team)
            {
                return;
            }
        }

        if (!hasSurvivingTeam)
        {
            return;
        }

        for (int i = 0; i < teamsInMatch.Count; i++)
        {
            UnitTeam team = teamsInMatch[i];
            if (team == survivingTeam)
            {
                continue;
            }

            if (HasPlayableUnits(team) || GetTeamLives(team) > 0)
            {
                return;
            }
        }

        Debug.Log($"{survivingTeam} Team wins");
        hasLoggedMatchResult = true;
    }

    private void DiscoverSceneUnits()
    {
        UnitController[] sceneUnits = FindObjectsByType<UnitController>(FindObjectsInactive.Exclude);
        units.AddRange(sceneUnits);
        units.Sort(CompareUnitsForStableDiscovery);
    }

    private void RemoveMissingUnits()
    {
        for (int i = units.Count - 1; i >= 0; i--)
        {
            if (units[i] == null)
            {
                units.RemoveAt(i);
            }
        }
    }

    private void RefreshTeamsInMatch()
    {
        teamsInMatch.Clear();
        for (int i = 0; i < units.Count; i++)
        {
            UnitController unit = units[i];
            if (unit != null && !teamsInMatch.Contains(unit.Team))
            {
                teamsInMatch.Add(unit.Team);
            }
        }
    }

    private void SetTeamLives(UnitTeam team, int lives)
    {
        lives = Mathf.Max(0, lives);
        if (team == UnitTeam.Blue)
        {
            blueTeamLives = lives;
        }
        else
        {
            redTeamLives = lives;
        }
    }

    private Vector3 GetRespawnPosition(UnitTeam team)
    {
        return team == UnitTeam.Blue ? blueRespawnPosition : redRespawnPosition;
    }

    private void LogActiveUnit()
    {
        if (ActiveUnit != null)
        {
            Debug.Log($"TurnManager active unit: {ActiveUnit.DisplayName} ({ActiveUnit.UnitId})");
        }
    }

    private static int CompareUnitsForStableDiscovery(UnitController left, UnitController right)
    {
        if (left == right)
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        int teamComparison = left.Team.CompareTo(right.Team);
        if (teamComparison != 0)
        {
            return teamComparison;
        }

        return string.CompareOrdinal(left.UnitId, right.UnitId);
    }
}
