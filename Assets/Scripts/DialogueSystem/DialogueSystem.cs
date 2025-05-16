using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<Dialogue> _dialogues = new List<Dialogue>();
    [SerializeField]
    private BoolReference _isInDialogue;
    [SerializeField]
    private BoolReference _isInStaticDialogue;

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

    private List<GameObject> _spawnedTextBalloons = new List<GameObject>();
    private List<GameObject> _spawnedTails = new List<GameObject>();

    private Dialogue _currentDialogue;
    private DialogueNode _currentDialogueNode;


    private void Start()
    {
        _isInStaticDialogue.variable.value = false;
        _isInDialogue.variable.value = false;

    }

    private void LateUpdate()
    {
        //if (!_stateManager.IsInDialogue.value || !_allowMovement || _stateManager.IsInStaticDialogue.value) return;
        //Vector2 newPos = Vector2.zero;
        //Vector2 Tailpos = Vector2.zero;
        //if (_firstTextBalloon.Count != 0)
        //{
        //    newPos = GetTextBalloonPos(_firstTextBalloon);

        //    _firstTextBalloon[0].transform.position = new Vector3(newPos.x, newPos.y, 0);
        //    _firstTextBalloon[2].transform.position = new Vector3(newPos.x, newPos.y, 0);
        //    _textLineOne.transform.position = new Vector3(newPos.x, newPos.y, 0);

        //    Tailpos = GetTailPosition(_firstTextBalloon);

        //    _firstTextBalloon[1].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        //    _firstTextBalloon[3].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        //}

        //if (_secondTextBalloon.Count != 0)
        //{
        //    newPos = _firstTextBalloon[0].transform.position;
        //    newPos.x += _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
        //    newPos.y -= _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;

        //    _secondTextBalloon[0].transform.position = newPos;
        //    _secondTextBalloon[2].transform.position = newPos;
        //    _textLineTwo.transform.position = newPos;
        //}
    }

    public void StartNewDialoge(Component sender, object obj)
    {
        DialogueTriggerEventArgs args = obj as DialogueTriggerEventArgs;
        if (args == null) return;
        if (_isInDialogue.value) return;
        _currentDialogueIndex = args.NextDialogueIndex;
        if (_dialogues.Count - 1 < _currentDialogueIndex) return;
        if (!_dialogues[_currentDialogueIndex].GetIsStarted()) _dialogues[_currentDialogueIndex].SetIsStarted(true);
        else return;
        _isInDialogue.variable.value = true;
        if(_dialogues[_currentDialogueIndex].GetIsStaticDialogue()) _isInStaticDialogue.variable.value = true;
        _currentDialogue = _dialogues[_currentDialogueIndex];
        _currentDialogueNode = _currentDialogue.GetAllNodes().ToList()[_currentLineIndex];
        StartCoroutine(EnableTextBalloon());
    }

    public void PlayNextLine(Component sender, object obj)
    {
        if (_isTyping)
        {
            _canSkip = true;
            return;
        }
        if (_dialogues[_currentDialogueIndex].GetIsStaticDialogue())
        {
            if (_currentLineIndex < _dialogues[_currentDialogueIndex].GetAllNodes().ToList().Count() - 1)
            {
                _currentLineIndex++;
                _currentDialogueNode = _currentDialogue.GetAllNodes().ToList()[_currentLineIndex];
                StartCoroutine(EnableTextBalloon());
            }
            else
            {
                StartCoroutine(DissableTextBalloon(false, _spawnedTextBalloons, false));
            }
            return;
        }
        StartCoroutine(DissableTextBalloon(true, _spawnedTextBalloons, true));
    }

    private Vector2 GetTextBalloonPos(List<GameObject> textBalloon, TextMeshProUGUI text)
    {
        GameObject target = GameObject.Find(_currentDialogueNode.GetCharacterName());
        Vector3 targetPos = target.transform.position;
        targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

        Vector2 TextballoonPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);

        TextballoonPos.y += textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.5f;

        float offsetX = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.5f;
        bool needsFlip = _currentDialogueNode.GetNeedsToBeFlipped();

        if (needsFlip)
        {
            TextballoonPos = new Vector2(TextballoonPos.x - offsetX, TextballoonPos.y);
            textBalloon[0].transform.rotation = Quaternion.Euler(0, 180, 0);
            textBalloon[2].transform.rotation = Quaternion.Euler(0, 180, 0);
            text.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            TextballoonPos = new Vector2(TextballoonPos.x + offsetX, TextballoonPos.y);
            if (textBalloon.Count != 0)
            {
                textBalloon[0].transform.rotation = Quaternion.Euler(0, 0, 0);
                textBalloon[2].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            text.transform.localRotation = Quaternion.Euler(0, 0, 0);
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
        List<GameObject> tempList = ConstructTextBalloon(true);

        string tempText = " ";

        GameObject textObject = Instantiate(new GameObject());
        textObject.AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (_currentDialogueNode.GetShoutingImages().Count == 0) tempText = InsertLineBreaksByWord(_currentDialogueNode.GetText(), 5, tempList, text);
        else
        {
            DistributeImages(tempList);
            DetermineTextBalloonSizeBasedOnImages();
        }

        Vector2 balloonPos = GetTextBalloonPos(tempList, text);


        tempList[0].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        tempList[2].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);

        Vector2 Tailpos = GetTailPosition(tempList);

        tempList[1].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        tempList[3].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);

        text.transform.parent = _balloonHolder.transform;
        text.transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Center;

        foreach(GameObject obj in tempList)
        {
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        float transparacy = 0;

        while(transparacy < 1)
        {
            transparacy += Time.deltaTime * _fadeInSpeed;

            foreach (GameObject obj in tempList)
            {
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            yield return null;
        }

        foreach (GameObject obj in tempList)
        {
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
        StartCoroutine(TypeText(tempText, text));

        //if(_currentLine.Text.SecondLine != "")
        //{
        //    yield return new WaitForSeconds(tempText.Length * (_currentLine.DisplaySpeed + 0.05f));

        //    _secondTextBalloon = ConstructTextBalloon(false);

        //    Vector3 newPos = _firstTextBalloon[0].transform.position;
        //    newPos.x += _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
        //    newPos.y -= _firstTextBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;

        //    _secondTextBalloon[0].transform.position = newPos;
        //    _secondTextBalloon[2].transform.position = newPos;

        //    tempText = " ";
        //    if (_currentLine.Images.Count == 0) tempText = InsertLineBreaksByWord(_currentLine.Text.SecondLine, 5, _secondTextBalloon, _textLineTwo);
        //    else
        //    {
        //        DistributeImages(_secondTextBalloon);
        //        DetermineTextBalloonSizeBasedOnImages();
        //    }

        //    _textLineTwo.transform.parent = _balloonHolder.transform;
        //    _textLineTwo.transform.position = new Vector3(_secondTextBalloon[0].transform.position.x, _secondTextBalloon[0].transform.position.y, 0);
        //    _textLineTwo.fontSize = _currentLine.BaseFontSize;
        //    _textLineTwo.font = _currentLine.BaseFont;

        //    foreach (GameObject obj in _secondTextBalloon)
        //    {
        //        if (obj == null) continue;
        //        Image image = obj.GetComponent<Image>();
        //        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        //    }

        //    transparacy = 0;

        //    while (transparacy < 1)
        //    {
        //        transparacy += Time.deltaTime * _fadeInSpeed;

        //        foreach (GameObject obj in _secondTextBalloon)
        //        {
        //            if (obj == null) continue;
        //            Image image = obj.GetComponent<Image>();
        //            image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
        //        }

        //        yield return null;
        //    }

        //    foreach (GameObject obj in _secondTextBalloon)
        //    {
        //        if (obj == null) continue;
        //        Image image = obj.GetComponent<Image>();
        //        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        //    }

        //    StartCoroutine(TypeText(tempText, _textLineTwo));
        //}
    }

    private List<GameObject> ConstructTextBalloon(bool spawnTail)
    {
        List <GameObject> list = new List<GameObject>();
        GameObject textBalloonBorder = Instantiate(_currentDialogueNode.GetBalloonObject());
        textBalloonBorder.GetComponent<Image>().color = Color.black;
        textBalloonBorder.transform.parent = _blackHolder.transform;

        list.Add(textBalloonBorder);
        _spawnedTextBalloons.Add(textBalloonBorder);

        GameObject tailBorder = null;

        if (spawnTail)
        {
            tailBorder = Instantiate(_currentDialogueNode.GetTailObject());
            tailBorder.GetComponent<Image>().color = Color.black;
            tailBorder.transform.parent = _blackHolder.transform;
        }

        list.Add(tailBorder);
        _spawnedTails.Add(tailBorder);

         GameObject textBalloon = Instantiate(_currentDialogueNode.GetBalloonObject());
         textBalloon.transform.parent = _whiteHolder.transform;

         list.Add(textBalloon);
        _spawnedTextBalloons.Add(textBalloon);

        GameObject tail = null;
        if (spawnTail)
        {
            tail = Instantiate(_currentDialogueNode.GetTailObject());
            tail.transform.parent = _whiteHolder.transform;
        }

        list.Add(tail);
        _spawnedTails.Add(tail);

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
        DialogueNode node = _currentDialogueNode;
        TMP_FontAsset tempFont = text.font;
        if (tempFont == null)
        {
            Debug.LogError("Font asset is null!");
            return;
        }

        // Get character 'A' metrics
        float characterWidth = 0;
        float characterHeight = 0;
        const char sampleChar = 'a';

        if (tempFont.characterLookupTable.TryGetValue(sampleChar, out TMP_Character character))
        {
            characterWidth = character.glyph.metrics.width * 0.60f;
            characterHeight = character.glyph.metrics.height * 0.60f;
        }
        else
        {
            Debug.LogError($"Font {tempFont.name} is missing character '{sampleChar}'");
        }

        float FontSizeMultiplier = DetermineFontSizeMultiplier();

        Vector2 size = new Vector2(((FontSizeMultiplier * characterWidth) * LetterCount), ((FontSizeMultiplier * characterHeight) * linecount));
       size += _currentDialogueNode.GetSizePadding() + new Vector2(20,20);

        textBalloon[0].GetComponent<Image>().rectTransform.sizeDelta = size * node.GetBorderSize() + node.GetSizePadding();
        textBalloon[1].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
        textBalloon[2].GetComponent<Image>().rectTransform.sizeDelta = new Vector2(size.x * node.GetBorderSize(), size.y * node.GetBorderSize()) + node.GetSizePadding();
        textBalloon[3].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();

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

        //imageSizeX += _activeImages[0].GetComponent<Image>().rectTransform.rect.width;
        //imagesSizeY += _activeImages[0].GetComponent<Image>().rectTransform.rect.height;


        //if (_currentLine.Images.Count > 1)
        //{
        //    imageSpacingX = Vector3.Distance(_activeImages[0].transform.position, _activeImages[1].transform.position);
        //    imageSpacingX -= imageSizeX / _currentLine.Images.Count;

        //    if (_currentLine.HasSecondImageLine)
        //    {
        //        imageSpacingY = Vector3.Distance(_activeImages[0].transform.position, _activeImages[(int)Mathf.Ceil(_currentLine.Images.Count * 0.5f)].transform.position);
        //        imageSpacingY -= imagesSizeY / 2;
        //    }
        //}

        //float imagesOnFirstLine = Mathf.Ceil(_currentLine.Images.Count * 0.5f);
        //Vector2 size = new Vector2((imageSizeX + imageSpacingX) * imagesOnFirstLine, (imagesSizeY + imageSpacingY) * 2);
        //size += _currentLine.Padding;

        //foreach (Image image in _textBalloonImages.Keys)
        //{
        //    RectTransform balloonRect = image.rectTransform;
        //    //if (_textBalloonImages[image]) balloonRect.sizeDelta = new Vector2(size.x * (1 + _currentLine.BorderSize), size.y * (1 + _currentLine.BorderSize));
        //    //else balloonRect.sizeDelta = size;
        //}
        //_textLineOne.rectTransform.sizeDelta = size;
    }

    private float DetermineFontSizeMultiplier()
    {
        float baseSize = 36;
        // float currentSize = _currentLine.BaseFontSize;

        //float multiplier = currentSize / baseSize;

        // return multiplier;
        return 0;
    }

    private void DistributeImages(List<GameObject> textBalloon)
    {
        DialogueNode node = _currentDialogueNode;
        RectTransform rectTransform = textBalloon[0].GetComponent<Image>().rectTransform;

        Rect imageRect = RectTransformUtility.PixelAdjustRect(rectTransform, _canvas.GetComponent<Canvas>());

        float balloonWith = imageRect.width;
        float balloonHeight = imageRect.height;

        float spacingX = 0;
        float spacingY = 0;

        int amountOfImagesInLine = 0;
        int imagesOnFirstLine = 0;
        int imagesOnSecondLine = 0;

        int linesindex = 1;

        //if (node.gethass)
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

        //int childCount = _currentLine.Images.Count;
        //int columns = Mathf.CeilToInt(Mathf.Sqrt(childCount));
        //int rows = Mathf.CeilToInt((float)childCount / columns);

        Image balloon = textBalloon[2].GetComponent<Image>();
        RectTransform balloonRect = balloon.rectTransform;

        // Define what percentage of space you want the images to occupy (0.6 = 60%)
        float imageSizeRatio = 1f;

        // Calculate desired cell dimensions based on the container size and image ratio
        //float desiredCellWidth = (balloonRect.rect.width / columns) * imageSizeRatio;
        //float desiredCellHeight = (balloonRect.rect.height / rows) * imageSizeRatio;

        // Calculate spacing based on desired cell size
        //float spacingX = (balloonRect.rect.width - (desiredCellWidth * columns)) / (columns - 1);
        //float spacingY = (balloonRect.rect.height - (desiredCellHeight * rows)) / (rows - 1);

        // Use these calculated values for placement
        //for (int i = 0; i < childCount; i++)
        //{
        //    int row = 0;
        //    if (_currentLine.HasSecondImageLine) row = i / columns;
        //    else row = 1;
        //    int col = i % columns;
        //    GameObject newImage = (Instantiate(_currentLine.Images[i], textBalloon[0].transform));
        //    if (newImage != null)
        //    {
        //        _activeImages.Add(newImage);
        //        // Calculate position using the spacing values we just determined
        //        float x = (col * desiredCellWidth) + (col * spacingX);
        //        float y = (row * desiredCellHeight) + (row * spacingY);

        //        // Adjust position based on balloon's position
        //        Vector3 position = new Vector3(x - balloonRect.rect.width / 2 + desiredCellWidth / 2, -y + balloonRect.rect.height / 2 - desiredCellHeight / 2, 0);

        //        newImage.transform.localPosition = position;

        //        //// Make sure the child has a RectTransform to set its size
        //        //RectTransform childRect = newImage.GetComponent<RectTransform>();
        //        //if (childRect != null)
        //        //{
        //        //    childRect.sizeDelta = new Vector2(desiredCellWidth, desiredCellHeight);
        //        //}
        //    }
        //}
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
            //yield return new WaitForSeconds(_currentLine.DisplaySpeed);
            yield return null;
        }
        _isTyping = false;
    }

    private IEnumerator DissableTextBalloon(bool startNextLine, List<GameObject> textBalloon, bool GoToNext)
    {
        float transparacy = 1;

        while (transparacy > 0)
        {
            transparacy -= Time.deltaTime * _fadeInSpeed;

            //foreach (Image image in _textBalloonImages.Keys)
            //{
            //    image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            //}

            //_textLineOne.color = new Color(0, 0, 0, transparacy);
            //if (_currentLine.Text.SecondLine != "")
            //    _textLineTwo.color = new Color(0, 0, 0, transparacy);

            //if (_activeImages.Count != 0)
            //{
            //    foreach (GameObject image in _activeImages)
            //    {
            //        image.GetComponent<Image>().color = new Color(0, 0, 0, transparacy);
            //    }
            //}
            yield return null;
        }

        //foreach (Image image in _textBalloonImages.Keys)
        //{
        //    image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        //}

        //_textLineOne.color = new Color(0, 0, 0, 0);
        //if (_currentLine.Text.SecondLine != "")
        //    _textLineTwo.color = new Color(0, 0, 0, 0);

        //if (_activeImages.Count != 0)
        //{
        //    foreach (GameObject image in _activeImages)
        //    {
        //        image.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        //        GameObject.Destroy(image.gameObject);
        //    }
        //}

        //_textLineOne.text = "";
        //_textLineOne.color = Color.black;
        //if (_currentLine.Text.SecondLine != "")
        //{
        //    _textLineTwo.text = "";
        //    _textLineTwo.color = Color.black;
        //}
        //_textLineOne.transform.parent = _canvas.transform;
        foreach(GameObject image in textBalloon)
        {
            Destroy(image);
        }
        //_textBalloonImages = new Dictionary<Image, bool>();
        _allowMovement = false;

        if (GoToNext)
        {
            //if (_currentLineIndex < _dialogues[_currentDialogueIndex].Lines.Count - 1)
            //{
            //    _currentLineIndex++;
            //    if (startNextLine) StartCoroutine(EnableTextBalloon());
            //    else _isInDialogue.variable.value = false;
            //}
            //else
            //{
            //    _isInDialogue.variable.value = false;
            //    if(_dialogues[_currentDialogueIndex].IsStatic) _isInStaticDialogue.variable.value = false;
            //    _currentLineIndex = 0;
            //}
        }
    }

    private void OnDestroy()
    {
        //foreach(Dialogues dialogue in _dialogues)
        //{
        //    dialogue.IsStarted = false;
        //}
    }

    private void OnDisable()
    {
        //foreach (Dialogues dialogue in _dialogues)
        //{
        //    dialogue.IsStarted = false;
        //}
    }
}