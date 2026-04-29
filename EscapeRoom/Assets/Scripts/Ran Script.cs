using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class RanScript : MonoBehaviour
{

    public GameObject barrel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        Vector3 randomSpawnPosition = new Vector3(Random.Range(-3.665f, 6.34f), 5, Random.Range(8.41f, -1));
        Instantiate(barrel, randomSpawnPosition, Quaternion.identity);

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
