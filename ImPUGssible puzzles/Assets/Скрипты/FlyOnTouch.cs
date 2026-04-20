using UnityEngine;

public class FlyOnTouch : MonoBehaviour
{
    public float speed = 10f;
    public float turnSpeed = 100f;
    public float fallSpeed = 1.5f;

    public Transform cameraTarget; // 👈 добавь пустой объект (точка камеры)

    private Transform player;
    private CharacterController controller;
    private Rigidbody rb;

    private bool isFlying = false;
    private bool used = false;

    private float input;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (!isFlying) return;

        input = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            StopFlying();
    }

    void FixedUpdate()
    {
        if (!isFlying) return;

        float turn = input * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));

        Vector3 move =
            transform.forward * speed * Time.fixedDeltaTime +
            Vector3.down * fallSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + move);
    }

    void OnTriggerEnter(Collider other)
    {
        if (used) return;

        if (other.CompareTag("Player"))
        {
            player = other.transform;
            controller = player.GetComponent<CharacterController>();

            StartFlying();
        }
    }

    void StartFlying()
    {
        isFlying = true;
        used = true;

        if (controller != null)
            controller.enabled = false;

        // 👇 игрок просто остаётся где есть
        // (можно слегка подвинуть к самолёту)
        player.position = transform.position;
    }

    void StopFlying()
    {
        isFlying = false;

        if (controller != null)
            controller.enabled = true;

        rb.useGravity = true;
    }
}