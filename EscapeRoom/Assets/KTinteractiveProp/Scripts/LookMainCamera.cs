using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTintercativeProp
{
    public class LookMainCamera : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            transform.LookAt(Camera.main.transform, Vector3.down);
        }
    }
}