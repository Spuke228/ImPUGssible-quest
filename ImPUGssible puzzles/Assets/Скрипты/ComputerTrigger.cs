using UnityEngine;

public class ComputerTrigger : MonoBehaviour
{
    public GameObject computerUI;
    public MonoBehaviour playerController;

    bool playerInside;

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            OpenComputer();
        }

        if (computerUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseComputer();
        }
    }

    void OpenComputer()
    {
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        computerUI.SetActive(true);

        if (playerController != null)
            playerController.enabled = false;
    }

    void CloseComputer()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        computerUI.SetActive(false);

        if (playerController != null)
            playerController.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    public void CloseComputerButton()
    {
        CloseComputer();
    }
}