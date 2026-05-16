using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class UnitView : MonoBehaviour
{
    [Header("Sprite Presentation")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color blueTeamColor = new Color(0.25f, 0.55f, 1f, 1f);
    [SerializeField] private Color redTeamColor = new Color(1f, 0.35f, 0.3f, 1f);
    [SerializeField] private Color inactiveTint = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color knockedOutTint = new Color(0.25f, 0.25f, 0.25f, 0.75f);

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void ApplyPresentation(UnitController unit)
    {
        if (unit == null)
        {
            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Sprite vehicleSprite = unit.VehicleData == null ? null : unit.VehicleData.VehicleSprite;
        if (vehicleSprite != null)
        {
            spriteRenderer.sprite = vehicleSprite;
        }

        Color teamColor = unit.Team == UnitTeam.Blue ? blueTeamColor : redTeamColor;
        spriteRenderer.color = unit.IsKnockedOut ? knockedOutTint : unit.IsActiveTurn ? teamColor : Color.Lerp(teamColor, inactiveTint, 0.45f);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (unit.Facing == UnitFacing.Right ? 1f : -1f);
        transform.localScale = scale;
    }
}
