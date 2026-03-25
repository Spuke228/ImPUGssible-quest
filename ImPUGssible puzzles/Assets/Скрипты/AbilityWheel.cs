using UnityEngine;

public class AbilityWheel : MonoBehaviour
{
    public Transform content;

    public float rotationSpeed = 300f;

    float rotation;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            rotation += scroll * rotationSpeed;

            content.localRotation =
                Quaternion.Euler(0, 0, rotation);
        }
    }
}