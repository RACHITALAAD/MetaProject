using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ATTACH TO: InteractionUIManager GameObject
///
/// HOW THE PANELS WORK:
/// - ALL panels start HIDDEN (SetActive false in Start)
/// - When player CLICKS a placed artwork → panels become available
/// - Player presses O → OpsPanel appears
/// - Player presses C → ColorPanel appears  
/// - Player presses R → RoomPanel appears
/// - X button or ESC → hides the open panel (deselects object)
///
/// IMPORTANT: The hint bar (HintText) shows when object is selected,
/// telling player which keys to press.
/// </summary>
public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    // ── Ops Panel ─────────────────────────────────────────────────
    [Header("=== OPS PANEL ===")]
    public GameObject opsPanel;
    public Button btnFlipH;
    public Button btnFlipV;
    public Button btnRotCW;
    public Button btnRotCCW;
    public Button btnBigger;
    public Button btnSmaller;
    public Button btnMove;
    public Button btnDelete;
    public Button btnCloseOps;

    // ── Color Panel ───────────────────────────────────────────────
    [Header("=== COLOR PANEL (for artwork) ===")]
    public GameObject colorPanel;
    public Button[]   colorBtns;
    public Color[]    colorValues;
    public Button     btnCloseColor;

    // ── Room Panel ────────────────────────────────────────────────
    [Header("=== ROOM PANEL (for walls) ===")]
    public GameObject roomPanel;
    public Button[]   roomBtns;
    public Color[]    roomValues;
    public Button     btnCloseRoom;

    // ── Hint Text ─────────────────────────────────────────────────
    [Header("=== HINT TEXT ===")]
    [Tooltip("A text showing: Click artwork to select | O=Ops C=Color R=Room")]
    public GameObject hintText;

    // ── Move button label ─────────────────────────────────────────
    private TMP_Text moveBtnLabel;

    // ── State ─────────────────────────────────────────────────────
    private PlacedObject selectedObject;

    // ─────────────────────────────────────────────────────────────
    void Awake() => Instance = this;

    void Start()
    {
        // Hide all panels at game start
        SetActive(opsPanel,   false);
        SetActive(colorPanel, false);
        SetActive(roomPanel,  false);
        SetActive(hintText,   false);

        // Cache move button label
        if (btnMove != null)
            moveBtnLabel = btnMove.GetComponentInChildren<TMP_Text>();

        // ── Wire Ops buttons ──────────────────────────────────────
        AddClick(btnFlipH,    () => selectedObject?.FlipH());
        AddClick(btnFlipV,    () => selectedObject?.FlipV());
        AddClick(btnRotCW,    () => selectedObject?.RotCW());
        AddClick(btnRotCCW,   () => selectedObject?.RotCCW());
        AddClick(btnBigger,   () => selectedObject?.Bigger());
        AddClick(btnSmaller,  () => selectedObject?.Smaller());
        AddClick(btnMove,     OnMoveClicked);
        AddClick(btnDelete,   OnDeleteClicked);

        // ── Wire X buttons ────────────────────────────────────────
        AddClick(btnCloseOps,   () => ClosePanel(opsPanel));
        AddClick(btnCloseColor, () => ClosePanel(colorPanel));
        AddClick(btnCloseRoom,  () => ClosePanel(roomPanel));

        // ── Wire Color buttons ────────────────────────────────────
        for (int i = 0; i < colorBtns.Length; i++)
        {
            int idx = i;
            if (colorBtns[idx] != null)
                colorBtns[idx].onClick.AddListener(() =>
                {
                    if (idx < colorValues.Length)
                        selectedObject?.SetColor(colorValues[idx]);
                });
        }

        // ── Wire Room buttons ─────────────────────────────────────
        for (int i = 0; i < roomBtns.Length; i++)
        {
            int idx = i;
            if (roomBtns[idx] != null)
                roomBtns[idx].onClick.AddListener(() =>
                {
                    if (idx < roomValues.Length)
                        PaintRoom(roomValues[idx]);
                });
        }
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        // Key shortcuts only work when an object is selected
        if (selectedObject == null) return;

        if (Input.GetKeyDown(KeyCode.O)) TogglePanel(opsPanel);
        if (Input.GetKeyDown(KeyCode.C)) TogglePanel(colorPanel);
        if (Input.GetKeyDown(KeyCode.R)) TogglePanel(roomPanel);

        // ESC closes whichever panel is open
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (opsPanel   != null && opsPanel.activeSelf)   { ClosePanel(opsPanel);   return; }
            if (colorPanel != null && colorPanel.activeSelf) { ClosePanel(colorPanel); return; }
            if (roomPanel  != null && roomPanel.activeSelf)  { ClosePanel(roomPanel);  return; }
            // If no panel open, deselect
            DeselectCurrent();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // PUBLIC API — called by PlacedObject
    // ─────────────────────────────────────────────────────────────

    /// <summary>Called when player clicks a placed artwork.</summary>
    public void ShowForObject(PlacedObject obj)
    {
        selectedObject = obj;

        // Show hint so player knows what keys to press
        SetActive(hintText, true);

        // Free cursor so player can use buttons
        FreeCursor(true);

        Debug.Log("[UI] Object selected: " + obj.name +
                  " | Press O=Ops, C=Color, R=Room");
    }

    /// <summary>Called when player deselects or object is deleted.</summary>
    public void HideAll()
    {
        selectedObject = null;

        SetActive(opsPanel,   false);
        SetActive(colorPanel, false);
        SetActive(roomPanel,  false);
        SetActive(hintText,   false);

        // Back to FPS mode
        FreeCursor(false);
    }

    /// <summary>Called by the RoomColorBtn in your scene (wire in Inspector).</summary>
    public void ToggleRoom()
    {
        TogglePanel(roomPanel);
    }

    // ─────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────

    void TogglePanel(GameObject panel)
    {
        if (panel == null) return;
        bool opening = !panel.activeSelf;

        // Close all other panels first
        if (opening)
        {
            SetActive(opsPanel,   false);
            SetActive(colorPanel, false);
            SetActive(roomPanel,  false);
        }

        SetActive(panel, opening);
        FreeCursor(opening);
    }

    void ClosePanel(GameObject panel)
    {
        SetActive(panel, false);

        // If all panels closed, lock cursor back to FPS
        bool anyOpen = (opsPanel   != null && opsPanel.activeSelf)   ||
                       (colorPanel != null && colorPanel.activeSelf) ||
                       (roomPanel  != null && roomPanel.activeSelf);

        if (!anyOpen) FreeCursor(false);
    }

    void DeselectCurrent()
    {
        selectedObject?.Deselect();
        selectedObject = null;
        HideAll();
    }

    void OnMoveClicked()
    {
        if (selectedObject == null) return;
        selectedObject.ToggleMoveMode();

        if (moveBtnLabel != null)
            moveBtnLabel.text = selectedObject.IsMoveMode ? "Stop Move" : "Move";

        // In move mode lock cursor so arrow keys slide the artwork
        if (selectedObject.IsMoveMode)
            FreeCursor(false);
        else
            FreeCursor(true);
    }

    void OnDeleteClicked()
    {
        selectedObject?.Delete();
        selectedObject = null;
        HideAll();
    }

    // ── Paint every wall/floor/ceiling in the scene ───────────────
    void PaintRoom(Color col)
    {
        foreach (var r in FindObjectsOfType<Renderer>())
        {
            // Skip placed artworks
            if (r.GetComponentInParent<PlacedObject>() != null) continue;

            string n = r.gameObject.name.ToLower();
            if (n.Contains("wall")    || n.Contains("floor") ||
                n.Contains("ceiling") || n.Contains("roof")  ||
                n.Contains("room"))
            {
                r.material.color = col;
            }
        }
    }

    void FreeCursor(bool free)
    {
        Cursor.lockState = free ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = free;
    }

    void SetActive(GameObject obj, bool active)
    {
        if (obj != null) obj.SetActive(active);
    }

    void AddClick(Button b, UnityEngine.Events.UnityAction action)
    {
        if (b != null) b.onClick.AddListener(action);
    }
}
