﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DialogUtilitySpruce.Editor
{
    public class DialogNode : Node
    {
        public DialogNode(DialogNodeController controller, DialogNodeModel model)
        {
            Model = model;
            Controller = controller;
            inputContainer.style.flexGrow = 1;
            outputContainer.style.flexGrow = 0;
    
            VisualElement nodeUxml =  DialogGraphUXMLData.Instance.dialogNode.CloneTree();
            var label = nodeUxml.Q<Label>("title");
            var dropdown = nodeUxml.Q<DropdownField>("dropdown");
            _dropdown = dropdown;
            var textField = nodeUxml.Q<TextField>("textField");
            var overrideSprite = nodeUxml.Q<ObjectField>("overrideSprite");

            CharacterList.Instance.OnLocalListChanged += () =>
            {
                _updateLocalCharacterList(CharacterList.Instance.GetLocalCharacterNames());
            };
            
            CharacterList.Instance.OnCharacterChanged += (c) =>
            {
                if (model.Character == c)
                {
                    _updateLocalCharacterName(c);
                    _updateLocalCharacterList(CharacterList.Instance.GetLocalCharacterNames());
                }
            };
            
            _updateLocalCharacterList(CharacterList.Instance.GetLocalCharacterNames());
            dropdown.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                controller.ChangeCharacter(evt.newValue);
            });
            dropdown.SetValueWithoutNotify(model.Character != null ? model.Character.Name : "<none>");

            model.OnCharacterUpdate = data =>
            {
                if (model.Character != null && data.Id == model.Character.Id)
                {
                    dropdown.SetValueWithoutNotify(model.Character.Name);
                }
                else
                {
                    dropdown.SetValueWithoutNotify("<none>");
                }
            }; 
            
            
            label.text = string.Format(label.text, model.Id.Value.Substring(0, 5));
            mainContainer.Add(nodeUxml);
            
            VisualElement addPortButtonUxml = DialogGraphUXMLData.Instance.dialogNodeAddPortButton.CloneTree();
            var addButton = addPortButtonUxml.Q<Button>("add");
            addButton.clicked += controller.AddConditionPort;

            model.OnPortsUpdate?.Invoke(model.Ports);

            model.OnPortsUpdate = list =>
            {
                foreach (var item in list)
                {
                    if(!_ports.ContainsKey(item.id))
                        _addPort(item.id, item.condition);
                    else
                        _updatePort(item.id, item.condition);
                }

                var l = _ports.Where(x => !list.Exists(y => y.id == x.Key)).ToList();
                for(int i = 0; i< l.Count; i++)
                {
                    _deletePort(l[i].Key, l[i].Value);
                }
            };
            outputContainer.Add(addPortButtonUxml);
            foreach (var item in model.Ports)
            {
                if(!_ports.ContainsKey(item.id))
                    _addPort(item.id, item.condition);
                else
                    _updatePort(item.id, item.condition);
            }
            
            textField.RegisterCallback<ChangeEvent<string>>(controller.ChangeText);
            model.OnTextUpdate += s =>
            {
                textField.SetValueWithoutNotify(s);
            }; 
            textField.SetValueWithoutNotify(model.Text);
            
            DialogLanguageHandler.Instance.OnLanguageChanged += _=>
            {
                _changeLanguage(textField, dropdown);
            };
            
            overrideSprite.RegisterCallback<ChangeEvent<Object>>(controller.ChangeSprite);
            model.OnSpriteUpdate += sprite =>
            {
                overrideSprite.SetValueWithoutNotify(sprite);
            };
            overrideSprite.SetValueWithoutNotify(model.Sprite);

            _radioButton = this.Q<RadioButton>("startNode");
            
            
            Port port = _generatePort(Direction.Input);
            port.portName = "Input";
            inputContainer.Add(port);
            RefreshExpandedState();
        }

        public Action<Port> OnPortDelete { get; set; }
        public DialogNodeModel Model { get; }
        private DialogNodeController Controller { get; }
        
        public void OnStartNodeIdChanged(SerializableGuid guid)
        {
            _radioButton.SetValueWithoutNotify(guid == Model.Id);
        }
        
        public override void SetPosition(Rect newPos)
        {
            Controller.SetPosition(newPos);
            base.SetPosition(newPos);
        }

        private void _updateLocalCharacterList(List<string> localCharacterNames)
        {
            _dropdown.choices = new List<string>(){"<none>"};
            _dropdown.choices.AddRange(localCharacterNames);
            if (!_dropdown.choices.Contains(_dropdown.value))
            {
                _dropdown.SetValueWithoutNotify("<none>");
            }
        }
        
        private void _updateLocalCharacterName(CharacterModel character)
        {
            if (character != null)
            {
                _dropdown.SetValueWithoutNotify(character.Name);
            }
            else
            {
                _dropdown.SetValueWithoutNotify("<none>");
            }
        }

        
        private Port _generatePort(Direction portDirection, Port.Capacity capacity = Port.Capacity.Multi)
        {
            return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private void _addPort(SerializableGuid id, Condition value)
        {
            VisualElement nodePortUxml = DialogGraphUXMLData.Instance.dialogNodePortItem.CloneTree();
            Port port = _generatePort(Direction.Output);
            
            port.name = id.Value;
            port.Add(nodePortUxml);
            var removeButton = nodePortUxml.Q<Button>("remove");
            removeButton.clicked += () =>
            {
                _deletePort(id, port);
            }; 

            var condition = nodePortUxml.Q<ObjectField>();
            condition.SetValueWithoutNotify(value);
            condition.RegisterCallback<ChangeEvent<Object>>((evt) =>
            {
                Controller.SetCondition(id, (Condition) evt.newValue);
            });
            
            _ports.Add(id, port);
            outputContainer.Insert(outputContainer.childCount-1,port);
            RefreshExpandedState();
        }

        private void _deletePort(SerializableGuid id, Port port)
        {
            OnPortDelete?.Invoke(port);
            RefreshPorts();
            _ports.Remove(id);
            outputContainer.Remove(port);
            Controller.RemoveConditionPort(id);
        }

        private void _updatePort(SerializableGuid id, Condition value)
        {
            var condition = _ports[id].Q<ObjectField>();
            condition.SetValueWithoutNotify(value);
            RefreshExpandedState();
        }
         
        private void _changeLanguage(TextField textField, DropdownField dropdown)
        {
            textField.SetValueWithoutNotify(Model.Text);
            if (Model.Character!=null)
            {
                dropdown.choices = new List<string>() {"<none>"};
                dropdown.choices.AddRange(CharacterList.Instance.GetLocalCharacterNames());
                dropdown.SetValueWithoutNotify(CharacterList.Instance.FindCharacter(Model.Character.Id)?.Name);
            }
        }
        
        private readonly DropdownField _dropdown;
        private readonly RadioButton _radioButton;
        private readonly Dictionary<SerializableGuid, Port> _ports = new(); 
    }
}