using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class PickupItem : MonoBehaviour
{
    public AudioClip pickupSound;
    public float pickupCooldown = 0.3f;

    public GameObject interactIcon;   // объект кнопки E
    public Vector3 iconOffset = new Vector3(0, 0.8f, 0);

    private AudioSource audioSource;
    private Rigidbody rb;
    private Collider col;

    private bool isPickedUp;
    private float nextPickupTime;

    private Transform player;

    private Transform cameraTransform;

    public void SetCamera(Transform cam)
    {
        cameraTransform = cam;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        col.isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        audioSource.playOnAwake = false;

        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (interactIcon == null || player == null) return;

        // позиция над предметом
        interactIcon.transform.position = transform.position + iconOffset;

        // поворот к игроку
        Vector3 dir = player.position - interactIcon.transform.position;
        dir.y = 0;
        interactIcon.transform.rotation = Quaternion.LookRotation(dir);
    }

    public void SetPlayer(Transform p)
    {
        player = p;
    }

    public void ShowIcon(bool show)
    {
        if (interactIcon != null)
            interactIcon.SetActive(show);
    }

    public bool CanBePickedUp()
    {
        return Time.time >= nextPickupTime && !isPickedUp;
    }

    public void OnPickup(Transform mouthPoint)
    {
        if (!CanBePickedUp()) return;

        ShowIcon(false);
        isPickedUp = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        col.isTrigger = true;

        transform.SetParent(mouthPoint, false);
        transform.localPosition = new Vector3(0f, -0.05f, 0.1f);
        transform.localRotation = Quaternion.identity;

        if (pickupSound != null)
            audioSource.PlayOneShot(pickupSound);
    }

    public void Drop(Vector3 dropForce)
    {
        if (!isPickedUp) return;

        isPickedUp = false;
        nextPickupTime = Time.time + pickupCooldown;

        transform.SetParent(null);

        col.isTrigger = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        if (dropForce != Vector3.zero)
            rb.AddForce(dropForce, ForceMode.Impulse);
    }
}