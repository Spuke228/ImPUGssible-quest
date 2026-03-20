using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PugLever : MonoBehaviour
{
    public Transform objectToMove;
    public Vector3 moveOffset = new Vector3(1f, 0f, 0f);
    public float moveDuration = 1f;

    public GameObject interactIcon;
    public Vector3 iconOffset = new Vector3(0, 1f, 0);

    private bool playerNearby;
    private bool activated;

    private Transform player;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (interactIcon != null && player != null)
        {
            // позиция иконки
            interactIcon.transform.position = transform.position + iconOffset;

            // поворот к игроку
            Vector3 dir = player.position - interactIcon.transform.position;
            dir.y = 0;
            interactIcon.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (playerNearby && !activated && Input.GetKeyDown(KeyCode.E))
        {
            activated = true;

            if (interactIcon != null)
                interactIcon.SetActive(false);

            if (objectToMove != null)
                StartCoroutine(MoveObject());
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            player = other.transform;

            if (interactIcon != null)
                interactIcon.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (interactIcon != null)
                interactIcon.SetActive(false);
        }
    }

    IEnumerator MoveObject()
    {
        Vector3 start = objectToMove.position;
        Vector3 end = start + moveOffset;

        float t = 0;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            objectToMove.position = Vector3.Lerp(start, end, t / moveDuration);
            yield return null;
        }

        objectToMove.position = end;
    }
}