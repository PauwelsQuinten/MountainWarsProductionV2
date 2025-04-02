using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private TriggerUpdatedEventArgs _currentArgs;

    private bool _isNearhidingSpot;

    private void Start()
    {
        _cam = Camera.main;
        _cam.transform.position = _panels[0].transform.position + (_cam.transform.forward * _offsetZ);
    }

    public void ChangePanel(Component sender, object obj)
    {
        TriggerUpdatedEventArgs args = obj as TriggerUpdatedEventArgs;
        if (args == null) return;

        if (args.ExitedTrigger)
        {
            if(args.IsHidingSpot) _isNearhidingSpot = false;
            return;
        }

        _currentArgs = args;
        if (!args.IsHidingSpot)
            StartCoroutine(DoSwitchPanel(_panels[args.NewPanelIndex].transform.position + (_cam.transform.forward * _offsetZ)));
        else
        {
            if (args.IsHidingSpot) _isNearhidingSpot = true;
        }

    }

    public void EnterHiding(Component sender, object obj)
    {
        if (!_isNearhidingSpot) return;
        if(_panels[_currentArgs.NewPanelIndex].active) StartCoroutine(ShowHidingSpot(true));
        else StartCoroutine(ShowHidingSpot(false));
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

    private IEnumerator ShowHidingSpot(bool isAvtive)
    {
        Vector3 startPos = Vector3.zero;
        Vector3 originPos = Vector3.zero;
        if (!isAvtive)
        {
            _panels[_currentArgs.NewPanelIndex].SetActive(true);
            originPos = _panels[_currentArgs.NewPanelIndex].transform.position;

            _panels[_currentArgs.NewPanelIndex].transform.position += Vector3.left * 10;

            startPos = _panels[_currentArgs.NewPanelIndex].transform.position;
            float time = 0;
            while (Vector3.Distance(_panels[_currentArgs.NewPanelIndex].transform.position, originPos) > 0.2f)
            {
                time += Time.deltaTime;
                _panels[_currentArgs.NewPanelIndex].transform.position = Vector3.Lerp(startPos, originPos, _camMoveSpeed * time);
                yield return null;
            }

            _panels[_currentArgs.NewPanelIndex].transform.position = originPos;
        }
        else
        {
            startPos = _panels[_currentArgs.NewPanelIndex].transform.position;


            originPos = _panels[_currentArgs.NewPanelIndex].transform.position + Vector3.left * 10;


            float time = 0;
            while (Vector3.Distance(_panels[_currentArgs.NewPanelIndex].transform.position, startPos) < 9.8f)
            {
                Debug.Log(Vector3.Distance(_panels[_currentArgs.NewPanelIndex].transform.position, startPos));
                time += Time.deltaTime;
                _panels[_currentArgs.NewPanelIndex].transform.position = Vector3.Lerp(startPos, originPos, _camMoveSpeed * time);
                yield return null;
            }
            _panels[_currentArgs.NewPanelIndex].transform.position = startPos;
            _panels[_currentArgs.NewPanelIndex].SetActive(false);
        }
    }
}
