using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro.Examples;


public class RoomBehavior : MonoBehaviour
{
    public GameObject[] walls; // 0 - Up 1 -Down 2 - Right 3- Left
    public GameObject[] doors;
    public GameObject barrel;
    public GameObject rug;
    public bool[] teststatus;
    public bool useTest = false;
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
    


    void Rugs ()
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

