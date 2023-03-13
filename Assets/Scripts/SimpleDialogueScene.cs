using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

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
    // Serialized Background elements
    [Header("Background elements")]
    [SerializeField]
    private AudioClip backgroundMusic;
    [SerializeField]
    private Sprite backgroundImage;

    // Starting Characters
    [Header("Left Starting Character")]
    [SerializeField]
    private CharacterPack leftCharacter;
    [SerializeField]
    private string leftCharacterEmotion;

    [Header("Right Starting Character")]
    [SerializeField]
    private CharacterPack rightCharacter;
    [SerializeField]
    private string rightCharacterEmotion;


    // Flags
    [Header("Flags")]
    [SerializeField]
    private bool lingerLastLine = false;

    // Actual lines
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



    // Main accessors
    public bool doesLastLineLinger() {
        return lingerLastLine;
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
        private SerializedProperty leftCharacter;
        private SerializedProperty leftCharacterEmotion;
        private SerializedProperty rightCharacter;
        private SerializedProperty rightCharacterEmotion;
        private SerializedProperty lingerLastLine;
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
            leftCharacter = serializedObject.FindProperty("leftCharacter");
            leftCharacterEmotion = serializedObject.FindProperty("leftCharacterEmotion");
            rightCharacter = serializedObject.FindProperty("rightCharacter");
            rightCharacterEmotion = serializedObject.FindProperty("rightCharacterEmotion");
            lingerLastLine = serializedObject.FindProperty("lingerLastLine");
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
                    // Indicate which element this is
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        "Element " + index,
                        EditorStyles.boldLabel
                    );
                    rect.y += EditorGUIUtility.singleLineHeight;

                    // Get properties
                    var curLine = lines.GetArrayElementAtIndex(index);
                    var characterSpeaker = curLine.FindPropertyRelative("characterSpeaker");
                    CharacterPack curCharacter = characterSpeaker.objectReferenceValue as CharacterPack;
                    var enumerator = curLine.GetEnumerator();

                    while (enumerator.MoveNext()) {
                        var curProperty = enumerator.Current as SerializedProperty;
                        var curHeight = EditorGUI.GetPropertyHeight(curProperty);

                        // Case for custom emotion dropdown
                        if (curProperty.name == "emotion") {
                            string[] options = (curCharacter != null) ? curCharacter.emotionList : new string[] {"EMPTY"};
                            int emotionIndex = (curCharacter != null) ? Array.FindIndex(curCharacter.emotionList, e => e == curProperty.stringValue) : 0;
                            emotionIndex = (emotionIndex < 0) ? 0 : emotionIndex;
                            emotionIndex = EditorGUI.Popup(
                                new Rect(rect.x, rect.y, rect.width, curHeight),
                                "Emotion",
                                emotionIndex,
                                options
                            );
                            curProperty.stringValue = (curCharacter != null) ? curCharacter.emotionList[emotionIndex] : "EMPTY";

                        // Everything else
                        } else {
                            EditorGUI.PropertyField(
                                new Rect(rect.x, rect.y, rect.width, curHeight),
                                curProperty
                            );
                        }

                        rect.y += curHeight;
                    }
                },

                // Getting the correct height
                // Get the correct display height of elements in the list
                // according to their values
                // in this case e.g. we add an additional line as a little spacing between elements
                elementHeightCallback = index => {
                    var curLine = lines.GetArrayElementAtIndex(index);
                    float height = EditorGUIUtility.singleLineHeight * 3;

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

            // Background
            EditorGUILayout.PropertyField(backgroundMusic);
            EditorGUILayout.PropertyField(backgroundImage);

            // Left character
            CharacterPack leftCharacterPack = leftCharacter.objectReferenceValue as CharacterPack;
            EditorGUILayout.PropertyField(leftCharacter);
            createCharacterEmotionDropdown(leftCharacterPack, leftCharacterEmotion);

            // Right character
            CharacterPack rightCharacterPack = rightCharacter.objectReferenceValue as CharacterPack;
            EditorGUILayout.PropertyField(rightCharacter);
            createCharacterEmotionDropdown(rightCharacterPack, rightCharacterEmotion);

            // Emotions
            EditorGUILayout.PropertyField(lingerLastLine);

            // Lines
            linesDisplay.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }


        // Private helper function to just create emotion dropdown from character
        private void createCharacterEmotionDropdown(CharacterPack curCharacter, SerializedProperty curProperty) {
            string[] options = (curCharacter != null) ? curCharacter.emotionList : new string[] {"EMPTY"};
            int emotionIndex = (curCharacter != null) ? Array.FindIndex(curCharacter.emotionList, e => e == curProperty.stringValue) : 0;
            emotionIndex = (emotionIndex < 0) ? 0 : emotionIndex;
            emotionIndex = EditorGUILayout.Popup(
                "Emotion",
                emotionIndex,
                options
            );
            curProperty.stringValue = (curCharacter != null) ? curCharacter.emotionList[emotionIndex] : "EMPTY";
        }

    }

#endif
