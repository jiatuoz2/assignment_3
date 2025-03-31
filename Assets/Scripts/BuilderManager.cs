using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuilderManager : MonoBehaviour
{
    public enum BuilderMode
    {
        PlacingFloor,
        PlacingObject
    }

    public BuilderMode currentMode = BuilderMode.PlacingFloor; 
    public static BuilderManager Instance; 
    public DebugSlider modeToggleSlider; 

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        modeToggleSlider.value = 1; 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleMode() {
        if (currentMode == BuilderMode.PlacingObject) {
            currentMode = BuilderMode.PlacingFloor; 
            // SetInteractionEnabledOnFloors(true); 
            modeToggleSlider.value = 1; 
        } else {
            currentMode = BuilderMode.PlacingObject;
            // SetInteractionEnabledOnFloors(false);
            modeToggleSlider.value = 0;
        }
    }

    void SetInteractionEnabledOnFloors(bool enabled)
    {
        foreach (var floor in GameObject.FindGameObjectsWithTag("Floor"))
        {
            var grab = floor.GetComponent<XRGrabInteractable>();
            if (grab != null)
                grab.enabled = enabled;
        }
    }
}
