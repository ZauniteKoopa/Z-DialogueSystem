using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class DialogueExecutor : MonoBehaviour
{
    // Events
    public UnityEvent dialogueSceneEnd;
    public UnityEvent dialogueSceneStart;

    // Serialized Properties
    [SerializeField]
    private PlayerInput inputHandler;
    [Header("Image slots")]
    [SerializeField]
    private Image leftCharacterSlot;
    [SerializeField]
    private Image rightCharacterSlot;
    [SerializeField]
    private Image backgroundSlot;

    [Header("Audio Slots")]
    [SerializeField]
    private AudioSource backgroundMusicSpeaker;

    [Header("Dialogue")]
    [SerializeField]
    private TMP_Text dialogueText;

    private int curDialogueLine = 0;
    private SimpleDialogueScene curScene;


    
    
    // Main function to start a dialogue scene
    //  Pre: entered dialogueScene is not null, user controls are connected to dialogue scene start event so that it's all disabled
    //  Post: Dialogue scene starts
    public void startScene(SimpleDialogueScene scene) {
        // Initialize scene
        curScene = scene;
        scene.setUpExecutor(leftCharacterSlot, rightCharacterSlot, backgroundSlot, backgroundMusicSpeaker);
        curDialogueLine = 0;

        // Enable input and make this executor visible
        inputHandler.enabled = true;
        gameObject.SetActive(true);

        // Present the first line
        presentLine(curScene.getLine(curDialogueLine));

        dialogueSceneStart.Invoke();
    }


    // Main private helper function for presenting a line
    private void presentLine(DialogueLine line) {
        // Check which character is silent and which is active
        Image speakingCharacter = (line.leftSide) ? leftCharacterSlot : rightCharacterSlot;
        Image silentCharacter = (line.leftSide) ? rightCharacterSlot : leftCharacterSlot;

        // Set active character to sprite
        speakingCharacter.sprite = line.characterSpeaker.getExpression(line.emotion);

        // Set colors
        speakingCharacter.color = Color.white;

        // Handle prev speaker if speaker disappears
        if (curDialogueLine > 0) {
            DialogueLine prevLine = curScene.getLine(curDialogueLine - 1);
            if (prevLine.disappearAfter && prevLine.leftSide != line.leftSide) {
                silentCharacter.color = Color.clear;
            } else {
                silentCharacter.color = Color.grey;
            }

        // Set to gray by default
        } else {
            silentCharacter.color = Color.grey;
        }

        // Set text (Change this to show gradually)
        dialogueText.text = line.dialogueLine;
    }


    // Main private helper function when scene ends
    private void onSceneEnd() {
        curScene = null;
        inputHandler.enabled = false;
        gameObject.SetActive(false);

        dialogueSceneEnd.Invoke();
    }


    // Main event handler function for player input
    //  Pre: player pressed the advance key
    //  Post: advance the dialogue
    public void handleAdvance(InputAction.CallbackContext context) {
        if (context.started) {
            // Move on to the next speaker
            curDialogueLine++;

            if (curDialogueLine == curScene.getLength()) {
                onSceneEnd();
            } else {
                presentLine(curScene.getLine(curDialogueLine));
            }

        }
    }

}
