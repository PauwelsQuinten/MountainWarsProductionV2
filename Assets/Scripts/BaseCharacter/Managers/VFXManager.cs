using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;
using static UnityEngine.Rendering.GPUSort;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private List<VFXState> _vfxPrefabs; 
    private Dictionary<VfxType, VFXState> _vfxInstances = new Dictionary<VfxType, VFXState>(); 
    

    public void SpawnVFX(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        VfxEventArgs args = obj as VfxEventArgs;
        if (args == null) return;

        var prf = FindPrefab(args.Type);
        if (prf == null) return;

        if (args.Cancel)
        {
            CancelVfx(args.Type);
            return;
        }

        CreateVfx(args, prf);
    }

    private void CreateVfx(VfxEventArgs args, VFXState prf)
    {
        var inst = Instantiate(prf, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;

        if (_vfxInstances.ContainsKey(args.Type))
        {
            Destroy(_vfxInstances[args.Type].gameObject);
            _vfxInstances[args.Type] = inst;
        }
        else
            _vfxInstances.Add(args.Type, inst);

        //inst.GetComponentInChildren<VisualEffect>().Play;

        float duration = args.Duration == 0 ? inst.Duration : args.Duration;
        _ = ManageLifeTime(inst, duration);
    }

    private async Task ManageLifeTime(VFXState inst, float durationInSeconds)
    {
        await Task.Delay(Mathf.RoundToInt(durationInSeconds * 1000));
        if (inst.gameObject == null) return;

        Destroy(inst.gameObject);
        _vfxInstances.Remove(inst.Type);

    }

    private void CancelVfx(VfxType type)
    {
        if (!_vfxInstances.ContainsKey(type) && _vfxInstances[type].gameObject == null) return;

        Destroy(_vfxInstances[type].gameObject);
        _vfxInstances.Remove(type);
    }

    private VFXState FindPrefab(VfxType type)
    {
        foreach( var vfx in _vfxPrefabs )
        {
            if (vfx.Type == type) return vfx;
        }
        return null;
    }

}
