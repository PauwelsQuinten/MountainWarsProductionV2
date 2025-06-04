using System.Collections.Generic;
using UnityEngine;

public class ScriptActivator : MonoBehaviour
{
    [SerializeField]
    List<MonoBehaviour> _scriptsToActivate = new List<MonoBehaviour>();

    public void ActivateScripts()
    {
        foreach (MonoBehaviour script in _scriptsToActivate)
        {
            script.enabled = true;
        }
    }
}
