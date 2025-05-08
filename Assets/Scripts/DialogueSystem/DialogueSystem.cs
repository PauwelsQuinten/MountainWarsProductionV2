using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;


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

    private List<GameObject> _activeImages = new List<GameObject>();

    private int _LineSizeX;
    private int _LineSizeY;

    private void Start()
    {
        _isInDialogue.variable.value = false;
    }

    private void LateUpdate()
    {
        if (!_stateManager.IsInDialogue.value || !_allowMovement || _currentTextBalloon == null) return;
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
        bool needsFlip = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex].FlipTextBalloon;

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

        _currentLine = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex];
        _currentTextBalloon = Instantiate(_currentLine.TextBalloon, _canvas.transform);

        string tempText = " ";
        if (_currentLine.Images.Count == 0) tempText = InsertLineBreaksByWord(_currentLine.Text, 5);

        Vector2 pos = GetTextBalloonPos();
        _currentTextBalloon.transform.position = new Vector3(pos.x, pos.y, 0);

        _text.transform.parent = _currentTextBalloon.transform;
        _text.transform.localPosition = Vector3.zero;
        _text.fontSize = _currentLine.FontSize;
        _text.font = _currentLine.Font;

        if(_currentLine.Bold)
            _text.fontStyle = FontStyles.Bold;
        else _text.fontStyle = FontStyles.Normal;

        if (_currentLine.Images.Count > 0)
        {
            DistributeImages();
            DetermineTextBalloonSizeBasedOnImages();
        }

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

        StartCoroutine(TypeText(tempText));
    }

    private string InsertLineBreaksByWord(string text, int maxWordsPerLine)
    {
        if (maxWordsPerLine <= 0) return text;

        var words = text.Split(' ');
        var result = new System.Text.StringBuilder();
        int wordCount = 0;
        int LineLength = 0;
        int lettercount = 0;
        int lineCount = 1;

        for (int i = 0; i < words.Length; i++)
        {
            result.Append(words[i]);
            wordCount++;

            lettercount += words[i].Length;
            if (LineLength < lettercount) LineLength = lettercount;

            if (wordCount >= maxWordsPerLine && i != words.Length - 1)
            {
                lineCount++;
                result.Append('\n');
                wordCount = 0;
                lettercount = 0;
            }
            else if (i != words.Length - 1)
            {
                result.Append(' ');
            }
        }

        DetermineTextBalloonSizeBasedOnText(LineLength, lineCount);
        return result.ToString();
    }

    private void DetermineTextBalloonSizeBasedOnText(int LetterCount, int linecount)
    {
        if (_currentLine.Font == null)
        {
            Debug.LogError("Font asset is null!");
            return;
        }

        TMP_FontAsset fontAsset = _currentLine.Font;

        // Get character 'A' metrics
        float characterWidth = 0;
        float characterHeight = 0;
        const char sampleChar = 'a';

        if (fontAsset.characterLookupTable.TryGetValue(sampleChar, out TMP_Character character))
        {
            characterWidth = character.glyph.metrics.width * 0.60f;
            characterHeight = character.glyph.metrics.height * 0.60f;
        }
        else
        {
            Debug.LogError($"Font {fontAsset.name} is missing character '{sampleChar}'");
        }

        float FontSizeMultiplier = DetermineFontSizeMultiplier();

        Vector2 size = new Vector2((((FontSizeMultiplier * characterWidth) + _currentLine.CharacterSpacing) * LetterCount), (((FontSizeMultiplier * characterHeight) + _currentLine.LineSpacing) * linecount));
        size += _currentLine.AddedScale;

        if (_currentTextBalloon != null)
        {
            RectTransform balloonRect = _currentTextBalloon.GetComponent<Image>().rectTransform;
            if (balloonRect != null) balloonRect.sizeDelta = size;
        }

        if (_text != null)
        {
            _text.rectTransform.sizeDelta = size;
        }
    }

    private void DetermineTextBalloonSizeBasedOnImages()
    {
        float imageSizeX = 0;
        float imagesSizeY = 0;

        float imageSpacingX = 0;
        float imageSpacingY = 0;

        imageSizeX += _activeImages[0].GetComponent<Image>().rectTransform.rect.width;
        imagesSizeY += _activeImages[0].GetComponent<Image>().rectTransform.rect.height;


        if (_currentLine.Images.Count > 1)
        {
            imageSpacingX = Vector3.Distance(_activeImages[0].transform.position, _activeImages[1].transform.position);
            imageSpacingX -= imageSizeX / _currentLine.Images.Count;

            if (_currentLine.HasSecondLine)
            {
                imageSpacingY = Vector3.Distance(_activeImages[0].transform.position, _activeImages[(int)Mathf.Ceil(_currentLine.Images.Count * 0.5f)].transform.position);
                imageSpacingY -= imagesSizeY / 2;
            }
        }

        float imagesOnFirstLine = Mathf.Ceil(_currentLine.Images.Count * 0.5f);
        Vector2 size = new Vector2((imageSizeX + imageSpacingX) * imagesOnFirstLine, (imagesSizeY + imageSpacingY) * 2);
        size += _currentLine.AddedScale;

        _currentTextBalloon.GetComponent<Image>().rectTransform.sizeDelta = size;
        _text.rectTransform.sizeDelta = size;
    }

    private float DetermineFontSizeMultiplier()
    {
        float baseSize = 36;
        float currentSize = _currentLine.FontSize;

        float multiplier = currentSize / baseSize;

        return multiplier;
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
                _activeImages.Add(newImage);
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
            if (_activeImages.Count != 0)
            {
                foreach (GameObject image in _activeImages)
                {
                    image.GetComponent<Image>().color = new Color(0, 0, 0, time);
                }
            }
            yield return null;
        }

        balloon.color = new Color(1, 1, 1, 0);
        _text.color = new Color(0, 0, 0, 0);
        if (_activeImages.Count != 0)
        {
            foreach (GameObject image in _activeImages)
            {
                image.GetComponent<Image>().color = balloon.color = new Color(0, 0, 0, time);
                GameObject.Destroy(image.gameObject);
            }
        }

        _text.text = "";
        _text.color = Color.black;
        _text.transform.parent = _canvas.transform;
        GameObject.Destroy(_currentTextBalloon);
        _allowMovement = false;

        if (_currentLineIndex < _dialogues[_currentDialogueIndex].Lines.Count - 1)
        {
            _currentLineIndex++;
            if (startNextLine) StartCoroutine(EnableTextBalloon());
            else _isInDialogue.variable.value = false;
        }
        else
        {
            _currentLineIndex = 0;
        }
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