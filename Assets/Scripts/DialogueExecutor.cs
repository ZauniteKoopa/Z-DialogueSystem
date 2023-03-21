using System;
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
    [SerializeField]
    private AudioSource voiceSpeaker;
    [SerializeField]
    [Min(1)]
    private int numCharsPerByte = 1;
    [Header("Dialogue")]
    [SerializeField]
    private TMP_Text dialogueText;

    private int curDialogueLine = 0;
    private SimpleDialogueScene curScene;
    
    private Coroutine runningTextRevealSequence = null;

    
    
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

        // Set colors for speaking character: if gotten a null sprite, make the sprite disappear
        speakingCharacter.color = (speakingCharacter.sprite != null) ? Color.white : Color.clear;

        // Handle prev speaker if speaker disappears
        if (curDialogueLine > 0) {
            DialogueLine prevLine = curScene.getLine(curDialogueLine - 1);

            // If disappear after and on opposing side, clear the silent character. else, make the side character grayed if character didn't leave
            if (prevLine.disappearAfter && prevLine.leftSide != line.leftSide) {
                silentCharacter.color = Color.clear;
            } else if (silentCharacter.color != Color.clear){
                silentCharacter.color = Color.grey;
            }

        // Set to gray if user was still there. if user isn't there, just clear it
        } else {
            silentCharacter.color = (silentCharacter.color != Color.clear) ? Color.grey : Color.clear;
        }

        // Get voice audio settings for this: if a voice clip found for this line, always play that. Else, do a running voice byte if 1 exists
        bool runningVoiceByte = false;
        voiceSpeaker.Stop();

        if (line.voiceClip != null) {
            voiceSpeaker.clip = line.voiceClip;
            voiceSpeaker.Play();

        } else if (line.characterSpeaker.getVoiceByte() != null) {
            voiceSpeaker.clip = line.characterSpeaker.getVoiceByte();
            runningVoiceByte = true;

        }

        // Set text (Change this to show gradually)
        runningTextRevealSequence = StartCoroutine(textRevealSequence(line, runningVoiceByte));
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
            // Case where the revealing sequence is still running
            if (runningTextRevealSequence != null) {
                StopCoroutine(runningTextRevealSequence);
                runningTextRevealSequence = null;

                dialogueText.maxVisibleCharacters = curScene.getLine(curDialogueLine).dialogueLine.Length;

            // Case where the revealing sequence ended and you want to move on to the next dialogue line
            } else {
                // Move on to the next speaker
                curDialogueLine++;

                // Either end the scene or present the next line
                if (curDialogueLine == curScene.getLength()) {
                    onSceneEnd();
                } else {
                    presentLine(curScene.getLine(curDialogueLine));
                }
            }
        }
    }


    // Main private helper ienumerator sequence for revealing text
    //  Pre: dialogueLine cannot be null
    //  Post: reveals the text gradually with the speed given
    private IEnumerator textRevealSequence(DialogueLine line, bool voiceByteRunning) {
        // Set up loop
        int totalCharacters = line.dialogueLine.Length;
        float timePerChar = 1f / line.textSpeed;

        dialogueText.text = line.dialogueLine;
        dialogueText.maxVisibleCharacters = 1;

        if (voiceByteRunning && Char.IsLetter(line.dialogueLine, 0)) {
            voiceSpeaker.Play();
        }

        // Run loop
        for (int c = 2; c <= totalCharacters; c++) {
            yield return new WaitForSeconds(timePerChar);

            // Reveal character
            dialogueText.maxVisibleCharacters = c;

            // Play voice byte for every numCharsPerByte character that's a letter if voiceByteRunning
            if (voiceByteRunning && (c - 1) % numCharsPerByte == 0 && Char.IsLetter(line.dialogueLine, c - 1)) {
                voiceSpeaker.Play();
            }
        }

        runningTextRevealSequence = null;
    }

}
