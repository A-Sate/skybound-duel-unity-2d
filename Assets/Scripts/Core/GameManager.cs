using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Foundation References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private WindManager windManager;

    public TurnManager TurnManager => turnManager;
    public WindManager WindManager => windManager;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManager>();
        }

        if (windManager == null)
        {
            windManager = GetComponent<WindManager>();
        }
    }

    private void Start()
    {
        windManager?.Initialize();
        turnManager?.Initialize();

        Debug.Log("Skybound Duel 2D foundation initialized.");
    }
}
