using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckNode : Node
{
    public string GUID;
    public CharacterSO CharacterData;
    public string DialogueText;
    public string LeftAnswer;
    public string RightAnswer;
    public bool EntryPoint = false;
}
