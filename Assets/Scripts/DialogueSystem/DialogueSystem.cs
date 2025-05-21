using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
    [SerializeField]
    private GameObject _bridgeObject;

    private int _currentDialogueIndex = 0;


    private int _currentLineIndex = 0;
    private bool _canSkip;
    private bool _isTyping;
    private bool _allowMovement;

    private List<GameObject> _spawnedTextBalloons = new List<GameObject>();
    private List<GameObject> _spawnedTails = new List<GameObject>();
    private List<GameObject> _activeShoutingImages = new List<GameObject>();
    private List<GameObject> _spawnedText = new List<GameObject>();

    private Dialogue _currentDialogue;
    private DialogueNode _currentDialogueNode;

    private bool _spawnSubBalloon;

    private Vector2 _previousPos;
    private Vector2 _previousTextBalloonSize;

    private Dictionary<DialogueNode, List<GameObject>> _spawnedDialogue = new Dictionary<DialogueNode, List<GameObject>>();
    private List<GameObject> _TextBalloonBridges = new List<GameObject>();

    private Vector3 _bridgeOrigin;


    private void Start()
    {
        _isInStaticDialogue.variable.value = false;
        _isInDialogue.variable.value = false;

    }

    private void LateUpdate()
    {
        if (!_stateManager.IsInDialogue.value || !_allowMovement || _stateManager.IsInStaticDialogue.value) return;
        Vector2 newPos = Vector2.zero;
        Vector2 Tailpos = Vector2.zero;
        if (_spawnedTextBalloons.Count != 0)
        {
            newPos = GetTextBalloonPos(_spawnedTextBalloons, _spawnedText[0].GetComponent<TextMeshProUGUI>(), _currentDialogueNode.GetNeedsToBeFlipped(), false);

            if (_currentDialogueNode.GetIsShouting())
            {
                newPos += new Vector2(150, 50);
            }

            if (_spawnedTextBalloons[0] != null) _spawnedTextBalloons[0].transform.position = new Vector3(newPos.x, newPos.y, 0);
            if (_spawnedTextBalloons[1] != null) _spawnedTextBalloons[1].transform.position = new Vector3(newPos.x, newPos.y, 0);

            if (_spawnedTextBalloons[0] != null)
                _spawnedText[0].transform.position = new Vector3(newPos.x, newPos.y, 0);
            else
                _spawnedText[0].transform.position = new Vector3(newPos.x, newPos.y + 200, 0);

            if (_spawnedTextBalloons.Count > 2)
            {
                float yOffset = _spawnedTextBalloons[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;
                float xOffset = _spawnedTextBalloons[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
                if (_spawnedTextBalloons[2] != null) _spawnedTextBalloons[2].transform.position = new Vector3(newPos.x + xOffset, newPos.y + -yOffset, 0);
                if (_spawnedTextBalloons[3] != null) _spawnedTextBalloons[3].transform.position = new Vector3(newPos.x + xOffset, newPos.y + -yOffset, 0);

                _spawnedText[1].transform.position = new Vector3(newPos.x + xOffset, newPos.y + -yOffset, 0);
            }
        }

        if (_spawnedTails.Count != 0)
        {
            Tailpos = GetTailPosition(_spawnedTextBalloons);

            if (_currentDialogueNode.GetIsShouting())
            {
                Tailpos -= new Vector2(-25, 0);
            }

            if(_spawnedTails[0] != null)_spawnedTails[0].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
            if(_spawnedTails[1]) _spawnedTails[1].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        }
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
        _previousPos = Vector2.zero;
        _previousTextBalloonSize = Vector2.zero;
        _spawnedDialogue = new Dictionary<DialogueNode, List<GameObject>>();
        _TextBalloonBridges = new List<GameObject>();
        StartCoroutine(EnableTextBalloon());
    }

    public void PlayNextLine(Component sender, object obj)
    {
        if (_isTyping)
        {
            _canSkip = true;
            return;
        }

        if (_currentLineIndex < _dialogues[_currentDialogueIndex].GetAllNodes().ToList().Count() - 1)
        {
            if (_dialogues[_currentDialogueIndex].GetIsStaticDialogue())
            {
                _currentLineIndex++;
                _currentDialogueNode = _currentDialogue.GetAllNodes().ToList()[_currentLineIndex];

                StartCoroutine(EnableTextBalloon());
            }
            else
            {
                if (_spawnSubBalloon)
                    StartCoroutine(EnableTextBalloon());
                else StartCoroutine(DissableTextBalloon(true, true));
            }
        }
        else
        {
            StartCoroutine(DissableTextBalloon(false, false));
        }
    }

    private Vector2 GetTextBalloonPos(List<GameObject> textBalloon, TextMeshProUGUI text, bool needsFlip, bool addOffset)
    {
        Vector2 TextballoonPos = Vector2.zero;
        if (_spawnSubBalloon && addOffset)
        {
            TextballoonPos = new Vector2(_previousPos.x, _previousPos.y);
            float yOffset = 0;
            float xOffset = 0;
            if (textBalloon[0] != null) yOffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;
            if (textBalloon[0] != null) xOffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
            TextballoonPos += new Vector2(xOffset, -yOffset);
            return TextballoonPos;
        }
        GameObject target = GameObject.Find(_currentDialogueNode.GetCharacterName());
        Vector3 targetPos = target.transform.position;
        targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

        TextballoonPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);

        float Xoffset = 0;
        float Yoffset = 0;
        if (textBalloon[0] != null) Xoffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.45f;
        if (textBalloon[0] != null) Yoffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.55f;

        if (_currentDialogue.GetIsStaticDialogue())
        {
            if (_previousPos != Vector2.zero)
            {
                TextballoonPos = _previousPos;
                if (textBalloon[0] != null) Xoffset = -textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.75f;
                if (textBalloon[0] != null) Yoffset = -(textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.5f) - (_previousTextBalloonSize.y * 0.5f) + 100;
            }
            else
            {
                if (textBalloon[0] != null) Xoffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.25f;
            }
        }

        if (needsFlip)
        {
            TextballoonPos = new Vector2(-Xoffset, Yoffset) + TextballoonPos;
        }
        else
        {
            TextballoonPos = new Vector2(Xoffset, Yoffset) + TextballoonPos;
        }

        return TextballoonPos;
    }

    private Vector2 GetTailPosition(List<GameObject> textBalloon)
    {
        int firstIndex = 0;
        int secondIndex = 0;
        if(textBalloon.Count() > 2)
        {
            firstIndex = 1;
            secondIndex = 3;
        }
        else
        {
            firstIndex = 0;
            secondIndex = 1;
        }
        if(textBalloon[firstIndex] == null || textBalloon[secondIndex] == null) return Vector2.zero;
        float textBalloonWidth = textBalloon[0].GetComponent<Image>().rectTransform.rect.width;
        float textBalloonHeight = textBalloon[0].GetComponent<Image>().rectTransform.rect.height;

        Vector2 tailPos = new Vector2(textBalloon[0].gameObject.transform.position.x, textBalloon[0].gameObject.transform.position.y);
        float Yoffset = -textBalloonHeight * 0.25f;
        float Xoffset = -textBalloonWidth * 0.25f;

        float xRotation = 0;
        if (_currentDialogue.GetIsStaticDialogue())
        {
            if (_previousPos != Vector2.zero)
            {
                Yoffset = -Yoffset;
                xRotation = 180;
            }
        }

        bool needsFlip = _currentDialogueNode.GetNeedsToBeFlipped();
        if (needsFlip)
        {
            tailPos = new Vector2(-Xoffset, Yoffset) + tailPos;
            textBalloon[firstIndex].transform.rotation = Quaternion.Euler(xRotation, 180, 0);
            textBalloon[secondIndex].transform.rotation = Quaternion.Euler(xRotation, 180, 0);
        }
        else
        {
            tailPos = new Vector2(Xoffset, Yoffset) + tailPos;
            textBalloon[firstIndex].transform.rotation = Quaternion.Euler(xRotation, 0, 0);
            textBalloon[secondIndex].transform.rotation = Quaternion.Euler(xRotation, 0, 0);
        }

        return tailPos;
    }

    private IEnumerator EnableTextBalloon()
    {
        _allowMovement = true;
        _isTyping = true;
        GameObject textObject = Instantiate(new GameObject());
        textObject.AddComponent<TextMeshProUGUI>();
        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        _spawnedText.Add(textObject);
        List<GameObject> tempList = ConstructTextBalloon(!_spawnSubBalloon);

        string tempText = " ";

        if (_currentDialogueNode.GetShoutingImages().Count == 0) tempText = InsertLineBreaksByWord(_currentDialogueNode.GetText(), 5, tempList, text);
        else
        {
            DistributeImages(tempList);
            DetermineTextBalloonSizeBasedOnImages(tempList, _currentDialogueNode, text);
        }

        Vector2 balloonPos = GetTextBalloonPos(tempList, text, _currentDialogueNode.GetNeedsToBeFlipped(), true);

        if (_currentDialogueNode.GetIsShouting())
        {
            balloonPos += new Vector2(150, 50);
        }

        if(tempList[0] != null) tempList[0].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        if (tempList[2] != null) tempList[2].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);

        if(tempList[1] != null || tempList[3] != null)
        {
            Vector2 Tailpos = GetTailPosition(tempList);

            if (_currentDialogueNode.GetIsShouting())
            {
                Tailpos -= new Vector2(-25, 0);
            }

            tempList[1].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
            tempList[3].transform.position = new Vector3(Tailpos.x, Tailpos.y, 0);
        }

        _spawnedDialogue.Add(_currentDialogueNode, tempList);

        if (_TextBalloonBridges.Count != 0 && !_spawnSubBalloon && _spawnedDialogue.Count != 0)
        {
            _TextBalloonBridges[0].transform.position = Vector3.Lerp(_bridgeOrigin, _spawnedDialogue[_spawnedDialogue.Keys.ToList()[_spawnedDialogue.Count - 1]][0].transform.position, 0.5f);
            _TextBalloonBridges[1].transform.position = _TextBalloonBridges[0].transform.position;
            Vector2 size = _TextBalloonBridges[0].GetComponent<Image>().rectTransform.rect.size;
            size.y = Vector3.Distance(_bridgeOrigin, _spawnedDialogue[_spawnedDialogue.Keys.ToList()[_spawnedDialogue.Count - 1]][0].transform.position);
            _TextBalloonBridges[0].GetComponent<Image>().rectTransform.sizeDelta = size;
            _TextBalloonBridges[1].GetComponent<Image>().color = Color.black;
            _TextBalloonBridges[1].GetComponent<Image>().rectTransform.sizeDelta = size * _currentDialogueNode.GetBorderSize();
            Vector3 from = _bridgeOrigin;
            Vector3 to = _spawnedDialogue[_spawnedDialogue.Keys.ToList()[_spawnedDialogue.Count - 1]][0].transform.position;
            Vector3 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            _TextBalloonBridges[0].transform.rotation = Quaternion.Euler(0, 0, angle + 90);
            _TextBalloonBridges[1].transform.rotation = Quaternion.Euler(0, 0, angle + 90);

            foreach (GameObject obj in _TextBalloonBridges)
            {
                if (obj == null) continue;
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            }
        }

        if (tempList[0] != null) _previousTextBalloonSize = tempList[0].GetComponent<Image>().rectTransform.sizeDelta;
        if (tempList[0] != null) _previousPos = new Vector2(tempList[0].transform.position.x, tempList[0].transform.position.y);

        text.transform.parent = _balloonHolder.transform;
        if (tempList[0] != null)
            text.transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        else
            text.transform.position = new Vector3(balloonPos.x, balloonPos.y + 200, 0);
        text.color = Color.black;
        text.alignment = TextAlignmentOptions.Center;

        foreach(GameObject obj in tempList)
        {
            if (obj == null) continue;
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        float transparacy = 0;

        while(transparacy < 1)
        {
            transparacy += Time.deltaTime * _fadeInSpeed;

            foreach (GameObject obj in tempList)
            {
                if (obj == null) continue;
                Image image = obj.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            if (_TextBalloonBridges.Count != 0 && !_spawnSubBalloon && _spawnedDialogue.Count != 0)
            {
                foreach (GameObject obj in _TextBalloonBridges)
                {
                    if (obj == null) continue;
                    Image image = obj.GetComponent<Image>();
                    image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
                }
            }

            yield return null;
        }

        if (_TextBalloonBridges.Count != 0)
        {
            _spawnedDialogue = new Dictionary<DialogueNode, List<GameObject>>();
        }

        foreach (GameObject obj in tempList)
        {
            if (obj == null) continue;
            Image image = obj.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        }
        StartCoroutine(TypeText(tempText, text, _currentDialogueNode));

        if (_currentDialogueNode.GetHasSecondaryLine())
        {
            yield return new WaitForSeconds(_currentDialogueNode.GetText().Length * 0.05f);
            _allowMovement = false;
            _spawnSubBalloon = true;
            _currentLineIndex++;
            _currentDialogueNode = _currentDialogue.GetAllNodes().ToList()[_currentLineIndex];
            StartCoroutine(EnableTextBalloon());
        }
        else _spawnSubBalloon = false;
    }

    private List<GameObject> ConstructTextBalloon(bool spawnTail)
    {
        List <GameObject> list = new List<GameObject>();
        GameObject textBalloonBorder = null;

        if (_currentDialogueNode.GetBalloonObject() != null)
        {
            textBalloonBorder = Instantiate(_currentDialogueNode.GetBalloonObject());
            textBalloonBorder.GetComponent<Image>().color = Color.black;
            textBalloonBorder.transform.parent = _blackHolder.transform;
        }

        list.Add(textBalloonBorder);
        _spawnedTextBalloons.Add(textBalloonBorder);

        if(_spawnedDialogue.Count != 0 && !_spawnSubBalloon && _currentDialogue.GetIsStaticDialogue())
        {
            foreach(DialogueNode node in _spawnedDialogue.Keys)
            {
                if(node.GetCharacterName() == _currentDialogueNode.GetCharacterName())
                {
                    spawnTail = false;
                    GameObject temp = Instantiate(_bridgeObject);
                    temp.transform.parent = _whiteHolder.transform;
                    _TextBalloonBridges.Add(temp);
                    _bridgeOrigin = _spawnedDialogue[node][0].transform.position;
                    temp = Instantiate(_bridgeObject);
                    temp.transform.parent = _blackHolder.transform;
                    _TextBalloonBridges.Add(temp);
                }
            }
        }

        GameObject tailBorder = null;

        if (spawnTail && _currentDialogueNode.GetTailObject() != null)
        {
            tailBorder = Instantiate(_currentDialogueNode.GetTailObject());
            tailBorder.GetComponent<Image>().color = Color.black;
            tailBorder.transform.parent = _blackHolder.transform;
        }

        list.Add(tailBorder);
        _spawnedTails.Add(tailBorder);

         GameObject textBalloon = null;
        if(_currentDialogueNode.GetBalloonObject() != null)
        {
            textBalloon = Instantiate(_currentDialogueNode.GetBalloonObject());
            textBalloon.transform.parent = _whiteHolder.transform;
        }

         list.Add(textBalloon);
        _spawnedTextBalloons.Add(textBalloon);

        GameObject tail = null;
        if (spawnTail && _currentDialogueNode.GetTailObject() != null)
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

        float FontSizeMultiplier = DetermineFontSizeMultiplier(text.fontSize);

        Vector2 size = new Vector2(((FontSizeMultiplier * characterWidth) * LetterCount), ((FontSizeMultiplier * characterHeight) * linecount));
       size += _currentDialogueNode.GetSizePadding() + new Vector2(20,20);

        if (textBalloon[0] != null) textBalloon[0].GetComponent<Image>().rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
        if (textBalloon[2] != null) textBalloon[2].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
        if (textBalloon[1] != null) textBalloon[1].GetComponent<Image>().rectTransform.sizeDelta = (size * node.GetBorderSize()) + node.GetSizePadding();
        if (textBalloon[3] != null) textBalloon[3].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
        if (_currentDialogueNode.GetIsShouting())
        {
            if (textBalloon[1] != null) textBalloon[1].GetComponent<Image>().rectTransform.sizeDelta += new Vector2(2000, 25);
            if (textBalloon[3] != null) textBalloon[3].GetComponent<Image>().rectTransform.sizeDelta += new Vector2(2000, 25);
        }

        if (text != null)
        {
            text.rectTransform.sizeDelta = size;
        }
    }

    private void DetermineTextBalloonSizeBasedOnImages(List<GameObject> textBalloon, DialogueNode node, TextMeshProUGUI text)
    {
        float imageSizeX = 0;
        float imagesSizeY = 0;

        float imageSpacingX = 0;
        float imageSpacingY = 0;

        imageSizeX += _activeShoutingImages[0].GetComponent<Image>().rectTransform.rect.width;
        imagesSizeY += _activeShoutingImages[0].GetComponent<Image>().rectTransform.rect.height;


        if (node.GetShoutingImages().Count > 1)
        {
            imageSpacingX = Vector3.Distance(_activeShoutingImages[0].transform.position, _activeShoutingImages[1].transform.position);
            imageSpacingX -= imageSizeX / node.GetShoutingImages().Count;

            if (node.GetHasSecondImageLine())
            {
                imageSpacingY = Vector3.Distance(_activeShoutingImages[0].transform.position, _activeShoutingImages[(int)Mathf.Ceil(node.GetShoutingImages().Count * 0.5f)].transform.position);
                imageSpacingY -= imagesSizeY / 2;
            }
        }

        float imagesOnFirstLine = Mathf.Ceil(node.GetShoutingImages().Count * 0.5f);
        Vector2 size = new Vector2((imageSizeX + imageSpacingX) * imagesOnFirstLine, (imagesSizeY + imageSpacingY) * 2);
        size += node.GetSizePadding() + new Vector2(20,20);

        textBalloon[0].GetComponent<Image>().rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
        textBalloon[2].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
        if (textBalloon[1] != null) textBalloon[1].GetComponent<Image>().rectTransform.sizeDelta = (size * node.GetBorderSize()) + node.GetSizePadding();
        if (textBalloon[3] != null) textBalloon[3].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
        text.rectTransform.sizeDelta = size;
    }

    private float DetermineFontSizeMultiplier(float size)
    {
        float baseSize = 36;
         float currentSize = size;

        float multiplier = currentSize / baseSize;

         return multiplier;
    }

    private void DistributeImages(List<GameObject> textBalloon)
    {
        DialogueNode node = _currentDialogueNode;
        //RectTransform rectTransform = textBalloon[0].GetComponent<Image>().rectTransform;

        //Rect imageRect = RectTransformUtility.PixelAdjustRect(rectTransform, _canvas.GetComponent<Canvas>());

        //float balloonWith = imageRect.width;
        //float balloonHeight = imageRect.height;

        //float spacingX = 0;
        //float spacingY = 0;

        //int amountOfImagesInLine = 0;
        //int imagesOnFirstLine = 0;
        //int imagesOnSecondLine = 0;

        //int linesindex = 1;

        //if (node.GetHasSecondImageLine())
        //{
        //    spacingX = balloonWith / Mathf.Ceil(node.GetShoutingImages().Count * 0.5f);
        //    spacingY = balloonHeight * 0.25f;

        //    imagesOnFirstLine = (int)Mathf.Ceil(node.GetShoutingImages().Count * 0.5f);
        //    imagesOnSecondLine = node.GetShoutingImages().Count - imagesOnFirstLine;
        //    linesindex = 2;
        //}
        //else
        //{
        //    spacingX = balloonWith / node.GetShoutingImages().Count;
        //    imagesOnFirstLine = node.GetShoutingImages().Count;
        //}

        //amountOfImagesInLine = imagesOnFirstLine;

        //for (int i = 0; i < linesindex; i++)
        //{
        //    for (int j = 0; j < amountOfImagesInLine; j++)
        //    {
        //        GameObject newImage = (Instantiate(node.GetShoutingImages()[j * (i + 1)], textBalloon[i].transform));

        //        //float newX = +spacingX - (spacingX * j);

        //        float newX = 0 + (balloonWith / 2f) - (spacingX * j) - (newImage.GetComponent<Image>().rectTransform.rect.width);
        //        float newY = 0;

        //        if (node.GetHasSecondImageLine()) newY = 0 + (balloonHeight * 0.25f) - (spacingY * i) - (newImage.GetComponent<Image>().rectTransform.rect.width / 2);

        //        newImage.transform.localPosition = new Vector3(newX, newY, 0);
        //        newImage.transform.parent = _balloonHolder.transform;
        //        _activeShoutingImages.Add(newImage);
        //    }

        //    amountOfImagesInLine = imagesOnSecondLine;
        //}

        int childCount = node.GetShoutingImages().Count;
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
            if (node.GetHasSecondImageLine()) row = i / columns;
            else row = 1;
            int col = i % columns;
            GameObject newImage = (Instantiate(node.GetShoutingImages()[i], textBalloon[0].transform));
            if (newImage != null)
            {
                _activeShoutingImages.Add(newImage);
                // Calculate position using the spacing values we just determined
                float x = (col * desiredCellWidth) + (col * spacingX);
                float y = (row * desiredCellHeight) + (row * spacingY);

                // Adjust position based on balloon's position
                Vector3 position = new Vector3(x - balloonRect.rect.width / 2 + desiredCellWidth / 2, -y + balloonRect.rect.height / 2 - desiredCellHeight / 2, 0);

                newImage.transform.localPosition = position;

                // Make sure the child has a RectTransform to set its size
                RectTransform childRect = newImage.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    childRect.sizeDelta = new Vector2(desiredCellWidth, desiredCellHeight);
                }
            }
        }
    }

    private IEnumerator TypeText(string text, TextMeshProUGUI textObject, DialogueNode node)
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
            yield return new WaitForSeconds(node.GetTextDisplaySpeed());
        }
        _isTyping = false;
    }

    private IEnumerator DissableTextBalloon(bool startNextLine, bool GoToNext)
    {
        float transparacy = 1;

        while (transparacy > 0)
        {
            transparacy -= Time.deltaTime * _fadeInSpeed;

            foreach (GameObject imageObject in _spawnedTextBalloons)
            {
                if (imageObject == null) continue;
                Image image = imageObject.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            foreach (GameObject imageObject in _spawnedTails)
            {
                if (imageObject == null) continue;
                Image image = imageObject.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            foreach (GameObject imageObject in _TextBalloonBridges)
            {
                if (imageObject == null) continue;
                Image image = imageObject.GetComponent<Image>();
                image.color = new Color(image.color.r, image.color.g, image.color.b, transparacy);
            }

            foreach (GameObject textObject in _spawnedText)
            {
                TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
                text.color = new Color(0, 0, 0, transparacy);
            }

            if (_activeShoutingImages.Count != 0)
            {
                foreach (GameObject image in _activeShoutingImages)
                {
                    image.GetComponent<Image>().color = new Color(0, 0, 0, transparacy);
                }
            }
            yield return null;
        }

        foreach (GameObject imageObject in _spawnedTextBalloons)
        {
            if (imageObject == null) continue;
            Image image = imageObject.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        foreach (GameObject imageObject in _spawnedTails)
        {
            if (imageObject == null) continue;
            Image image = imageObject.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        foreach (GameObject imageObject in _TextBalloonBridges)
        {
            if (imageObject == null) continue;
            Image image = imageObject.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        }

        foreach (GameObject textObject in _spawnedText)
        {
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.color = new Color(0, 0, 0, 0);
        }

        if (_activeShoutingImages.Count != 0)
        {
            foreach (GameObject image in _activeShoutingImages)
            {
                image.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
        }

        foreach (GameObject textObject in _spawnedText)
        {
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = "";
            text.color = Color.black;
            text.transform.parent = _canvas.transform;
        }

        foreach (GameObject image in _spawnedTextBalloons)
        {
            Destroy(image);
        }

        foreach (GameObject imageObject in _spawnedTails)
        {
            Destroy(imageObject);
        }

        foreach (GameObject textObject in _spawnedText)
        {
            Destroy(textObject);
        }

        _spawnedTextBalloons = new List<GameObject>();
        _spawnedTails = new List<GameObject>();
        _spawnedText = new List<GameObject>();
        _allowMovement = false;

        if (GoToNext)
        {
            if (_currentLineIndex < _dialogues[_currentDialogueIndex].GetAllNodes().ToList().Count - 1)
            {
                _currentLineIndex++;
                _currentDialogueNode = _currentDialogue.GetAllNodes().ToList()[_currentLineIndex];
                if (startNextLine) StartCoroutine(EnableTextBalloon());
                else _isInDialogue.variable.value = false;
            }
            else
            {
                _isInDialogue.variable.value = false;
                if (_dialogues[_currentDialogueIndex].GetIsStaticDialogue()) _isInStaticDialogue.variable.value = false;
                _currentLineIndex = 0;
            }
        }
        else
        {
            _isInDialogue.variable.value = false;
            if (_dialogues[_currentDialogueIndex].GetIsStaticDialogue()) _isInStaticDialogue.variable.value = false;
            _currentLineIndex = 0;
        }
    }

    private void OnDestroy()
    {
        foreach (Dialogue dialogue in _dialogues)
        {
            dialogue.SetIsStarted(false);
        }
    }

    private void OnDisable()
    {
        foreach (Dialogue dialogue in _dialogues)
        {
            dialogue.SetIsStarted(false);
        }
    }
}