using UnityEngine;

public class RoomPrefabTester : MonoBehaviour
{
    public RoomBehavior roomPrefab;

    private RoomBehavior spawnedRoom;

    // 0 = Up, 1 = Down, 2 = Right, 3 = Left
    private bool[][] testCases =
    {
        new bool[] { true, false, false, false },   // Up
        new bool[] { false, true, false, false },   // Down
        new bool[] { false, false, true, false },   // Right
        new bool[] { false, false, false, true },   // Left
        new bool[] { true, true, false, false },    // Up + Down
        new bool[] { false, false, true, true },    // Right + Left
        new bool[] { true, true, true, true },      // All open
        new bool[] { false, false, false, false }   // None
    };

    private int index = 0;

    void Start()
    {
        SpawnRoom();
        ApplyTest();
    }

    void Update()
    {
        // Press SPACE to cycle through test cases
        if (Input.GetKeyDown(KeyCode.Space))
        {
            index++;

            if (index >= testCases.Length)
                index = 0;

            ApplyTest();
        }

        // Press R to respawn room
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnRoom();
            ApplyTest();
        }
    }

    void SpawnRoom()
    {
        if (spawnedRoom != null)
            Destroy(spawnedRoom.gameObject);

        spawnedRoom = Instantiate(roomPrefab, Vector3.zero, Quaternion.identity);
    }

    void ApplyTest()
    {
        bool[] test = testCases[index];

        Debug.Log($"TEST {index} -> U:{test[0]} D:{test[1]} R:{test[2]} L:{test[3]}");

        spawnedRoom.UpdateRoom(test);
    }
}