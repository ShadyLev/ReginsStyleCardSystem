using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _moneyModifierText;
    [SerializeField] TextMeshProUGUI _fameModifierText;
    [SerializeField] TextMeshProUGUI _crewModifierText;
    [SerializeField] TextMeshProUGUI _shipModifierText;

    private ModifierData _playerData;

    // Start is called before the first frame update
    void Start()
    {
        _playerData.MoneyModifier = 100;
        _playerData.FameModifier = 100;
        _playerData.CrewModifier = 100;
        _playerData.ShipModifier = 100;

        UpdateText(_moneyModifierText, _playerData.MoneyModifier);
        UpdateText(_fameModifierText, _playerData.FameModifier);               
        UpdateText(_crewModifierText, _playerData.CrewModifier);
        UpdateText(_shipModifierText, _playerData.ShipModifier);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AdjustModifier(ModifierData modifier)
    {
        _playerData.MoneyModifier += modifier.MoneyModifier;
        UpdateText(_moneyModifierText, _playerData.MoneyModifier);
        _playerData.FameModifier += modifier.FameModifier;
        UpdateText(_fameModifierText, _playerData.FameModifier);               
        _playerData.CrewModifier += modifier.CrewModifier;
        UpdateText(_crewModifierText, _playerData.CrewModifier);
        _playerData.ShipModifier += modifier.ShipModifier;
        UpdateText(_shipModifierText, _playerData.ShipModifier);
    }

    private void UpdateText(TextMeshProUGUI text, int value)
    {
        text.text = value.ToString();
    }
}
