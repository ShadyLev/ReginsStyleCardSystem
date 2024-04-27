using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class DeckNodeData
{
    public string Guid;
    public CharacterSO characterSO;
    public string DialogueText;
    public string LeftAnswer;
    public string RightAnswer;
    public Vector2 Position;
}
