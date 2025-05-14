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
    private TextMeshProUGUI _textLineOne;
    [SerializeField]
    private TextMeshProUGUI _textLineTwo;
    [SerializeField]
    private BoolReference _isInDialogue;

    [Header("TextBalloons")]
    [SerializeField]
    private float _fadeInSpeed;
    [SerializeField]
    private StateManager _stateManager;
    [SerializeField]
    private GameObject _canvas;
    [SerializeField]
    private GameObject _whiteHolder;
    [SerializeField]
    private GameObject _blackHolder;
    [SerializeField]
    private GameObject _balloonHolder;

    private int _currentDialogueIndex = 0;


    private int _currentLineIndex = 0;
    private bool _canSkip;
    private bool _isTyping;
    private bool _allowMovement;

    private List<GameObject> _firstTextBalloon = new List<GameObject>();
    private List<GameObject> _secondTextBalloon = new List<GameObject>();

    private DialogueLines _currentLine;

    private List<GameObject> _activeImages = new List<GameObject>();

    private Dictionary<Image, bool> _textBalloonImages = new Dictionary<Image, bool>();


    private void Start()
    {
        _isInDialogue.variable.value = false;
    }

    private void LateUpdate()
    {
        //if (!_stateManager.IsInDialogue.value || !_allowMovement || _currentTextBalloon == null) return;
        //Vector2 newPos = GetTextBalloonPos();
        //_currentTextBalloon.transform.position = new Vector3(newPos.x, newPos.y, 0);
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

        if(_currentLine.Text.SecondLine != "")
        {
            StartCoroutine(DissableTextBalloon(true, _firstTextBalloon, false));
            if (_secondTextBalloon.Count != 0) StartCoroutine(DissableTextBalloon(true, _secondTextBalloon, true));
        }
        else StartCoroutine(DissableTextBalloon(true, _firstTextBalloon, true));
    }

    private Vector2 GetTextBalloonPos(List<GameObject> textBalloon)
    {
        GameObject target = GameObject.Find(_currentLine._characterName);
        Vector3 targetPos = target.transform.position;
        targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

        Vector2 TextballoonPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);

        TextballoonPos.y += _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.5f;

        float offsetX = _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.5f;
        bool needsFlip = _currentLine.FlipTextBalloon;

        if (needsFlip)
        {
            TextballoonPos = new Vector2(TextballoonPos.x - offsetX, TextballoonPos.y);
            textBalloon[0].transform.rotation = Quaternion.Euler(0, 180, 0);
            textBalloon[2].transform.rotation = Quaternion.Euler(0, 180, 0);
            _textLineOne.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            TextballoonPos = new Vector2(TextballoonPos.x + offsetX, TextballoonPos.y);
            if (_firstTextBalloon.Count != 0)
            {
                textBalloon[0].transform.rotation = Quaternion.Euler(0, 0, 0);
                textBalloon[2].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            _textLineOne.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
            return TextballoonPos;
    }

    private Vector2 GetTailPosition(List<GameObject> textBalloon)
    {
        float textBalloonWidth = textBalloon[0].GetComponent<Image>().rectTransform.rect.width;
        float textBalloonHeight = textBalloon[0].GetComponent<Image>().rectTransform.rect.height;

        Vector2 textBalloonPos = new Vector2(textBalloon[0].gameObject.transform.position.x, textBalloon[0].gameObject.transform.position.y);
        float Yoffset = -textBalloonHeight * 0.25f;
        float Xoffset = -textBalloonWidth * 0.25f;
        return new Vector2(Xoffset, Yoffset) + textBalloonPos;
    }

    private IEnumerator EnableTextBalloon()
    {
        _allowMovement = true;
        _isTyping = true;

        _currentLine = _dialogues[_currentDialogueIndex].Lines[_currentLineIndex];
        _firstTextBalloon = ConstructTextBalloon(true);

        string tempText = " ";
        if (_currentLine.Images.Count == 0) tempText = InsertLineBreaksByWord(_currentLine.Text.FirstLine, 5, _firstTextBalloon, _textLineOne);
        else
        {
            DistributeImages(_firstTextBalloon);
            DetermineTextBalloonSizeBasedOnImages();
        }

        Vector2 balloonPos = GetTextBalloonPos(_firstTextBalloon);


        _firstTextBalloon[0].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        _firstTextBalloon[2].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);

        Vector2 Tailpos = GetTailPosition(_firstTextBalloon);

        _firstTextBalloon[1].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        _firstTextBalloon[3].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);

        _textLineOne.transform.parent = _balloonHolder.transform;
        _textLineOne.transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        _textLineOne.fontSize = _currentLine.BaseFontSize;
        _textLineOne.font = _currentLine.BaseFont;

        foreach(GameObject obj in _firstTextBalloon)
        {
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        float transparacy = 0;

        while(transparacy < 1)
        {
            transparacy += Time.deltaTime * _fadeInSpeed;

            foreach (GameObject obj in _firstTextBalloon)
            {
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            yield return null;
        }

        foreach (GameObject obj in _firstTextBalloon)
        {
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
        StartCoroutine(TypeText(tempText, _textLineOne));

        if(_currentLine.Text.SecondLine != "")
        {
            yield return new WaitForSeconds(tempText.Length * (_currentLine.DisplaySpeed + 0.05f));

            _secondTextBalloon = ConstructTextBalloon(false);

            Vector3 newPos = _firstTextBalloon[0].transform.position;
            newPos.x += _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
            newPos.y -= _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;

            _secondTextBalloon[0].transform.position = newPos;
            _secondTextBalloon[2].transform.position = newPos;

            tempText = " ";
            if (_currentLine.Images.Count == 0) tempText = InsertLineBreaksByWord(_currentLine.Text.SecondLine, 5, _secondTextBalloon, _textLineTwo);
            else
            {
                DistributeImages(_secondTextBalloon);
                DetermineTextBalloonSizeBasedOnImages();
            }

            _textLineTwo.transform.parent = _balloonHolder.transform;
            _textLineTwo.transform.position = new Vector3(_secondTextBalloon[0].transform.position.x, _secondTextBalloon[0].transform.position.y, 0);
            _textLineTwo.fontSize = _currentLine.BaseFontSize;
            _textLineTwo.font = _currentLine.BaseFont;

            foreach (GameObject obj in _secondTextBalloon)
            {
                if (obj == null) continue;
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            }

            transparacy = 0;

            while (transparacy < 1)
            {
                transparacy += Time.deltaTime * _fadeInSpeed;

                foreach (GameObject obj in _secondTextBalloon)
                {
                    if (obj == null) continue;
                    Image image = obj.GetComponent<Image>();
                    image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
                }

                yield return null;
            }

            foreach (GameObject obj in _secondTextBalloon)
            {
                if (obj == null) continue;
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            }

            StartCoroutine(TypeText(tempText, _textLineTwo));
        }
    }

    private List<GameObject> ConstructTextBalloon(bool spawnTail)
    {
        List <GameObject> list = new List<GameObject>();
        GameObject textBalloonBorder = Instantiate(_currentLine.TextBalloon, _canvas.transform);
        _textBalloonImages.Add(textBalloonBorder.GetComponent<Image>(), true);
        textBalloonBorder.GetComponent<Image>().color = Color.black;
        textBalloonBorder.transform.parent = _blackHolder.transform;

        list.Add(textBalloonBorder);

        GameObject tailBorder = null;

        if (spawnTail)
        {
            tailBorder = Instantiate(_currentLine.Tail, _canvas.transform);
            _textBalloonImages.Add(tailBorder.GetComponent<Image>(), true);
            tailBorder.GetComponent<Image>().color = Color.black;
            tailBorder.transform.parent = _blackHolder.transform;
        }

        list.Add(tailBorder);

        GameObject textBalloon = Instantiate(_currentLine.TextBalloon, _canvas.transform);
        _textBalloonImages.Add(textBalloon.GetComponent<Image>(), false);
        textBalloon.transform.parent = _whiteHolder.transform;

        list.Add(textBalloon);

        GameObject tail = null;
        if (spawnTail)
        {
            tail = Instantiate(_currentLine.Tail, _canvas.transform);
            _textBalloonImages.Add(tail.GetComponent<Image>(), false);
            tail.transform.parent = _whiteHolder.transform;
        }

        list.Add(tail);

        return list;
    }

    private string InsertLineBreaksByWord(string text, int maxWordsPerLine, List<GameObject> textBalloon, TextMeshProUGUI TextArea)
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

        DetermineTextBalloonSizeBasedOnText(LineLength, lineCount, textBalloon, TextArea);
        return result.ToString();
    }

    private void DetermineTextBalloonSizeBasedOnText(int LetterCount, int linecount, List<GameObject> textBalloon, TextMeshProUGUI text)
    {
        if (_currentLine.BaseFont == null)
        {
            Debug.LogError("Font asset is null!");
            return;
        }

        TMP_FontAsset fontAsset = _currentLine.BaseFont;

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
        size += _currentLine.Padding;

        if (textBalloon.Count != 0)
        {
            foreach(GameObject temp in textBalloon)
            {
                if (temp == null) continue;
                Image image = temp.GetComponent<Image>();
                if (image != null)
                {
                    RectTransform balloonRect = image.rectTransform;
                    if (_textBalloonImages[image]) balloonRect.sizeDelta = new Vector2(size.x * (1 + ((_currentLine.BorderSize * 0.25f))), size.y * (1 + _currentLine.BorderSize));
                    else balloonRect.sizeDelta = size;
                }
            }
        }

        if (text != null)
        {
            text.rectTransform.sizeDelta = size;
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

            if (_currentLine.HasSecondImageLine)
            {
                imageSpacingY = Vector3.Distance(_activeImages[0].transform.position, _activeImages[(int)Mathf.Ceil(_currentLine.Images.Count * 0.5f)].transform.position);
                imageSpacingY -= imagesSizeY / 2;
            }
        }

        float imagesOnFirstLine = Mathf.Ceil(_currentLine.Images.Count * 0.5f);
        Vector2 size = new Vector2((imageSizeX + imageSpacingX) * imagesOnFirstLine, (imagesSizeY + imageSpacingY) * 2);
        size += _currentLine.Padding;

        foreach (Image image in _textBalloonImages.Keys)
        {
            RectTransform balloonRect = image.rectTransform;
            if (_textBalloonImages[image]) balloonRect.sizeDelta = new Vector2(size.x * (1 + _currentLine.BorderSize), size.y * (1 + _currentLine.BorderSize));
            else balloonRect.sizeDelta = size;
        }
        _textLineOne.rectTransform.sizeDelta = size;
    }

    private float DetermineFontSizeMultiplier()
    {
        float baseSize = 36;
        float currentSize = _currentLine.BaseFontSize;

        float multiplier = currentSize / baseSize;

        return multiplier;
    }

    private void DistributeImages(List<GameObject> textBalloon)
    {
        //RectTransform rectTransform = _currentTextBalloon.GetComponent<Image>().rectTransform;

        //Rect imageRect = RectTransformUtility.PixelAdjustRect(rectTransform, _canvas.GetComponent<Canvas>());

        //float balloonWith = imageRect.width;
        //float balloonHeight = imageRect.height;

        //float spacingX = 0;
        //float spacingY = 0;

        //int amountOfImagesInLine = 0;
        //int imagesOnFirstLine = 0;
        //int imagesOnSecondLine = 0;

        //int linesindex = 1;

        //if (_currentLine.HasSecondLine)
        //{
        //    spacingX = balloonWith / Mathf.Ceil(_currentLine.Images.Count * 0.5f);
        //    spacingY = balloonHeight * 0.25f;

        //    imagesOnFirstLine = (int)Mathf.Ceil(_currentLine.Images.Count * 0.5f);
        //    imagesOnSecondLine = _currentLine.Images.Count - imagesOnFirstLine;
        //    linesindex = 2;
        //}
        //else
        //{
        //    spacingX = balloonWith / _currentLine.Images.Count;
        //    imagesOnFirstLine = _currentLine.Images.Count;
        //}

        //    amountOfImagesInLine = imagesOnFirstLine;

        //for (int i = 0; i < linesindex; i++)
        //{
        //    for (int j = 0; j < amountOfImagesInLine; j++)
        //    {
        //        GameObject newImage = (Instantiate(_currentLine.Images[j * (i + 1)], _currentTextBalloon.transform));

        //        float newX = +spacingX - (spacingX * j);

        //    //    float newX = 0 + (balloonWith / 2f) - (spacingX * j) - (newImage.GetComponent<Image>().rectTransform.rect.width);
        //        float newY = 0;

        //        if (_currentLine.HasSecondLine) newY = 0 + (balloonHeight * 0.25f) - (spacingY * i) - (newImage.GetComponent<Image>().rectTransform.rect.width / 2);

        //        newImage.transform.localPosition = new Vector3(newX, newY, 0);
        //        newImage.transform.parent = _currentTextBalloon.transform;
        //        _activeImages.Add(newImage);
        //    }

        //    amountOfImagesInLine = imagesOnSecondLine;
        //}

        int childCount = _currentLine.Images.Count;
        int columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
        int rows = Mathf.CeilToInt((float)childCount / columns);

        Image balloon = textBalloon[2].GetComponent<Image>();
        RectTransform balloonRect = balloon.rectTransform;

        // Define what percentage of space you want the images to occupy (0.6 = 60%)
        float imageSizeRatio = 1f;

        // Calculate desired cell dimensions based on the container size and image ratio
        float desiredCellWidth = (balloonRect.rect.width / columns) * imageSizeRatio;
        float desiredCellHeight = (balloonRect.rect.height / rows) * imageSizeRatio;

        // Calculate spacing based on desired cell size
        float spacingX = (balloonRect.rect.width - (desiredCellWidth * columns)) / (columns - 1);
        float spacingY = (balloonRect.rect.height - (desiredCellHeight * rows)) / (rows - 1);

        // Use these calculated values for placement
        for (int i = 0; i < childCount; i++)
        {
            int row = 0;
            if (_currentLine.HasSecondImageLine) row = i / columns;
            else row = 1;
            int col = i % columns;
            GameObject newImage = (Instantiate(_currentLine.Images[i], textBalloon[0].transform));
            if (newImage != null)
            {
                _activeImages.Add(newImage);
                // Calculate position using the spacing values we just determined
                float x = (col * desiredCellWidth) + (col * spacingX);
                float y = (row * desiredCellHeight) + (row * spacingY);

                // Adjust position based on balloon's position
                Vector3 position = new Vector3(x - balloonRect.rect.width / 2 + desiredCellWidth / 2, -y + balloonRect.rect.height / 2 - desiredCellHeight / 2, 0);

                newImage.transform.localPosition = position;

                //// Make sure the child has a RectTransform to set its size
                //RectTransform childRect = newImage.GetComponent<RectTransform>();
                //if (childRect != null)
                //{
                //    childRect.sizeDelta = new Vector2(desiredCellWidth, desiredCellHeight);
                //}
            }
        }
    }

    private IEnumerator TypeText(string text, TextMeshProUGUI textObject)
    {
        textObject.text = "";
        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int tagEnd = text.IndexOf('>', i);
                if (tagEnd != -1)
                {
                    string tag = text.Substring(i, tagEnd - i + 1);
                    textObject.text += tag;
                    i = tagEnd + 1;
                    continue;
                }
            }
            textObject.text += text[i];
            if (_canSkip)
            {
                textObject.text = text;
                _canSkip = false;
                _isTyping = false;
                break;
            }
            i++;
            yield return new WaitForSeconds(_currentLine.DisplaySpeed);
        }
        _isTyping = false;
    }

    private IEnumerator DissableTextBalloon(bool startNextLine, List<GameObject> textBalloon, bool GoToNext)
    {
        float transparacy = 1;

        while (transparacy > 0)
        {
            transparacy -= Time.deltaTime * _fadeInSpeed;

            foreach (Image image in _textBalloonImages.Keys)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            _textLineOne.color = new Color(0, 0, 0, transparacy);
            if (_currentLine.Text.SecondLine != "")
                _textLineTwo.color = new Color(0, 0, 0, transparacy);

            if (_activeImages.Count != 0)
            {
                foreach (GameObject image in _activeImages)
                {
                    image.GetComponent<Image>().color = new Color(0, 0, 0, transparacy);
                }
            }
            yield return null;
        }

        foreach (Image image in _textBalloonImages.Keys)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        _textLineOne.color = new Color(0, 0, 0, 0);
        if (_currentLine.Text.SecondLine != "")
            _textLineTwo.color = new Color(0, 0, 0, 0);

        if (_activeImages.Count != 0)
        {
            foreach (GameObject image in _activeImages)
            {
                image.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                GameObject.Destroy(image.gameObject);
            }
        }

        _textLineOne.text = "";
        _textLineOne.color = Color.black;
        if (_currentLine.Text.SecondLine != "")
        {
            _textLineTwo.text = "";
            _textLineTwo.color = Color.black;
        }
        _textLineOne.transform.parent = _canvas.transform;
        foreach(GameObject image in textBalloon)
        {
            Destroy(image);
        }
        _textBalloonImages = new Dictionary<Image, bool>();
        _allowMovement = false;

        if (GoToNext)
        {
            if (_currentLineIndex < _dialogues[_currentDialogueIndex].Lines.Count - 1)
            {
                _currentLineIndex++;
                if (startNextLine) StartCoroutine(EnableTextBalloon());
                else _isInDialogue.variable.value = false;
            }
            else
            {
                _isInDialogue.variable.value = false;
                _currentLineIndex = 0;
            }
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