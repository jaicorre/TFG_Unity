using System;
using System.Reflection;
using UnityEngine;
using Meta.XR;

public class ForceStartCamera : MonoBehaviour
{
    [Header("Assign this from the scene")]
    public PassthroughCameraAccess cameraAccess;

    [Header("Debug")]
    public bool spamLogs = false;

    private bool _triedStart;

    void Update()
    {
        if (cameraAccess == null) return;

        // 1) Ver si está capturando
        if (spamLogs)
            Debug.Log($"[ForceStartCamera] IsPlaying={cameraAccess.IsPlaying}");

        // 2) Si ya está capturando, no hagas nada más
        if (cameraAccess.IsPlaying) return;

        // 3) Intenta arrancar una vez
        if (!_triedStart)
        {
            _triedStart = true;
            TryStartCamera(cameraAccess);
        }
    }

    private void TryStartCamera(PassthroughCameraAccess cam)
    {
        // Lista de métodos típicos que cambian según versión del SDK
        string[] candidateMethods =
        {
            "Play",
            "Start",
            "StartCamera",
            "StartCapture",
            "Begin",
            "BeginCapture",
            "StartRecording",
            "StartStream"
        };

        var t = cam.GetType();

        foreach (var name in candidateMethods)
        {
            // Busca método sin parámetros
            MethodInfo mi = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (mi != null)
            {
                Debug.Log($"[ForceStartCamera] Calling {t.Name}.{name}()");
                mi.Invoke(cam, null);
                return;
            }

            // Busca método con 1 bool (algunas APIs hacen Play(true))
            mi = t.GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null);
            if (mi != null)
            {
                Debug.Log($"[ForceStartCamera] Calling {t.Name}.{name}(true)");
                mi.Invoke(cam, new object[] { true });
                return;
            }
        }

        Debug.LogWarning($"[ForceStartCamera] No start method found on {t.FullName}. " +
                         $"Open the PassthroughCameraAccess script and check what method starts capture in your version.");
    }
}