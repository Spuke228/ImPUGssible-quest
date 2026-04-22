using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneRide : MonoBehaviour
{
    public Transform seatPoint;
    public float speed = 15f;
    public float turnSpeed = 120f;
    public float fallSpeed = 2f;
    public float respawnDelay = 0.1f; // Почти сразу

    [Header("Controls")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode jumpOffKey = KeyCode.Space;

    private Transform player;
    private CharacterController playerController;
    private bool playerInTrigger;
    private bool riding;
    private bool isRespawning;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        ConfigureRigidbody();
    }

    void ConfigureRigidbody()
    {
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // БЛОКИРУЕМ НАКЛОНЫ: Самолет не будет заваливаться на бок или клевать носом
        // Он сможет вращаться только влево-вправо по оси Y
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        if (playerInTrigger && !riding && !isRespawning && Input.GetKeyDown(interactKey))
            StartRide();
        else if (riding && Input.GetKeyDown(jumpOffKey))
            JumpOff();
    }

    void FixedUpdate()
    {
        if (!riding) return;

        // 1. Поворот (только вокруг оси Y)
        float turn = Input.GetAxis("Horizontal");
        float turnAngle = turn * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turnAngle, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);

        // 2. Жесткое движение вперед и вниз
        // Используем transform.forward, чтобы лететь строго туда, куда смотрит нос
        Vector3 direction = transform.forward * speed;
        direction.y = -fallSpeed; // Принудительная вертикальная скорость

        rb.MovePosition(rb.position + direction * Time.fixedDeltaTime);

        // Обнуляем физические силы, чтобы они не мешали MovePosition
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (riding && player != null)
        {
            player.position = seatPoint.position;
            player.rotation = seatPoint.rotation;
        }
    }

    void StartRide()
    {
        riding = true;
        if (playerController != null) playerController.enabled = false;
        rb.useGravity = false;
    }

    void JumpOff()
    {
        riding = false;
        if (playerController != null) playerController.enabled = true;

        player.position += transform.right * 1.5f;

        // Придаем импульс при спрыгивании
        rb.linearVelocity = (transform.forward * speed) + (Vector3.down * fallSpeed);

        StopAllCoroutines();
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);

        // Мгновенный сброс скоростей перед телепортацией
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPosition;
        transform.rotation = startRotation;

        isRespawning = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
            player = other.transform;
            playerController = player.GetComponent<CharacterController>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInTrigger = false;
    }
}
