using UnityEngine;
using System.Runtime.InteropServices;

public class FullscreenFocusFix : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void InitAutoCanvasFocus();
#endif

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        InitAutoCanvasFocus();
#endif
    }
}