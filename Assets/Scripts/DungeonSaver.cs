using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class DungeonSaver : MonoBehaviour
{
    private NotifierSystem notifierSystem;
    public TextMeshProUGUI debugText; 

    void Awake()
    {
        notifierSystem = NotifierSystem.Instance;
    }

    [System.Serializable]
    public class DungeonData
    {
        public List<RoomData> rooms = new();
    }

    [System.Serializable]
    public class RoomData
    {
        public int roomNumber;
        public float[] position;
        public float[] rotation;
        public float[] scale; 
        public List<ObjectData> objects = new();
    }

    [System.Serializable]
    public class ObjectData
    {
        public string name;
        public float[] position;
        public float[] rotation;
        public float[] scale;
    }

    public void SaveDungeon()
    {
        if (!RequirementsManager.Instance.AreAllRoomsComplete())
        {
            // notifierSystem.ShowNotifier("Requirements not met", "Failure", 2);
            debugText.text = "requirements not met";
            return;
        }

        DungeonData dungeonData = new DungeonData();

        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Object");

        foreach (GameObject obj in allObjects)
        {
            ObjectController ctrl = obj.GetComponent<ObjectController>();
            if (ctrl == null) continue;

            FloorController floor = ctrl.floor.GetComponent<FloorController>();
            if (floor == null) continue;

            int roomNum = floor.roomNumber;
            RoomData room = dungeonData.rooms.Find(r => r.roomNumber == roomNum);
            if (room == null)
            {
                room = new RoomData { 
                    roomNumber = roomNum,
                    position = new float[] {
                        floor.transform.position.x,
                        floor.transform.position.y,
                        floor.transform.position.z
                    },
                    rotation = new float[] {
                        floor.transform.eulerAngles.x,
                        floor.transform.eulerAngles.y,
                        floor.transform.eulerAngles.z
                    },
                    scale = new float[] {
                        floor.transform.localScale.x,
                        floor.transform.localScale.y,
                        floor.transform.localScale.z
                    }
                };
                dungeonData.rooms.Add(room);
            }

            ObjectData data = new ObjectData
            {
                name = ctrl.objectName,
                position = new float[] {
                    obj.transform.localPosition.x,
                    obj.transform.localPosition.y,
                    obj.transform.localPosition.z
                },
                rotation = new float[] {
                    obj.transform.localEulerAngles.x,
                    obj.transform.localEulerAngles.y,
                    obj.transform.localEulerAngles.z
                },
                scale = new float[] {
                    obj.transform.localScale.x,
                    obj.transform.localScale.y,
                    obj.transform.localScale.z
                }
            };

            room.objects.Add(data);
        }

        string json = JsonUtility.ToJson(dungeonData, true);
        string path = "/storage/emulated/0/Download/dungeon_layout.json";
        File.WriteAllText(path, json);
        debugText.text = "saved data to " + path;
        // notifierSystem.ShowNotifier("Layout saved", "Success", 2);
    }
}
