using UnityEngine;

public class ImageZoom : MonoBehaviour
{
    public RectTransform image;
    float zoom = 1f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            zoom += scroll * 2f;
            zoom = Mathf.Clamp(zoom, 0.5f, 4f);
            image.localScale = Vector3.one * zoom;
        }
    }
}