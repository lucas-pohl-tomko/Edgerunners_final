using System.Collections;
using UnityEngine;

public class ShottyPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Avatar avatar = other.GetComponentInParent<Avatar>();
        if (avatar == null)
            return;

        avatar.ChangeWeapon();
        Destroy(gameObject);
    }
}