using UnityEngine;
using Ink.Runtime;
using TMPro; // For displaying text

public class NPCDialogue : MonoBehaviour
{
    public string npcStoryPath; // Set this per NPC in the Inspector
    public GameObject visualCue;
    private bool playerInRange;
    public Transform player;
    private Quaternion defaultRotation;

    void Start()
    {
        playerInRange = false;
        visualCue.SetActive(false);
        defaultRotation = transform.rotation;
    }

    private void Update()
    {
        if (playerInRange)
        {
            RotateTowardsPlayer();

            if (!DialogueManager.GetInstance().dialogueIsPlaying)
            {
                visualCue.SetActive(true);

                if (Input.GetKeyDown(KeyCode.F))
                {
                    StartDialogue();
                }
            }
            else
            {
                visualCue.SetActive(false);
            }
        }
        else
        {
            visualCue.SetActive(false);
            ReturnToDefaultRotation();
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Keep rotation only on the Y-axis
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Smooth rotation
    }

    private void ReturnToDefaultRotation()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, Time.deltaTime * 5f);
    }

    public void StartDialogue()
    {
        DialogueManager.GetInstance().EnterDialogueMode(npcStoryPath);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
