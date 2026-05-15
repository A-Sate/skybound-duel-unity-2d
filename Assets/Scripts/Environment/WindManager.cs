using UnityEngine;
using UnityEngine.Serialization;

public class WindManager : MonoBehaviour
{
    [Header("Wind Foundation")]
    [Range(0f, 20f)]
    [SerializeField] private float windStrength = 10f;
    [SerializeField] private float windDirectionDegrees;
    [SerializeField] private float windAccelerationScale = 0.03f;

    [Header("Wind Debug")]
    [FormerlySerializedAs("currentWind")]
    [SerializeField] private Vector2 currentWindVector;

    public float WindStrength => windStrength;
    public float WindDirectionDegrees => windDirectionDegrees;
    public float WindAccelerationScale => windAccelerationScale;
    public Vector2 CurrentWindVector => currentWindVector;
    public Vector2 CurrentWind => currentWindVector;

    public void Initialize()
    {
        RefreshCurrentWindVector();
        Debug.Log($"WindManager initialized with strength {windStrength:0.##}, direction {windDirectionDegrees:0.##} degrees, acceleration {GetWindAcceleration()}.");
    }

    public Vector2 GetWindAcceleration()
    {
        RefreshCurrentWindVector();
        return currentWindVector * windAccelerationScale;
    }

    public Vector2 GetWindAcceleration(float projectileWindAccelerationScale)
    {
        return GetWindAcceleration() * Mathf.Max(0f, projectileWindAccelerationScale);
    }

    public void SetWind(float strength, float directionDegrees)
    {
        windStrength = Mathf.Clamp(strength, 0f, 20f);
        windDirectionDegrees = NormalizeDegrees(directionDegrees);
        RefreshCurrentWindVector();
    }

    public void SetWind(Vector2 wind)
    {
        windStrength = Mathf.Clamp(wind.magnitude, 0f, 20f);
        windDirectionDegrees = wind.sqrMagnitude <= Mathf.Epsilon ? 0f : NormalizeDegrees(Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg);
        RefreshCurrentWindVector();
    }

    private void OnValidate()
    {
        windStrength = Mathf.Clamp(windStrength, 0f, 20f);
        windDirectionDegrees = NormalizeDegrees(windDirectionDegrees);
        windAccelerationScale = Mathf.Max(0f, windAccelerationScale);
        RefreshCurrentWindVector();
    }

    private void RefreshCurrentWindVector()
    {
        if (windStrength <= 0f)
        {
            currentWindVector = Vector2.zero;
            return;
        }

        float directionRadians = windDirectionDegrees * Mathf.Deg2Rad;
        currentWindVector = new Vector2(Mathf.Cos(directionRadians), Mathf.Sin(directionRadians)) * windStrength;
    }

    private static float NormalizeDegrees(float degrees)
    {
        degrees %= 360f;
        return degrees < 0f ? degrees + 360f : degrees;
    }
}
