using UnityEngine;
using PassthroughCameraSamples.MultiObjectDetection;

public class PersonAnchorFromDetections : MonoBehaviour
{
    [Header("References")]
    public Transform personAnchor;                 // tu GameObject PersonAnchor
    public SentisInferenceUiManager uiInference;   // el de la escena sample (m_uiInference)

    [Header("Filtering")]
    [Tooltip("En COCO normalmente 0 = person. Si tu modelo usa otro id, cámbialo.")]
    public int personClassId = 0;

    [Header("Smoothing")]
    public float followLerp = 12f;

    [Header("Behaviour")]
    public float lostHoldSeconds = 0.75f;

    private float _lastSeenTime;
    private Vector3 _lastTarget;

    void Update()
    {
        if (!personAnchor || !uiInference) return;

        // Buscar la mejor detección "person"
        SentisInferenceUiManager.BoundingBoxData best = null;

        foreach (var box in uiInference.m_boxDrawn)
        {
            if (box == null) continue;
            if (box.ClassId != personClassId) continue;

            // Elegimos el más grande (aprox) por área del rect transform
            var size = box.BoxRectTransform.sizeDelta;
            float area = Mathf.Abs(size.x * size.y);

            if (best == null)
            {
                best = box;
            }
            else
            {
                var bestSize = best.BoxRectTransform.sizeDelta;
                float bestArea = Mathf.Abs(bestSize.x * bestSize.y);
                if (area > bestArea) best = box;
            }
        }

        if (best != null)
        {
            _lastSeenTime = Time.time;
            _lastTarget = best.BoxRectTransform.position; // ya está en WORLD SPACE
        }

        // Si no se ve persona, mantenemos un rato la última posición
        if (Time.time - _lastSeenTime > lostHoldSeconds)
            return;

        float t = 1f - Mathf.Exp(-followLerp * Time.deltaTime);
        personAnchor.position = Vector3.Lerp(personAnchor.position, _lastTarget, t);
    }
}
