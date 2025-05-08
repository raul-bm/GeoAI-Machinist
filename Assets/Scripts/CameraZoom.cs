using System;
using Cinemachine;
using UnityEngine;

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

    bool zoomIn = true; // Zoom In -> true -- Zoom Out -> false

    [SerializeField] float sensitivity = 0.5f;
    bool disableControlZoom = false;

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
            if (zoomIn) ZoomOut();
            else ZoomIn();
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
        ChangeZoomSmooth(1.5f); // Change it to a better value for ZoomIn (or if is it made an option menu, change it to the user value setted)
        zoomIn = true;
    }

    public void ZoomOut()
    {
        ChangeZoomSmooth(5.7f); // Change it to a better value for ZoomOut (or if is it made an option menu, change it to the user value setted)
        zoomIn = false;
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
}
