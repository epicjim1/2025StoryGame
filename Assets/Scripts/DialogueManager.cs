using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Ink.Runtime;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class DialogueManager : MonoBehaviour
{
    public TextAsset inkJSON;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private static DialogueManager instance;

    public CinemachineCamera npcCamera;
    public CinemachineCamera playerCamera;

    private void Awake()
    {
        if (instance == null)
        {
            Debug.Log("Found more than one Dialogue Manger in the scene");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying || currentStory.currentChoices.Count > 0)
        {
            return;        
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(string npcStoryPath)
    {
        currentStory = new Story(inkJSON.text);
        currentStory.ChoosePathString(npcStoryPath);
        dialoguePanel.SetActive(true);
        dialogueIsPlaying = true;

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialoguePanel.SetActive(false);
        dialogueIsPlaying = false;
        dialogueText.text = "";
        ResetCameras();
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            //Debug.Log("story continued");
            string text = currentStory.Continue();
            Debug.Log(text);

            dialogueText.text = text;
            
            //StartCoroutine(DisplayLine(text));
            SwitchCameraBasedOnSpeaker(text);
            DisplayChoices();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: " + currentChoices.Count);
        }

        EventSystem.current.SetSelectedGameObject(null);

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            choicesText[index].color = Color.white; // Default color

            List<string> tags = choice.tags;
            //Debug.Log(tags); 

            if (choice.tags != null && choice.tags.Count > 0)
            {
                if (tags.Contains("sticky"))
                {
                    choicesText[index].color = Color.yellow; // Set sticky choices to yellow
                }
            }

            //choicesText[index].text = choice.text;
            index++;
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        //StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        Debug.Log("Choice clicked: " + choiceIndex);
        List<Choice> currentChoices = currentStory.currentChoices;

        if (choiceIndex < 0 || choiceIndex >= currentChoices.Count)
        {
            Debug.LogError("Invalid choice index: " + choiceIndex);
            return;
        }

        currentStory.ChooseChoiceIndex(choiceIndex);

        // Hide choices after selection
        foreach (GameObject choice in choices)
        {
            choice.SetActive(false);
        }

        ContinueStory();
    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = "";
        bool isAddingRichTextTag = false;
        foreach (char letter in line.ToCharArray())
        {
            if (letter == '<' || isAddingRichTextTag)
            {
                dialogueText.text += letter;
                isAddingRichTextTag = true;
                if (letter == '>')
                {
                    isAddingRichTextTag = false;
                }
            }
            else
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(.05f);
            }
        }
    }

    private void SwitchCameraBasedOnSpeaker(string text)
    {
        if (text.StartsWith("NPC:"))  // If NPC is talking
        {
            npcCamera.Priority = 15;
            playerCamera.Priority = 5;
        }
        else if (text.StartsWith("Player:"))  // If Player is talking
        {
            playerCamera.Priority = 15;
            npcCamera.Priority = 5;
        }
    }

    private void ResetCameras()
    {
        npcCamera.Priority = 0;
        playerCamera.Priority = 0;
    }
}
