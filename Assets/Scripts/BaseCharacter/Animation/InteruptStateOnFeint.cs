using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class InteruptStateOnFeint : StateMachineBehaviour
{
    [SerializeField] private float _normalisedExitTime = 0.3f;
    [SerializeField] private string _exitParameter;
    [SerializeField] private string _goToState;
    private int _exitTime = 0;
    private int _layer = 0;

    private CancellationTokenSource _cts;


    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _layer = layerIndex;
        float playTime = stateInfo.length / stateInfo.speedMultiplier;
        _exitTime = Mathf.RoundToInt(playTime * _normalisedExitTime * 1000);

        _cts = new CancellationTokenSource();
        _ = InteruptState(animator);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }


    private async Task InteruptState(Animator animator)
    {
        try
        {
            await Task.Delay(_exitTime);
            if (animator.GetBool(_exitParameter))
                animator.Play(_goToState, _layer);
        }
        catch(TaskCanceledException)
        {

        }
       
    }
}
