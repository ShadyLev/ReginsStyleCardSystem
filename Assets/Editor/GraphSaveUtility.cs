using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphSaveUtility
{
    private DeckGraphView _targetGraphView;
    private DeckContainer _containerCache;
    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DeckNode> Nodes => _targetGraphView.nodes.ToList().Cast<DeckNode>().ToList();

    public static GraphSaveUtility GetInstance(DeckGraphView targetGarphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGarphView
        };
    }

        public void SaveGraph(string fileName)
        {
            var deckContainerObject = ScriptableObject.CreateInstance<DeckContainer>();
            if (!SaveNodes(fileName, deckContainerObject))
                return;

            if(AssetDatabase.IsValidFolder("Assets/Resources") == false)
            {      
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // If it doesn't exists create Decks folder in Assets
            if(AssetDatabase.IsValidFolder("Assets/Resources/Decks") == false)
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Decks");
            }

            UnityEngine.Object loadedAsset = AssetDatabase.LoadAssetAtPath($"Assets/Resources/Decks/{fileName}.asset", typeof(DeckContainer));

            if (loadedAsset == null || !AssetDatabase.Contains(loadedAsset)) 
			{
                AssetDatabase.CreateAsset(deckContainerObject, $"Assets/Resources/Decks/{fileName}.asset");
            }
            else 
			{
                DeckContainer container = loadedAsset as DeckContainer;
                container.NodeLinks = deckContainerObject.NodeLinks;
                container.DeckNodeDatas = deckContainerObject.DeckNodeDatas;
                EditorUtility.SetDirty(container);
            }

            AssetDatabase.SaveAssets();
        }

    public bool SaveNodes(string fileName, DeckContainer deckContainer)
    {
        // No connections
        if(Edges.Any() == false)
        {
            return false;
        }
                
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();

        Debug.Log("Connected ports: " + connectedPorts.Length);

        for(int i = 0; i < connectedPorts.Length; i++)
        {
            var outputNode = connectedPorts[i].output.node as DeckNode;
            var inputNode = connectedPorts[i].input.node as DeckNode;;
            deckContainer.NodeLinks.Add(new NodeLinkData
            {
                BaseNodeGuid = outputNode.GUID,
                PortName = connectedPorts[i].output.portName,
                TargetNodeGuid = inputNode.GUID,
            });

            Debug.Log("Saving node port: " + connectedPorts[i].output.portName);
        }

        foreach(var deckNode in Nodes.Where(node => node.EntryPoint == false))
        {
            deckContainer.DeckNodeDatas.Add(new DeckNodeData
            {
                Guid = deckNode.GUID,
                DialogueText = deckNode.DialogueText,
                Position = deckNode.GetPosition().position,
                LeftAnswer = deckNode.LeftAnswer,
                RightAnswer = deckNode.RightAnswer,
                characterSO = deckNode.CharacterData,
            });
        }

        return true;
    }

    public void LoadGraphFromPath(string fileName)
    {
        _containerCache = Resources.Load<DeckContainer>($"Decks/{fileName}");

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found!", "target deck graph file does not exist!", "Ok");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    public void LoadGraphFromObject(DeckContainer container)
    {
        _containerCache = container;

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File not found!", "target deck graph file does not exist!", "Ok");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }


    private void ClearGraph()
    {
        // Set entry point guid from save. Discard existing guid.
        Nodes.Find(x => x.EntryPoint).GUID = _containerCache.NodeLinks.Find(x => x.PortName == "Output").BaseNodeGuid;

        foreach(var node in Nodes)
        {
            if(node.EntryPoint) 
                continue;
            
            // Remove edges connected to this node
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));

            // Remove node
            _targetGraphView.RemoveElement(node);
        }

    }

    private void CreateNodes()
    {
        foreach(var nodeData in _containerCache.DeckNodeDatas)
        {
            var tempNode = _targetGraphView.CreateDeckNode(nodeData.Position, nodeData.characterSO, nodeData.DialogueText, nodeData.LeftAnswer, nodeData.RightAnswer);
            tempNode.GUID = nodeData.Guid;

            _targetGraphView.AddElement(tempNode);
        }
    }

    private void ConnectNodes()
    {
        for(var i = 0; i< Nodes.Count; i++)
        {
            var connections = _containerCache.NodeLinks.Where(x => x.BaseNodeGuid == Nodes[i].GUID).ToList();
            for(var j = 0; j < connections.Count; j++)
            {
                var targetNodeGuid = connections[j].TargetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
            
                var inputNode = targetNode.titleContainer.Q<Port>();
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), inputNode);

                targetNode.SetPosition(new Rect(_containerCache.DeckNodeDatas.First(x => x.Guid == targetNodeGuid).Position,
                _targetGraphView.defaultNodeSize));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        var tempEdge = new Edge
        {
            input = input,
            output = output
        };

        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }
}
