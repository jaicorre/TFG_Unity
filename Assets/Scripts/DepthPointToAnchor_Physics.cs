using UnityEngine;

public class DepthPointToAnchor_Physics : MonoBehaviour
{
    [Header("References")]
    public Transform anchorToMove;  // PersonAnchor
    public Transform head;          // CenterEyeAnchor (camara del rig)

    [Header("Viewport point (0..1)")]
    public Vector2 viewportPoint = new Vector2(0.5f, 0.5f);

    [Header("Raycast settings")]
    public float fallbackDistance = 2.0f;
    public float maxRayDistance = 20f;
    public LayerMask hitMask = ~0;

    // ✅ Nuevo: permite que otro script actualice el punto de mira/persona
    public void SetViewportPoint(Vector2 vp01)
    {
        viewportPoint = new Vector2(Mathf.Clamp01(vp01.x), Mathf.Clamp01(vp01.y));
    }

    void Update()
    {
        if (!anchorToMove || !head) return;

        Vector2 vp = viewportPoint;
        vp.x = Mathf.Clamp01(vp.x);
        vp.y = Mathf.Clamp01(vp.y);

        float x = (vp.x - 0.5f) * 2f;
        float y = (vp.y - 0.5f) * 2f;

        Vector3 dir = (head.forward + head.right * x + head.up * y).normalized;
        Ray ray = new Ray(head.position, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, hitMask))
        {
            anchorToMove.position = hit.point;
        }
        else
        {
            anchorToMove.position = ray.origin + ray.direction * fallbackDistance;
        }
    }
}
