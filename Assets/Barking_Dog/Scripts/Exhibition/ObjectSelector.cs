using UnityEngine;

/// <summary>
/// ATTACH TO: Camera (child of Player)
/// 
/// selectMask → set to "Selectable" layer in Inspector
/// </summary>
public class ObjectSelector : MonoBehaviour
{
    [Header("How far you can reach to select artwork")]
    public float range = 10f;

    [Header("Set this to the 'Selectable' layer")]
    public LayerMask selectMask;

    private PlacedObject selected;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        // Block when catalog is open
        if (ExhibitionManager.Instance != null &&
            ExhibitionManager.Instance.IsCatalogOpen) return;

        // Block while actively placing
        var placer = ExhibitionManager.Instance?
            .GetComponent<PlacementController>();
        if (placer != null && placer.IsPlacing) return;

        // Block when cursor is free AND we're not in move mode
        // (cursor is free when a panel is open)
        bool cursorFree = Cursor.lockState == CursorLockMode.None;
        bool inMoveMode = selected != null && selected.IsMoveMode;
        if (cursorFree && !inMoveMode) return;

        // Only fire on left mouse click
        if (!Input.GetMouseButtonDown(0)) return;

        // Shoot ray from screen centre (crosshair)
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width * .5f, Screen.height * .5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, range, selectMask))
        {
            var po = hit.collider.GetComponentInParent<PlacedObject>();
            if (po != null)
            {
                // Deselect previous
                if (selected != null && selected != po)
                    selected.Deselect();

                selected = po;
                po.Select();
                return;
            }
        }

        // Clicked on nothing — deselect
        if (selected != null)
        {
            selected.Deselect();
            selected = null;
        }
    }
}
