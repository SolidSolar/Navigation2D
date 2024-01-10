using System;
using Navigation2D.Editor.NavigationEditor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace arcticDEV.Editor.SceneLoadingTools
{
    public class TestingToolkitEditor : EditorWindow
    {
        private ScrollView _list;
        [MenuItem("arcticDEV/Testing toolkit")]
        public static void OpenMergeGraphWindow()
        {
            var window = GetWindow<TestingToolkitEditor>();
            window.titleContent = new UnityEngine.GUIContent("Testing toolkit");
        }
        private void OnEnable()
        {
            _list = new ScrollView();
            rootVisualElement.Add(_list);
            AddSaveIndexField();
            AddGenerateMeshButton();
            AddGenerateShapeButton();
        }

        void AddSaveIndexField()
        {
            IntegerField field = new IntegerField("whatever index");
            field.RegisterCallback<ChangeEvent<int>>((val) =>
            {
            });
            _list.Add(field);
        }
        
        void AddGenerateMeshButton()
        {
            Button button = new Button(() =>
            {
                Navigation2DEditorService.GenerateNavigationMesh();
            });
            button.text = "Generate mesh";
            _list.Add(button);
        }

        void AddGenerateShapeButton()
        {
            Button button = new Button(() =>
            {
                TestingToolkit.DrawColliderToShapeResult(GameObject.Find("TestCollider").GetComponent<Collider2D>());
            });
            button.text = "Generate mesh";
            _list.Add(button);
        }
        
    }
}
