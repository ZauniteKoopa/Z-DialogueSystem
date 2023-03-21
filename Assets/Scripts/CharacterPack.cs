using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditorInternal;
#endif


[CreateAssetMenu(menuName = "Z-Dialogue/CharacterPack")]
public class CharacterPack : ScriptableObject
{
    // Serializable object to keep track of emotion pairs
    [System.Serializable]
    public class EmotionExpressionPair {
        public string emotion;
        public Sprite expression;
    }

    // Serialized fields that developers can edit
    public string[] emotionList = {"EMPTY"};
    public EmotionExpressionPair[] expressions;
    [SerializeField]
    private AudioClip voiceByte;

    // Runtime dictionary
    private Dictionary<string, Sprite> runtimeExpressionDictionary;


    // Private helper function for initializing the dictionary
    private void initializeDictionary() {
        if (expressions.Length == 0) {
            Debug.LogWarning("No sprite expressions found within character pack! Did you forget to add expressions?");
        }

        runtimeExpressionDictionary = new Dictionary<string, Sprite>();
        foreach (EmotionExpressionPair ePair in expressions) {
            runtimeExpressionDictionary.Add(ePair.emotion, ePair.expression);
        }
    }


    // Main function to get an expression for a given emotion
    //  Pre: emotion is a string that shows how a character is feeling
    //  Post: returns the expression for an emotion. if it doesn't exist, return null
    public Sprite getExpression(string emotion) {
        if (runtimeExpressionDictionary == null) {
            initializeDictionary();
        }

        if (runtimeExpressionDictionary.ContainsKey(emotion)) {
            return runtimeExpressionDictionary[emotion];
        }

        return null;
    }


    // Main accessor function to get the sound clip
    //  Pre: none
    //  Post: get sound byte
    public AudioClip getVoiceByte() {
        return voiceByte;
    }
}



// Editor specific code for designers and developers for the character pack
#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterPack))]
    public class CharacterPackEditor : Editor {
        // Serialized clone properties
        private SerializedProperty emotionList;
        private SerializedProperty expressions;
        private SerializedProperty voiceByte;

        // Displays
        private ReorderableList expressionsDisplay;

        // Reference to the actual Character Pack
        private CharacterPack characterPackTarget;


        // On enable, set up
        private void OnEnable() {
            // Get data from the target
            characterPackTarget = (CharacterPack) target;
            emotionList = serializedObject.FindProperty(nameof(characterPackTarget.emotionList));
            expressions = serializedObject.FindProperty(nameof(characterPackTarget.expressions));
            voiceByte = serializedObject.FindProperty("voiceByte");

            // Set up reorderable list for expressions
            expressionsDisplay = new ReorderableList(serializedObject, expressions) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                // Header just displays expressions display name
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, expressions.displayName),

                // How elements are displayed
                drawElementCallback = (rect, index, focused, active) => {
                    // Get properties
                    var curExpressionPair = expressions.GetArrayElementAtIndex(index);
                    var emotion = curExpressionPair.FindPropertyRelative("emotion");
                    var expression = curExpressionPair.FindPropertyRelative("expression");

                    // Create emotion popup
                    var emotionPopupHeight = EditorGUI.GetPropertyHeight(emotion);
                    int emotionIndex = Array.FindIndex(characterPackTarget.emotionList, e => e == emotion.stringValue);
                    emotionIndex = (emotionIndex < 0) ? 0 : emotionIndex;
                    emotionIndex = EditorGUI.Popup(
                        new Rect(rect.x, rect.y, rect.width, emotionPopupHeight),
                        "Emotion",
                        emotionIndex,
                        characterPackTarget.emotionList
                    );
                    emotion.stringValue = characterPackTarget.emotionList[emotionIndex];

                    rect.y += emotionPopupHeight;

                    // Create expression
                    var expressionFieldHeight = EditorGUI.GetPropertyHeight(emotion);
                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width, emotionPopupHeight),
                        expression
                    );
                },

                // Getting the correct height
                // Get the correct display height of elements in the list
                // according to their values
                // in this case e.g. we add an additional line as a little spacing between elements
                elementHeightCallback = index => {
                    var element = expressions.GetArrayElementAtIndex(index);
                    var emotion = element.FindPropertyRelative("emotion");
                    var expression = element.FindPropertyRelative("expression");

                    return EditorGUI.GetPropertyHeight(emotion) + EditorGUI.GetPropertyHeight(expression) + EditorGUIUtility.singleLineHeight;
                }
            };

        }

        // Called when inspector is open and scriptable object is selected
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(voiceByte);
            EditorGUILayout.PropertyField(emotionList);
            expressionsDisplay.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
