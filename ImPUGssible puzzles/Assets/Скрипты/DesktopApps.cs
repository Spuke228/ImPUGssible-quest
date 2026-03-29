using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DesktopSystem : MonoBehaviour
{
    [Header("Windows")]
    public GameObject desktop;
    public GameObject galleryWindow;
    public GameObject imageViewer;
    public GameObject minesweeperWindow;
    public GameObject robloxWindow;
    public GameObject dotaWindow;

    [Header("Image Viewer")]
    public Image viewerImage;

    void Start()
    {
        CloseAll();
        desktop.SetActive(true);
    }

    void CloseAll()
    {
        desktop.SetActive(false);
        galleryWindow.SetActive(false);
        imageViewer.SetActive(false);
        minesweeperWindow.SetActive(false);
        robloxWindow.SetActive(false);
        dotaWindow.SetActive(false);
    }

    public void OpenDesktop()
    {
        CloseAll();
        desktop.SetActive(true);
    }

    public void OpenGallery()
    {
        CloseAll();
        galleryWindow.SetActive(true);
    }

    public void OpenImage(Sprite img)
    {
        CloseAll();
        viewerImage.sprite = img;
        imageViewer.SetActive(true);
    }

    public void OpenMinesweeper()
    {
        CloseAll();
        minesweeperWindow.SetActive(true);
    }

    public void OpenRoblox()
    {
        CloseAll();
        robloxWindow.SetActive(true);
    }

    public void OpenDota()
    {
        CloseAll();
        dotaWindow.SetActive(true);
    }
}