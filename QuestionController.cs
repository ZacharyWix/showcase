using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


/*
*
* This script's purpose is to manage the Questions in our game's conversation system.
* A Question is a Unity ScriptableObject with text for the Question's text, as well as
* a list of answers to choose between. Depending on the QuestionType enum value of the
* Question, these answers are displayed as either multiple choice, or toggle (choose all
* that apply) answers. If the QuestionType is FreeResponse, then the answers aren't used,
* and instead a free response box appears. Nothing is done with the text of the free response
* box, since our clients didn't want to grade what people wrote in them anyways.
*
*/





public class QuestionController : MonoBehaviour
{
    public Question question;
    public Text questionText;

    [Tooltip("Multiple Choice Button Template")]
    public Button choiceButton;

    [Tooltip("Toggle Button Template")]
    public Toggle toggleButton; //toggle button template
    public Button confirmToggleAnswersButton;

    public InputField freeResponseField;
    public Button confirmFreeResponseButton;

    public ScoreChangeEvent scoreChangeEvent;
    public ConversationChangeEvent conversationChangeEvent;

    private List<ChoiceController> choiceControllers = new List<ChoiceController>();

    public AudioClipEvent audioClipEvent;

    // Called after the Conversation catches a QuestionEvent
    // Removes any lingering choices from previous Questions, Initializes the newQuestion, and then plays the audio clip
    public void Change(Question newQuestion)
    {
        RemoveChoices();
        question = newQuestion;
        gameObject.SetActive(true);
        Initialize();
        PlayAudioClip();
    }

    // Called after the Question prefab catches a ConversationChangeEvent
    // Deactivates the QuestionUI and deselects any UI elements that were selected
    public void Hide(Conversation conversation)
    {
        //RemoveChoices();
        gameObject.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    // Destroys any multiple choice or toggle question choices from the UI and then clears the choiceControllers list
    private void RemoveChoices()
    {
        foreach (ChoiceController c in choiceControllers)
            Destroy(c.gameObject);

        choiceControllers.Clear();
    }

    // Displays the text of the question in the QuestionUI, then sets up the QuestionUI depending on the QuestionType
    private void Initialize()
    {
        questionText.text = question.text;

        // MULTIPLE CHOICE QUESTION SETUP
        if(question.questionType == QuestionType.MultipleChoice)
        {
            // Add all of the multiple choice choices to the QuestionUI
            for(int index = 0; index < question.choices.Length; index++)
            {
                ChoiceController c = ChoiceController.AddChoiceButton(choiceButton, question.choices[index], index);
                choiceControllers.Add(c);
            }

            choiceButton.gameObject.SetActive(false);
            toggleButton.gameObject.SetActive(false);
            confirmToggleAnswersButton.gameObject.SetActive(false);
            confirmFreeResponseButton.gameObject.SetActive(false);
            freeResponseField.gameObject.SetActive(false);
        }
        // TOGGLE QUESTION SETUP
        else if(question.questionType == QuestionType.Toggle)
        {
            // Add all of the toggle choices to the QuestionUI
            for (int index = 0; index < question.choices.Length; index++)
            {
                ChoiceController c = ChoiceController.AddToggleButton(toggleButton, question.choices[index], index);
                choiceControllers.Add(c);
            }

            choiceButton.gameObject.SetActive(false);
            toggleButton.gameObject.SetActive(false);
            confirmFreeResponseButton.gameObject.SetActive(false);
            confirmToggleAnswersButton.gameObject.SetActive(true);
            freeResponseField.gameObject.SetActive(false);
        }
        // FREE RESPONSE QUESTION SETUP
        else if(question.questionType == QuestionType.FreeResponse)
        {
            choiceButton.gameObject.SetActive(false);
            toggleButton.gameObject.SetActive(false);
            confirmToggleAnswersButton.gameObject.SetActive(false);
            confirmFreeResponseButton.gameObject.SetActive(true);
            freeResponseField.gameObject.SetActive(true);
            freeResponseField.text = "";
        }
    }

    // If the audioClipEvent isn't null, then invoke the audioClipEvent
    private void PlayAudioClip()
    {
        if (audioClipEvent != null)
        {
            audioClipEvent.Invoke(question.audioClip);
        }
    }

    // Checks if the selected answers are correct, then invokes the conversationChangeEvent to advance to the next conversation
    public void confirmToggledAnswers()
    {
        checkToggledAnswers(); //Applies Score of selected/non-selected answers

        conversationChangeEvent.Invoke(question.toggleOrFreeQuestionConvo); //Advance to the post-toggle-question conversation
    }

    // Reset the freeResponseField to be empty, then invoke the conversationChangeEvent to advance to the next conversation
    // Note that we don't actually process the answers in the freeResponseField, so we simply erase the input
    public void confirmFreeResponse()
    {
        freeResponseField.text = "";

        conversationChangeEvent.Invoke(question.toggleOrFreeQuestionConvo); //Advance to the post free-response question convo
    }

    // Searches through the list of choices, choiceControllers, and then computes the score
    //  by determining if each choice was correctly selected
    public void checkToggledAnswers()
    {
        int scoreChange = 0;
        for(int index = 0; index < question.choices.Length; index++)
        {
            if (choiceControllers[index].getSelected() == false) //If the choice was not selected
            {
                if (choiceControllers[index].choice.score < 0) //If the choice was not correct
                    scoreChange -= choiceControllers[index].choice.score;
            }
            else if (choiceControllers[index].getSelected() == true) //If the choice was selected
            {
                if(choiceControllers[index].choice.score > 0) //If the choice was correct
                    scoreChange += choiceControllers[index].choice.score;
            }
        }

        scoreChangeEvent.Invoke(scoreChange); // Update the score
    }
}
