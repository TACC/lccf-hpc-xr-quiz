using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using zSpace.Core.EventSystems;
using zSpace.Core.Input;

// Lets a hardware part be dragged with the zSpace stylus
public class PlacementDraggableItem :
    ZPointerInteractable, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Placement Setup")]

    // If itemID matches expectedItemID on drop target, the placement is correct
    public string itemID;

    // Used for showing the glow after a wrong attempt
    // Floating the item to the correct spot after the second wrong attempt
    public PlacementDropTarget correctTarget;


    [Header("Drag Plane")]

    // This is the plane the object moves along while being dragged
    public Transform PlaneQuadTransform;

    [Header("Depth / Rotation Lock")]
    public float frontOffset = -0.15f;
    public bool lockRotation = true;

    private float lockedWorldZ;
    private Quaternion lockedWorldRotation;


    [Header("Auto Snap")]

    // How long it takes for the object to float to the correct spot
    public float autoSnapDuration = 1.5f;


    [Header("Reset")]

    // How long it takes for the object to move back to starting position
    public float resetDuration = 0.4f;

    private Vector3 startLocalPosition;

    private Quaternion startLocalRotation;

    private bool startingPositionSaved = false;


    // Stores the offset between where the stylus grabs and the object's center
    private Vector3 initialGrabOffset = Vector3.zero;

    // This remembers whether the Rigidbody was originally kinematic
    private bool originalIsKinematic = false;


    // This stores the drop target the item is currently touching/hovering over
    private PlacementDropTarget currentHoverTarget;

    private int wrongAttempts = 0;

    // Once the item is correctly placed this becomes true
    private bool isLocked = false;

    // This prevents the user from dragging it during an automatic movement
    private bool isMoving = false;

    private void SaveStartingPosition()
    {
        if (startingPositionSaved)
        {
            return;
        }

        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        lockedWorldZ = transform.position.z + frontOffset;
        lockedWorldRotation = transform.rotation;

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            originalIsKinematic = rb.isKinematic;
        }

        startingPositionSaved = true;
    }

    void Start()
    {
        SaveStartingPosition();

        Rigidbody rb = GetComponent<Rigidbody>();

        // If the object has a Rigidbody, make sure physics does not pull it down
        if (rb != null)
        {
            // Turn off gravity so it does not fall
            rb.useGravity = false;

            // Make it kinematic so the script controls movement, not Unity physics
            rb.isKinematic = true;
        }
    }


    public override ZPointer.DragPolicy GetDragPolicy(ZPointer pointer)
    {
        // Tells zSpace how the object should move while being dragged
        // Prevent movement in 3D space
        return ZPointer.DragPolicy.LockToCustomPlane;
    }


    public override Plane GetDragPlane(ZPointer pointer)
    {
        // If a PlaneQuadTransform was assigned in the Inspector
        // use it to create the drag plane
        if (PlaneQuadTransform != null)
        {
            return new Plane(
                PlaneQuadTransform.forward,
                PlaneQuadTransform.position
            );
        }

        return base.GetDragPlane(pointer);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        // Runs when the user first starts dragging the item

        if (isLocked || isMoving)
        {
            return;
        }

        // Convert the normal Unity pointer data into zSpace pointer data
        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Get the stylus endpoint position and rotation in world space
        Pose pose = pointerEventData.Pointer.EndPointWorldPose;

        // Calculate the offset between the stylus grab point and the object
        // Object keep the same grabbed spot while moving
        initialGrabOffset =
            Quaternion.Inverse(transform.rotation) *
            (transform.position - pose.position);

        // If the object has a Rigidbody pause physics while dragging
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Remember the Rigidbody's original kinematic setting
            originalIsKinematic = rb.isKinematic;

            // Force it to be kinematic while dragging
            rb.isKinematic = true;

            // Keep gravity off
            rb.useGravity = false;
        }

        pointerEventData.Pointer.CapturePointer(gameObject);
    }


    public void OnDrag(PointerEventData eventData)
    {
        // Runs every frame while the item is being dragged

        if (isLocked || isMoving)
        {
            return;
        }

        // Convert the event data into zSpace pointer data
        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Get the current stylus endpoint pose
        Pose pose = pointerEventData.Pointer.EndPointWorldPose;

        Vector3 newPosition =
            pose.position +
            (lockedWorldRotation * initialGrabOffset);

        // Keep the object in front of the motherboard
        newPosition.z = lockedWorldZ;

        transform.position = newPosition;

        // Stop the object from rotating while dragging
        if (lockRotation)
        {
            transform.rotation = lockedWorldRotation;
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        // Runs when the user lets go of the dragged item

        if (isLocked || isMoving)
        {
            return;
        }

        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Release pointer capture
        pointerEventData.Pointer.CapturePointer(null);

        // Restore Rigidbody settings
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Put the Rigidbody back to original kinematic setting
            rb.isKinematic = originalIsKinematic;

            rb.useGravity = false;
        }

        // Check whether the item is currently touching a drop target
        // and expected item ID
        if (currentHoverTarget != null &&
            currentHoverTarget.expectedItemID == itemID)
        {
            // Snap the object into the target's snap point
            SnapToTarget(currentHoverTarget);
        }
        else
        {
            HandleWrongPlacement();
        }
    }


    private void HandleWrongPlacement()
    {
        wrongAttempts++;

        // Show the glow on the correct target
        if (correctTarget != null)
        {
            correctTarget.ShowGlow();
        }

        // Move item to original spot to try again
        if (wrongAttempts == 1)
        {
            StartCoroutine(MoveBackToStart());
        }
        else
        {
            // Automatically float the item to the correct target
            if (correctTarget != null)
            {
                StartCoroutine(FloatToCorrectSpot());
            }
            else
            {
                // If no correct target, just move back to start
                StartCoroutine(MoveBackToStart());
            }
        }
    }


    private IEnumerator MoveBackToStart()
    {
        // Moves the item back to its starting spot

        // Prevent dragging while moving back
        isMoving = true;

        Vector3 beginLocalPosition = transform.localPosition;
        Quaternion beginLocalRotation = transform.localRotation;

        // Tracks time elapsed
        float elapsed = 0f;

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / resetDuration;

            // SmoothStep makes the motion start and end smoothly
            t = Mathf.SmoothStep(0f, 1f, t);

            // Move from the current local position back to the saved start position
            transform.localPosition = Vector3.Lerp(
                beginLocalPosition,
                startLocalPosition,
                t
            );

            // Rotate from the current local rotation back to the saved start rotation
            transform.localRotation = Quaternion.Slerp(
                beginLocalRotation,
                startLocalRotation,
                t
            );

            // Wait until the next frame before continuing
            yield return null;
        }

        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;

        currentHoverTarget = null;

        isMoving = false;
    }


    private IEnumerator FloatToCorrectSpot()
    {
        // Moves the item to the correct target

        isMoving = true;

        // Lock the object so it cannot be dragged again
        isLocked = true;

        Vector3 beginPosition = transform.position;
        Quaternion beginRotation = transform.rotation;

        // Get the final position and rotation from the correct target's snap point
        Vector3 endPosition = correctTarget.snapPoint.position;
        endPosition.z = lockedWorldZ;
        Quaternion endRotation = lockedWorldRotation;

        float elapsed = 0f;

        // Animate until elapsed reaches autoSnapDuration
        while (elapsed < autoSnapDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / autoSnapDuration;

            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(beginPosition, endPosition, t);

            transform.rotation = Quaternion.Slerp(beginRotation, endRotation, t);

            yield return null;
        }

        transform.position = endPosition;
        transform.rotation = endRotation;

        // Hides the glow and SuperCityManager moves to the next part
        correctTarget.TriggerSuccess();

        isMoving = false;
    }


    private void SnapToTarget(PlacementDropTarget target)
    {
        // Runs when the user drops the item on the correct target

        isLocked = true;

        Vector3 snapPosition = target.snapPoint.position;

        // Keep object in front of the motherboard
        snapPosition.z = lockedWorldZ;

        transform.position = snapPosition;

        // Keep original rotation instead of using the snap point rotation
        if (lockRotation)
        {
            transform.rotation = lockedWorldRotation;
        }

        target.TriggerSuccess();
    }


    private void OnTriggerEnter(Collider other)
    {
        // This runs when this item's trigger collider touches another collider

        // Check if the other object has a PlacementDropTarget script
        PlacementDropTarget target = other.GetComponent<PlacementDropTarget>();


        // Save as draggable item is touching a target
        if (target != null)
        {
            currentHoverTarget = target;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        // This runs when this item's trigger collider stops touching another collider

        PlacementDropTarget target = other.GetComponent<PlacementDropTarget>();

        // If the target we exited is the same one we were hovering over
        // clear currentHoverTarget
        if (target != null && currentHoverTarget == target)
        {
            currentHoverTarget = null;
        }
    }

    public void ResetForReplay()
    {

        SaveStartingPosition();
        StopAllCoroutines();

        isLocked = false;
        isMoving = false;
        wrongAttempts = 0;
        currentHoverTarget = null;
        initialGrabOffset = Vector3.zero;

        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;

        lockedWorldZ = transform.position.z + frontOffset;
        lockedWorldRotation = transform.rotation;

        enabled = true;
        gameObject.SetActive(true);

        Collider[] colliders =
            GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            originalIsKinematic = true;
        }
    }
}