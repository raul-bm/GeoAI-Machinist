using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            Transform parent = transform.parent;

            if (parent != null)
            {
                CNNLayer layer = parent.GetComponent<CNNLayer>();
                if (GameManager.instance.IsSolved(layer.type))
                {
                    return;
                }

                GameManager.instance.StartMiniGame(layer.type);
            }
        }
    }
}
