using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private DeckGraphView _graphView;
    private EditorWindow _editorWindow;

    public void Init(DeckGraphView graphView, EditorWindow editorWindow)
    {
        _graphView = graphView;
        _editorWindow = editorWindow;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Deck"), 1),
            new SearchTreeEntry(new GUIContent("Deck Node"))
            {
                userData = new DeckNode(), level = 2
            },
        };

        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousePos = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, context.screenMousePosition - _editorWindow.position.position);
        var localMousePos = _graphView.contentViewContainer.WorldToLocal(worldMousePos);
        switch(SearchTreeEntry.userData)
        {
            case DeckNode deckNode:
                _graphView.CreateNode(localMousePos);
                return true;
            default:
                return false;
        }
    }
}
