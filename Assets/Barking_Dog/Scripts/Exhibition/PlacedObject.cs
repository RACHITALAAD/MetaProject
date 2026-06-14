using UnityEngine;

/// <summary>
/// Auto-added to every artwork when placed.
/// FIX: Was using .material.color which only changes one renderer.
///      Now properly stores and restores all renderer materials.
/// </summary>
public class PlacedObject : MonoBehaviour
{
    [HideInInspector] public ArtworkData artworkData;

    private Renderer[] rends;
    private Color[]    baseColors;  // original colors per renderer

    private bool    moveMode;
    private Vector3 surfaceRight;
    private Vector3 surfaceUp;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        rends      = GetComponentsInChildren<Renderer>();
        baseColors = new Color[rends.Length];

        for (int i = 0; i < rends.Length; i++)
        {
            // Instance the material so we don't affect the shared asset
            rends[i].material = new Material(rends[i].material);
            baseColors[i]     = rends[i].material.color;
        }

        // Remember wall directions at placement time
        surfaceRight = transform.right;
        surfaceUp    = transform.up;
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (!moveMode) return;

        float spd = 2f * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow))    transform.position += surfaceUp    * spd;
        if (Input.GetKey(KeyCode.DownArrow))  transform.position -= surfaceUp    * spd;
        if (Input.GetKey(KeyCode.LeftArrow))  transform.position -= surfaceRight * spd;
        if (Input.GetKey(KeyCode.RightArrow)) transform.position += surfaceRight * spd;
    }

    // ─────────────────────────────────────────────────────────────
    // Called by ObjectSelector
    public void Select()
    {
        ApplyTint(true);
        InteractionUI.Instance?.ShowForObject(this);
    }

    public void Deselect()
    {
        moveMode = false;
        ApplyTint(false);
        InteractionUI.Instance?.HideAll();
    }

    // ─────────────────────────────────────────────────────────────
    // Operations - called by InteractionUI buttons
    public void FlipH()
    {
        var s = transform.localScale; s.x *= -1;
        transform.localScale = s;
    }
    public void FlipV()
    {
        var s = transform.localScale; s.y *= -1;
        transform.localScale = s;
    }
    public void RotCW()  => transform.Rotate(0, 0, -45, Space.Self);
    public void RotCCW() => transform.Rotate(0, 0,  45, Space.Self);
    public void Bigger()  => transform.localScale *= 1.25f;
    public void Smaller() => transform.localScale *= 0.8f;

    // ── FIX: SetColor now correctly applies to ALL renderers ──────
    public void SetColor(Color c)
    {
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material.color = c;
            baseColors[i] = c; // update baseline so tint still works
        }
    }

    public void Delete()
    {
        InteractionUI.Instance?.HideAll();
        Destroy(gameObject);
    }

    public void ToggleMoveMode() => moveMode = !moveMode;
    public bool IsMoveMode       => moveMode;

    // ─────────────────────────────────────────────────────────────
    void ApplyTint(bool selected)
    {
        for (int i = 0; i < rends.Length; i++)
            rends[i].material.color = selected
                ? Color.Lerp(baseColors[i], Color.yellow, 0.35f)
                : baseColors[i];
    }
}
