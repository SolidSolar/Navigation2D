using System;
using System.Collections;
using DialogUtilitySpruce.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DialogUtilitySpruce.Examples
{
    public class DialogBox : MonoBehaviour
    {
        public GameObject textContainer;
        public Image image;
        public TextMeshProUGUI textBox;

        [SerializeField] private DialogGraphContainer _currentDialog;
        private DialogReader _dialogReader;
        private bool _ended;
        void Start()
        {
            DialogReaderSettings.Initialize(DialogReaderSettings.DefaultLanguage);
            _dialogReader = new DialogReader();
            ShowDialog(_currentDialog);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextMessage();
            }
        }

        public void NextMessage()
        {
            if (!_ended)
            {
                _dialogReader.NextMessage();
            }
            else
            {
                _end();
            }
        }

        public void ShowDialog(DialogGraphContainer dialog)
        {
            _ended = false;
            _currentDialog = dialog;
            _dialogReader.OnNextMessage.AddListener(_updateTextBox);
            _dialogReader.OnDialogEnded.AddListener(_markEnded);
            _dialogReader.BeginDialog(dialog);
        }


        private void _updateTextBox(DialogNode data)
        {
            textContainer.SetActive(true);
            image.sprite = data.Sprite ? data.Sprite : data.Character?.Icon;
            textBox.text = data.Text;
        }

        private void _markEnded()
        {
            _ended = true;
        }

        private void _end()
        {
            textContainer.SetActive(false);
            image.gameObject.SetActive(false);
            _dialogReader.OnNextMessage.RemoveListener(_updateTextBox);
            _dialogReader.OnDialogEnded.RemoveListener(_markEnded);
        }
    }
}