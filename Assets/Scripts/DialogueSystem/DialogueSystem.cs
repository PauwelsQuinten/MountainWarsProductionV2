using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
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

    private DialogueNode _shoutingNode;


    private void Start()
    {
        _isInStaticDialogue.variable.value = false;
        _isInDialogue.variable.value = false;

    }

    private void LateUpdate()
    {
        if (!_stateManager.IsInDialogue.value || !_allowMovement || _stateManager.IsInStaticDialogue.value) return;

        Vector2 newBalloonPos = Vector2.zero;

        newBalloonPos = GetTextBalloonPos(_spawnedTextBalloons, _spawnedText[0].GetComponent<TextMeshProUGUI>(), _currentDialogueNode.GetNeedsToBeFlipped(), false);

        if (_currentDialogueNode.GetIsShouting() || _shoutingNode != null)
        {
            newBalloonPos += new Vector2(150 * _shoutingNode.GetShoutIntensity(), 50);
        }

        if (_spawnedTextBalloons[0] != null) _spawnedTextBalloons[0].transform.position = new Vector3(newBalloonPos.x, newBalloonPos.y, 0);
        if (_spawnedTextBalloons[1] != null) _spawnedTextBalloons[1].transform.position = new Vector3(newBalloonPos.x, newBalloonPos.y, 0);

        if (_spawnedTextBalloons[0] != null)
            _spawnedText[0].transform.position = new Vector3(newBalloonPos.x, newBalloonPos.y, 0);
        else
            _spawnedText[0].transform.position = new Vector3(newBalloonPos.x, newBalloonPos.y + 200, 0);

        if (_spawnedTextBalloons.Count > 2)
        {
            float yOffset = _spawnedTextBalloons[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;
            float xOffset = _spawnedTextBalloons[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
            if (_spawnedTextBalloons[2] != null) _spawnedTextBalloons[2].transform.position = new Vector3(newBalloonPos.x + xOffset, newBalloonPos.y + -yOffset, 0);
            if (_spawnedTextBalloons[3] != null) _spawnedTextBalloons[3].transform.position = new Vector3(newBalloonPos.x + xOffset, newBalloonPos.y + -yOffset, 0);

            _spawnedText[1].transform.position = new Vector3(newBalloonPos.x + xOffset, newBalloonPos.y + -yOffset, 0);
        }

        if(_activeShoutingImages.Count != 0) DistributeImages(_spawnedTextBalloons,false, _activeShoutingImages);
        SetTailsPosition(_spawnedTextBalloons, true);
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
        foreach (var img in _activeShoutingImages)
            Destroy(img.gameObject);
        _activeShoutingImages.Clear();
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
                foreach (var img in _activeShoutingImages)
                    Destroy(img);
                _activeShoutingImages.Clear();

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
        Vector2 textBalloonPos = Vector2.zero;
        if (_spawnSubBalloon && addOffset)
        {
            textBalloonPos = new Vector2(_previousPos.x, _previousPos.y);
            float yOffset = 0;
            float xOffset = 0;
            if (textBalloon[0] != null) yOffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.35f;
            if (textBalloon[0] != null) xOffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.35f;
            textBalloonPos += new Vector2(xOffset, -yOffset);
            return textBalloonPos;
        }
        GameObject target = GameObject.Find(_currentDialogueNode.GetCharacterName()[0]);
        Vector3 targetPos = target.transform.position;
        targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

        textBalloonPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);
        RenderTexture texture = _stateManager.CurrentCamera.targetTexture;
        if (texture.width != 1920)
        {
            if(_currentDialogueNode.GetCharacterName()[0] != "Player") textBalloonPos.x += texture.width;
            else textBalloonPos.x += texture.width / 2;
        }

            float Xoffset = 0;
        float Yoffset = 0;
        if (textBalloon[0] != null) Xoffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.width * 0.45f;
        if (textBalloon[0] != null) Yoffset = textBalloon[0].GetComponent<Image>().rectTransform.rect.height * 0.55f;

        if (_currentDialogue.GetIsStaticDialogue())
        {
            if (_previousPos != Vector2.zero)
            {
                textBalloonPos = _previousPos;
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
            textBalloonPos = new Vector2(-Xoffset, Yoffset) + textBalloonPos;
        }
        else
        {
            textBalloonPos = new Vector2(Xoffset, Yoffset) + textBalloonPos;
        }

        return textBalloonPos;
    }

    private void SetTailsPosition(List<GameObject> textBalloon, bool moveTail)
    {
        if (!moveTail) return;
        Vector2 textballonPos = textBalloon[0].GetComponent<Image>().rectTransform.position;
        Vector2 tailPos = new Vector2(textballonPos.x, textballonPos.y);
        Vector2 returnPos = Vector2.zero;
        float textBalloonWidth = textBalloon[0].GetComponent<Image>().rectTransform.rect.width;
        float textBalloonHeight = textBalloon[0].GetComponent<Image>().rectTransform.rect.height;
        float Yoffset = -textBalloonHeight * 0.25f;
        float Xoffset = -textBalloonWidth * 0.25f;

        int index = 0;
        if (_spawnedTails.Count != 0)
            index = _spawnedTails.Count - (2 * _currentDialogueNode.GetCharacterName().Count);
        int tailsOnLeft = 0;
        int tailsOnRight = 0;

        foreach(string character in _currentDialogueNode.GetCharacterName())
        {
            GameObject target = GameObject.Find(character);
            Vector3 targetPos = target.transform.position;
            targetPos.y += target.GetComponent<CapsuleCollider>().height * 0.5f;

            Vector2 characterPos = _stateManager.CurrentCamera.WorldToScreenPoint(targetPos);

            float xRotation = 0;
            if (_currentDialogue.GetIsStaticDialogue())
            {
                if (_previousPos != Vector2.zero)
                {
                    Yoffset = -Yoffset;
                    xRotation = 180;
                }
            }

            int side = GetTailSide(textballonPos - new Vector2(0,10), textballonPos + new Vector2(0, 10), characterPos);

            if (side == 1)
            {
                tailPos = new Vector2(-Xoffset - (70 * tailsOnRight), Yoffset - (7 * tailsOnRight)) + textballonPos;
                if (returnPos == Vector2.zero) returnPos = tailPos;
                tailsOnRight++;
                _spawnedTails[index].transform.rotation = Quaternion.Euler(xRotation, 180, 0);
                _spawnedTails[index + 1].transform.rotation = Quaternion.Euler(xRotation, 180, 0);
            }
            else
            {
                tailPos = new Vector2(Xoffset + (70 * tailsOnLeft), Yoffset - (7 * tailsOnLeft)) + textballonPos;
                if (returnPos == Vector2.zero) returnPos = tailPos;
                tailsOnLeft++;
                _spawnedTails[index].transform.rotation = Quaternion.Euler(xRotation, 0, 0);
                _spawnedTails[index + 1].transform.rotation = Quaternion.Euler(xRotation, 0, 0);
            }

            if (_currentDialogueNode.GetIsShouting() || _shoutingNode != null && _spawnSubBalloon)
            {
                tailPos -= new Vector2(15 * _shoutingNode.GetShoutIntensity(), -15);
            }

            _spawnedTails[index].transform.position = new Vector3(tailPos.x, tailPos.y, 0);
            _spawnedTails[index + 1].transform.position = new Vector3(tailPos.x, tailPos.y, 0);
            index += 2;
        }
    }

    public static int GetTailSide(Vector2 linePointA, Vector2 linePointB, Vector2 pointC)
    {
        Vector2 AB = linePointB - linePointA;
        Vector2 AC = pointC - linePointA;
        float cross = AB.x * AC.y - AB.y * AC.x;

        if (cross > 0) return -1;    
        if (cross < 0) return 1;   
        return 0;                   
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

        Vector2 balloonPos = GetTextBalloonPos(tempList, text, _currentDialogueNode.GetNeedsToBeFlipped(), true);

        if (_currentDialogueNode.GetIsShouting())
        {
            _shoutingNode = _currentDialogueNode;
            balloonPos += new Vector2(150 * _shoutingNode.GetShoutIntensity(), 50);
        }

        if(tempList[0] != null) tempList[0].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);
        if (tempList[1] != null) tempList[1].transform.position = new Vector3(balloonPos.x, balloonPos.y, 0);


        if (_spawnedTails.Count != 0)
        {
            SetTailsPosition(tempList, _TextBalloonBridges.Count == 0);
        }

        if (_currentDialogueNode.GetShoutingImages().Count == 0) tempText = InsertLineBreaksByWord(_currentDialogueNode.GetText(), 5, tempList, text);
        else DistributeImages(tempList, true);


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

        if(_spawnedDialogue.Count != 0 && !_spawnSubBalloon && _currentDialogue.GetIsStaticDialogue())
        {
            foreach(DialogueNode node in _spawnedDialogue.Keys)
            {
                if (node.GetCharacterName()[0] == _currentDialogueNode.GetCharacterName()[0])
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

         GameObject textBalloon = null;
        if(_currentDialogueNode.GetBalloonObject() != null)
        {
            textBalloon = Instantiate(_currentDialogueNode.GetBalloonObject());
            textBalloon.transform.parent = _whiteHolder.transform;
        }

        list.Add(textBalloonBorder);
        _spawnedTextBalloons.Add(textBalloonBorder);

        list.Add(textBalloon);
        _spawnedTextBalloons.Add(textBalloon);

        GameObject tailBorder = null;
        GameObject tail = null;

        for (int i = 0; i < _currentDialogueNode.GetCharacterName().Count; i++)
        {
            if (spawnTail && _currentDialogueNode.GetTailObject() != null)
            {
                GameObject tempBorder = Instantiate(_currentDialogueNode.GetTailObject());
                tempBorder.GetComponent<Image>().color = Color.black;
                tempBorder.transform.parent = _blackHolder.transform;
                if(tailBorder == null) tailBorder = tempBorder;
                tailBorder.name = $"{tempBorder.name} {i}";
                _spawnedTails.Add(tempBorder);

                GameObject temptail = Instantiate(_currentDialogueNode.GetTailObject());
                temptail.transform.parent = _whiteHolder.transform;
                if (tail == null) tail = temptail;
                temptail.name = $"{temptail.name} {i}";
                _spawnedTails.Add(temptail);

                list.Add(temptail);
                list.Add(tempBorder);
            }
        }
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

        //for(int i = 0; i < textBalloon.Count; i++)
        //{
        //    Image image = textBalloon[i].GetComponent<Image>();
        //    if (image.color == Color.black)
        //    {
        //        image.rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
        //    }
        //    else
        //    {
        //        image.rectTransform.sizeDelta = size + node.GetSizePadding();
        //    }
        //}

        foreach (GameObject go in textBalloon)
        {
            Image image = go.GetComponent<Image>();

            if (image.color == Color.black)
            {
                image.rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
            }
            else
            {
                image.rectTransform.sizeDelta = size + node.GetSizePadding();
            }
        }

        if (_currentDialogueNode.GetIsShouting())
        {
            for (int i = 0; i < textBalloon.Count - 2; i++)
            {
                textBalloon[1 + i].GetComponent<Image>().rectTransform.sizeDelta += new Vector2(2000 * _currentDialogueNode.GetShoutIntensity(), 25);
                textBalloon[1 + (i + 1)].GetComponent<Image>().rectTransform.sizeDelta += new Vector2(2000 * _currentDialogueNode.GetShoutIntensity(), 25);
            }
        }

        if (text != null)
        {
            text.rectTransform.sizeDelta = size;
        }
    }

    //private void DetermineTextBalloonSizeBasedOnImages(List<GameObject> textBalloon, DialogueNode node, TextMeshProUGUI text)
    //{
    //    float imageSizeX = 0;
    //    float imagesSizeY = 0;

    //    float imageSpacingX = 0;
    //    float imageSpacingY = 0;

    //    imageSizeX += _activeShoutingImages[0].GetComponent<Image>().rectTransform.rect.width;
    //    imagesSizeY += _activeShoutingImages[0].GetComponent<Image>().rectTransform.rect.height;


    //    if (node.GetShoutingImages().Count > 1)
    //    {
    //        imageSpacingX = Vector3.Distance(_activeShoutingImages[0].transform.position, _activeShoutingImages[1].transform.position);
    //        imageSpacingX -= imageSizeX / node.GetShoutingImages().Count;

    //        if (node.GetHasSecondImageLine())
    //        {
    //            imageSpacingY = Vector3.Distance(_activeShoutingImages[0].transform.position, _activeShoutingImages[(int)Mathf.Ceil(node.GetShoutingImages().Count * 0.5f)].transform.position);
    //            imageSpacingY -= imagesSizeY / 2;
    //        }
    //    }

    //    float imagesOnFirstLine = Mathf.Ceil(node.GetShoutingImages().Count * 0.5f);
    //    Vector2 size = new Vector2((imageSizeX + imageSpacingX) * imagesOnFirstLine, (imagesSizeY + imageSpacingY) * 2);
    //    size += node.GetSizePadding() + new Vector2(20,20);

    //    textBalloon[0].GetComponent<Image>().rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
    //    textBalloon[2].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
    //    if (textBalloon[1] != null) textBalloon[1].GetComponent<Image>().rectTransform.sizeDelta = (size * node.GetBorderSize()) + node.GetSizePadding();
    //    if (textBalloon[3] != null) textBalloon[3].GetComponent<Image>().rectTransform.sizeDelta = size + node.GetSizePadding();
    //    text.rectTransform.sizeDelta = size;
    //}

    private float DetermineFontSizeMultiplier(float size)
    {
        float baseSize = 36;
         float currentSize = size;

        float multiplier = currentSize / baseSize;

         return multiplier;
    }

    public void DistributeImages(List<GameObject> textBalloon, bool spawnImages, List<GameObject> shoutingImages = null)
    {
        List<GameObject> newShoutingImages = _currentDialogueNode.GetShoutingImages();
        RectTransform balloonRect = textBalloon[0].GetComponent<RectTransform>();
        bool hasSecondRow = _currentDialogueNode.GetHasSecondImageLine();

        int totalImages = newShoutingImages.Count;
        if (totalImages == 0) return;

        // Calculate image layout BEFORE resizing balloon
        int firstRowCount = hasSecondRow ? Mathf.CeilToInt(totalImages / 2f) : totalImages;
        int secondRowCount = hasSecondRow ? totalImages - firstRowCount : 0;

        // Fixed image size (not dependent on balloon size)
        float fixedImageSize = _currentDialogueNode.getImageSize(); // Set your desired fixed size here
        float spacing = 10f; // Fixed spacing between images

        // Calculate required balloon size based on fixed image sizes
        if (spawnImages)
        {
            ResizeBalloonToFitImages(textBalloon, newShoutingImages, hasSecondRow, fixedImageSize, spacing, firstRowCount, secondRowCount);
        }

        // NOW get the updated balloon dimensions
        float balloonWidth = balloonRect.rect.width;
        float balloonHeight = balloonRect.rect.height;

        float yOffsetFirstRow = hasSecondRow ? fixedImageSize * 0.6f : 0f;
        float yOffsetSecondRow = -fixedImageSize * 0.6f;

        // Helper function to place a row of images
        void PlaceRow(int rowIndex, int count, float yOffset)
        {
            if (count == 0) return;

            // Calculate row width and starting position
            float totalRowWidth = (count * fixedImageSize) + ((count - 1) * spacing);
            float startX = -totalRowWidth / 2f + fixedImageSize / 2f;

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = null;
                GameObject imgObj = null;

                if (spawnImages)
                {
                    prefab = newShoutingImages[rowIndex == 0 ? i : firstRowCount + i];
                    imgObj = Instantiate(prefab);
                }
                else
                {
                    imgObj = shoutingImages[rowIndex == 0 ? i : firstRowCount + i];
                }

                // Parent to balloon holder with world position false to prevent scaling
                imgObj.transform.SetParent(_balloonHolder.transform, false);
                RectTransform imgRect = imgObj.GetComponent<RectTransform>();

                // Set FIXED size (won't scale with balloon)
                imgRect.sizeDelta = new Vector2(fixedImageSize, fixedImageSize);
                imgRect.localScale = Vector3.one; // Ensure no inherited scaling

                // Calculate X position for this row
                float x = startX + i * (fixedImageSize + spacing);

                // Use anchoredPosition relative to balloon center
                Vector2 balloonCenter = balloonRect.anchoredPosition;
                imgRect.anchoredPosition = balloonCenter + new Vector2(x, yOffset);

                if (spawnImages) _activeShoutingImages.Add(imgObj);
            }
        }

        // Place the rows
        PlaceRow(0, firstRowCount, yOffsetFirstRow);
        if (hasSecondRow)
            PlaceRow(1, secondRowCount, yOffsetSecondRow);
    }

    public void ResizeBalloonToFitImages(List<GameObject> textBalloon, List<GameObject> imagePrefabs, bool hasSecondRow, float fixedImageSize, float spacing, int firstRowCount, int secondRowCount)
    {
        if (imagePrefabs == null || imagePrefabs.Count == 0 || textBalloon.Count == 0)
            return;

        DialogueNode node = _currentDialogueNode;

        // Calculate required width based on the largest row
        int maxRowCount = Mathf.Max(firstRowCount, secondRowCount);
        float maxRowWidth = (maxRowCount * fixedImageSize) + ((maxRowCount - 1) * spacing);
        float requiredWidth = maxRowWidth + node.GetSizePadding().x;

        // Calculate required height based on fixed image size
        float requiredHeight = fixedImageSize + node.GetSizePadding().y;
        if (hasSecondRow)
            requiredHeight = (fixedImageSize * 2) + (fixedImageSize * 0.2f) + node.GetSizePadding().y; // Small gap between rows

        Vector2 size = new Vector2(requiredWidth, requiredHeight);

        // Resize balloon elements
        foreach (GameObject go in textBalloon)
        {
            Image image = go.GetComponent<Image>();
            if (image.color == Color.black)
            {
                image.rectTransform.sizeDelta = (size * new Vector2(node.GetBorderSize() * 0.85f, node.GetBorderSize())) + node.GetSizePadding();
            }
            else
            {
                image.rectTransform.sizeDelta = size + node.GetSizePadding();
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

                if (_currentDialogueNode.GetEnemiesToActivate().Count != 0)
                {
                    _currentDialogueNode.GetOnCompletionEvent().Raise(this, new ActivateEnemyEventArgs { EnemyNames = _currentDialogueNode.GetEnemiesToActivate() });
                }
            }
        }
        else
        {
            _isInDialogue.variable.value = false;
            if (_dialogues[_currentDialogueIndex].GetIsStaticDialogue()) _isInStaticDialogue.variable.value = false;
            _currentLineIndex = 0;

            if (_currentDialogueNode.GetEnemiesToActivate().Count != 0)
            {
                _currentDialogueNode.GetOnCompletionEvent().Raise(this, new ActivateEnemyEventArgs { EnemyNames = _currentDialogueNode.GetEnemiesToActivate() });
            }
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