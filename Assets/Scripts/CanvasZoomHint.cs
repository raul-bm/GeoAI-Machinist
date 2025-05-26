using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasZoomHint : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q) && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
