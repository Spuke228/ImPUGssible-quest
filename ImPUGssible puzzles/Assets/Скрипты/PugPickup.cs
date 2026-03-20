using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PugPickup : MonoBehaviour
{
    public Transform mouthPoint;
    public KeyCode interactKey = KeyCode.E;

    public float pickupDistance = 3f;

    private PickupItem currentItem;
    private PickupItem nearbyItem;

    void Update()
    {
        FindNearestItem();

        if (Input.GetKeyDown(interactKey))
        {
            if (currentItem == null && nearbyItem != null)
            {
                currentItem = nearbyItem;
                currentItem.OnPickup(mouthPoint);
                currentItem.ShowIcon(false);
            }
            else if (currentItem != null)
            {
                currentItem.Drop(Vector3.zero);
                currentItem = null;
            }
        }
    }

    void FindNearestItem()
    {
        PickupItem[] items = FindObjectsOfType<PickupItem>();

        float closest = pickupDistance;
        PickupItem bestItem = null;

        foreach (PickupItem item in items)
        {
            if (!item.CanBePickedUp()) continue;

            float dist = Vector3.Distance(transform.position, item.transform.position);

            if (dist < closest)
            {
                closest = dist;
                bestItem = item;
            }
        }

        if (nearbyItem != bestItem)
        {
            if (nearbyItem != null)
                nearbyItem.ShowIcon(false);

            nearbyItem = bestItem;

            if (nearbyItem != null)
            {
                nearbyItem.SetPlayer(transform);
                nearbyItem.ShowIcon(true);
            }
        }
    }
}