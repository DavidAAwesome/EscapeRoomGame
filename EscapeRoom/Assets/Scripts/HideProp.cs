using UnityEngine;

namespace KTintercativeProp
{
    public class HideProp : MonoBehaviour, IInteractable
    {
        [Header("Hiding Spots")]
        [SerializeField] private Transform hideSpot;
        [SerializeField] private Transform exitSpot;

        [Header("UI")]
        [SerializeField] private GameObject icon;
        [SerializeField] private string enterPrompt = "Press E to hide";
        [SerializeField] private string exitPrompt = "Press E to leave";

        [Header("Audio")]
        [SerializeField] private AudioClip enterSound;
        [SerializeField] private AudioClip exitSound;

        [Header("Noise")]
        [SerializeField] private float noiseRadius = 4f;

        private AudioSource audioSource;
        private bool playerNearby;
        private bool playerInside;
        private PlayerController hiddenPlayer;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (hideSpot == null)
                hideSpot = transform;

            if (exitSpot == null)
                exitSpot = transform;
        }

        void Update()
        {
            if (icon != null)
                icon.SetActive(playerNearby || playerInside);
        }

        public string GetPrompt(InteractionSystem player)
        {
            return playerInside ? exitPrompt : enterPrompt;
        }

        public void OnInteract(InteractionSystem player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController == null)
                return;

            if (!playerInside)
            {
                hiddenPlayer = playerController;
                playerInside = true;

                PlaySound(enterSound);
                playerController.HidePlayer(hideSpot);

                // Optional: hiding makes a small noise
                EmitNoise();
            }
            else
            {
                playerInside = false;

                PlaySound(exitSound);

                if (hiddenPlayer != null)
                    hiddenPlayer.LeaveHiding(exitSpot);

                hiddenPlayer = null;

                // Leaving can also alert nearby enemies
                EmitNoise();
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource == null || clip == null)
                return;

            audioSource.PlayOneShot(clip);
        }

        private void EmitNoise()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, noiseRadius);

            foreach (Collider hit in hits)
            {
                GrannyAI granny = hit.GetComponent<GrannyAI>();

                if (granny != null)
                    granny.AlertToSound(transform.position);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                playerNearby = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && !playerInside)
                playerNearby = false;
        }
    }
}