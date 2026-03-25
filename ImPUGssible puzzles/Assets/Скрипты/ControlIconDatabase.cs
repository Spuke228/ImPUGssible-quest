using UnityEngine;

[System.Serializable]
public class ControlIcon
{
    public KeyCode key;
    public Sprite icon;
}

public class ControlIconDatabase : MonoBehaviour
{
    public ControlIcon[] icons;

    public Sprite GetIcon(KeyCode key)
    {
        foreach (ControlIcon icon in icons)
        {
            if (icon.key == key)
                return icon.icon;
        }

        return null;
    }
}