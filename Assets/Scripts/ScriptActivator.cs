using System.Collections;
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
                foreach (MonoBehaviour script in _scriptsToActivate)
                {
                    script.enabled = true;
                }
                StartCoroutine(SetLayerWithDelay());
            }
        }
    }

    private static int LayerMaskToLayer(LayerMask mask)
    {
        int value = mask.value;
        for (int i = 0; i < 32; i++)
        {
            if ((value & (1 << i)) != 0)
                return i;
        }
        return -1; // No layer found
    }

    private IEnumerator SetLayerWithDelay()
    {
        yield return new WaitForSeconds(0.75f);
        gameObject.layer = LayerMaskToLayer(_layermask);
    }
}
