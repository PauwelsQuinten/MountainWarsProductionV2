using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ViewManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private List<GameObject> _panels = new List<GameObject>();
    [SerializeField]
    private List<GameObject> _biomePanels = new List<GameObject>();
    [SerializeField]
    private GameEvent _panelSwitchSound;

    [Header("Camera")]
    [SerializeField]
    private float _camZoomSpeed;
    [SerializeField]
    private float _camMoveSpeed;
    [SerializeField] 
    private float _offsetZ = -2;

    [Header("Showdown")]
    [SerializeField]
    private GameObject _vsImage;
    [SerializeField]
    private GameEvent _ShowdownSound;

    private Camera _cam;
    private GameObject _player;

    private TriggerEnterEventArgs _currentArgs;
    private TriggerEnterEventArgs _previousArgs;

    private bool _isNearHidingSpot;

    private bool _isSwitchingPanel;

    private void Start()
    {
        _cam = Camera.main;
        _cam.transform.position = _panels[0].transform.position + (_cam.transform.forward * _offsetZ);
    }

    public void EnterNewPanel(Component sender, object obj)
    {
        TriggerEnterEventArgs args = obj as TriggerEnterEventArgs;
        if (args == null) return;

        if(args.IsHidingSpot) _isNearHidingSpot = true;

        _currentArgs = args;

        if (!args.IsHidingSpot && !args.IsShowDown)
        {
            if (!_isSwitchingPanel)
            {
                _panelSwitchSound.Raise(this, EventArgs.Empty);
                StartCoroutine(DoSwitchPanel(_biomePanels[args.newSceneIndex].transform.position + (_cam.transform.forward * _offsetZ), _panels[args.NewViewIndex].transform.position + (_cam.transform.forward * _offsetZ)));
            }
        }
        else
        {
            if (args.IsHidingSpot)
                _isNearHidingSpot = true;
            else if (args.IsShowDown)
            {
                _ShowdownSound.Raise(this, EventArgs.Empty);
                StartCoroutine(DoShowDown());
            }
        }
        _previousArgs = _currentArgs;
    }

    public void ExitTrigger(Component sender, object obj)
    {
        if(_isNearHidingSpot)_isNearHidingSpot = false;

        TriggerEnterEventArgs args = obj as TriggerEnterEventArgs;
        if (args == null) return;
    }

    public void EnterHiding(Component sender, object obj)
    {
        if (!_isNearHidingSpot) return;
        if (_panels[_currentArgs.NewViewIndex].active) StartCoroutine(ShowHidingSpot(true));
        else
        {
            StartCoroutine(ShowHidingSpot(false));
        }
    }

    private IEnumerator DoSwitchPanel(Vector3 newCamPosBiome, Vector3 newCamPosPanel)
    {
        _isSwitchingPanel = true;
        float camSize = _cam.orthographicSize;
        float time = 0;
        Vector3 startpos = _cam.transform.position;
        while (_cam.orthographicSize < camSize + 0.76f)
        {
            _cam.orthographicSize += _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize + 0.76f;

        while (Vector3.Distance(_cam.transform.position, newCamPosBiome) > 0.2f)
        {
            time += Time.deltaTime;
            _cam.transform.position = Vector3.Lerp(startpos, newCamPosBiome, _camMoveSpeed * time);
            yield return null;
        }
        _cam.transform.position = newCamPosBiome;

        while (_cam.orthographicSize > camSize + 0.76f)
        {
            _cam.orthographicSize -= _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize;

        yield return new WaitForSeconds(1.5f);

        camSize = _cam.orthographicSize;
        time = 0;
        startpos = _cam.transform.position;
        while (_cam.orthographicSize < camSize + 0.76f)
        {
            _cam.orthographicSize += _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize + 0.76f;

        while (Vector3.Distance(_cam.transform.position, newCamPosPanel) > 0.2f)
        {
            time += Time.deltaTime;
            _cam.transform.position = Vector3.Lerp(startpos, newCamPosPanel, _camMoveSpeed * time);
            yield return null;
        }
        _cam.transform.position = newCamPosPanel;

        while (_cam.orthographicSize > camSize + 0.76f)
        {
            _cam.orthographicSize -= _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize;
        _isSwitchingPanel = false;
    }

    private IEnumerator ShowHidingSpot(bool isAvtive)
    {
        Vector3 startPos = Vector3.zero;
        Vector3 originPos = Vector3.zero;
        if (!isAvtive)
        {
            _panels[_currentArgs.NewViewIndex].SetActive(true);
            originPos = _panels[_currentArgs.NewViewIndex].transform.position;

            _panels[_currentArgs.NewViewIndex].transform.position += Vector3.left * 10;

            startPos = _panels[_currentArgs.NewViewIndex].transform.position;
            float time = 0;
            while (Vector3.Distance(_panels[_currentArgs.NewViewIndex].transform.position, originPos) > 0.2f)
            {
                time += Time.deltaTime;
                _panels[_currentArgs.NewViewIndex].transform.position = Vector3.Lerp(startPos, originPos, _camMoveSpeed * time);
                yield return null;
            }

            _panels[_currentArgs.NewViewIndex].transform.position = originPos;
        }
        else
        {
            startPos = _panels[_currentArgs.NewViewIndex].transform.position;


            originPos = _panels[_currentArgs.NewViewIndex].transform.position + Vector3.left * 10;


            float time = 0;
            while (Vector3.Distance(_panels[_currentArgs.NewViewIndex].transform.position, startPos) < 9.8f)
            {
                time += Time.deltaTime;
                _panels[_currentArgs.NewViewIndex].transform.position = Vector3.Lerp(startPos, originPos, _camMoveSpeed * time);
                yield return null;
            }
            _panels[_currentArgs.NewViewIndex].transform.position = startPos;
            _panels[_currentArgs.NewViewIndex].SetActive(false);
        }
    }

    private IEnumerator DoShowDown()
    {
        float time = 0;
        int index = 0;
        if(_previousArgs != null) index = _previousArgs.NewViewIndex;
        else index = _currentArgs.NewViewIndex;
        index++;
        int newIndex = index;

        GameObject playerPanel = _panels[newIndex].gameObject;
        GameObject enemyPanel = _panels[++newIndex].gameObject;
        GameObject enemy = GameObject.Find("Enemy");
        GameObject player = GameObject.Find("Player");
        CharacterMovement enemyMove = enemy.GetComponent<CharacterMovement>();
        NavMeshAgent enemyNavMove = enemy.GetComponent<NavMeshAgent>();
        CharacterMovement playerMove = player.GetComponent<CharacterMovement>();

        Vector3 playerToPos = playerPanel.transform.position;
        Vector3 enemyToPos = enemyPanel.transform.position;
        Vector3 vsToPos = _vsImage.transform.position;

        Vector3 playerStart = playerPanel.transform.position + (-Vector3.right * 10);
        Vector3 enemyStart = enemyPanel.transform.position +(Vector3.right * 10);
        Vector3 vsStart = _vsImage.transform.position + (Vector3.forward * 10);

        playerPanel.transform.position = playerStart;
        enemyPanel.transform.position = enemyStart;
        _vsImage.transform.position = vsStart;

        playerPanel.SetActive(true);
        enemyPanel.SetActive(true);
        _vsImage.SetActive(true);
        playerMove.enabled = false;
        enemyMove.enabled = false;
        enemyNavMove.isStopped = true;
        enemyNavMove.enabled = false;

        while(Vector3.Distance(playerPanel.transform.position, playerToPos) > 0.2f)
        {
            time += Time.deltaTime;
            playerPanel.transform.position = Vector3.Lerp(playerStart, playerToPos, _camMoveSpeed * time);
            yield return null;
        }
        playerPanel.transform.position = playerToPos;
        time = 0;

        while (Vector3.Distance(enemyPanel.transform.position, enemyToPos) > 0.2f)
        {
            time += Time.deltaTime;
            enemyPanel.transform.position = Vector3.Lerp(enemyStart, enemyToPos, _camMoveSpeed * time);
            yield return null;
        }
        enemyPanel.transform.position = enemyToPos;
        time = 0;

        while (Vector3.Distance(_vsImage.transform.position, vsToPos) > 0.2f)
        {
            time += Time.deltaTime;
            _vsImage.transform.position = Vector3.Lerp(vsStart, vsToPos, _camMoveSpeed * time);
            yield return null;
        }
        _vsImage.transform.position = vsToPos;
        time = 0;

        yield return new WaitForSeconds(2);

        while (Vector3.Distance(playerPanel.transform.position, playerStart) > 0.2f)
        {
            time += Time.deltaTime;
            playerPanel.transform.position = Vector3.Lerp(playerToPos, playerStart, _camMoveSpeed * time);
            yield return null;
        }
        playerPanel.transform.position = playerStart;
        time = 0;

        while (Vector3.Distance(enemyPanel.transform.position, enemyStart) > 0.2f)
        {
            time += Time.deltaTime;
            enemyPanel.transform.position = Vector3.Lerp(enemyToPos, enemyStart, _camMoveSpeed * time);
            yield return null;
        }
        enemyPanel.transform.position = enemyStart;
        time = 0;

        while (Vector3.Distance(_vsImage.transform.position, vsStart) > 0.2f)
        {
            time += Time.deltaTime;
            _vsImage.transform.position = Vector3.Lerp(vsToPos, vsStart, _camMoveSpeed * time);
            yield return null;
        }
        _vsImage.transform.position = vsStart;
        time = 0;

        playerPanel.SetActive(false);
        enemyPanel.SetActive(false);
        _vsImage.SetActive(false);
        playerMove.enabled = true;
        enemyMove.enabled = true;
        enemyNavMove.enabled = true;
        enemyNavMove.isStopped = false;

    }
}
