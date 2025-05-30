using System.Collections.Generic;
using TezcaProject.Core;
using TMPro;
using UnityEditor;
using UnityEngine;


public class DialogueNode : ScriptableObject
{
    //public string uniqueID;
    [SerializeField]
    bool isPlayerSpeaking = false;
    [SerializeField]
    bool hasImageSupport;
    [SerializeField]
    List<GameObject> shoutingImages;
    [SerializeField]
    bool hasSecondImageLine;
    [SerializeField]
    string text;
    [SerializeField]
    List<string> CharacterName;
    [SerializeField]
    float textDisplaySpeed;
    [SerializeField]
    bool hasSecondaryLine = false;
    [SerializeField]
    bool isShouting = false;
    [SerializeField]
    float shoutIntensity;
    [SerializeField]
    TMP_FontAsset newFont;
    [SerializeField]
    int newTextSize;
    [SerializeField]
    GameObject balloonObject;
    [SerializeField]
    GameObject tailObject;
    [SerializeField]
    float borderSize;
    [SerializeField]
    Vector2 sizePadding;
    [SerializeField]
    bool needsToBeFlipped;
    [SerializeField]
    List<string> children = new List<string>();
    [SerializeField]
    Rect rect = new Rect(0, 0, 200, 100);
    [SerializeField]
    string onEnterAction;

    [SerializeField]
    string onExitAction;

    [SerializeField]
    Condition condition;

    public Rect GetRect()
    {
        return rect;
    }

    public bool GetHasImageSupport()
    {
        return hasImageSupport;
    }

    public List<GameObject> GetShoutingImages()
    {
        return shoutingImages;
    }

    public bool GetHasSecondImageLine()
    {
        return hasSecondImageLine;
    }

    public string GetText()
    {
        return text;
    }

    public List<string> GetCharacterName()
    {
        return CharacterName;
    }
    public float GetTextDisplaySpeed()
    {
        return textDisplaySpeed;
    }

    public bool GetHasSecondaryLine()
    {
        return hasSecondaryLine;
    }

    public bool GetIsShouting()
    {
        return isShouting;
    }

    public float GetShoutIntensity()
    {
        return shoutIntensity;
    }


    public TMP_FontAsset GetNewFont()
    {
        return newFont;
    }

    public int GetNewFontSize()
    {
        return newTextSize;
    }

    public GameObject GetBalloonObject()
    {
        return balloonObject;
    }

    public GameObject GetTailObject()
    {
        return tailObject;
    }

    public float GetBorderSize()
    {
        return borderSize;
    }

    public Vector2 GetSizePadding()
    {
        return sizePadding;
    }

    public bool GetNeedsToBeFlipped()
    {
        return needsToBeFlipped;
    }


    public List<string> GetChildren() { return children; }


    public bool IsPlayerSpeaking()
    {
        return isPlayerSpeaking;
    }


    public string GetOnEnterAction()
    {
        return onEnterAction;
    }

    public string GetOnExitAction()
    {
        return onExitAction;
    }

    public float GetHeight()
    {
        return rect.height;
    }

    public bool CheckCondition(IEnumerable<IPredicateEvaluator> evaluators)
    {
        return condition.Check(evaluators);
    }

#if UNITY_EDITOR
    public void SetPosition(Vector2 newPosition)
    {
        Undo.RecordObject(this, "Move Dialogue Node");

        rect.position = newPosition;
        EditorUtility.SetDirty(this);
    }

    public void SetHasImageSupport(bool support)
    {
        Undo.RecordObject(this, "Update Has Image Suport");

        hasImageSupport = support;
        EditorUtility.SetDirty(this);
    }

    public void SetShoutingImages(List<GameObject> images)
    {
        if (images != shoutingImages)
        {
            Undo.RecordObject(this, "Update Shouting Images");
            shoutingImages = images;
            EditorUtility.SetDirty(this);
        }
    }

    public void SethasSecondImageLine(bool newHasSecondImageLine)
    {
        if(newHasSecondImageLine != hasSecondImageLine)
        {
            Undo.RecordObject(this, "Update Shouting Images");

            hasSecondImageLine = newHasSecondImageLine;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetText(string newText)
    {
        if (newText != text)
        {
            Undo.RecordObject(this, " Update Dialogue Text");

            text = newText;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetCharacterName(List<string> newCharacterName)
    {
        if (newCharacterName != CharacterName)
        {
            Undo.RecordObject(this, " Update Dialogue Character Name ");

            CharacterName = newCharacterName;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetTextDisplaySpeed(float newTextDisplaySpeed)
    {
        if (newTextDisplaySpeed != textDisplaySpeed)
        {
            Undo.RecordObject(this, " Update Text Display Speed ");

            textDisplaySpeed = newTextDisplaySpeed;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetHasSecondaryLine(bool newHasSecondaryLine)
    {
        if (newHasSecondaryLine != hasSecondaryLine)
        {
            Undo.RecordObject(this, " Update Dialogue Has Secondary Line ");

            hasSecondaryLine = newHasSecondaryLine;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetIsShouting(bool newIsShouting)
    {
        if (newIsShouting != isShouting)
        {
            Undo.RecordObject(this, " Update Is Shouting");

            isShouting = newIsShouting;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetIsShoutIntensity(float newIsShoutIntensity)
    {
        if (newIsShoutIntensity != shoutIntensity)
        {
            Undo.RecordObject(this, " Update Shout Intensity");

            shoutIntensity = newIsShoutIntensity;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetNewFont(TMP_FontAsset Font)
    {
        if (Font != newFont)
        {
            Undo.RecordObject(this, " Update Dialogue New Font ");

            newFont = Font;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetNewSize(int size)
    {
        if (size != newTextSize)
        {
            Undo.RecordObject(this, " Update Dialogue New text Size ");

            newTextSize = size;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetHeight(float newHeight)
    {
        if (newHeight != rect.height)
        {
            Undo.RecordObject(this, " Update Dialogue Height");

            rect.height = newHeight;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetBalloonObject(GameObject newBalloonObject)
    {
        if (newBalloonObject != balloonObject)
        {
            Undo.RecordObject(this, " Update Balloon Object");

            balloonObject = newBalloonObject;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetTailObject(GameObject newTailObject)
    {
        if (newTailObject != tailObject)
        {
            Undo.RecordObject(this, " Update Balloon Object");

            tailObject = newTailObject;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetBorderSize(float newBorserSize)
    {
        if (newBorserSize != borderSize)
        {
            Undo.RecordObject(this, " Update Border Size");

            borderSize = newBorserSize;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetSizePadding(Vector2 newSizePadding)
    {
        if (newSizePadding != sizePadding)
        {
            Undo.RecordObject(this, " Update Size Padding");

            sizePadding = newSizePadding;
            EditorUtility.SetDirty(this);
        }
    }

    public void SetNeedsToBeFlipped(bool newNeedsToBeFlipped)
    {
        if (newNeedsToBeFlipped != needsToBeFlipped)
        {
            Undo.RecordObject(this, " Update Needs To Be Flipped");

            needsToBeFlipped = newNeedsToBeFlipped;
            EditorUtility.SetDirty(this);
        }
    }

    public void AddChild(string childID)
    {
        Undo.RecordObject(this, "Add Dialogue Link");

        children.Add(childID);
        EditorUtility.SetDirty(this);
    }

    public void RemoveChild(string childID)
    {
        Undo.RecordObject(this, "Remove Dialogue Link");

        children.Remove(childID);
        EditorUtility.SetDirty(this);
    }

    public void SetPlayerSpeaking(bool newIsPlayerSpeaking)
    {
        Undo.RecordObject(this, "Change Dialogue Speaker");
        isPlayerSpeaking = newIsPlayerSpeaking;
        EditorUtility.SetDirty(this);
    }




#endif
}