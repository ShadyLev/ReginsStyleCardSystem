using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CharacterScriptableObject", order = 1)]
public class CharacterSO : ScriptableObject
{
    public Texture2D characterAvatar;

    public string characterName;
}
