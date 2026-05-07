using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTintercativeProp
{
    public class SoundProp : MonoBehaviour
    {
        public bool activate;
        public GameObject icon;

        public AudioClip sound;

        AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            activate = false;
            audioSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (activate)
            {
                icon.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    audioSource.clip = sound;
                    audioSource.Play();
                }
            }
            else
            {
                icon.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                activate = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                activate = false;
            }
        }
    }
}