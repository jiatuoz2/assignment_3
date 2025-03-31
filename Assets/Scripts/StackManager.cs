using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StackManager : MonoBehaviour
{
    public static StackManager Instance;

    private Stack<IUndoAction> undoStack = new();
    private Stack<IUndoAction> redoStack = new();
    private const int MaxHistory = 10;
    private Dictionary<string, GameObject> objectRegistry = new();
    public Button undoButton;
    public Button redoButton;
    public TextMeshProUGUI debugText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // debugText.text = "Undo count: " + undoStack.Count.ToString() + " Redo count: " + redoStack.Count.ToString(); 
        if (undoButton != null)
            undoButton.interactable = undoStack.Count > 0;

        if (redoButton != null)
            redoButton.interactable = redoStack.Count > 0;
    }

    public void RegisterAction(IUndoAction action)
    {
        if (undoStack.Count >= MaxHistory) {
            var tempList = new List<IUndoAction>(undoStack);
            tempList.RemoveAt(0); // Remove oldest
            undoStack = new Stack<IUndoAction>(tempList); 
        }

        undoStack.Push(action);
        redoStack.Clear();
    }

    public void RegisterObject(GameObject obj)
    {
        var id = obj.GetComponent<ObjectController>()?.objectID;
        if (!string.IsNullOrEmpty(id))
            objectRegistry[id] = obj;
    }

    public void UnregisterObject(GameObject obj)
    {
        var id = obj.GetComponent<ObjectController>()?.objectID;
        if (!string.IsNullOrEmpty(id))
            objectRegistry.Remove(id);
    }

    public GameObject GetObjectById(string id)
    {
        objectRegistry.TryGetValue(id, out var obj);
        return obj;
    }

    public void RegisterTransformChange(GameObject obj, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, Transform parent, string objectID)
    {
        RegisterAction(new TransformAction(obj, fromPos, toPos, fromRot, toRot, parent, objectID));
    }

    public void RegisterCreateObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, GameObject instance, string objectID) 
    {
        RegisterAction(new CreateAction(prefab, position, rotation, scale, parent, instance, objectID)); 
    }

    public void RegisterDeleteObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, GameObject instance, string objectID) 
    {
        RegisterAction(new DeleteAction(prefab, position, rotation, scale, parent, instance, objectID)); 
    }

    public void Undo()
    {
        while (undoStack.Count > 0)
        {
            var action = undoStack.Pop();
            if (action.IsValid())
            {
                action.Undo();
                redoStack.Push(action);
                break;
            }
        }
    }

    public void Redo()
    {
        while (redoStack.Count > 0)
        {
            var action = redoStack.Pop();
            if (action.IsValid())
            {
                action.Redo();
                undoStack.Push(action);
                break;
            }
        }
    }

    public interface IUndoAction
    {
        void Undo();
        void Redo();
        bool IsValid(); 
    }

    public class CreateAction : IUndoAction
    {
        private GameObject prefab;
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;
        private Transform parent;
        private GameObject instance;
        private string objectID; 

        public CreateAction(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, GameObject instance, string objectID)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.parent = parent;
            this.instance = instance;
            this.objectID = objectID;

            StackManager.Instance.RegisterObject(instance);
        }

        public void Undo()
        {
            if (instance != null)
            {
                ObjectController objectController = instance.GetComponent<ObjectController>();
                objectController.isPerformingUndoRedo = true;
                Destroy(instance);
                StackManager.Instance.UnregisterObject(instance);
                instance = null;
            }
        }

        public void Redo()
        {
            // if floor is gone, cannot recreate
            if (parent == null) {
                return;
            }

            instance = Instantiate(prefab, parent);
            ObjectController objectController = instance.GetComponent<ObjectController>();
            objectController.isPerformingUndoRedo = true;
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;

            // inherit the old ID for the new instance
            objectController.objectID = objectID;
            StackManager.Instance.RegisterObject(instance); 
        }

        public bool IsValid()
        {
            return parent != null;
        }
    }

    public class DeleteAction : IUndoAction
    {
        private GameObject prefab;
        private Vector3 position;
        private Quaternion rotation;
        private Vector3 scale;
        private Transform parent;
        private GameObject instance;
        private string objectID;

        public DeleteAction(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, GameObject instance, string objectID)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            this.parent = parent;
            this.instance = instance;
            this.objectID = objectID;
        }

        public void Undo()
        {
            // if floor is gone, cannot recreate
            if (parent == null) return;

            instance = Instantiate(prefab, parent);
            ObjectController objectController = instance.GetComponent<ObjectController>();
            objectController.isPerformingUndoRedo = true;
            instance.transform.localPosition = position;
            instance.transform.localRotation = rotation;
            instance.transform.localScale = scale;

            // inherit the old ID for the new instance
            objectController.objectID = objectID;
            StackManager.Instance.RegisterObject(instance);
        }

        public void Redo()
        {
            if (instance != null)
            {
                ObjectController objectController = instance.GetComponent<ObjectController>();
                objectController.isPerformingUndoRedo = true;
                Destroy(instance);
                StackManager.Instance.UnregisterObject(instance);
                instance = null;
            }
        }

        public bool IsValid()
        {
            return parent != null;
        }
    }

    public class TransformAction : IUndoAction
    {
        private GameObject obj;
        private Vector3 fromPos, toPos;
        private Quaternion fromRot, toRot;
        private Transform parent;
        private string objectID; 

        public TransformAction(GameObject obj, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, Transform parent, string objectID)
        {
            this.obj = obj;
            this.fromPos = fromPos;
            this.toPos = toPos;
            this.fromRot = fromRot;
            this.toRot = toRot;
            this.parent = parent;
            this.objectID = objectID;
        }

        public void Undo()
        {
            obj = StackManager.Instance.GetObjectById(objectID);
            if (obj != null) {
                obj.transform.position = fromPos;
                obj.transform.rotation = fromRot;
            } 
        }

        public void Redo()
        {
            obj = StackManager.Instance.GetObjectById(objectID);
            if (obj!= null) {
                obj.transform.position = toPos;
                obj.transform.rotation = toRot;
            }
        }

        public bool IsValid()
        {
            return parent != null;
        }
    }
}
