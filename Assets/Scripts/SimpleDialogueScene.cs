using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditorInternal;
#endif

// Serializable object to keep track of dialogue lines
[System.Serializable]
public class DialogueLine {
    public CharacterPack characterSpeaker;
    public string emotion;

    public bool leftSide;
    public bool disappearAfter;
    public AudioClip voiceClip;

    [TextArea]
    public string dialogueLine;
}


// Actual dialogue scene class
[CreateAssetMenu(menuName = "Z-Dialogue/SimpleDialogueScene")]
public class SimpleDialogueScene : ScriptableObject
{
    // Serialized fields
    [SerializeField]
    private AudioClip backgroundMusic;
    [SerializeField]
    private Sprite backgroundImage;
    [SerializeField]
    private DialogueLine[] lines;


    // Main accessor function to get lines, given an index
    public DialogueLine getLine(int index) {
        Debug.Assert(0 <= index && index < lines.Length);

        return lines[index];
    }


    // Main accessor method to the length of the dialogue scene
    public int getLength() {
        return lines.Length;
    }
}




// ---------------------------------------------------------------------------
// Custom editor for SimpleDialogueScene object
// ---------------------------------------------------------------------------
#if UNITY_EDITOR
    [CustomEditor(typeof(SimpleDialogueScene))]
    public class SimpleDialogueSceneEditor : Editor {
        // Serialized properties
        private SerializedProperty backgroundMusic;
        private SerializedProperty backgroundImage;
        private SerializedProperty lines;

        // List displays
        private ReorderableList linesDisplay;

        // Actual target
        private SimpleDialogueScene sceneTarget;


        // On enable, setup
        private void OnEnable() {
            // Get data from the target
            sceneTarget = (SimpleDialogueScene) target;
            backgroundMusic = serializedObject.FindProperty("backgroundMusic");
            backgroundImage = serializedObject.FindProperty("backgroundImage");
            lines = serializedObject.FindProperty("lines");


            // Set up lines display
            linesDisplay = new ReorderableList(serializedObject, lines) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                // Header just displays expressions display name
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, lines.displayName),

                // How elements are displayed
                drawElementCallback = (rect, index, focused, active) => {
                    // Get properties
                    var curLine = lines.GetArrayElementAtIndex(index);
                    var characterSpeaker = curLine.FindPropertyRelative("characterSpeaker");
                    var emotion = curLine.FindPropertyRelative("emotion");
                    var leftSide = curLine.FindPropertyRelative("leftSide");
                    var disappearAfter = curLine.FindPropertyRelative("disappearAfter");
                    var voiceClip = curLine.FindPropertyRelative("voiceClip");
                    var dialogueLine = curLine.FindPropertyRelative("dialogueLine");

                    // Create character speaker
                    var curHeight = EditorGUI.GetPropertyHeight(characterSpeaker);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        characterSpeaker
                    );
                    CharacterPack curCharacter = characterSpeaker.objectReferenceValue as CharacterPack;
                    rect.y += curHeight;

                    // Create emotion popup
                    curHeight = EditorGUI.GetPropertyHeight(emotion);
                    string[] options = (curCharacter != null) ? curCharacter.emotionList : new string[] {"EMPTY"};
                    int emotionIndex = (curCharacter != null) ? Array.FindIndex(curCharacter.emotionList, e => e == emotion.stringValue) : 0;
                    emotionIndex = (emotionIndex < 0) ? 0 : emotionIndex;
                    emotionIndex = EditorGUI.Popup(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        "Emotion",
                        emotionIndex,
                        options
                    );
                    emotion.stringValue = (curCharacter != null) ? curCharacter.emotionList[emotionIndex] : "EMPTY";
                    rect.y += curHeight;

                    // Create leftSide
                    curHeight = EditorGUI.GetPropertyHeight(leftSide);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        leftSide
                    );
                    rect.y += curHeight;

                    // Create disappear after
                    curHeight = EditorGUI.GetPropertyHeight(disappearAfter);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        disappearAfter
                    );
                    rect.y += curHeight;

                    // Create voice clip
                    curHeight = EditorGUI.GetPropertyHeight(voiceClip);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        voiceClip
                    );
                    rect.y += curHeight;

                    // Create dialogue line
                    curHeight = EditorGUI.GetPropertyHeight(dialogueLine);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, curHeight),
                        dialogueLine
                    );
                    rect.y += curHeight;
                },

                // Getting the correct height
                // Get the correct display height of elements in the list
                // according to their values
                // in this case e.g. we add an additional line as a little spacing between elements
                elementHeightCallback = index => {
                    var curLine = lines.GetArrayElementAtIndex(index);
                    float height = EditorGUIUtility.singleLineHeight;

                    var enumerator = curLine.GetEnumerator();
                    while (enumerator.MoveNext()) {
                        curLine = enumerator.Current as SerializedProperty;
                        height += EditorGUI.GetPropertyHeight(curLine);
                    }

                    return height;
                }
            };
        }


        // Called when inspector is open and scriptable object is selected
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(backgroundMusic);
            EditorGUILayout.PropertyField(backgroundImage);
            linesDisplay.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

    }

#endif
