using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomInRegion : MonoBehaviour
{
    /*public CameraZoom cameraZoom;

    public float zoom = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cameraZoom.ChangeZoomSmooth(zoom);
        }
    }*/

    public CameraZoom cameraZoom;
    public GameObject canvasZoomHint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && cameraZoom.zoomState != ZoomState.zoomIn)
        {
            canvasZoomHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            canvasZoomHint.SetActive(false);
        }
    }
}
