using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TransformTrack : MonoBehaviour
{
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isBeingTransformed = false;
    private GameObject floor; 
    private string objectID;

    void Start()
    {
        // object's original spawn transform
        lastPosition = transform.position; 
        lastRotation = transform.rotation;

        // record current parent
        ObjectController objectController = GetComponent<ObjectController>();
        floor = objectController.floor;
        objectID = objectController.objectID;

        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.selectEntered.AddListener(OnGrab);
            grab.selectExited.AddListener(OnRelease);
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isBeingTransformed = true;
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        if (isBeingTransformed)
        {
            Vector3 newPos = transform.position;
            Quaternion newRot = transform.rotation;

            if (newPos != lastPosition || newRot != lastRotation) {
                StackManager.Instance.RegisterTransformChange(gameObject, lastPosition, newPos, lastRotation, newRot, floor.transform, objectID);
            }
        }

        isBeingTransformed = false;
    }
}