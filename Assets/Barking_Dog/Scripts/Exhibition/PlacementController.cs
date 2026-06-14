using UnityEngine;

/// <summary>
/// ATTACH TO: ExhibitionManager GameObject
/// FIX: Image was black because we were saving sharedMaterials (shared across ALL instances)
///      and then overwriting them. Now we instance each material properly.
/// </summary>
public class PlacementController : MonoBehaviour
{
    [Header("Leave empty - auto finds Camera.main")]
    public Camera playerCam;

    [Header("Max distance to detect wall/floor")]
    public float maxDistance = 15f;

    public bool IsPlacing => isPlacing;

    private bool isPlacing;
    private GameObject ghost;
    private ArtworkData artData;
    private Material ghostMat;

    // Store each renderer's original material INSTANCES (not shared)
    private Renderer[] ghostRenderers;
    private Material[][] originalMaterials; // [rendIndex][slotIndex]

    void Start()
    {
        if (playerCam == null) playerCam = Camera.main;
        CreateGhostMaterial();
    }

    // ─────────────────────────────────────────────────────────────
    public void BeginPlacement(ArtworkData art)
    {
        if (isPlacing) return;
        if (art == null || art.prefab == null)
        {
            Debug.LogError("[Placer] Prefab missing on ArtworkData: " + art?.artworkName);
            return;
        }

        if (playerCam == null) playerCam = Camera.main;

        artData   = art;
        isPlacing = true;

        // Spawn ghost
        ghost      = Instantiate(art.prefab);
        ghost.name = "GHOST_" + art.artworkName;

        // Disable all physics
        foreach (var col in ghost.GetComponentsInChildren<Collider>())
            col.enabled = false;
        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        // ── KEY FIX: Instance every material properly ─────────────
        ghostRenderers  = ghost.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[ghostRenderers.Length][];

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            // Access .materials (not .sharedMaterials) to get instanced copies
            Material[] mats = ghostRenderers[i].materials;
            originalMaterials[i] = new Material[mats.Length];
            for (int j = 0; j < mats.Length; j++)
                originalMaterials[i][j] = mats[j]; // these are already instanced

            // Replace with ghost (transparent blue) material
            Material[] ghostSlots = new Material[mats.Length];
            for (int j = 0; j < ghostSlots.Length; j++)
                ghostSlots[j] = ghostMat;
            ghostRenderers[i].materials = ghostSlots;
        }

        ghost.SetActive(false);
        Debug.Log("[Placer] Ghost created: " + art.artworkName);
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (!isPlacing || ghost == null) return;

        MoveGhostToSurface();

        if (Input.GetMouseButtonDown(0) && ghost.activeSelf)
        { PlaceObject(); return; }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelPlacement();
    }

    // ─────────────────────────────────────────────────────────────
    void MoveGhostToSurface()
    {
        Ray ray = playerCam.ScreenPointToRay(
            new Vector3(Screen.width * .5f, Screen.height * .5f, 0));

        LayerMask mask = ExhibitionManager.Instance != null
            ? ExhibitionManager.Instance.placementLayers
            : ~0;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask))
        {
            ghost.SetActive(true);
            ghost.transform.position = hit.point + hit.normal * 0.02f;

            bool isFloor = Vector3.Dot(hit.normal, Vector3.up) > 0.7f;
            ghost.transform.rotation = isFloor
                ? Quaternion.FromToRotation(Vector3.forward, hit.normal)
                : Quaternion.LookRotation(-hit.normal, Vector3.up);
        }
        else
        {
            ghost.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────────────────────
    void PlaceObject()
    {
        // ── RESTORE ORIGINAL MATERIALS ────────────────────────────
        // This was broken before - now we restore the instanced materials
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            if (originalMaterials[i] != null)
                ghostRenderers[i].materials = originalMaterials[i];
        }

        // Re-enable colliders
        foreach (var col in ghost.GetComponentsInChildren<Collider>())
            col.enabled = true;

        // Add PlacedObject component
        var po = ghost.GetComponent<PlacedObject>()
               ?? ghost.AddComponent<PlacedObject>();
        po.artworkData = artData;

        // Set to Selectable layer
        int layer = LayerMask.NameToLayer("Selectable");
        if (layer >= 0) SetLayerAll(ghost, layer);
        else Debug.LogWarning("[Placer] Create a layer named 'Selectable' in Project Settings!");

        ghost.name = artData.artworkName;
        Debug.Log("[Placer] Placed: " + artData.artworkName);

        // Clear state
        ghost             = null;
        isPlacing         = false;
        artData           = null;
        ghostRenderers    = null;
        originalMaterials = null;
    }

    // ─────────────────────────────────────────────────────────────
    public void CancelPlacement()
    {
        if (ghost != null) Destroy(ghost);
        ghost             = null;
        isPlacing         = false;
        artData           = null;
        ghostRenderers    = null;
        originalMaterials = null;
    }

    void CreateGhostMaterial()
    {
        ghostMat       = new Material(Shader.Find("Standard"));
        ghostMat.color = new Color(0.3f, 0.75f, 1f, 0.4f);
        ghostMat.SetFloat("_Mode", 3);
        ghostMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        ghostMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        ghostMat.SetInt("_ZWrite", 0);
        ghostMat.EnableKeyword("_ALPHABLEND_ON");
        ghostMat.renderQueue = 3000;
    }

    void SetLayerAll(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerAll(t.gameObject, layer);
    }
}
