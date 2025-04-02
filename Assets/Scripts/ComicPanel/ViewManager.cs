using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ViewManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private List<GameObject> _panels = new List<GameObject>();

    [Header("Camera")]
    [SerializeField]
    private float _camZoomSpeed;
    [SerializeField]
    private float _camMoveSpeed;
    [SerializeField] 
    private float _offsetZ = -2;

    private Camera _cam;
    private GameObject _player;

    private void Start()
    {
        _cam = Camera.main;
        _cam.transform.position = _panels[0].transform.position + (_cam.transform.forward * _offsetZ);
    }

    public void ChangePanel(Component sender, object obj)
    {
        TriggerUpdatedEventArgs args = obj as TriggerUpdatedEventArgs;
        if (args == null) return;

        if (args.ExitedTrigger) return;
        if(!args.IsHidingSpot)
            StartCoroutine(DoSwitchPanel(_panels[args.NewPanelIndex].transform.position + (_cam.transform.forward * _offsetZ)));
    }

    private IEnumerator DoSwitchPanel(Vector3 newCamPos)
    {
        float camSize = _cam.orthographicSize;
        float time = 0;
        Vector3 startpos = _cam.transform.position;
        while (_cam.orthographicSize < camSize + 0.76f)
        {
            _cam.orthographicSize += _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize + 0.76f;

        while (Vector3.Distance(_cam.transform.position, newCamPos) > 0.2f)
        {
            time += Time.deltaTime;
            _cam.transform.position = Vector3.Lerp(startpos, newCamPos, _camMoveSpeed * time);
            yield return null;
        }
        _cam.transform.position = newCamPos;

        while (_cam.orthographicSize > camSize + 0.76f)
        {
            _cam.orthographicSize -= _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize;
    }
}
