using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Navigation2D.Editor.NavigationEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Navigation2D.Editor.ConfigurationMenu
{
    public class ConfigurationWindow : EditorWindow
    {
        private string XMLPath = Path.Combine("Assets", "Navigation2D", "Editor", "xml", "configuration.uxml");
        private string ListItemXMLPath = Path.Combine("Assets", "Navigation2D", "Editor", "xml", "list_item.uxml");
        private TemplateContainer _container;
        private VisualTreeAsset _listItem;
        private VisualElement _agentMenu;
        private VisualElement _bakeMenu;
        private ListView _areasList;
        private ListView _agentsList;
        private TextField _agentNameField;
        private TextField _areaNameField;
        private FloatField _agentRadiusField;
        private Button _bakeAreaButton;
        private int _currentAgent;
        private int _currentArea;
        
        private ConfigurationDataContainer _config => Navigation2DEditorService.Configuration;
        
        [MenuItem("Navigation2D/Configuration Window")]
        public static void ShowConfigurationWindow()
        {
            var wnd = GetWindow<ConfigurationWindow>();
            wnd.titleContent = new GUIContent("ConfigurationWindow");
        }
        
        public void CreateGUI()
        {
            var document = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(XMLPath);
            _listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ListItemXMLPath);
            _container = document.CloneTree();
            
            VisualElement root = rootVisualElement;
            root.Add(_container);

            _agentMenu = _container.Q<VisualElement>("agentMenu");
            _bakeMenu = _container.Q<VisualElement>("bakeMenu");

            _bakeAreaButton = _bakeMenu.Q<Button>("bake");
            
            var addAgentButton = _agentMenu.Q<Button>("add");
            addAgentButton.clicked += _addAgent;
            
            var removeAgentButton = _agentMenu.Q<Button>("remove");
            removeAgentButton.clicked += _removeAgent;
            
            var addAreaButton = _bakeMenu.Q<Button>("add");
            addAreaButton.clicked += _addArea;
            
            var removeAreaButton = _bakeMenu.Q<Button>("remove");
            removeAreaButton.clicked += _removeArea;
            
            _agentNameField = _agentMenu.Q<TextField>("name");
            _agentNameField.RegisterCallback(new EventCallback<ChangeEvent<string>>(evt =>
            {
                EditorUtility.SetDirty(_config);
                Undo.RecordObject(_config, "Changed agent name");
                _config.agentNames[_currentAgent] = evt.newValue;
                AssetDatabase.SaveAssetIfDirty(_config);
                EditorUtility.ClearDirty(_config);
                _agentsList.Rebuild();
            }));
            
            _areaNameField = _bakeMenu.Q<TextField>("name");
            _areaNameField.RegisterCallback(new EventCallback<ChangeEvent<string>>(evt =>
            {
                EditorUtility.SetDirty(_config);
                Undo.RecordObject(_config, "Changed agent name");
                _config.areaNames[_currentArea] = evt.newValue;
                AssetDatabase.SaveAssetIfDirty(_config);
                EditorUtility.ClearDirty(_config);
                _areasList.Rebuild();
            }));
            
            _agentRadiusField = _agentMenu.Q<FloatField>("radius");
            _agentRadiusField.RegisterCallback(new EventCallback<ChangeEvent<float>>(evt =>
            {
                EditorUtility.SetDirty(_config);
                Undo.RecordObject(_config, "Changed agent radius");
                _config.agentRadius[_currentAgent] = evt.newValue;
                AssetDatabase.SaveAssetIfDirty(_config);
                EditorUtility.ClearDirty(_config);
                _agentsList.Rebuild();
            }));
            
            var toolbar = _container.Q<DropdownField>("toolbar");
            toolbar.choices = new List<string>() {"Agents", "Bake"};
            toolbar.RegisterValueChangedCallback(v =>
            {
                if (v.newValue == "Agents")
                {
                    _showAgentsPage();
                }
                if (v.newValue == "Bake")
                {
                    _showBakePage();
                }
            });
            toolbar.value = toolbar.choices[0];
            _showAgentsPage();
        }

        private void _showAgentsPage()
        {
            _agentMenu.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _bakeMenu.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _agentMenu.MarkDirtyRepaint();
            _agentsList = _container.Q<ListView>("agentsList");
            _agentsList.Clear();

            Func<VisualElement> makeItem = () => _listItem.CloneTree();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Q<Label>().text = _config.agentNames[i];
            };

            _agentsList.makeItem = makeItem;
            _agentsList.bindItem = bindItem;
            _agentsList.itemsSource = _config.agentNames;
            _agentsList.selectionType = SelectionType.Single;
            _agentsList.onSelectionChange += objects =>
            {
                _currentAgent = _config.agentNames.IndexOf((string) objects.First());
                _agentNameField.SetValueWithoutNotify(_config.agentNames[_currentAgent]);
                _agentRadiusField.SetValueWithoutNotify(_config.agentRadius[_currentAgent]);
            };
            
            _showAgentProperties(_config.agentNames.Count != 0);
        }
        
        private void _showBakePage()
        {
            _agentMenu.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _bakeMenu.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _bakeMenu.MarkDirtyRepaint();
            _areasList = _container.Q<ListView>("areasList");
            _areasList.Clear();

            Func<VisualElement> makeItem = () => _listItem.CloneTree();

            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Q<Label>().text = _config.areaNames[i];
            };

            _areasList.makeItem = makeItem;
            _areasList.bindItem = bindItem;
            _areasList.itemsSource = _config.areaNames;
            _areasList.selectionType = SelectionType.Single;
            _areasList.onSelectionChange += objects =>
            {
                _currentArea = _config.areaNames.IndexOf((string) objects.First());
                _areaNameField.SetValueWithoutNotify(_config.areaNames[_currentArea]);
            };
            
            _showAreaProperties(_config.areaNames.Count != 0);
        }
              
        private void _addAgent()
        {
            EditorUtility.SetDirty(_config);
            Undo.RecordObject(_config, "Added new agent");
            _config.agentNames.Add("RandomName" + Guid.NewGuid().ToString().Substring(0, 6));
            _config.agentRadius.Add(1f);
            AssetDatabase.SaveAssetIfDirty(_config);
            EditorUtility.ClearDirty(_config);
            _showAgentProperties(true);
            _agentsList.SetSelection(_config.agentNames.Count-1);
            _agentsList.Rebuild();
        }
        
        private void _removeAgent()
        {
            EditorUtility.SetDirty(_config);
            Undo.RecordObject(_config, "Removed agent");
            _config.agentNames.RemoveAt(_currentAgent);
            _config.agentRadius.RemoveAt(_currentAgent);
            AssetDatabase.SaveAssetIfDirty(_config);
            EditorUtility.ClearDirty(_config);
            _showAgentProperties(_config.agentNames.Count != 0);
            if (_config.agentNames.Count != 0)
            {
                _agentsList.SetSelection(_currentAgent-1 >=0 ? _currentAgent-1:0);
            }
            _agentsList.Rebuild();
        }

        private void _showAgentProperties(bool value)
        {
            _agentNameField.visible = value;
            _agentRadiusField.visible = value;
        }
        
        private void _addArea()
        {
            EditorUtility.SetDirty(_config);
            Undo.RecordObject(_config, "Added new agent");
            _config.areaNames.Add("RandomName" + Guid.NewGuid().ToString().Substring(0, 6));
            AssetDatabase.SaveAssetIfDirty(_config);
            EditorUtility.ClearDirty(_config);
            _showAreaProperties(true);
            _areasList.SetSelection(_config.areaNames.Count-1);
            _areasList.Rebuild();
        }
        
        private void _removeArea()
        {
            EditorUtility.SetDirty(_config);
            Undo.RecordObject(_config, "Removed agent");
            _config.areaNames.RemoveAt(_currentArea);
            AssetDatabase.SaveAssetIfDirty(_config);
            EditorUtility.ClearDirty(_config);
            _showAreaProperties(_config.areaNames.Count != 0);
            if (_config.areaNames.Count != 0)
            {
                _areasList.SetSelection(_currentArea-1 >=0 ? _currentArea-1:0);
            }
            _areasList.Rebuild();
        }

        private void _showAreaProperties(bool value)
        {
            _areaNameField.visible = value;
            _bakeAreaButton.visible = value;
        }
    }
}