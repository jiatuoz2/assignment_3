using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance;
    public GameObject dungeonRoot; 
    public GameObject handle = null; 
    public bool isInTransformMode = false;
    public DebugSlider transformToggleSlider; 
    public ObjectSpawner objectSpawner;
    public HandleController handleController = null;
    public Vector3 InitialPosition;
    public Quaternion InitialRotation;
    public Vector3 InitialScale;

    void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // dungeonInteractable = GetComponent<XRGrabInteractable>(); 
        // SetCubeActive(false); 
        transformToggleSlider.value = 0;
        dungeonRoot = gameObject;

        if (handleController = HandleController.Instance) {
            handle = handleController.gameObject; 
        }

        InitialPosition = dungeonRoot.transform.position;
        InitialRotation = dungeonRoot.transform.rotation;
        InitialScale = dungeonRoot.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyHandleToDungeon(); 
    }

    public void ToggleDungeonTransform()
    {
        if (isInTransformMode == false)
        {
            isInTransformMode = true; 
            // SetCubeToCenterOfFloors();
            // GroupFloors();
            // SetCubeActive(true);
            transformToggleSlider.value = 1;
        }
        else
        {
            isInTransformMode = false;
            // UngroupFloors();
            // SetCubeActive(false);
            transformToggleSlider.value = 0;
        }
    }

    void ApplyHandleToDungeon()
    {
        if (handleController = HandleController.Instance) {
            handle = handleController.gameObject; 
        }
        if (handle == null || !isInTransformMode) return;

        dungeonRoot.transform.position = InitialPosition + handleController.GetPositionChange();
        dungeonRoot.transform.rotation = handleController.GetRotationChange() * InitialRotation; 
        Vector3 scaleFactor = handleController.GetScaleFactor();
        dungeonRoot.transform.localScale = new Vector3(
            scaleFactor.x * InitialScale.x, 
            scaleFactor.y * InitialScale.y, 
            scaleFactor.z * InitialScale.z);
    }

    // public void SetCubeActive(bool active)
    // {
    //     visualCubeRenderer.enabled = active;
    //     cubeCollider.enabled = active;
    //     dungeonInteractable.enabled = active;
    // }

    // public void GroupFloors()
    // {
    //     GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");
    //     foreach (var floor in floors)
    //     {
    //         floor.transform.SetParent(transform, true);
    //     }
    // }

    // public void UngroupFloors() {
    //     GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");
    //     foreach (var floor in floors)
    //     {
    //         floor.transform.SetParent(null, true);
    //     }
    // }

    // public void SetCubeToCenterOfFloors()
    // {
    //     GameObject[] floors = GameObject.FindGameObjectsWithTag("Floor");
    //     if (floors.Length == 0) return;

    //     // Compute centroid of all floors
    //     Vector3 center = Vector3.zero;
    //     foreach (var floor in floors)
    //     {
    //         center += floor.transform.position;
    //     }
    //     center /= floors.Length;

    //     // Vector3 yOffset = new Vector3(0, 1, 0);

    //     transform.position = center;
    // }
}
