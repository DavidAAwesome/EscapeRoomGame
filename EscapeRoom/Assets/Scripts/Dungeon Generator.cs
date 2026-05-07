using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
public class DungeonGenerator : MonoBehaviour
{

    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
    }

    public Vector2Int size;
    public int startPos = 0;
    public GameObject room;
    public Vector2 offset; // disytance between each room
   List<Cell> board;
   List<RoomBehavior> spawnedRooms = new List<RoomBehavior>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MazeGenerator();
    }

    // Update is called once per frame
    void Update()
    {




    }

    void GenerateDungeon()
    {

        for (int i = 0; i < size.x; i++)
        {

            for (int j = 0; j < size.y; j++)
            {

                Cell currentCell = board[Mathf.FloorToInt(i + j * size.x)];

                if (currentCell.visited)
                {


                    var newRoom = Instantiate(room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehavior>();
                    spawnedRooms.Add(newRoom);
                    newRoom.UpdateRoom(board[Mathf.FloorToInt(i + j * size.x)].status);
                    int x = Mathf.FloorToInt(i + j * size.x);

                    newRoom.name += " " + i + "-" + j;
                    Debug.Log($"Room {i},{j} | U:{board[x].status[0]} D:{board[x].status[1]} R:{board[x].status[2]} L:{board[x].status[3]} at {newRoom.transform.position}");
                    Debug.DrawRay(newRoom.transform.position, Vector3.forward * 2, Color.blue, 10f);
                    Debug.DrawRay(newRoom.transform.position, Vector3.back * 2, Color.red, 10f);



                }

            }



        }

    }

    void PlaceKeys()
    {
        if (spawnedRooms.Count < 4)
        {
            Debug.LogError("Not enough rooms for keys");
            return;
        }

        List<RoomBehavior> validRooms = new List<RoomBehavior>(spawnedRooms);

        // remove a random "final room candidate" (furthest room)
        RoomBehavior finalRoom = validRooms[0];
        float maxDist = -1f;

        foreach (var room in validRooms)
        {
            float dist = room.transform.position.sqrMagnitude;

            if (dist > maxDist)
            {
                maxDist = dist;
                finalRoom = room;
            }
        }

        validRooms.Remove(finalRoom);

        // shuffle
        for (int i = 0; i < validRooms.Count; i++)
        {
            RoomBehavior temp = validRooms[i];
            int rand = Random.Range(i, validRooms.Count);
            validRooms[i] = validRooms[rand];
            validRooms[rand] = temp;
        }

        // spawn 3 keys in different rooms
        SpawnKey(validRooms[0], KeyColor.Red);
        SpawnKey(validRooms[1], KeyColor.Blue);
        SpawnKey(validRooms[2], KeyColor.Green);
    }

    void SpawnKey(RoomBehavior room, KeyColor color)
    {
        GameObject keyPrefab = room.GetKeyPrefab(color);

        if (keyPrefab == null)
        {
            Debug.LogError("Missing key prefab: " + color);
            return;
        }

        Vector3 offset = new Vector3(
            Random.Range(-2f, 2f),
            1f,
            Random.Range(-2f, 2f)
        );

        Instantiate(keyPrefab, room.transform.position + offset, Quaternion.identity, room.transform);

        Debug.Log("Spawned " + color + " key in " + room.name);
    }
    void MazeGenerator()
    {

        board = new List<Cell>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }

        int currentCell = startPos;

        Stack<int> path = new Stack<int>(); // keep track of where you are and how many you went through

        int k = 0;

        while (k < 1000)
        {
            k++;

            board[currentCell].visited = true;

            if (currentCell == board.Count - 1)
            {
                break;
            }


            //Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    //down or right
                    if (newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    //up or left
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }

            }

        }
        GenerateDungeon();
        PlaceKeys();
    }


    public enum KeyColor
    {
        Red,
        Blue,
        Green
    }


    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell - size.x)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + size.x));
        }

        //check right neighbor
        if ((cell + 1) % size.x != 0 && !board[Mathf.FloorToInt(cell + 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell + 1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited)
        {
            neighbors.Add(Mathf.FloorToInt(cell - 1));
        }

        return neighbors;
    }
}





























//} 



//using UnityEngine;
//using System.Collections.Generic;

//public class DungeonGenerator : MonoBehaviour
//{
//    public class Cell
//    {
//        public bool visited = false;
//        public bool[] status = new bool[4];
//    }

//    public Vector2Int size;
//    public int startPos = 0;
//    public GameObject room;
//    public Vector2 offset;

//    List<Cell> board;
//    List<RoomBehavior> spawnedRooms = new List<RoomBehavior>();

//    void Start()
//    {
//        MazeGenerator();
//    }

//    void GenerateDungeon()
//    {
//        for (int i = 0; i < size.x; i++)
//        {
//            for (int j = 0; j < size.y; j++)
//            {
//                Cell currentCell = board[i + j * size.x];

//                if (currentCell.visited)
//                {
//                    RoomBehavior newRoom = Instantiate(
//                        room,
//                        new Vector3(i * offset.x, 0, -j * offset.y),
//                        Quaternion.identity,
//                        transform
//                    ).GetComponent<RoomBehavior>();

//                    newRoom.UpdateRoom(currentCell.status);
//                    newRoom.name += " " + i + "-" + j;

//                    spawnedRooms.Add(newRoom); // TRACK ROOMS
//                }
//            }
//        }

//        // Mark last room
//        RoomBehavior lastRoom = spawnedRooms[0];
//        float maxDist = -1f;

//        foreach (RoomBehavior room in spawnedRooms)
//        {
//            float dist = room.transform.position.sqrMagnitude;

//            if (dist > maxDist)
//            {
//                maxDist = dist;
//                lastRoom = room;
//            }
//        }

//        lastRoom.isLastRoom = true;
//        lastRoom.ActivateFinalRoom();

//        Debug.Log("[GENERATOR] FINAL ROOM SET: " + lastRoom.name);

//        lastRoom.isLastRoom = true;
//        lastRoom.ActivateFinalRoom();

//        Debug.Log("[GENERATOR] FINAL ROOM SET: " + lastRoom.name);

//        Debug.Log($"[GENERATOR] Marked LAST ROOM: {lastRoom.name}");

//        // Spawn keys in earlier rooms
//        PlaceKeys();
//    }

//    void PlaceKeys()
//    {
//        List<RoomBehavior> validRooms = new List<RoomBehavior>(spawnedRooms);

//        // remove last room (door room)
//        validRooms.RemoveAt(validRooms.Count - 1);

//        // Shuffle rooms so we get unique ones
//        for (int i = 0; i < validRooms.Count; i++)
//        {
//            RoomBehavior temp = validRooms[i];
//            int randomIndex = Random.Range(i, validRooms.Count);
//            validRooms[i] = validRooms[randomIndex];
//            validRooms[randomIndex] = temp;
//        }

//        // Now first 3 rooms are guaranteed different
//        SpawnKey(validRooms[0], KeyColor.Red);
//        SpawnKey(validRooms[1], KeyColor.Blue);
//        SpawnKey(validRooms[2], KeyColor.Green);
//    }

//    void SpawnKey(RoomBehavior room, KeyColor color)
//    {
//        GameObject keyPrefab = room.GetKeyPrefab(color);

//        Vector3 randomOffset = new Vector3(
//            Random.Range(-3f, 3f),
//            0.5f,
//            Random.Range(-3f, 3f)
//        );

//        Vector3 spawnPosition = room.transform.position + randomOffset;

//        Instantiate(keyPrefab, spawnPosition, Quaternion.identity, room.transform);
//        Debug.Log("Spawning " + color + " key in " + room.name);
//    }

//    void MazeGenerator()
//    {
//        board = new List<Cell>();

//        for (int i = 0; i < size.x * size.y; i++)
//            board.Add(new Cell());

//        int currentCell = startPos;
//        Stack<int> path = new Stack<int>();

//        while (true)
//        {
//            board[currentCell].visited = true;

//            List<int> neighbors = CheckNeighbors(currentCell);

//            if (neighbors.Count == 0)
//            {
//                if (path.Count == 0) break;
//                currentCell = path.Pop();
//            }
//            else
//            {
//                path.Push(currentCell);
//                int newCell = neighbors[Random.Range(0, neighbors.Count)];

//                if (newCell > currentCell)
//                {
//                    if (newCell - 1 == currentCell)
//                    {
//                        board[currentCell].status[2] = true;
//                        currentCell = newCell;
//                        board[currentCell].status[3] = true;
//                    }
//                    else
//                    {
//                        board[currentCell].status[1] = true;
//                        currentCell = newCell;
//                        board[currentCell].status[0] = true;
//                    }
//                }
//                else
//                {
//                    if (newCell + 1 == currentCell)
//                    {
//                        board[currentCell].status[3] = true;
//                        currentCell = newCell;
//                        board[currentCell].status[2] = true;
//                    }
//                    else
//                    {
//                        board[currentCell].status[0] = true;
//                        currentCell = newCell;
//                        board[currentCell].status[1] = true;
//                    }
//                }
//            }
//        }

//        GenerateDungeon();
//    }

//    List<int> CheckNeighbors(int cell)
//    {
//        List<int> neighbors = new List<int>();

//        if (cell - size.x >= 0 && !board[cell - size.x].visited)
//            neighbors.Add(cell - size.x);

//        if (cell + size.x < board.Count && !board[cell + size.x].visited)
//            neighbors.Add(cell + size.x);

//        if ((cell + 1) % size.x != 0 && !board[cell + 1].visited)
//            neighbors.Add(cell + 1);

//        if (cell % size.x != 0 && !board[cell - 1].visited)
//            neighbors.Add(cell - 1);

//        return neighbors;
//    }
//}

//public enum KeyColor { Red, Blue, Green }