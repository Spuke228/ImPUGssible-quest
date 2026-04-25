using UnityEngine;

public class WinTrigger : MonoBehaviour
{
    public GameObject winCanvas;

    void OnTriggerEnter(Collider other)
    {
        Управлениемопсом controller = other.GetComponent<Управлениемопсом>();
        if (controller != null)
            controller.enabled = false;
        if (other.CompareTag("Player"))
        {
            Time.timeScale = 0f;

            if (winCanvas != null)
                winCanvas.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}