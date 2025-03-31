using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ObjectController : MonoBehaviour
{
    public float yOffset = 0.035f; // small offset to ensure it sits just above floor
    public Vector3 originalScale; 
    public Vector3 lastValidPosition;
    public Quaternion lastValidRotation; 
    private bool justReverted = false; 
    private BoxCollider col; 
    private BoxCollider floorCol; 
    private BuilderManager builderManager;
    private RequirementsManager requirementsManager;
    private bool initialized = false; 
    private float initialYPosition;
    public string objectType = ""; // set in inspector for this prefab 
    public GameObject floor; // current living floor of the object
    public string objectID = null; 
    private StackManager stackManager; 
    public bool isPerformingUndoRedo = false; 
    public string objectName;
    private PrefabList prefabList; 

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        requirementsManager = RequirementsManager.Instance;
        builderManager = BuilderManager.Instance;
        stackManager = StackManager.Instance;
        prefabList = PrefabList.Instance;
        objectID = Guid.NewGuid().ToString();
    }

    IEnumerator Start()
    {
        // check if current mode is valid
        if (builderManager.currentMode == BuilderManager.BuilderMode.PlacingFloor) {
            Debug.Log("Current mode does not support placement of objects"); 
            Destroy(gameObject);
        }

        if (!ValidatePlacement(out floor))
        {
            Debug.LogWarning("Invalid placement. Destroying object.");
            Destroy(gameObject);
        }

        yield return null;

        // check if the object is overlapping with existing ones
        if (IsOverlapping()) {
            Debug.Log("please place farther away from other objects"); 
            Destroy(gameObject);
        }

        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;

        // maintain scale
        Vector3 floorScale = floor.transform.localScale; 
        originalScale = new Vector3(
            1f / floorScale.x,
            1f / floorScale.y,
            1f / floorScale.z
        );
        transform.localScale = originalScale; 

        // Snap on top of floor
        Vector3 floorTop = floor.GetComponent<BoxCollider>().bounds.max;
        transform.position = new Vector3(transform.position.x, floorTop.y + yOffset, transform.position.z);
        floorCol = floor.GetComponent<BoxCollider>();

        // Parent to floor so it moves with it
        transform.SetParent(floor.transform);

        // register requirements for current room 
        FloorController floorController = floor.GetComponent<FloorController>(); 
        int roomNumber = floorController.roomNumber;
        requirementsManager.RegisterObject(roomNumber, objectType); 

        initialYPosition = transform.position.y; 

        // register the creation action for this object if it is not an undo
        if (!isPerformingUndoRedo) {
            GameObject prefab = prefabList.GetPrefab(objectName);
            stackManager.RegisterCreateObject(prefab, transform.position, transform.rotation, transform.localScale, floor.transform, gameObject, objectID);
        }
    
        initialized = true;
    }

    void OnDestroy()
    {
        // deregister current object from the room 
        FloorController floorController = floor.GetComponent<FloorController>(); 
        int roomNumber = floorController.roomNumber;
        requirementsManager.DeregisterObject(roomNumber, objectType);

        // register the deletion action for this object if it is not an undo
        if (!isPerformingUndoRedo && !IsClose() && !IsOverlapping()) {
            GameObject prefab = prefabList.GetPrefab(objectName);
            stackManager.RegisterDeleteObject(prefab, transform.position, transform.rotation, transform.localScale, floor.transform, gameObject, objectID);
        }
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
        
        if (!initialized) {
            return; 
        }

        if (justReverted) {
            justReverted = false;
            return;
        }

        // always lock the scale
        transform.localScale = originalScale;

        // check if the object is still on top of the floor 
        if (!IsOnFloor())
        {
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
            justReverted = true; 
        } else if (IsOverlapping()) {
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
            justReverted = true; 
        } else if (!IsClose()) {
            lastValidPosition = transform.position;
            lastValidRotation = transform.rotation;
        }
    }

    bool ValidatePlacement(out GameObject floorHit)
    {
        floorHit = null;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (hit.collider.CompareTag("Floor"))
            {
                floorHit = hit.collider.gameObject;
                return true;
            }
        }

        return false;
    }

    bool IsOnFloor() {
        if (floorCol == null || col == null) {
            return false;
        }

        // Bounds floorBounds = floorCol.bounds;
        // float margin = floorBounds.size.x * 0.008f;
        // floorBounds.Expand(-margin); 
        // Bounds objBounds = col.bounds;

        // Vector3 objCenter = objBounds.center;
        // Vector3 objCenterProjected = new Vector3(objCenter.x, floorBounds.center.y, objCenter.z); 

        // // Check if object bounds are still fully within floor bounds (XZ only)
        // return floorBounds.Contains(objCenterProjected);

        Bounds floorBounds = floorCol.bounds;
        float marginX = 0.0066f;
        float marginZ = 0.0066f;

        floorBounds.min = new Vector3(floorBounds.min.x + marginX, floorBounds.min.y, floorBounds.min.z + marginZ);
        floorBounds.max = new Vector3(floorBounds.max.x - marginX, floorBounds.max.y, floorBounds.max.z - marginZ);

        Bounds objBounds = col.bounds;

        Vector3 min = new Vector3(objBounds.min.x, floorBounds.center.y, objBounds.min.z);
        Vector3 max = new Vector3(objBounds.max.x, floorBounds.center.y, objBounds.max.z);

        return floorBounds.Contains(min) && floorBounds.Contains(max);
    }

    public bool IsOverlapping() {
        if (col == null) {
            return false;
        }

        Collider[] all = FindObjectsOfType<BoxCollider>();

        foreach (var other in all)
        {
            if (other == col || !other.CompareTag("Object"))
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

        float proximityThreshold = 0.01f; 

        Bounds myBounds = col.bounds;
        myBounds.Expand(proximityThreshold); // grow the box outward

        foreach (var other in FindObjectsOfType<BoxCollider>())
        {
            if (other == col || !other.CompareTag("Object")) continue;

            Bounds otherBounds = other.bounds;
            otherBounds.Expand(proximityThreshold);

            if (myBounds.Intersects(otherBounds))
                return true;
        }

        return false;
    }
}
