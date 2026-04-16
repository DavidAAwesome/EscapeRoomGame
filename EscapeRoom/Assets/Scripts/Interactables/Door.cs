using UnityEngine;

public class Door : MonoBehaviour
{
    public void GameStarting()
    {
        // gameObject.SetActive(true);
        GetComponent<BoxCollider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }
    
    public void OnUnlocked()
    {
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        // gameObject.SetActive(false);
    }
}
