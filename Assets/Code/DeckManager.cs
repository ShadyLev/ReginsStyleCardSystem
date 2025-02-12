using System.Linq;
using CardSwipe;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public CardSwipeEffect _cardSwipeEffect;
    public PlayerManager playerManager;
    public DeckContainer deck;
    private int _deckNodeIndex;
    private DeckNodeData _currentNode;

    [SerializeField] TextMeshProUGUI _characterName;
    [SerializeField] TextMeshProUGUI _dialogueText;
    [SerializeField] TextMeshProUGUI _leftAnswerText;
    [SerializeField] TextMeshProUGUI _rightAnswerText;
    [SerializeField] Image _characterImage;

    // Start is called before the first frame update
    void Start()
    {
        _cardSwipeEffect = GetComponentInChildren<CardSwipeEffect>();
        _cardSwipeEffect.CardSelectAction += CardSelected;

        _deckNodeIndex = 0;
        _currentNode = deck.DeckNodeDatas[0];
        SetNewCard(deck.DeckNodeDatas[0]);
    }

    private void CardSelected()
    {
        ChooseAnswer(_cardSwipeEffect.SwipedLeft);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetNewCard(DeckNodeData card)
    {
        // We save the image as a texture2D so we need to create a sprite from it
        var texture2d = card.characterSO.characterAvatar;
        var spriteAvatar = Sprite.Create(texture2d, new Rect(0f,0f, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f), 100.0f);

        // Set all the text
        _characterImage.sprite = spriteAvatar;
        _characterName.text = card.characterSO.characterName;
        _dialogueText.text = card.DialogueText;
        _leftAnswerText.text = card.LeftAnswer;
        _rightAnswerText.text = card.RightAnswer;
    }

    public void ChooseAnswer(bool left)
    {
        var connections = deck.NodeLinks.Where(x => x.BaseNodeGuid == deck.DeckNodeDatas[_deckNodeIndex].Guid).ToList();
        NodeLinkData linkedNode;

        if(connections.Count == 0)
        {
            Debug.Log("Reached the end");
            return;
        }

        if(left)
            linkedNode = connections[0];
        else
            linkedNode = connections[1];

        var modifierToApply = left ? _currentNode.leftModifier : _currentNode.rightModifier;

        playerManager.AdjustModifier(modifierToApply);

        var targetNode = deck.DeckNodeDatas.First(x => x.Guid == linkedNode.TargetNodeGuid);
        _currentNode = targetNode;
        _deckNodeIndex++;
        SetNewCard(targetNode);
    }
}
