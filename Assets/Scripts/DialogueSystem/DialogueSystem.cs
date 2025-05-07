using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField]
    private List<Dialogues> _dialogues = new List<Dialogues>();
    [SerializeField]
    private TextMeshProUGUI _text;
    [SerializeField]
    private BoolReference _isInDialogue;

    [Header("TextBalloons")]
    [SerializeField]
    private float _fadeInSpeed;
    [SerializeField]
    private StateManager _stateManager;
    [SerializeField]
    private GameObject _canvas;

    private int _currentDialogueIndex = 0;


    private int _currentLineIndex = 0;
    private bool _canSkip;
    private bool _isTyping;
    private bool _allowMovement;

    private GameObject _currentTextBalloon;

    private DialogueLines _currentLine;

    private void Start()
    {
        _isInDialogue.variable.value = false;
    }

    private void LateUpdate()
    {
        if (!_stateManager.IsInDialogue.value || !_allowMovement) return;
        Vector2 newPos = GetTextBalloonPos();
        _currentTextBalloon.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }

    public void StartNewDialoge(Component sender, object obj)
    {
        DialogueTriggerEventArgs args = obj as DialogueTriggerEventArgs;
        if (args == null) return;
        if (_isInDialogue.variable.value) return;
        _currentDialogueIndex = args.NextDialogueIndex;
        if (_dialogues.Count - 1 < _currentDialogueIndex) return;
        if (!_dialogues[_currentDialogueIndex].IsStarted) _dialogues[_currentDialogueIndex].IsStarted = true;
        else return;
        _isInDialogue.variable.value = true;
        StartCoroutine(EnableTextBalloon());
    }

    public void PlayNextLine(Component sender, object obj)
    {
        if (_isTyping)
        {
            _canSkip = true;
            return;
        }

        if (_currentLineIndex < _dialogues[_currentDialogueIndex].Lines.Count - 1)
        {
            _allowMovement = false;
            _currentLineIndex++;
        }
        else
        {
            _currentLineIndex = 0;

            StartCoroutine(DissableTextBalloon(false));
            return;
        }
        StartCoroutine(DissableTextBalloon(true));
    }

    private Vector2 GetTextBalloonPos()
    {
        GameObject target = GameObject.Find(_dialogues[_currentDialogueIndex].Lines[_currentLineIndex]._characterName);
        Vector3 targetPos = target.transform.position;
        targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

        Vector2 TextballoonPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);

        TextballoonPos.y += _dialogues[_currentDialogueIndex].Lines[_currentLineIndex].TextBalloon.GetComponent<Image>().rectTransform.rect.height * 0.5f;

        float offsetX = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex].TextBalloon.GetComponent<Image>().rectTransform.rect.width * 0.5f;
        bool needsFlip = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex]._flipTextBalloon;
        if (needsFlip)
        {
            TextballoonPos = new Vector2(TextballoonPos.x - offsetX, TextballoonPos.y);
            _currentTextBalloon.transform.rotation = Quaternion.Euler(0, 180, 0);
            _text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            TextballoonPos = new Vector2(TextballoonPos.x + offsetX, TextballoonPos.y);
            if(_currentTextBalloon != null)_currentTextBalloon.transform.rotation = Quaternion.Euler(0, 0, 0);
            _text.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
            return TextballoonPos;
    }

    private IEnumerator EnableTextBalloon()
    {
        _allowMovement = true;
        _isTyping = true;
        Vector2 pos = GetTextBalloonPos();

        _currentLine = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex];

        _currentTextBalloon = Instantiate(_currentLine.TextBalloon, _canvas.transform);
        _currentTextBalloon.transform.position = new Vector3(pos.x, pos.y, 0);

        _text.transform.parent = _currentTextBalloon.transform;
        _text.transform.localPosition = Vector3.zero;
        _text.fontSize = _currentLine.FontSize;
        _text.font = _currentLine.Font;

        if(_currentLine.Bold)
            _text.fontStyle = FontStyles.Bold;
        else _text.fontStyle = FontStyles.Normal;

        if(_currentLine.Images.Count > 0) DistributeImages();

        Image balloon = _currentTextBalloon.GetComponent<Image>();
        balloon.color = new Color(1, 1, 1, 0);

        float time = 0;

        while(balloon.color.a < 1)
        {
            time += Time.deltaTime * _fadeInSpeed;

            balloon.color = new Color(1, 1, 1, time);
            yield return null;
        }

        balloon.color = Color.white;

        StartCoroutine(TypeText(_currentLine.Text));
    }

    private void DistributeImages()
    {
        RectTransform rectTransform = _currentTextBalloon.GetComponent<Image>().rectTransform;

        Rect imageRect = RectTransformUtility.PixelAdjustRect(rectTransform, _canvas.GetComponent<Canvas>());

        float balloonWith = imageRect.width;
        float balloonHeight = imageRect.height;

        float spacingX = 0;
        float spacingY = 0;

        int amountOfImagesInLine = 0;
        int imagesOnFirstLine = 0;
        int imagesOnSecondLine = 0;

        int linesindex = 1;

        if (_currentLine.HasSecondLine)
        {
            spacingX = balloonWith / Mathf.Ceil(_currentLine.Images.Count * 0.5f);
            spacingY = balloonHeight * 0.25f;

            imagesOnFirstLine = (int)Mathf.Ceil(_currentLine.Images.Count * 0.5f);
            imagesOnSecondLine = _currentLine.Images.Count - imagesOnFirstLine;
            linesindex = 2;
        }
        else
        {
            spacingX = balloonWith / _currentLine.Images.Count;
            imagesOnFirstLine = _currentLine.Images.Count;
        }

            amountOfImagesInLine = imagesOnFirstLine;

        for (int i = 0; i < linesindex; i++)
        {
            for (int j = 0; j < amountOfImagesInLine; j++)
            {
                GameObject newImage = (Instantiate(_currentLine.Images[j * (i + 1)], _currentTextBalloon.transform));

                float newX = 0 + (balloonWith / 2f) - (spacingX * j) - (newImage.GetComponent<Image>().rectTransform.rect.width);
                float newY = 0;

                if (_currentLine.HasSecondLine) newY = 0 + (balloonHeight * 0.25f) - (spacingY * i) - (newImage.GetComponent<Image>().rectTransform.rect.width / 2);

                newImage.transform.localPosition = new Vector3(newX, newY, 0);
                newImage.transform.parent = _currentTextBalloon.transform;
            }

            amountOfImagesInLine = imagesOnSecondLine;
        }
    }

    private IEnumerator TypeText(string text)
    {
        _text.text = "";
        foreach (char letter in text)
        {
            _text.text += letter;
            if (_canSkip)
            {
                _text.text = text;
                _canSkip = false;
                _isTyping = false;
                break;
            }
            yield return new WaitForSeconds(_currentLine.DisplaySpeed);
        }
        _isTyping = false;
    }

    private IEnumerator DissableTextBalloon(bool startNextLine)
    {
        Image balloon = _currentTextBalloon.GetComponent<Image>();

        float time = 1;

        while (balloon.color.a > 0)
        {
            time -= Time.deltaTime * _fadeInSpeed;

            balloon.color = new Color(1, 1, 1, time);
            _text.color = new Color(0, 0, 0, time);
            yield return null;
        }

        balloon.color = new Color(1, 1, 1, 0);
        _text.color = new Color(0, 0, 0, 0);

        _text.text = "";
        _text.color = Color.black;
        _text.transform.parent = _canvas.transform;
        GameObject.Destroy(_currentTextBalloon);

        if (startNextLine) StartCoroutine(EnableTextBalloon());
        else _isInDialogue.variable.value = false;
    }

    private void OnDestroy()
    {
        foreach(Dialogues dialogue in _dialogues)
        {
            dialogue.IsStarted = false;
        }
    }

    private void OnDisable()
    {
        foreach (Dialogues dialogue in _dialogues)
        {
            dialogue.IsStarted = false;
        }
    }
}