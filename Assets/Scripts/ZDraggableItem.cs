using UnityEngine;
using UnityEngine.EventSystems;
using zSpace.Core.EventSystems;
using zSpace.Core.Input;


// Makes an object draggable with the zSpace stylus (for hardware pieces)
//
// Lets the user grab and drag the object, keeps the object locked on drag 
// plane, detects when the object is hovering over a drop target, checks if 
// correct target, triggers success/failure, and turns orbiting off while 
// dragging and back on if the answer is wrong
public class ZDraggableItem :
    ZPointerInteractable, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Answer Setup")]

    // Used to check for expected itemID
    public string itemID;


    [Header("Effects")]

    public GameObject heatsinkSmoke;


    [Header("Drag Plane")]

    // Plane that controls where the object can move while dragging
    public Transform PlaneQuadTransform;

    // Object returns to this position if answered wrong
    private Vector3 originalPosition;

    private Quaternion originalRotation;


    // This stores the offset between the stylus pointer and the object
    // Helps the object keep the same position from the stylus as when grabbed
    private Vector3 initialGrabOffset = Vector3.zero;

    private bool originalIsKinematic = false;


    // Stores the drop target the object is currently touching
    // Checked by OnEndDrag
    private ZDropTarget currentHoverTarget;

    // This stores the OrbitAroundTarget script, disabled when object grabbed
    private OrbitAroundTarget orbitScript;


    // Runs when this object first becomes active in the scene
    void Start()
    {
        // Where the item returns if the user drops it incorrectly
        originalPosition = transform.position;

        originalRotation = transform.rotation;

        // Disable OrbitAroundTarget script during dragging
        orbitScript = GetComponent<OrbitAroundTarget>();
    }


    // Tells zSpace how the object should be dragged
    public override ZPointer.DragPolicy GetDragPolicy(ZPointer pointer)
    {
        return ZPointer.DragPolicy.LockToCustomPlane;
    }


    // Tells zSpace which plane to use when dragging
    // Math that defines a flat surface
    public override Plane GetDragPlane(ZPointer pointer)
    {
        if (PlaneQuadTransform != null)
        {
            // PlaneQuadTransform.forward controls the direction the plane faces
            // PlaneQuadTransform.position controls where the plane is located
            return new Plane(
                PlaneQuadTransform.forward,
                PlaneQuadTransform.position);
        }


        // No Plane Quad was assigned, use default drag plane from ZPointerInteractable
        return base.GetDragPlane(pointer);
    }


    // Runs when the user first starts dragging the object
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Convert regular Unity pointer event data into zSpace pointer event data
        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Turn off orbit script while dragging
        if (orbitScript != null)
        {
            orbitScript.enabled = false;
        }


        // Get the world position and rotation of the stylus endpoint
        Pose pose = pointerEventData.Pointer.EndPointWorldPose;


        // Calculate the offset between the object and the stylus grab point
        initialGrabOffset =
            Quaternion.Inverse(transform.rotation) *
            (transform.position - pose.position);

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            originalIsKinematic = rb.isKinematic;

            // The script controls movement when kinematic is on 
            rb.isKinematic = true;
        }

        // Stylus stays connected to this object while dragging
        pointerEventData.Pointer.CapturePointer(gameObject);
    }


    // Runs every frame while the user is dragging the object
    public void OnDrag(PointerEventData eventData)
    {
        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        Pose pose = pointerEventData.Pointer.EndPointWorldPose;


        // Move the object to follow the stylus
        transform.position =
            pose.position +
            (transform.rotation * initialGrabOffset);
    }


    // Runs when the user lets go of the object
    public void OnEndDrag(PointerEventData eventData)
    {
        ZPointerEventData pointerEventData = eventData as ZPointerEventData;

        if (pointerEventData == null ||
            pointerEventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // Release the captured pointer
        pointerEventData.Pointer.CapturePointer(null);

        Rigidbody rb = GetComponent<Rigidbody>();

        // Restore original kinematic setting
        if (rb != null)
        {
            rb.isKinematic = originalIsKinematic;
        }

        // Check if the object is currently touching a drop target
        // currentHoverTarget is set by OnTriggerEnter
        if (currentHoverTarget != null)
        {
            if (currentHoverTarget.expectedItemID == itemID)
            {
                currentHoverTarget.TriggerSuccess();

                // Turn off smoke for heatsink
                if (heatsinkSmoke != null)
                {
                    heatsinkSmoke.SetActive(false);
                }

                // Hide this draggable item
                gameObject.SetActive(false);
            }
            else
            {
                currentHoverTarget.TriggerFailure();

                ResetPosition();
            }
        }
        else
        {
            // Object not dropped on any target, return to original position
            ResetPosition();
        }
    }


    // Runs when this object's trigger collider enters another trigger collider
    // How the draggable item knows it is touching a drop zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we entered has a ZDropTarget script
        ZDropTarget zone = other.GetComponent<ZDropTarget>();


        // If yes, store it as the current hover target for OnEndDrag to later
        // check if this is the correct target
        if (zone != null)
        {
            currentHoverTarget = zone;
        }
    }


    // This runs when this object's trigger collider exits another trigger collider
    private void OnTriggerExit(Collider other)
    {
        ZDropTarget zone = other.GetComponent<ZDropTarget>();

        // Clear currentHoverTarget if the zone left is the same one currently stored
        if (zone != null && currentHoverTarget == zone)
        {
            currentHoverTarget = null;
        }
    }


    // Snaps the object to a target transform (the snap point on the motherboard)
    public void SnapToTarget(Transform targetTransform)
    {
        transform.position = targetTransform.position;

        transform.rotation = targetTransform.rotation;

        if (orbitScript != null)
        {
            orbitScript.enabled = false;
        }

        // Remove ZDraggableItem so the object can no longer be dragged
        Destroy(this);
    }


    // Sends the object back to its original starting position
    // For when dropped on wrong target or no target
    public void ResetPosition()
    {
        transform.position = originalPosition;

        transform.rotation = originalRotation;

        // Turn orbiting back on
        if (orbitScript != null)
        {
            orbitScript.enabled = true;
        }
    }
}