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

    // Touples are not serializable so im splitting them into two
    public ModifierData leftModifier;
    public ModifierData rightModifier;

    public Vector2 Position;
}

[Serializable]
public struct ModifierData
{
    public int MoneyModifier;
    public int FameModifier;
    public int CrewModifier;
    public int ShipModifier;
}
