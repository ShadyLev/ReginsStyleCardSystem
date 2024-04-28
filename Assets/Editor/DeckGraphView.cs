using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;

public class DeckGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(400, 400);
    public readonly Vector2 defaultAvatarImageSize = new Vector2(100,100); // in pixels
    private NodeSearchWindow _searchWindow;

    public DeckGraphView(EditorWindow editorWindow)
    {
        styleSheets.Add(Resources.Load<StyleSheet>("DeckGraph"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        AddElement(GenerateEntryPointNode());
        AddSearchWindow(editorWindow);
    }

    private void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(this, editorWindow);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }


    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) => 
        {
            if(startPort != port && startPort.node != port.node)
            {
                compatiblePorts.Add(port);
            }
        });

        return compatiblePorts;
    }

    private DeckNode GenerateEntryPointNode()
    {
        var node = new DeckNode()
        {
            title = "Start",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "Test entry",
            EntryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Output";
        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));

        return node;
    }

    public void CreateNode(Vector2 nodePosition,
        CharacterSO characterData = default, string dialogueText = "Dialogue Text", 
        string leftAnswer = "Left Answer", string rightAnswer = "Right Answer",
        ModifierData leftModifier = default, ModifierData rightModifier = default)
    {
        AddElement(CreateDeckNode(nodePosition, characterData, dialogueText, leftAnswer, rightAnswer, leftModifier, rightModifier));
    }

    public DeckNode CreateDeckNode(Vector2 nodePosition, CharacterSO characterData,
     string dialogueText, string leftAnswer, string rightAnswer,
     ModifierData leftModifier, ModifierData rightModifier)
    {
        var node = new DeckNode()
        {
            title = "",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = dialogueText,
            CharacterData = characterData,
            LeftAnswer = leftAnswer,
            RightAnswer = rightAnswer,
            leftModifier = leftModifier,
            rightModifier = rightModifier
        };

        //node.style.flexDirection = FlexDirection.ColumnReverse;
        node.topContainer.style.flexDirection = FlexDirection.Column;

        node.styleSheets.Add(Resources.Load<StyleSheet>("DeckNode"));

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.titleContainer.Add(inputPort);

        var bgColor = new StyleColor();
        bgColor.value = new Color(0.188f, 0.188f, 0.18f);
        node.topContainer.style.backgroundColor = bgColor;

        AddCharacterDetails(node);

        AddDialogText(node);

        AddModifiers(node);

        AddAnswerFields(node);

        AddOutputPorts(node);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(nodePosition, defaultNodeSize));

        return node;
    }

    public void AddOutputPorts(DeckNode deckNode)
    {
        var leftOutputNode = GeneratePort(deckNode, Direction.Output);
        leftOutputNode.portName = "Left";
        leftOutputNode.style.position = Position.Absolute;
        leftOutputNode.style.left = 0;
        leftOutputNode.style.width = StyleKeyword.Auto;
        deckNode.outputContainer.Add(leftOutputNode);
        deckNode.RefreshExpandedState();

        var rightOutputNode = GeneratePort(deckNode, Direction.Output);
        rightOutputNode.portName = "Right";
        rightOutputNode.style.right = 0;
        rightOutputNode.style.width = StyleKeyword.Auto;
        deckNode.outputContainer.Add(rightOutputNode);

        deckNode.RefreshExpandedState();
        deckNode.RefreshPorts();
    }

    private Port GeneratePort(DeckNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Vertical, portDirection, capacity, typeof(float));
    }

    private void AddAnswerFields(DeckNode node)
    {
        node.topContainer.Add(Header("Anwsers"));

        var customElement = new VisualElement();
        customElement.style.flexDirection = FlexDirection.Row;

        var leftAnswerTextField = new TextField(string.Empty);
        leftAnswerTextField.RegisterValueChangedCallback(evt => 
        {
            node.LeftAnswer = evt.newValue;
        });
        leftAnswerTextField.SetValueWithoutNotify(node.LeftAnswer);
        leftAnswerTextField.style.flexGrow = 1;
        customElement.Add(leftAnswerTextField);
        node.RefreshExpandedState();

        var rightAnswerTextField = new TextField(string.Empty);
        rightAnswerTextField.RegisterValueChangedCallback(evt => 
        {
            node.RightAnswer = evt.newValue;
        });
        rightAnswerTextField.SetValueWithoutNotify(node.RightAnswer);
        rightAnswerTextField.style.flexGrow = 1;
        customElement.Add(rightAnswerTextField); 

        node.topContainer.Add(customElement);
        node.RefreshExpandedState();
    }

    private void AddModifiers(DeckNode node)
    {
        node.topContainer.Add(Header("Modifiers"));
        node.topContainer.Add(Divider(5));

        AddModifier(node, ModifiersEnum.Money);
        AddModifier(node, ModifiersEnum.Fame);
        AddModifier(node, ModifiersEnum.Crew);
        AddModifier(node, ModifiersEnum.Ship);

        node.topContainer.Add(Divider(15));
    }

    private void AddModifier(DeckNode node, ModifiersEnum modifier)
    {
        const int MAX_CHAR = 5;
        const int WIDTH = 40;

        var element = new VisualElement();
        element.style.flexDirection = FlexDirection.Row;
        var bgColor = new StyleColor();
        bgColor.value = new Color(0.31f, 0.31f, 0.31f);
        element.style.backgroundColor = bgColor;
        element.styleSheets.Add(Resources.Load<StyleSheet>("TextStyle"));

        // field = name = field
        var leftModifierField = new IntegerField(string.Empty);
        leftModifierField.RegisterValueChangedCallback(evt => 
        {
            SetNodeModifier(node, modifier, true, evt.newValue);
        });
        leftModifierField.styleSheets.Add(Resources.Load<StyleSheet>("TextStyle"));
        leftModifierField.SetValueWithoutNotify(GetNodeModifier(node, modifier, true));
        leftModifierField.maxLength = MAX_CHAR;
        leftModifierField.style.width = WIDTH;
        leftModifierField.AddToClassList("center-aligned-text");

        element.Add(leftModifierField);
        node.RefreshExpandedState();

        var text = new Label(modifier.ToString());
        text.style.unityFontStyleAndWeight = FontStyle.Bold;
        text.style.alignSelf = Align.Center;
        text.style.unityTextAlign = TextAnchor.MiddleCenter;
        text.style.flexGrow = 1;
        element.Add(text);
        node.RefreshExpandedState();

        var rightModifierField = new IntegerField(string.Empty);
        rightModifierField.styleSheets.Add(Resources.Load<StyleSheet>("TextStyle"));
        rightModifierField.RegisterValueChangedCallback(evt => 
        {
            SetNodeModifier(node, modifier, false, evt.newValue);
        });
        rightModifierField.SetValueWithoutNotify(GetNodeModifier(node, modifier, false));
        rightModifierField.style.alignSelf = Align.FlexEnd;
        rightModifierField.maxLength = MAX_CHAR;
        rightModifierField.style.width = WIDTH;
    
        rightModifierField.AddToClassList("center-aligned-text");

        element.Add(rightModifierField);
        node.RefreshExpandedState();

        node.topContainer.Add(element);
    }

    private void AddDialogText(DeckNode node)
    {
        node.topContainer.Add(Header("Dialogue text"));

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt => 
        {
            node.DialogueText = evt.newValue;
        });
        textField.SetValueWithoutNotify(node.DialogueText);
        textField.multiline = true;

        node.topContainer.Add(textField);
        node.RefreshExpandedState();

        node.topContainer.Add(Divider(15));
    }

    private void AddCharacterDetails(DeckNode node)
    {
        var element = new VisualElement();
        element.style.backgroundColor = Color.grey;

        node.title = node.CharacterData?.characterName ?? "";
        var characterAvatar = new Image();
        characterAvatar.style.height = defaultAvatarImageSize.y;
        characterAvatar.style.width = defaultAvatarImageSize.x;
        characterAvatar.style.alignSelf = Align.Center;
        //characterAvatar.StretchToParentSize();

        var soField = new ObjectField
        {
            objectType = typeof(CharacterSO),
            value = node.CharacterData
        };

        soField.RegisterValueChangedCallback(evt =>
        {
            var newSO = (CharacterSO)evt.newValue;
            node.CharacterData = (CharacterSO)evt.newValue;
            node.title = newSO.characterName;
            characterAvatar.image = node.CharacterData.characterAvatar;
        });
        
        characterAvatar.image = node.CharacterData?.characterAvatar ?? null;
        element.Add(characterAvatar);
        element.Add(soField);
        node.topContainer.Add(element);
        node.RefreshExpandedState();

        node.topContainer.Add(Divider(15));
        node.RefreshExpandedState();
    }

    // ALL THESE SHOULD BE IN A UTIL CLASS BUT I CBA HONESTLY
    private VisualElement Divider(int height)
    {
        var divider = new VisualElement();
        divider.style.height = height;
        return divider;
    }

    private Label Header(string header)
    {
        var label = new Label(header);
        label.style.unityFontStyleAndWeight = FontStyle.Bold;
        label.style.alignSelf = Align.Center;
        return label;
    }

    void SetNodeModifier(DeckNode node, ModifiersEnum modifier, bool leftChoice, int newValue)
    {
        switch (modifier)
        {
            case ModifiersEnum.Money:
                if(leftChoice)
                    node.leftModifier.MoneyModifier = newValue;
                else
                    node.rightModifier.MoneyModifier = newValue;

                Debug.Log("SET MONEY TO: " + node.leftModifier.MoneyModifier);
                break;
            case ModifiersEnum.Fame:
                if(leftChoice)
                    node.leftModifier.FameModifier = newValue;
                else
                    node.rightModifier.FameModifier = newValue;            
                break;
            case ModifiersEnum.Crew:
                if(leftChoice)
                    node.leftModifier.CrewModifier = newValue;
                else
                    node.rightModifier.CrewModifier = newValue;            
                break;
            case ModifiersEnum.Ship:
                if(leftChoice)
                    node.leftModifier.ShipModifier = newValue;
                else
                    node.rightModifier.ShipModifier = newValue;            
                break;
        }
    }

    int GetNodeModifier(DeckNode node, ModifiersEnum modifier, bool leftChoice)
    {
        switch (modifier)
        {
            case ModifiersEnum.Money:
                if(leftChoice)
                    return node.leftModifier.MoneyModifier;
                else
                    return node.rightModifier.MoneyModifier;
            case ModifiersEnum.Fame:
                if(leftChoice)
                    return node.leftModifier.FameModifier;
                else
                    return node.rightModifier.FameModifier;            
            case ModifiersEnum.Crew:
                if(leftChoice)
                    return node.leftModifier.CrewModifier;
                else
                    return node.rightModifier.CrewModifier;            
            case ModifiersEnum.Ship:
                if(leftChoice)
                    return node.leftModifier.ShipModifier;
                else
                    return node.rightModifier.ShipModifier;  
            default:
                return 0;          
        }
    }
}
