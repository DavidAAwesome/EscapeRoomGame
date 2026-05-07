using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

namespace KTintercativeProp
{
    public class AnimProp : MonoBehaviour, IInteractable
    {
        public Animator animator;
        public bool activate;
        public bool toggleState;
        public GameObject icon;

        public AudioClip sound_A;
        public AudioClip sound_B;
        
        [SerializeField] private float noiseRadius = 4f;

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
                // if (Input.GetKeyDown(KeyCode.E))
                // {
                //     if (toggleState)
                //     {
                //         animator.Play("A");
                //         audioSource.clip = sound_A;
                //         audioSource.Play();
                //         toggleState = false;
                //     }
                //     else
                //     {
                //         animator.Play("B");
                //         audioSource.clip = sound_B;
                //         audioSource.Play();
                //         toggleState = true;
                //     }
                // }
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

        public string GetPrompt(InteractionSystem player)
        {
            return null;
        }

        public void OnInteract(InteractionSystem player)
        {
            if (toggleState)
            {
                animator.Play("A");
                audioSource.clip = sound_A;
                audioSource.Play();
                toggleState = false;
                EmitNoise();
            }
            else
            {
                animator.Play("B");
                audioSource.clip = sound_B;
                audioSource.Play();
                toggleState = true;
                EmitNoise();
            }
        }
        
        private void EmitNoise()
        {
            float radius = noiseRadius;

            Collider[] hits = Physics.OverlapSphere(transform.position, radius);

            foreach (Collider hit in hits)
            {
                GrannyAI granny = hit.GetComponent<GrannyAI>();

                if (granny != null)
                    granny.AlertToSound(transform.position);
            }
        }
        
    }
    
}