using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeathManager : MonoBehaviour
{
    [SerializeField]
    private GameEvent _gameLost;
    public void CharacterDied(Component sender, object obj)
    {
        CharacterDeathEventArgs args = obj as CharacterDeathEventArgs;
        if (args == null) return;

        if (args.CharacterName == "Player")
        {
            _gameLost.Raise(this, EventArgs.Empty);
        }
        else Destroy(sender.gameObject);
    }
}
