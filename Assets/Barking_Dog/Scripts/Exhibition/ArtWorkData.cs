using UnityEngine;

[CreateAssetMenu(fileName = "ArtworkItem",
    menuName = "Exhibition/Artwork Item")]
public class ArtworkData : ScriptableObject
{
    public string artworkName;
    public Sprite thumbnail;
    public GameObject prefab;
    public string description;
    public ArtworkCategory category;
}

public enum ArtworkCategory
{
    Painting,
    Sculpture,
    Material,
    Artifact
}