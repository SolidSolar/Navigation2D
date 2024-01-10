using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DialogUtilitySpruce.API
{
    public class DialogReader
    {
        public UnityEvent<DialogNode> OnNextMessage { get; } = new();
        public UnityEvent OnDialogEnded { get; } = new();
        public UnityEvent<DialogNode> OnDialogStarted { get; } = new();
        public Dictionary<DialogNode, Dictionary<DialogChoiceOption, DialogNode>> Graph { get; private set; }
        public bool IsActive { get; private set; }
        
        private DialogGraphContainer _container;
        private DialogNodeFactory _nodeFactory;
        private DialogNode _currentNode;
        private DialogNode _startNode;
        
        /// <summary>
        /// Loads dialog graph container into dialog reader and begins iteration.
        /// Localization is loaded on this step
        /// </summary>
        /// <param name="container"></param>
        public void BeginDialog(DialogGraphContainer container)
        {
            if (IsActive)
            {
                Debug.LogWarning("You are trying to begin dialog already begun");
                return;
            }

            _container = container;
            LocalisationResource localisationResource = DialogReaderSettings.GetContainerLocalisation(_container.name);
            _container.localisationResource = localisationResource;
            _nodeFactory = new DialogNodeFactory(container);
            
            _load();

            IsActive = true;
            OnDialogStarted?.Invoke(_startNode);
        }

        /// <summary>
        /// Iterates to next message, prioritizing available or empty options if choice parameter is null.
        /// Ends dialog iteration if the message dialog node has no continuation
        /// </summary>
        /// <param name="choice">Choice used to navigate in dialog tree</param>
        public void NextMessage(DialogChoiceOption choice = null)
        {
            if (!IsActive)
            {
                Debug.LogWarning("You are trying to show next message for dialog not begun or ended");
                return;
            }

            if (_currentNode == null)
            {
                _currentNode = _startNode;
            }
            else
            {
                choice ??= Graph[_currentNode].Keys.First(x => !x.Condition || x.Condition.IsTrue(_currentNode, x.Index));
                _currentNode = Graph[_currentNode][choice];
            }
            
            OnNextMessage?.Invoke(_currentNode);
            
            if (Graph[_currentNode].Count == 0)
            {
                EndDialog();
                return;
            }
            
            var viableOptions = Graph[_currentNode].Keys
                .Count(option => !option.Condition || option.Condition.IsTrue(_currentNode, option.Index));
            
            if (viableOptions < 1)
            {
                EndDialog();
            }
        }

        /// <summary>
        /// Ends dialog.
        /// </summary>
        public void EndDialog()
        {
            if (!IsActive)
            {
                Debug.LogWarning("You are trying to end dialog not begun or ended");
                return;
            }
            IsActive = false;
            OnDialogEnded?.Invoke();
        }

        private void _load()
        {
            Graph = new Dictionary<DialogNode, Dictionary<DialogChoiceOption, DialogNode>>();
            var nodeDictionary = new Dictionary<SerializableGuid, Tuple<DialogNode, DialogNodeData>>();
            _container.dialogNodeDataList.ForEach(x =>
            {
                var data = x.GetDataCopy(_container.localisationResource);
                nodeDictionary.Add(x.Id, new Tuple<DialogNode, DialogNodeData>(_nodeFactory.GetDialogNode(data), data)); 
            });
            nodeDictionary.Values.ToList().ForEach(node =>
            {
                Graph.Add(node.Item1, new Dictionary<DialogChoiceOption, DialogNode>());
                if (node.Item2.id == _container.startNodeId)
                    _startNode = node.Item1;

                foreach (NodeLinkData link in _container.nodeLinks)
                {
                    for (int j = 0; j < node.Item2.ports.Count; j++)
                    {
                        if (link.basePortID == node.Item2.ports[j].id)
                        {
                            Graph[node.Item1][node.Item1.ChoiceOptions[j]] = nodeDictionary[link.targetNodeID].Item1;
                        }
                    }
                }
            });
        }
    }
}