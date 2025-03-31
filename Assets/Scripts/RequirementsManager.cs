using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RequirementsManager : MonoBehaviour
{
    public static RequirementsManager Instance;
    private List<FloorController> floors = new();

    // UI References per room
    public TMP_Text room1MonsterText;
    public TMP_Text room1ChestText;
    public TMP_Text room1WallText;
    public TMP_Text room1SpawnText;
    public TMP_Text room1DoorText;
    public TMP_Text room1BossText;

    public TMP_Text room2MonsterText;
    public TMP_Text room2ChestText;
    public TMP_Text room2WallText;
    public TMP_Text room2SpawnText;
    public TMP_Text room2DoorText;
    public TMP_Text room2BossText;

    public TMP_Text room3MonsterText;
    public TMP_Text room3ChestText;
    public TMP_Text room3WallText;
    public TMP_Text room3SpawnText;
    public TMP_Text room3DoorText;
    public TMP_Text room3BossText;

    [System.Serializable]
    public class RoomRequirements
    {
        public FloorController floor; // ← reference to actual floor
        public int roomNumber => floor.roomNumber; // computed from floor

        // Current counts
        public int monsterCount;
        public int chestCount;
        public int wallCount;
        public int spawnCount;
        public int doorCount;
        public bool hasFinalBoss;

        // UI References
        public TMP_Text monsterText;
        public TMP_Text chestText;
        public TMP_Text wallText;
        public TMP_Text spawnText;
        public TMP_Text doorText;
        public TMP_Text bossText;

        // Required counts (set when constructed)
        public int requiredMonsters;
        public int requiredChests;
        public int requiredWalls = 1;
        public int requiredSpawns = 1;
        public int requiredDoors;
        public bool requiresFinalBoss;

        public RoomRequirements(FloorController floor, int reqMonsters, int reqChests, int reqDoors, bool reqBoss)
        {
            this.floor = floor;
            requiredMonsters = reqMonsters;
            requiredChests = reqChests;
            requiredDoors = reqDoors;
            requiresFinalBoss = reqBoss;

            if (floor.roomNumber == 1)
            {
                monsterText = RequirementsManager.Instance.room1MonsterText;
                chestText = RequirementsManager.Instance.room1ChestText;
                wallText = RequirementsManager.Instance.room1WallText;
                spawnText = RequirementsManager.Instance.room1SpawnText;
                doorText = RequirementsManager.Instance.room1DoorText;
                bossText = RequirementsManager.Instance.room1BossText;
            }
            else if (floor.roomNumber == 2)
            {
                monsterText = RequirementsManager.Instance.room2MonsterText;
                chestText = RequirementsManager.Instance.room2ChestText;
                wallText = RequirementsManager.Instance.room2WallText;
                spawnText = RequirementsManager.Instance.room2SpawnText;
                doorText = RequirementsManager.Instance.room2DoorText;
                bossText = RequirementsManager.Instance.room2BossText;
            }
            else if (floor.roomNumber == 3)
            {
                monsterText = RequirementsManager.Instance.room3MonsterText;
                chestText = RequirementsManager.Instance.room3ChestText;
                wallText = RequirementsManager.Instance.room3WallText;
                spawnText = RequirementsManager.Instance.room3SpawnText;
                doorText = RequirementsManager.Instance.room3DoorText;
                bossText = RequirementsManager.Instance.room3BossText;
            }
        }

        public void CheckAndUpdateUI()
        {
            if (floor.roomNumber == 1)
            {
                monsterText = RequirementsManager.Instance.room1MonsterText;
                chestText = RequirementsManager.Instance.room1ChestText;
                wallText = RequirementsManager.Instance.room1WallText;
                spawnText = RequirementsManager.Instance.room1SpawnText;
                doorText = RequirementsManager.Instance.room1DoorText;
                bossText = RequirementsManager.Instance.room1BossText;
            }
            else if (floor.roomNumber == 2)
            {
                monsterText = RequirementsManager.Instance.room2MonsterText;
                chestText = RequirementsManager.Instance.room2ChestText;
                wallText = RequirementsManager.Instance.room2WallText;
                spawnText = RequirementsManager.Instance.room2SpawnText;
                doorText = RequirementsManager.Instance.room2DoorText;
                bossText = RequirementsManager.Instance.room2BossText;
            }
            else if (floor.roomNumber == 3)
            {
                monsterText = RequirementsManager.Instance.room3MonsterText;
                chestText = RequirementsManager.Instance.room3ChestText;
                wallText = RequirementsManager.Instance.room3WallText;
                spawnText = RequirementsManager.Instance.room3SpawnText;
                doorText = RequirementsManager.Instance.room3DoorText;
                bossText = RequirementsManager.Instance.room3BossText;
            }

            if (monsterText != null) monsterText.alpha = monsterCount >= requiredMonsters ? 0.3f : 1f;
            if (chestText != null)   chestText.alpha   = chestCount >= requiredChests ? 0.3f : 1f;
            if (wallText != null)    wallText.alpha    = wallCount >= requiredWalls ? 0.3f : 1f;
            if (spawnText != null)   spawnText.alpha   = spawnCount >= requiredSpawns ? 0.3f : 1f;
            if (doorText != null)    doorText.alpha    = doorCount >= requiredDoors ? 0.3f : 1f;
            if (bossText != null)    bossText.alpha    = (!requiresFinalBoss || hasFinalBoss) ? 0.3f : 1f;
        }

        public bool IsComplete()
        {
            return monsterCount >= requiredMonsters &&
                chestCount >= requiredChests &&
                wallCount >= requiredWalls &&
                spawnCount >= requiredSpawns &&
                doorCount >= requiredDoors &&
                (!requiresFinalBoss || hasFinalBoss);
        }
    }

    public List<RoomRequirements> allRoomRequirements = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void RegisterFloor(FloorController floor)
    {
        floors.Add(floor);
        ReassignRoomNumbers();
        if (floor.roomNumber == 1) {
            AddRoomRequirement(floor, 1, 0, 1, false); 
        } else if (floor.roomNumber == 2) {
            AddRoomRequirement(floor, 2, 2, 1, false); 
        } else if (floor.roomNumber == 3) {
            AddRoomRequirement(floor, 1, 1, 0, true); 
        }
        RefreshAllRoomUI();
    }

    public void UnregisterFloor(FloorController floor)
    {
        floors.Remove(floor);
        RemoveRequirement(floor); 
        ReassignRoomNumbers();
        RefreshAllRoomUI();
    }

    private void ReassignRoomNumbers()
    {
        for (int i = 0; i < floors.Count; i++)
        {
            floors[i].roomNumber = i + 1;

            RoomRequirements req = allRoomRequirements.Find(r => r.floor == floors[i]);
            if (req == null) continue;

            int newRoomNumber = i + 1;

            // Reset all UI alphas to 1f (full visible)
            if (req.monsterText != null) req.monsterText.alpha = 1f;
            if (req.chestText != null)   req.chestText.alpha   = 1f;
            if (req.wallText != null)    req.wallText.alpha    = 1f;
            if (req.spawnText != null)   req.spawnText.alpha   = 1f;
            if (req.doorText != null)    req.doorText.alpha    = 1f;
            if (req.bossText != null)    req.bossText.alpha    = 1f;

            // Update required values based on new room number
            if (newRoomNumber == 1)
            {
                req.requiredMonsters = 1;
                req.requiredChests = 0;
                req.requiredDoors = 1;
                req.requiresFinalBoss = false;
            }
            else if (newRoomNumber == 2)
            {
                req.requiredMonsters = 2;
                req.requiredChests = 2;
                req.requiredDoors = 1;
                req.requiresFinalBoss = false;
            }
            else if (newRoomNumber == 3)
            {
                req.requiredMonsters = 1;
                req.requiredChests = 1;
                req.requiredDoors = 0;
                req.requiresFinalBoss = true;
            }
            else
            {
                // Optional fallback for floors beyond 3
                req.requiredMonsters = 0;
                req.requiredChests = 0;
                req.requiredDoors = 0;
                req.requiresFinalBoss = false;
            }
        }
    }

    public void RefreshAllRoomUI()
    {
        foreach (var room in allRoomRequirements)
        {
            room.CheckAndUpdateUI();
        }
    }

    public void AddRoomRequirement(FloorController floor, int monsters, int chests, int doors, bool boss)
    {
        RoomRequirements req = new RoomRequirements(floor, monsters, chests, doors, boss);
        allRoomRequirements.Add(req);
    }

    public void RemoveRequirement(FloorController floor)
    {
        allRoomRequirements.RemoveAll(r => r.floor == floor);
    }

    public void RegisterObject(int roomNumber, string objectType)
    {
        RoomRequirements room = allRoomRequirements.Find(r => r.roomNumber == roomNumber);
        if (room == null) return;

        switch (objectType)
        {
            case "Monster": room.monsterCount++; break;
            case "Chest": room.chestCount++; break;
            case "Wall": room.wallCount++; break;
            case "Spawn": room.spawnCount++; break;
            case "Door": room.doorCount++; break;
            case "Boss": room.hasFinalBoss = true; break;
        }

        room.CheckAndUpdateUI();
    }

    public void DeregisterObject(int roomNumber, string objectType)
    {
        RoomRequirements room = allRoomRequirements.Find(r => r.roomNumber == roomNumber);
        if (room == null) return;

        switch (objectType)
        {
            case "Monster": room.monsterCount--; break;
            case "Chest": room.chestCount--; break;
            case "Wall": room.wallCount--; break;
            case "Spawn": room.spawnCount--; break;
            case "Door": room.doorCount--; break;
            case "Boss": room.hasFinalBoss = false; break;
        }

        room.CheckAndUpdateUI();
    }

    public bool AreAllRoomsComplete()
    {
        foreach (var room in allRoomRequirements)
        {
            if (!room.IsComplete())
                return false;
        }

        if (allRoomRequirements.Count != 3) return false;
        return true;
    }
}
