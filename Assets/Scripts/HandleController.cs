using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class HandleController : MonoBehaviour
{
    public static HandleController Instance;
    public Vector3 InitialPosition;
    public Quaternion InitialRotation;
    public Vector3 InitialScale;
    public DungeonManager dungeonManager;
    public GameObject dungeonRoot;
    private bool rootAnchored = false; 
    private Dictionary<Transform, Vector3> originalWorldPos = new Dictionary<Transform, Vector3>();
    private float initialYPosition = 0;


    IEnumerator Start()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("A second Dungeon Handle was spawned and will be destroyed.");
            Destroy(gameObject);
        }

        dungeonManager = DungeonManager.Instance;
        if (!dungeonManager.isInTransformMode) {
            Destroy(gameObject);
        }

        Instance = this;
        dungeonRoot = DungeonManager.Instance.gameObject; 
        RecordInitialTransform();

        if (!rootAnchored) {
            rootAnchored = true; 
            SaveLocation(); 
            Vector3 positionChange = InitialPosition - dungeonRoot.transform.position;
            dungeonRoot.transform.position += positionChange;
            // dungeonRoot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); 
            yield return null;
            RelocateFloors();
            // ResizeFloors();
        }

        if (!rootAnchored) {
            rootAnchored = true;
        }
    }

    void Update()
    {
        if (!dungeonManager.isInTransformMode) {
            Destroy(gameObject); 
        }
        ClampFloorHeight();
    }

    void ClampFloorHeight() {
        GameObject[] floors = GetAllFloorsInDungeon();
        foreach (var floor in floors)
        {
            floor.transform.position = new Vector3(floor.transform.position.x, initialYPosition, floor.transform.position.z);
        }
    }

    void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    void RecordInitialTransform()
    {
        InitialPosition = transform.position;
        InitialRotation = transform.rotation;
        InitialScale = transform.localScale;
    }

    public Vector3 GetPositionChange() {
        return transform.position - InitialPosition;
    }

    public Quaternion GetRotationChange() {
        return transform.rotation * Quaternion.Inverse(InitialRotation);
    }

    public Vector3 GetScaleFactor()
    {
        Vector3 factor = new Vector3(
            transform.localScale.x / InitialScale.x,
            transform.localScale.y / InitialScale.y,
            transform.localScale.z / InitialScale.z
        );
        return factor;
    }

    public void ResetToInitialTransform()
    {
        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
        transform.localScale = InitialScale;
    }

    public void SaveLocation() {
        GameObject[] floors = GetAllFloorsInDungeon();

        // Step 1: Store original world positions
        foreach (var floor in floors)
        {
            originalWorldPos[floor.transform] = floor.transform.position + dungeonRoot.transform.position;
        }
    }
    public void RelocateFloors()
    {
        GameObject[] floors = GetAllFloorsInDungeon();

        foreach (var floor in floors)
        {
            floor.transform.position = originalWorldPos[floor.transform] - dungeonRoot.transform.position;
            initialYPosition += floor.transform.position.y;
        }
        initialYPosition /= floors.Length;
    }

    public void ResizeFloors() {
        GameObject[] floors = GetAllFloorsInDungeon();

        foreach (var floor in floors)
        {
            floor.transform.localScale *= 5; 
        }
    }

    public void GroupFloors()
    {
        GameObject[] floors = GetAllFloorsInDungeon();
        foreach (var floor in floors)
        {
            floor.transform.SetParent(transform, true);
        }
    }

    public void UngroupFloors() {
        GameObject[] floors = GetAllFloorsInDungeon();
        foreach (var floor in floors)
        {
            floor.transform.SetParent(null, true);
        }
    }




    public GameObject[] GetAllFloorsInDungeon()
    {
        return dungeonRoot.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.CompareTag("Floor"))
                        .Select(t => t.gameObject)
                        .ToArray();
    }
}
