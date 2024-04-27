using System;
using System.Collections.Generic;
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

    public void CreateNode(Vector2 nodePosition, CharacterSO characterData = default, string dialogueText = "Dialogue Text", string leftAnswer = "Left Answer", string rightAnswer = "Right Answer")
    {
        AddElement(CreateDeckNode(nodePosition, characterData, dialogueText, leftAnswer, rightAnswer));
    }

    public DeckNode CreateDeckNode(Vector2 nodePosition, CharacterSO characterData, string dialogueText, string leftAnswer, string rightAnswer)
    {
        var node = new DeckNode()
        {
            title = "",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = dialogueText,
            CharacterData = characterData,
            LeftAnswer = leftAnswer,
            RightAnswer = rightAnswer
        };

        //node.style.flexDirection = FlexDirection.ColumnReverse;
        node.topContainer.style.flexDirection = FlexDirection.Column;

        node.styleSheets.Add(Resources.Load<StyleSheet>("DeckNode"));

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.titleContainer.Add(inputPort);

        AddCharacterDetails(node);

        AddDialogText(node);

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
        leftOutputNode.style.left = 0f;
        deckNode.outputContainer.Add(leftOutputNode);
        deckNode.RefreshExpandedState();

        var rightOutputNode = GeneratePort(deckNode, Direction.Output);
        rightOutputNode.portName = "Right";
        //leftOutputNode.style.position = Position.Absolute;
        //leftOutputNode.style.left = deckNode.style.width.value;
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
        var leftAnswerTextField = new TextField(string.Empty);
        leftAnswerTextField.RegisterValueChangedCallback(evt => 
        {
            node.LeftAnswer = evt.newValue;
        });
        leftAnswerTextField.SetValueWithoutNotify(node.LeftAnswer);

        node.topContainer.Add(leftAnswerTextField);
        node.RefreshExpandedState();

        var rightAnswerTextField = new TextField(string.Empty);
        rightAnswerTextField.RegisterValueChangedCallback(evt => 
        {
            node.RightAnswer = evt.newValue;
        });
        rightAnswerTextField.SetValueWithoutNotify(node.RightAnswer);

        node.topContainer.Add(rightAnswerTextField);
        node.RefreshExpandedState();
    }

    private void AddDialogText(DeckNode node)
    {
        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt => 
        {
            node.DialogueText = evt.newValue;
        });
        textField.SetValueWithoutNotify(node.DialogueText);

        node.topContainer.Add(textField);
        node.RefreshExpandedState();
    }

    private void AddCharacterDetails(DeckNode node)
    {
        node.title = node.CharacterData?.characterName ?? "";
        var characterAvatar = new Image();
        characterAvatar.style.height = defaultAvatarImageSize.y;
        characterAvatar.style.width = defaultAvatarImageSize.x;
        //characterAvatar.StretchToParentSize();

        var soField = new ObjectField
        {
            objectType = typeof(CharacterSO),
            value = node.CharacterData
        };

        soField.RegisterValueChangedCallback(evt =>
        {
            node.CharacterData = (CharacterSO)evt.newValue;
            characterAvatar.image = node.CharacterData.characterAvatar;
        });
        
        characterAvatar.image = node.CharacterData?.characterAvatar ?? null;

        node.topContainer.Add(characterAvatar);
        node.topContainer.Add(soField);
        node.RefreshExpandedState();
    }
}
