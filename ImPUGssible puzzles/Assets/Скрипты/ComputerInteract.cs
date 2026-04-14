using UnityEngine;

public class ComputerInteract : MonoBehaviour
{
    public ComputerOSManager computerOS;

    public GameObject interactIcon;
    public Vector3 iconOffset = new Vector3(0, 1.2f, 0);

    private Transform player;
    private bool playerNear;

    void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            computerOS.OpenComputer();
        }

        if (interactIcon != null && player != null)
        {
            interactIcon.transform.position = transform.position + iconOffset;

            Vector3 dir = player.position - interactIcon.transform.position;
            dir.y = 0;
            interactIcon.transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            playerNear = true;

            if (interactIcon != null)
                interactIcon.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;

            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }
}