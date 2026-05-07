using UnityEngine;

namespace KTintercativeProp
{
    public class HideProp : MonoBehaviour, IInteractable
    {
        [Header("Hiding")]
        [SerializeField] private Transform hideSpot;
        [SerializeField] private Transform exitSpot;

        [Header("Prompt")]
        [SerializeField] private string enterPrompt = "Press E to hide";
        [SerializeField] private string exitPrompt = "Press E to leave";
        [SerializeField] private GameObject icon;

        [Header("Audio")]
        [SerializeField] private AudioClip enterSound;
        [SerializeField] private AudioClip exitSound;

        [Header("Noise")]
        [SerializeField] private float noiseRadius = 4f;

        private AudioSource audioSource;
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
                icon.SetActive(playerInside);
        }

        public string GetPrompt(InteractionSystem player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController != null && playerController.IsHiding)
                return exitPrompt;

            return enterPrompt;
        }

        public void OnInteract(InteractionSystem player)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController == null)
                return;

            if (playerController.IsHiding)
            {
                playerController.LeaveHiding();
                playerInside = false;
                hiddenPlayer = null;
                PlaySound(exitSound);
                EmitNoise();
                return;
            }

            hiddenPlayer = playerController;
            playerInside = true;

            playerController.HidePlayer(hideSpot, exitSpot);
            PlaySound(enterSound);
            EmitNoise();
        }

        void GameStarting()
        {
            playerInside = false;
            hiddenPlayer = null;
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
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
    }
}