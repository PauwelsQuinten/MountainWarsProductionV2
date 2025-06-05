using System.Collections.Generic;
using UnityEngine;

public class ScriptActivator : MonoBehaviour
{
    [SerializeField]
    private List<MonoBehaviour> _scriptsToActivate = new List<MonoBehaviour>();
    [SerializeField]
    private LayerMask _layermask;

    public void ActivateScripts(Component sender, object obj)
    {
        ActivateEnemyEventArgs args = obj as ActivateEnemyEventArgs;
        if (args == null) return;

        foreach(string character in args.EnemyNames)
        {
            if (character == gameObject.name)
            {
                gameObject.layer = _layermask;
                foreach (MonoBehaviour script in _scriptsToActivate)
                {
                    script.enabled = true;
                }
            }
        }
    }
}
