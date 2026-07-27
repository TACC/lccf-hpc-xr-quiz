using UnityEngine;
using UnityEngine.EventSystems;
using zSpace.Core.EventSystems;
using zSpace.Core.Input;

public class ZDraggableItem :
    ZPointerInteractable,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("Answer Setup")]
    public string itemID;

    [Tooltip("Assign the ZDropTarget attached to the middle analogy object.")]
    public ZDropTarget analogyTarget;

    [Header("Effects")]
    public GameObject heatsinkSmoke;

    [Header("Drag Plane")]
    public Transform PlaneQuadTransform;

    [Header("Drag Settings")]
    public float minimumDragDistance = 0.01f;

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Rigidbody rb;
    private ZPointer capturedPointer;
    private ZDropTarget currentHoverTarget;

    private Vector3 pointerStartPosition;
    private Vector3 grabOffset;
    private Vector3 pivotToVisualCenter;

    private bool homePositionStored;
    private bool dragging;
    private bool movedDuringDrag;
    private bool answerHandled;
    private bool suppressNextClick;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        StoreHomePosition();
        PreparePhysics();
    }

    private void OnEnable()
    {
        StoreHomePosition();

        answerHandled = false;
        dragging = false;
        movedDuringDrag = false;
        suppressNextClick = false;
        currentHoverTarget = null;
        capturedPointer = null;

        EnableColliders();
        PreparePhysics();
    }

    private void OnDisable()
    {
        ReleasePointer();

        dragging = false;
        movedDuringDrag = false;
        currentHoverTarget = null;
    }

    private void StoreHomePosition()
    {
        if (homePositionStored)
        {
            return;
        }

        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        homePositionStored = true;
    }

    private void EnableColliders()
    {
        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
    }

    private void PreparePhysics()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb == null)
        {
            return;
        }

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public override ZPointer.DragPolicy GetDragPolicy(ZPointer pointer)
    {
        return ZPointer.DragPolicy.LockToCustomPlane;
    }

    public override Plane GetDragPlane(ZPointer pointer)
    {
        if (PlaneQuadTransform != null)
        {
            return new Plane(
                PlaneQuadTransform.forward,
                PlaneQuadTransform.position
            );
        }

        return base.GetDragPlane(pointer);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (answerHandled)
        {
            return;
        }

        if (suppressNextClick)
        {
            suppressNextClick = false;
            return;
        }

        CheckAnswer(analogyTarget);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ZPointerEventData pointerEventData =
            eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left ||
            answerHandled)
        {
            return;
        }

        ReleasePointer();

        capturedPointer = pointerEventData.Pointer;
        capturedPointer.CapturePointer(gameObject);

        Pose pointerPose =
            pointerEventData.Pointer.EndPointWorldPose;

        pointerStartPosition = pointerPose.position;

        Vector3 visualCenter = GetVisualCenter();

        grabOffset = visualCenter - pointerPose.position;
        pivotToVisualCenter = visualCenter - transform.position;

        dragging = true;
        movedDuringDrag = false;
        currentHoverTarget = null;

        PreparePhysics();
    }

    public void OnDrag(PointerEventData eventData)
    {
        ZPointerEventData pointerEventData =
            eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left ||
            answerHandled ||
            !dragging)
        {
            return;
        }

        Pose pointerPose =
            pointerEventData.Pointer.EndPointWorldPose;

        float distanceMoved = Vector3.Distance(
            pointerStartPosition,
            pointerPose.position
        );

        if (!movedDuringDrag &&
            distanceMoved < minimumDragDistance)
        {
            return;
        }

        movedDuringDrag = true;

        Vector3 desiredVisualCenter =
            pointerPose.position + grabOffset;

        transform.position =
            desiredVisualCenter - pivotToVisualCenter;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ZPointerEventData pointerEventData =
            eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left ||
            answerHandled)
        {
            ReleasePointer();
            return;
        }

        ReleasePointer();

        dragging = false;
        PreparePhysics();

        if (!movedDuringDrag)
        {
            suppressNextClick = true;
            CheckAnswer(analogyTarget);
            return;
        }

        suppressNextClick = true;

        if (currentHoverTarget != null)
        {
            CheckAnswer(currentHoverTarget);
        }
        else
        {
            ResetPosition();
        }
    }

    private Vector3 GetVisualCenter()
    {
        Renderer[] renderers =
            GetComponentsInChildren<Renderer>(true);

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds.center;
        }

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
            }

            return bounds.center;
        }

        return transform.position;
    }

    private void CheckAnswer(ZDropTarget target)
    {
        if (answerHandled)
        {
            return;
        }

        if (target == null)
        {
            Debug.LogWarning(
                gameObject.name +
                " does not have an analogy target assigned."
            );

            ResetPosition();
            return;
        }

        if (target.expectedItemID == itemID)
        {
            answerHandled = true;

            target.TriggerSuccess();

            if (heatsinkSmoke != null)
            {
                heatsinkSmoke.SetActive(false);
            }

            ReleasePointer();
            gameObject.SetActive(false);
        }
        else
        {
            target.TriggerFailure();
            ResetPosition();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ZDropTarget target =
            other.GetComponent<ZDropTarget>();

        if (target == null)
        {
            target = other.GetComponentInParent<ZDropTarget>();
        }

        if (target != null)
        {
            currentHoverTarget = target;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ZDropTarget target =
            other.GetComponent<ZDropTarget>();

        if (target == null)
        {
            target = other.GetComponentInParent<ZDropTarget>();
        }

        if (target != null)
        {
            currentHoverTarget = target;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ZDropTarget target =
            other.GetComponent<ZDropTarget>();

        if (target == null)
        {
            target = other.GetComponentInParent<ZDropTarget>();
        }

        if (target != null &&
            currentHoverTarget == target)
        {
            currentHoverTarget = null;
        }
    }

    private void ReleasePointer()
    {
        if (capturedPointer == null)
        {
            return;
        }

        capturedPointer.CapturePointer(null);
        capturedPointer = null;
    }

    public void ResetPosition()
    {
        ReleasePointer();

        transform.SetParent(originalParent, false);
        transform.localPosition = originalLocalPosition;
        transform.localRotation = originalLocalRotation;

        dragging = false;
        movedDuringDrag = false;
        currentHoverTarget = null;

        PreparePhysics();
    }

    public void ResetForNewPhase()
    {
        answerHandled = false;
        suppressNextClick = false;

        gameObject.SetActive(true);
        enabled = true;

        EnableColliders();
        ResetPosition();
    }

    public void SnapToTarget(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return;
        }

        ReleasePointer();

        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;

        PreparePhysics();

        enabled = false;
    }
}