using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KTintercativeProp
{
    public class LightProp : MonoBehaviour
    {
        public bool activate;
        public bool toggleState;
        public GameObject icon;
        public GameObject lightObj;

        public Renderer lantern;
        public int uvCN;

        void Update()
        {
            if (activate)
            {
                icon.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (toggleState)
                    {
                        lightObj.SetActive(false);
                        if (lantern != null)
                        {
                            lantern.materials[uvCN].SetColor("_EmissionColor", Color.black);
                        }
                        toggleState = false;
                    }
                    else
                    {
                        lightObj.SetActive(true);
                        if (lantern != null)
                        {
                            lantern.materials[uvCN].SetColor("_EmissionColor", Color.white);
                        }
                        toggleState = true;
                    }
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