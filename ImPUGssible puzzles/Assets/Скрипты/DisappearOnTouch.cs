using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DisappearWithEffect : MonoBehaviour
{
    public GameObject effectPrefab;
    public float effectLifetime = 3f;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectLifetime);
        }

        Destroy(gameObject);
    }
}
