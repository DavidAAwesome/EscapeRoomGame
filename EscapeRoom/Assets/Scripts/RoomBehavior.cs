using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro.Examples;
using UnityEditor;
using static DungeonGenerator;


public class RoomBehavior : MonoBehaviour
{
    public GameObject[] walls; // 0 - Up 1 -Down 2 - Right 3- Left
    public GameObject[] doors;
    public GameObject barrel;
    public GameObject rug;
    public GameObject redDoor;
    public GameObject blueDoor;
    public GameObject greenDoor;

    public GameObject redKey;
    public GameObject blueKey;
    public GameObject greenKey;

    public Transform[] keySpawnPoints;   // empty objects inside rooms
    public Transform[] doorSpawnPoints;  // only used by last room
    public bool isLastRoom = false;



    public bool[] teststatus;
    public bool useTest = false;



    public GameObject GetKeyPrefab(KeyColor color)
    {
        switch (color)
        {
            case KeyColor.Red:
                return redKey;

            case KeyColor.Blue:
                return blueKey;

            case KeyColor.Green:
                return greenKey;
        }

        return null;
    }
    void Start()
    {
        if (useTest)
        {
            UpdateRoom(teststatus);
        }
    }

    public void UpdateRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            doors[i].SetActive(status[i]);
            walls[i].SetActive(!status[i]);

            Debug.Log($"Index {i} | Door: {doors[i].name} | Wall: {walls[i].name}");
        }


        SpawnBarrel();

        Rugs();
    }

    void SpawnBarrel()
    {
        int barrelCount = Random.Range(1, 4);

        for (int i = 0; i < barrelCount; i++)
        {

            Vector3 randomOffset = new Vector3(
                Random.Range(-3.665f, 6.34f), // where i want barrel x
                0,
                Random.Range(-1f, 8.41f) // where i want barrel z
            );

            Vector3 spawnPosition = transform.position + randomOffset; // where room is plus my parameer

            Instantiate(barrel, spawnPosition, Quaternion.identity, transform);  // barrel being made

        }
    }



    void Rugs()
    {
        int rugCount = Random.Range(1, 1);
        for (int i = 0; i < rugCount; i++)
        {

            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 4f), // where i want rug x 
                0,
                Random.Range(.25f, 7f) // where i want rug z 
            );

            Vector3 spawnPosition = transform.position + randomOffset; // where room is plus my parameer

            Instantiate(rug, spawnPosition, Quaternion.identity, transform);  // rug being made

        }



    }


}







//using UnityEngine;

//public class RoomBehavior : MonoBehaviour
//{
//    public GameObject[] walls;
//    public GameObject[] doors;

//    public GameObject barrel;
//    public GameObject rug;

//    //public GameObject redDoor;
//  //  public GameObject blueDoor;
//   // public GameObject greenDoor;

//    public GameObject redKey;
//    public GameObject blueKey;
//    public GameObject greenKey;

//    public Transform[] doorSpawnPoints;

//    public bool isLastRoom = false;
//    bool doorsSpawned = false;

//    public void ActivateFinalRoom()
//    {
//        if (doorsSpawned) return;

//        Debug.Log($"[FINAL ROOM ACTIVE] {name}");
//        SpawnFinalDoors();
//        doorsSpawned = true;
//    }

//    public void UpdateRoom(bool[] status)
//    {
//        for (int i = 0; i < status.Length; i++)
//        {
//            doors[i].SetActive(status[i]);
//            walls[i].SetActive(!status[i]);
//        }

//        SpawnBarrel();
//        SpawnRug();
//    }

//    void SpawnFinalDoors()
//    {
//        if (doorSpawnPoints == null || doorSpawnPoints.Length < 3)
//        {
//            Debug.LogError("[FINAL ROOM ERROR] doorSpawnPoints not set correctly! Needs 3 points.");
//            return;
//        }

//        //SpawnDoor(redDoor, doorSpawnPoints[0], "RED");
//        //SpawnDoor(blueDoor, doorSpawnPoints[1], "BLUE");
//        //SpawnDoor(greenDoor, doorSpawnPoints[2], "GREEN");
//    }

//    void SpawnDoor(GameObject doorPrefab, Transform point, string color)
//    {
//        GameObject door = Instantiate(
//            doorPrefab,
//            point.position,
//            Quaternion.identity,
//            transform
//        );

//        Debug.Log(
//            $"[DOOR SPAWNED] {color} door in room '{name}' at {door.transform.position}"
//        );
//    }

//    void SpawnBarrel()
//    {
//        int barrelCount = Random.Range(1, 4);

//        for (int i = 0; i < barrelCount; i++)
//        {
//            Vector3 randomOffset = new Vector3(
//                Random.Range(-3.6f, 6.3f),
//                0,
//                Random.Range(-1f, 8.4f)
//            );

//            Instantiate(barrel, transform.position + randomOffset, Quaternion.identity, transform);
//        }
//    }

//    void SpawnRug()
//    {
//        Vector3 randomOffset = new Vector3(
//            Random.Range(-2f, 4f),
//            0,
//            Random.Range(0.25f, 7f)
//        );

//        Instantiate(rug, transform.position + randomOffset, Quaternion.identity, transform);
//    }

//    public GameObject GetKeyPrefab(KeyColor color)
//    {
//        switch (color)
//        {
//            case KeyColor.Red: return redKey;
//            case KeyColor.Blue: return blueKey;
//            case KeyColor.Green: return greenKey;
//        }
//        return null;
//    }
//}