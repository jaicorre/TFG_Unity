using UnityEngine;

public class PanelFollowHeadMeta : MonoBehaviour
{
    [Header("References")]
    public Transform head;

    [Header("Follow")]
    public float distance = 0.8f;
    public float heightOffset = 0.0f;
    public float positionLerp = 8f;
    public float rotationLerp = 8f;
    public bool followHead = true;

    private Vector3 _localOffsetFromHead;

    private void Awake()
    {
        // Offset inicial seguro: delante de la cabeza
        _localOffsetFromHead = new Vector3(0f, heightOffset, distance);
    }

    private void Update()
    {
        if (head == null || !followHead) return;

        Vector3 targetPos = head.TransformPoint(_localOffsetFromHead);

        Vector3 toHead = head.position - targetPos;
        toHead.y = 0f;
        if (toHead.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(-toHead.normalized, Vector3.up);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-positionLerp * Time.deltaTime)
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            1f - Mathf.Exp(-rotationLerp * Time.deltaTime)
        );
    }

    public void StopFollowingHead()
    {
        followHead = false;
    }

    public void ResumeFollowingFromCurrentPosition()
    {
        if (head == null) return;

        _localOffsetFromHead = head.InverseTransformPoint(transform.position);
        followHead = true;
    }

    public void ResetInFrontOfHead()
    {
        if (head == null) return;

        _localOffsetFromHead = new Vector3(0f, heightOffset, distance);
        transform.position = head.TransformPoint(_localOffsetFromHead);

        Vector3 toHead = head.position - transform.position;
        toHead.y = 0f;
        if (toHead.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
    }
}