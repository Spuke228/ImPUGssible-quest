using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WallPuzzleWithIcon : MonoBehaviour
{
    public GameObject puzzleUI;
    public MonoBehaviour playerController;

    public GameObject interactIconPrefab;
    public Vector3 iconOffset = new Vector3(0, 1f, 0);

    private GameObject interactIconInstance;
    private Transform player;

    private bool isPlayerNearby = false;
    private bool opened = false;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (interactIconPrefab != null)
        {
            interactIconInstance = Instantiate(interactIconPrefab);
            interactIconInstance.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = other.transform;

            if (interactIconInstance != null)
                interactIconInstance.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (interactIconInstance != null)
                interactIconInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (interactIconInstance != null && player != null)
        {
            interactIconInstance.transform.position = transform.position + iconOffset;

            Vector3 dir = player.position - interactIconInstance.transform.position;
            dir.y = 0;
            interactIconInstance.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (isPlayerNearby && !opened && Input.GetKeyDown(KeyCode.E))
        {
            puzzleUI.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerController != null)
                playerController.enabled = false;

            opened = true;

            if (interactIconInstance != null)
                interactIconInstance.SetActive(false);
        }
    }
}