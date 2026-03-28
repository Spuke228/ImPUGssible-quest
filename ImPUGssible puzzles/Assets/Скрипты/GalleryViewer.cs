using UnityEngine;
using UnityEngine.UI;

public class GalleryViewer : MonoBehaviour
{
    public Image viewer;
    public GameObject viewerPanel;

    public void ShowImage(Sprite img)
    {
        viewer.sprite = img;
        viewerPanel.SetActive(true);
    }

    public void CloseImage()
    {
        viewerPanel.SetActive(false);
    }
}