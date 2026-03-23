using UnityEngine;

public class ControllerDistanceGrabPanel : MonoBehaviour
{
    [Header("References")]
    public Transform head;
    public Transform rightController;
    public Collider panelCollider;
    public PanelFollowHeadMeta followScript;

    [Header("Grab settings")]
    public float minDistance = 0.35f;
    public float maxDistance = 3.0f;
    public float zoomSpeed = 1.2f;
    public float moveSmoothing = 20f;
    public float rotateSmoothing = 20f;

    [Header("Input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;
    public OVRInput.RawButton grabButton = OVRInput.RawButton.RHandTrigger;
    public OVRInput.RawAxis2D thumbstickAxis = OVRInput.RawAxis2D.RThumbstick;

    private bool _isGrabbed;
    private float _grabDistance;

    private void Reset()
    {
        panelCollider = GetComponent<Collider>();
        followScript = GetComponent<PanelFollowHeadMeta>();
    }

    private void Update()
    {
        if (rightController == null || panelCollider == null) return;

        if (!_isGrabbed)
        {
            if (OVRInput.GetDown(grabButton, controller) && IsPointingAtPanel())
            {
                StartGrab();
            }
        }
        else
        {
            UpdateGrab();

            if (OVRInput.GetUp(grabButton, controller))
            {
                EndGrab();
            }
        }
    }

    private bool IsPointingAtPanel()
    {
        Ray ray = new Ray(rightController.position, rightController.forward);
        return panelCollider.Raycast(ray, out _, 10f);
    }

    private void StartGrab()
    {
        _isGrabbed = true;

        if (followScript != null)
            followScript.StopFollowingHead();

        Vector3 fromControllerToPanel = transform.position - rightController.position;
        _grabDistance = Vector3.Dot(fromControllerToPanel, rightController.forward);

        if (_grabDistance < minDistance)
            _grabDistance = Mathf.Clamp(Vector3.Distance(rightController.position, transform.position), minDistance, maxDistance);
    }

    private void UpdateGrab()
    {
        Vector2 stick = OVRInput.Get(thumbstickAxis, controller);
        _grabDistance += stick.y * zoomSpeed * Time.deltaTime;
        _grabDistance = Mathf.Clamp(_grabDistance, minDistance, maxDistance);

        Vector3 targetPos = rightController.position + rightController.forward * _grabDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            1f - Mathf.Exp(-moveSmoothing * Time.deltaTime)
        );

        if (head != null)
        {
            Vector3 toHead = head.position - transform.position;
            toHead.y = 0f;

            if (toHead.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    1f - Mathf.Exp(-rotateSmoothing * Time.deltaTime)
                );
            }
        }
    }

    private void EndGrab()
    {
        _isGrabbed = false;

        if (followScript != null)
            followScript.ResumeFollowingFromCurrentPosition();
    }
}