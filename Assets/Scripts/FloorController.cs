using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FloorController : MonoBehaviour
{
    static public int maxCount = 3; 
    static public int floorCount = 0; 
    public int roomNumber;
    public float minSize = 0.4f;
    public float maxSize = 2.5f;
    public Vector3 lastFreePosition = Vector3.zero;
    public Vector3 lastFreeScale = Vector3.zero;
    Quaternion lastFreeRotation = Quaternion.identity; 
    private BoxCollider col;
    private NotifierSystem notifierSystem;
    private BuilderManager builderManager;
    private bool justRevered = false; 
    public GameObject dungeonRoot;
    private RequirementsManager requirementsManager;


    void Awake()
    {
        col = GetComponent<BoxCollider>();
        notifierSystem = NotifierSystem.Instance; 
        builderManager = BuilderManager.Instance;
        requirementsManager = RequirementsManager.Instance;
    }

    // Start is called before the first frame update
    IEnumerator Start()
    {
        // check if the mode allows floor placement
        if (builderManager.currentMode == BuilderManager.BuilderMode.PlacingObject) {
            Debug.Log("Cannot place floor in current mode");
            Destroy(gameObject); 
        }
        
        floorCount++;
        // Disallow spawning more than 3 floors at a time
        if (floorCount > maxCount) {
            notifierSystem.ShowNotifier("Limit Reached", "At most 3 floors can be spawned", 3); 
            Destroy(gameObject);
        }

        // wait one frame for the physics engine to initialize
        yield return null;

        // disallow spawning that causes overlapping
        if (IsClose())
        {
            notifierSystem.ShowNotifier("Area Blocked", "Cannot spawn floor in this location", 3);
            Destroy(gameObject);
        }

        lastFreePosition = transform.position;
        lastFreeScale = transform.localScale;
        lastFreeRotation = transform.rotation;

        // add to parent
        dungeonRoot = DungeonManager.Instance.dungeonRoot;
        transform.SetParent(dungeonRoot.transform); 

        // register room number for this floor
        requirementsManager.RegisterFloor(this); 
    }



    void OnDestroy()
    {
        floorCount--;
        requirementsManager.UnregisterFloor(this);
    }

    void LateUpdate()
    {
        if (DungeonManager.Instance.isInTransformMode) {
            var interactable = GetComponent<XRGrabInteractable>();
            interactable.enabled = false; 
            return; 
        } else {
            var interactable = GetComponent<XRGrabInteractable>();
            interactable.enabled = true; 
        }

        if (builderManager.currentMode == BuilderManager.BuilderMode.PlacingObject) {
            var grab = GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = false;
        } else {
            var grab = GetComponent<XRGrabInteractable>();
            if (grab!= null)
                grab.enabled = true;
        }

        if (justRevered) {
            justRevered = false;
            return; 
        }

        if (ContainsObject()) {
            transform.localScale = lastFreeScale;
        } 
    
        Vector3 scale = transform.localScale;

        // Clamp X and Z to min/max size
        scale.x = Mathf.Clamp(scale.x, minSize, maxSize);
        scale.z = Mathf.Clamp(scale.z, minSize, maxSize);

        // Lock Y scale to original value (e.g., 1.0)
        scale.y = 1.0f;

        transform.localScale = scale;

        if (IsOverlapping())
        {
            notifierSystem.ShowNotifier("Overlapping Detected", "Block transform to prevent intersection", 3); 
            transform.position = lastFreePosition;
            transform.localScale = lastFreeScale;
            transform.rotation = lastFreeRotation; 
            justRevered = true;  // flag to prevent reverting transform again in the next frame (until the next physics update)
        }
        else
        {
            if (!IsClose()) {
                lastFreePosition = transform.position;
                lastFreeScale = transform.localScale;
                lastFreeRotation = transform.rotation;
            }
        }
    }

    public bool IsOverlapping() {
        if (col == null) {
            return false;
        }

        Collider[] all = FindObjectsOfType<BoxCollider>();

        foreach (var other in all)
        {
            if (other == col || !other.CompareTag("Floor"))
                continue;

            if (col.bounds.Intersects(other.bounds))
                return true;
        }

        return false;
    }

    public bool IsClose() {
        if (col == null) {
            return false;
        }

        float proximityThreshold = 0.02f; 

        Bounds myBounds = col.bounds;
        myBounds.Expand(myBounds.size.x * proximityThreshold); // grow the box outward

        foreach (var other in FindObjectsOfType<BoxCollider>())
        {
            if (other == col || !other.CompareTag("Floor")) continue;

            Bounds otherBounds = other.bounds;
            otherBounds.Expand(otherBounds.size.x * proximityThreshold);

            if (myBounds.Intersects(otherBounds))
                return true;
        }

        return false;
    }

    public bool ContainsObject()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Object"))
                return true;
        }
        return false;
    }

}
