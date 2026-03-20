using UnityEngine;
using UnityEngine.UI;

public class PawTrail : MonoBehaviour
{
    public Slider slider;
    public RectTransform trailParent;  // сюда будут спавниться следы
    public GameObject pawPrefab;       // префаб следа лапки
    public float spacing = 20f;        // расстояние между следами

    private float lastSpawnX;

    void Start()
    {
        lastSpawnX = slider.fillRect.rect.xMin;
        slider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnSliderChanged(float value)
    {
        float currentX = Mathf.Lerp(slider.fillRect.rect.xMin, slider.fillRect.rect.xMax, value);
        if (Mathf.Abs(currentX - lastSpawnX) > spacing)
        {
            SpawnPaw(currentX);
            lastSpawnX = currentX;
        }
    }

    void SpawnPaw(float xPos)
    {
        GameObject paw = Instantiate(pawPrefab, trailParent);
        paw.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
        Destroy(paw, 2f); // след исчезнет через 2 сек (можно убрать)
    }
}
