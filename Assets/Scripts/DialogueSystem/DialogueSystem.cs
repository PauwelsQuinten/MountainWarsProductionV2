using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField]
    private List<Dialogues> _dialogues = new List<Dialogues>();
    [SerializeField]
    private TextMeshProUGUI _text;

    private int _currentDialogueIndex = 0;


    private int _currentLineIndex = 0;
    private bool _canSkip;
    private bool _isTyping;

    [ContextMenu("StartDialogue")]
    public void StartNewDialoge()
    {
        if (_isTyping) return;
        if (_dialogues.Count - 1 < _currentDialogueIndex) return;
        if (!_dialogues[_currentDialogueIndex].IsStarted) _dialogues[_currentDialogueIndex].IsStarted = true;
        else return;
        StartCoroutine(TypeText(_dialogues[_currentDialogueIndex].Lines[_currentLineIndex].Text));
    }

    [ContextMenu("NextLine")]
    public void PlayNextLine()
    {
        if (_isTyping)
        {
            _canSkip = true;
            return;
        }

        if(_currentLineIndex < _dialogues[_currentDialogueIndex].Lines.Count - 1) _currentLineIndex++;
        else
        {
            _currentLineIndex = 0;
            _currentDialogueIndex++;
            _text.text = "";
            return;
        }

        StartCoroutine(TypeText(_dialogues[_currentDialogueIndex].Lines[_currentLineIndex].Text));
    }

    private IEnumerator TypeText(string text)
    {
        _isTyping = true;
        _text.text = "";
        foreach (char letter in text)
        {
            _text.text += letter;
            if (_canSkip)
            {
                _text.text = text;
                _canSkip = false;
                break;
            }
            yield return new WaitForSeconds(_dialogues[_currentDialogueIndex].Lines[_currentLineIndex].DisplaySpeed);
        }

        _isTyping = false;
    }
}
