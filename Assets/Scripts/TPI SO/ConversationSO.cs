using System.Collections.Generic;
using UnityEngine;
using System;

namespace VARLab.DLX
{
    /// <summary>
    ///     Scriptable Object for managing conversation data used in the introduction sequence.
    ///     This defines the conversation structure when introducing yourself to the studio manager.
    /// </summary>
    /// <remarks>
    ///     Used by <see cref="Conversation"/> to display dialogue between the player and studio manager.
    /// </remarks>
    [CreateAssetMenu(fileName = "ConversationSO", menuName = "ScriptableObjects/ConversationSO")]
    public class ConversationSO : ScriptableObject
    {
        /// <summary>
        ///     Represents a single dialogue entry in the conversation.
        /// </summary>
        [Serializable]
        public struct Dialogue
        {
            [Tooltip("Image of the speaker")]
            public Sprite Avatar;
            
            [Tooltip("Name or title of the speaker")]
            public string Speaker;
            
            [TextArea(3, 10)]
            [Tooltip("The actual dialogue text")]
            public string Text;
            
            [Tooltip("Audio clip for this dialogue entry")]
            public AudioClip AudioClip;
            
            [Tooltip("Background color for the text bubble")]
            public Color TextBackgroundColour;
            
            [Tooltip("Border color for the avatar image")]
            public Color ImageBorderColour;
            
            [Tooltip("Right border color for the text bubble")]
            public Color TextBorderRight;
            
            [Tooltip("Left border color for the text bubble")]
            public Color TextBorderLeft;
            
            [Tooltip("Top border color for the text bubble")]
            public Color TextBorderTop;
            
            [Tooltip("Bottom border color for the text bubble")]
            public Color TextBorderBottom;
        }

        [Tooltip("Title that appears in the conversation window")]
        public string Name;
        
        [Tooltip("Description text for the task")]
        public string TaskText;
        
        [Tooltip("Text to display on the primary button")]
        public string ButtonText;
        
        [Tooltip("Whether to start dialogue on the left side of the screen")]
        public bool StartDialogueOnLeft = true;
        
        [Tooltip("List of dialogue entries that make up the conversation")]
        public List<Dialogue> dialogue = new List<Dialogue>();
    }
}
