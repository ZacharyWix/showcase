using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// QuestionEvents are caught by the Conversation prefab to switch between conversation and questions
[System.Serializable]
public class QuestionEvent : UnityEvent<Question> { } 

// AudioClipEvents are caught by the Conversation or Question prefabs to switch the AudioClip in the AudioController script
[System.Serializable]
public class AudioClipEvent : UnityEvent<AudioClip> { }

// ConversationLineEvents are caught by the Conversation prefab to execute the event with the correct eventIndex:(int)
[System.Serializable]
public class ConversationLineEvent : UnityEvent<int> { }


public class ConversationController : MonoBehaviour
{
    public Conversation conversation; // Active conversation

    public QuestionEvent questionEvent;
    public AudioClipEvent audioClipEvent;
    public ConversationLineEvent conversationLineEvent;

    public GameObject speaker1;
    public GameObject speaker2;

    private SpeakerUIController speakerUI1;
    private SpeakerUIController speakerUI2;

    private int activeLineIndex = 0;
    private bool conversationStarted = false;

    // Input: Conversation nextConversation
    // Changes the active conversation to nextConversation, and sets it up for use
    public void ChangeConversation(Conversation nextConversation)
    {
        conversationStarted = false;
        conversation = nextConversation;
        AdvanceLine();
    }

    // Runs immediately before the first frame of execution
    // Retrieves the conversation speaker UI for speaker 1 and 2
    private void Start()
    {
        speakerUI1 = speaker1.GetComponent<SpeakerUIController>();
        speakerUI2 = speaker2.GetComponent<SpeakerUIController>();
    }

    // Runs on every frame of execution
    // Checks for keyboard input to control conversations
    private void Update()
    {
        if (Input.GetKeyDown("space"))
            AdvanceLine();
        else if (Input.GetKeyDown("x"))
            EndConversation();
    }

    // Ends the conversation chain. Only called if user manually ends the conversation
    //  or if the end of the conversation is reached without a follow-up question or conversation
    public void EndConversation()
    {
        conversation = null;
        conversationStarted = false;
        speakerUI1.Hide();
        speakerUI2.Hide();
    }

    // Starts the conversation and assigns characters to the speakerUIs
    private void Initialize()
    {
        conversationStarted = true;
        activeLineIndex = 0;
        speakerUI1.Speaker = conversation.speaker1;
        speakerUI2.Speaker = conversation.speaker2;
    }

    // Advances the conversation by 1 line while displaying the line, playing audio, and invoking conversationLineEvents
    public void AdvanceLine()
    {
        if (conversation == null) return;
        if (!conversationStarted) Initialize();

        if (activeLineIndex < conversation.lines.Length)
        {
            DisplayLine();
            PlayAudioClip();
            conversationLineEvent.Invoke(conversation.lines[activeLineIndex].eventIndex);

            activeLineIndex += 1;
        }
        else
            AdvanceConversation(); // If the current conversation is finished, then swap to the next conversation/question or end the Dialogue
    }

    // Displays the text of the line and character name in the Conversation UI text boxes
    private void DisplayLine()
    {
        Line line = conversation.lines[activeLineIndex];
        Character character = line.character;

        if (speakerUI1.SpeakerIs(character))
        {
            SetDialogue(speakerUI1, speakerUI2, line.text);
        }
        else
        {
            SetDialogue(speakerUI2, speakerUI1, line.text);
        }
    }

    // Retrieves the current line of the conversation and invokes the audio event to play the voice clip for it
    private void PlayAudioClip()
    {
        if (audioClipEvent != null)
        {
            Line line = conversation.lines[activeLineIndex];
            audioClipEvent.Invoke(line.audioClip);
        }
    }

    // Called when a conversation has finished.
    // Attempts to switch the conversation to a follow up question, and if there isn't one, then tries to switch
    //  to a follow up conversation. If there are no follow-ups, end the conversation system.
    private void AdvanceConversation()
    {
        if (conversation.question != null)
        {
            questionEvent.Invoke(conversation.question);
        }
        else if (conversation.nextConversation != null)
        {
            ChangeConversation(conversation.nextConversation);
        }
        else
        {
            EndConversation();
        }
    }

    // Shows the activeSpeakerUI, Hides the inactiveSpeakerUI, and displays text on the activeSpeakerUI
    void SetDialogue(SpeakerUIController activeSpeakerUI, SpeakerUIController inactiveSpeakerUI, string text)
    {
        activeSpeakerUI.Dialogue = text;
        activeSpeakerUI.Show();
        inactiveSpeakerUI.Hide();
    }
}
