using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ATTACH TO: ExhibitionManager GameObject
/// ALSO ADD:  PlacementController to the SAME GameObject
/// </summary>
public class ExhibitionManager : MonoBehaviour
{
    public static ExhibitionManager Instance;

    [Header("=== CATALOG UI ===")]
    [Tooltip("Drag CatalogPanel here (NOT Canvas, the Panel inside Canvas)")]
    public GameObject catalogPanel;

    [Tooltip("Drag the Content object inside ArtWorkScrollView/Viewport/Content")]
    public Transform catalogContent;

    [Tooltip("Drag your CatalogItem prefab from Project window")]
    public GameObject catalogItemPrefab;

    [Header("=== ARTWORK DATA ===")]
    [Tooltip("Drag all your ArtworkData .asset files here")]
    public List<ArtworkData> availableArtworks = new List<ArtworkData>();

    [Header("=== PLACEMENT ===")]
    [Tooltip("Tick the Wall layer (and Floor/Ceiling if needed)")]
    public LayerMask placementLayers;

    private bool catalogOpen = false;
    private PlacementController placer;

    public bool IsCatalogOpen => catalogOpen;

    // ─────────────────────────────────────────────────────────────
    void Awake() => Instance = this;

    void Start()
    {
        placer = GetComponent<PlacementController>();

        if (placer == null)
            Debug.LogError("[ExhibitionManager] PlacementController is missing! " +
                "Add it to the same GameObject as ExhibitionManager.");

        if (catalogPanel == null)
            Debug.LogError("[ExhibitionManager] catalogPanel is not assigned in Inspector!");
        else
            catalogPanel.SetActive(false);

        BuildCatalog();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleCatalog();
    }

    // ─────────────────────────────────────────────────────────────
    public void ToggleCatalog()
    {
        catalogOpen = !catalogOpen;
        catalogPanel.SetActive(catalogOpen);
        Cursor.lockState = catalogOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = catalogOpen;
    }

    public void StartPlacing(ArtworkData art)
    {
        if (art == null) return;

        // Close catalog
        catalogOpen = false;
        catalogPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;

        placer?.BeginPlacement(art);
    }

    // ─────────────────────────────────────────────────────────────
    void BuildCatalog()
    {
        if (catalogContent == null)
        { Debug.LogError("[ExhibitionManager] catalogContent not assigned!"); return; }

        if (catalogItemPrefab == null)
        { Debug.LogError("[ExhibitionManager] catalogItemPrefab not assigned!"); return; }

        // Clear existing items
        foreach (Transform t in catalogContent)
            Destroy(t.gameObject);

        // Spawn one item per artwork
        int count = 0;
        foreach (var art in availableArtworks)
        {
            if (art == null) continue;
            var go = Instantiate(catalogItemPrefab, catalogContent);
            var ci = go.GetComponent<CatalogItem>();
            if (ci != null) ci.Setup(art, this);
            else Debug.LogWarning("[ExhibitionManager] CatalogItem script missing on prefab!");
            count++;
        }

        Debug.Log("[ExhibitionManager] Catalog built: " + count + " artworks.");
    }
}
