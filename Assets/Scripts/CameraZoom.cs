using System;
using Cinemachine;
using UnityEngine;

public enum ZoomState
{
    zoomIn,
    zoomMiddle,
    zoomOut
}

public class CameraZoom : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] float MaxOrthoSize = 8f;
    [SerializeField] float MinOrthoSize = 0.5f;

    // float orthoSize;
    float targetSize;
    readonly float zoomSpeed = 3.0f; // Speed of zoom
    readonly float deltaOrthoSize = 0.05f;
    bool IsZooming = false;

    public ZoomState zoomState = ZoomState.zoomIn;
    ZoomState lastZoomState = ZoomState.zoomOut;

    [SerializeField] float sensitivity = 0.5f;
    bool disableControlZoom = false;

    public float minZoom { get; private set; }
    public float maxZoom { get; private set; }
    public float middleZoom { get; private set; }

    private void Awake()
    {
        ValuesChanged(PlayerPrefs.GetFloat("minZoomValue"), PlayerPrefs.GetFloat("maxZoomValue"));
        lastZoomState = ZoomState.zoomOut;
        ZoomMiddle();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        /*if (disableControlZoom == false)
        {
            // Debug.Log("Manual control of the camera zoom");
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetSize = Input.GetAxis("Mouse ScrollWheel") * sensitivity;
                targetSize = virtualCamera.m_Lens.OrthographicSize - targetSize;
                targetSize = Mathf.Clamp(targetSize, MinOrthoSize, MaxOrthoSize);

                virtualCamera.m_Lens.OrthographicSize = targetSize;
                // Debug.Log("virtual camera follow " + virtualCamera.Follow.tag);
                // Debug.Log("virtual camera follow to position " + virtualCamera.Follow.transform.position);
            }
        }*/

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(zoomState == ZoomState.zoomIn || zoomState == ZoomState.zoomOut)
            {
                lastZoomState = zoomState;
                ZoomMiddle();
            }
            else
            {
                if(lastZoomState == ZoomState.zoomIn)
                {
                    ZoomOut();
                }
                else if(lastZoomState == ZoomState.zoomOut)
                {
                    ZoomIn();
                }
            }
        }

        if (IsZooming)
        {
            // Debug.Log($"IsZooming: OrthographicSize = {virtualCamera.m_Lens.OrthographicSize}, Target = {targetSize}");

            if (IsApproximate(targetSize, virtualCamera.m_Lens.OrthographicSize))
            {
                // Debug.Log("virtual camera follow " + virtualCamera.Follow.tag);
                // Debug.Log("virtual camera follow to position " + virtualCamera.Follow.transform.position);

                IsZooming = false;
            }
            else
            {
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(virtualCamera.m_Lens.OrthographicSize, targetSize, Time.deltaTime * zoomSpeed);
                // Debug.Log($"After: OrthographicSize = {virtualCamera.m_Lens.OrthographicSize}, Target = {targetSize}");
            }
        }
    }

    public void ZoomIn()
    {
        ChangeZoomSmooth(minZoom);
        lastZoomState = ZoomState.zoomMiddle;
        zoomState = ZoomState.zoomIn;
    }

    public void ZoomMiddle()
    {
        ChangeZoomSmooth(middleZoom);
        zoomState = ZoomState.zoomMiddle;
    }

    public void ZoomOut()
    {
        ChangeZoomSmooth(maxZoom);
        lastZoomState = ZoomState.zoomMiddle;
        zoomState = ZoomState.zoomOut;
    }

    public void Block()
    {
        disableControlZoom = true;
    }

    public void Release()
    {
        disableControlZoom = false;
    }

    public void ChangeZoom(float orthoSize)
    {
        // Debug.Log("change zoom suddenly to " + orthoSize);

        virtualCamera.m_Lens.OrthographicSize = orthoSize;
    }

    private bool IsApproximate(float valueA, float valueB)
    {
        return Math.Abs(valueA - valueB) < deltaOrthoSize;
    }

    public void ChangeZoomSmooth(float orthoSize)
    {
        // Debug.Log("change zoom smooth to " + orthoSize);
        // Debug.Log($"Before: OrthographicSize = {virtualCamera.m_Lens.OrthographicSize}, Target = {orthoSize}");
        targetSize = Mathf.Clamp(orthoSize, MinOrthoSize, MaxOrthoSize);
        IsZooming = true;
    }

    public void ChangeZoomTarget(GameObject target)
    {
        if (virtualCamera == null)
        {
            Debug.LogError("virtualCamera is not assigned!");
            return;
        }

        virtualCamera.Follow = target.transform;
    }

    public void ValuesChanged(float minValue, float maxValue)
    {
        minZoom = minValue;
        maxZoom = maxValue;

        middleZoom = Mathf.Round((maxZoom + minZoom) / 2 * 10.0f) * 0.1f;

        // Refresh the zoom with the new changes
        if (zoomState == ZoomState.zoomIn) ZoomIn();
        else if (zoomState == ZoomState.zoomMiddle) ZoomMiddle();
        else ZoomOut();
    }
}
