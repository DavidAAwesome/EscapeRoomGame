using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

           if(currentCell == board.Count - 1)
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
