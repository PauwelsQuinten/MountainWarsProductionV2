using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterDeathManager : MonoBehaviour
{
    [SerializeField]
    private GameEvent _gameLost;

    [SerializeField]
    private List<BoolReference> _deathBools;
    [SerializeField]
    private List<string> _characterNames;

    private Dictionary<string, BoolReference> _charactreDeathBools = new Dictionary<string, BoolReference>();

    private void Start()
    {
        foreach(BoolReference value in _deathBools)
        {
            value.variable.value = false;
        }

        for (int i = 0; i < _characterNames.Count; i++)
        {
            _charactreDeathBools.Add(_characterNames[i], _deathBools[i]);
        }
    }
    public void CharacterDied(Component sender, object obj)
    {
        CharacterDeathEventArgs args = obj as CharacterDeathEventArgs;
        if (args == null) return;

        if (args.CharacterName == "Player")
        {
            _gameLost.Raise(this, EventArgs.Empty);
        }
        else
        {
            if(_charactreDeathBools.ContainsKey(args.CharacterName))
                _charactreDeathBools[args.CharacterName].variable.value = true;
            Destroy(sender.gameObject);
        }
    }
}
