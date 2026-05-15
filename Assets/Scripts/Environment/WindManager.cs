using UnityEngine;

public class WindManager : MonoBehaviour
{
    [Header("Wind Foundation")]
    [SerializeField] private Vector2 currentWind = new Vector2(0.25f, 0f);
    [SerializeField] private float windAccelerationScale = 1f;

    public Vector2 CurrentWind => currentWind;
    public float WindAccelerationScale => windAccelerationScale;

    public void Initialize()
    {
        Debug.Log($"WindManager initialized with wind {currentWind}.");
    }

    public Vector2 GetWindAcceleration()
    {
        return currentWind * windAccelerationScale;
    }

    public void SetWind(Vector2 wind)
    {
        currentWind = wind;
    }
}
