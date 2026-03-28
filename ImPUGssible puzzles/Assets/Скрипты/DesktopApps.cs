using UnityEngine;

public class DesktopApps : MonoBehaviour
{
    public GameObject desktop;

    public GameObject roblox;
    public GameObject dota;
    public GameObject gallery;
    public GameObject minesweeper;

    public void OpenRoblox()
    {
        CloseAll();
        roblox.SetActive(true);
    }

    public void OpenDota()
    {
        CloseAll();
        dota.SetActive(true);
    }

    public void OpenGallery()
    {
        CloseAll();
        gallery.SetActive(true);
    }

    public void OpenMinesweeper()
    {
        CloseAll();
        minesweeper.SetActive(true);
    }

    public void BackToDesktop()
    {
        CloseAll();
        desktop.SetActive(true);
    }

    void CloseAll()
    {
        desktop.SetActive(false);
        roblox.SetActive(false);
        dota.SetActive(false);
        gallery.SetActive(false);
        minesweeper.SetActive(false);
    }
}