using UnityEngine;
using Ink.Runtime;
using TMPro; // For displaying text

public class NPCDialogue : MonoBehaviour
{
    public string npcStoryPath; // Set this per NPC in the Inspector
    public GameObject visualCue;
    private bool playerInRange;

    void Start()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
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
