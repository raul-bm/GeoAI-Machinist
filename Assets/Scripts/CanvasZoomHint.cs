using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasZoomHint : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z) && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
