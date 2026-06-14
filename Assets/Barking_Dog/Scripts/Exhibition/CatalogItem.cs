using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatalogItem : MonoBehaviour
{
    public Image       thumbnail;
    public TMP_Text    nameText;
    public TMP_Text    categoryText;
    public Button      placeButton;

    public void Setup(ArtworkData d,
        ExhibitionManager mgr)
    {
        if (d == null || mgr == null) return;

        if (thumbnail != null && d.thumbnail != null)
            thumbnail.sprite = d.thumbnail;

        if (nameText != null)
            nameText.text = d.artworkName;

        if (categoryText != null)
            categoryText.text = d.category.ToString();

        if (placeButton != null)
        {
            placeButton.onClick.RemoveAllListeners();
            placeButton.onClick.AddListener(
                () => mgr.StartPlacing(d));
        }
    }
}