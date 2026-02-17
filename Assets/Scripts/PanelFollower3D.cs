using UnityEngine;

public class PanelFollower3D : MonoBehaviour
{
    [Header("References")]
    public Transform anchor;   // PersonAnchor
    public Transform head;     // CenterEyeAnchor (camara)

    [Header("Offsets (meters)")]
    public float sideOffset = 0.35f;
    public float upOffset = 0.10f;
    public float forwardOffset = 0.00f;

    [Header("Smoothing")]
    public float positionLerp = 12f;
    public float rotationLerp = 12f;

    void LateUpdate()
    {
        if (!anchor || !head) return;

        // Direcciones basadas en la camara (para que siempre quede "a un lado" del usuario)
        Vector3 right = Vector3.ProjectOnPlane(head.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

        // Posicion objetivo
        Vector3 targetPos =
            anchor.position +
            right * sideOffset +
            Vector3.up * upOffset +
            forward * forwardOffset;

        // Rotacion: mira hacia la camara pero solo en Y
        Vector3 toHead = head.position - targetPos;
        toHead.y = 0f;

        if (toHead.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(-toHead.normalized, Vector3.up);


        // Suavizado
        float posT = 1f - Mathf.Exp(-positionLerp * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationLerp * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPos, posT);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotT);
    }
}
