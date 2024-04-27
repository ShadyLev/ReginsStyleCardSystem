using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class DeckGraph : EditorWindow
{
    private const string GRAPH_NAME = "Deck builder";
    private string _fileName = "New Deck";

    private DeckGraphView _graphView;

    private DeckContainer _deckContainer;

    [MenuItem("DeckBuilder/Open build graph")]
    public static void OpenDeckGraphWindow()
    {
        var window = GetWindow<DeckGraph>();
        window.titleContent = new GUIContent(GRAPH_NAME);
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMinimap();
    }

    private void OnDisable() 
    {
        rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphView()
    {
        _graphView = new DeckGraphView(this)
        {
            name = GRAPH_NAME
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField("File name: ");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(new Button(()=>RequestDataOperation(true)){text = "Save Data"});

        var objectField = new ObjectField()
        {
            objectType = typeof(DeckContainer)
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            var container = evt.newValue ?? null;
            var name = container == null ? "" : evt.newValue.name;
            fileNameTextField.SetValueWithoutNotify(name);
            _deckContainer = (DeckContainer)evt.newValue ?? null;
            Debug.Log((DeckContainer)evt.newValue ?? null);
        });
        toolbar.Add(objectField);

        //toolbar.Add(new Button(()=>RequestDataOperation(false)){text = "Load Data from name"});
        toolbar.Add(new Button(()=>RequestDataOperation2()){text = "Load Data from object"});

        var nodeCreateButton = new Button( () =>
        {        
            _graphView.CreateNode(new Vector2(200,200));
        });

        nodeCreateButton.text = "Create node";
        toolbar.Add(nodeCreateButton);
        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if(string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "Ok");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        
        if(save)
        {
            saveUtility.SaveGraph(_fileName);
        }
        else
        {
            saveUtility.LoadGraphFromPath(_fileName);
        }
    }

    private void RequestDataOperation2()
    {
        if(_deckContainer == null)
        {
            EditorUtility.DisplayDialog("Invalid deck!", "Please select a valid object.", "Ok");
            return;
        }
        
        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        saveUtility.LoadGraphFromObject(_deckContainer);
        
    }

    
    private void GenerateMinimap()
    {
        var minimap = new MiniMap()
        {
            anchored = true
        };
        minimap.SetPosition(new Rect(10,30,200,140));
        _graphView.Add(minimap);
    }

}   
