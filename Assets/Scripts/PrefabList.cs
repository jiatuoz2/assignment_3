using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabList : MonoBehaviour
{
    public static PrefabList Instance;

    [System.Serializable]
    public class PrefabEntry
    {
        public string name;
        public GameObject prefab;
    }

    public List<PrefabEntry> prefabEntries = new();
    private Dictionary<string, GameObject> prefabDict = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        foreach (var entry in prefabEntries)
        {
            prefabDict[entry.name] = entry.prefab;
        }
    }

    public GameObject GetPrefab(string name)
    {
        return prefabDict.TryGetValue(name, out var prefab) ? prefab : null;
    }
}
