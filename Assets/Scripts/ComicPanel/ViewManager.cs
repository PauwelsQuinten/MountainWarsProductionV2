using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.Rendering.GPUSort;

public class ViewManager : MonoBehaviour
{
    private const string TAG_VILLAGER = "Villager";

    [Header("Panels")]
    [SerializeField]
    private float _panelswitchPauseTime = 1.5f;
    [SerializeField]
    private float _panelMoveSpeed = 5f;
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
    private float _showdownPauseTime = 1.5f;
    [SerializeField]
    private float _showdownMoveSpeed = 5f;
    [SerializeField]
    private float _startOffset = 10f;
    [SerializeField]
    private GameObject _vsImage;
    [SerializeField]
    private GameEvent _ShowdownSound;
    [SerializeField]
    private GameEvent _vsSound;
    [SerializeField]
    private RenderTexture _renderTexture;

    [Header("canvas")]
    [SerializeField]
    private Canvas _canvas;

    private Camera _cam;

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
                StartCoroutine(DoSwitchPanel(_biomePanels[args.newSceneIndex].transform.position + (_cam.transform.forward * _offsetZ), _panels[args.NewViewIndex].transform.position + (_cam.transform.forward * _offsetZ), args.CurrentSceneIndex, args.newSceneIndex, args.CurrentSceneIndex != args.newSceneIndex));
            }
        }
        else
        {
            if (args.IsHidingSpot)
                _isNearHidingSpot = true;
            else if (args.IsShowDown && !args.VsTarget.CompareTag(TAG_VILLAGER))
            {
                _ShowdownSound.Raise(this, EventArgs.Empty);
                StartCoroutine(DoShowDown(args.VsTarget));
            }
        }
        _previousArgs = _currentArgs;
    }

    public void ExitTrigger(Component sender, object obj)
    {
        if(_isNearHidingSpot)_isNearHidingSpot = false;

        TriggerExitEventArgs args = obj as TriggerExitEventArgs;
        if (args == null) return;
        if (!args.DoRunTriggerExit) return;

        if (!_isSwitchingPanel)
        {
            StartCoroutine(DoSwitchPanel(_biomePanels[args.CurrentSceneIndex].transform.position + (_cam.transform.forward * _offsetZ), _panels[args.CurrentViewIndex].transform.position + (_cam.transform.forward * _offsetZ), args.newSceneIndex, args.CurrentSceneIndex, args.CurrentSceneIndex != args.newSceneIndex));
        }
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

    private IEnumerator DoSwitchPanel(Vector3 newCamPosBiome, Vector3 newCamPosPanel, int currentBiomeIndex, int nextBiomeIndex, bool doBiomeSwitch)
    {
        _canvas.gameObject.SetActive(false);
        GameObject player = GameObject.Find("Player");
        CharacterMovement playerMove = player.GetComponent<CharacterMovement>();
        playerMove.enabled = false;

        _isSwitchingPanel = true;
        float camSize = _cam.orthographicSize;
        float time = 0;
        Vector3 startpos = _cam.transform.position;
        _panelSwitchSound.Raise(this, EventArgs.Empty);

        if (doBiomeSwitch)
        {
            _biomePanels[currentBiomeIndex].gameObject.SetActive(false);
            _biomePanels[nextBiomeIndex].gameObject.SetActive(true);


            while (_cam.orthographicSize < camSize + 0.76f)
            {
                _cam.orthographicSize += _camZoomSpeed * Time.deltaTime;
                yield return null;
            }
            _cam.orthographicSize = camSize + 0.76f;

            while (Vector3.Distance(_cam.transform.position, newCamPosBiome) > 0.2f)
            {
                time += Time.deltaTime;
                _cam.transform.position = Vector3.Lerp(startpos, newCamPosBiome, _panelMoveSpeed * time);
                yield return null;
            }
            _cam.transform.position = newCamPosBiome;

            while (_cam.orthographicSize > camSize + 0.76f)
            {
                _cam.orthographicSize -= _camZoomSpeed * Time.deltaTime;
                yield return null;
            }
            _cam.orthographicSize = camSize;

            yield return new WaitForSeconds(_panelswitchPauseTime);

            camSize = _cam.orthographicSize;
            time = 0;
            startpos = _cam.transform.position;
            _panelSwitchSound.Raise(this, EventArgs.Empty);
        }

        while (_cam.orthographicSize < camSize + 0.76f)
        {
            _cam.orthographicSize += _camZoomSpeed * Time.deltaTime;
            yield return null;
        }
        _cam.orthographicSize = camSize + 0.76f;

        while (Vector3.Distance(_cam.transform.position, newCamPosPanel) > 0.2f)
        {
            time += Time.deltaTime;
            _cam.transform.position = Vector3.Lerp(startpos, newCamPosPanel, _panelMoveSpeed * time);
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
        playerMove.enabled = true;
        _canvas.gameObject.SetActive(true);
    }

    private IEnumerator ShowHidingSpot(bool isAvtive)
    {
        _canvas.gameObject.SetActive(false);
        Vector3 startPos = Vector3.zero;
        Vector3 originPos = Vector3.zero;
        if (!isAvtive)
        {
            _panels[_currentArgs.NewViewIndex].SetActive(true);
            originPos = _panels[_currentArgs.NewViewIndex].transform.position;

            _panels[_currentArgs.NewViewIndex].transform.position += Vector3.left * _startOffset;

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


            originPos = _panels[_currentArgs.NewViewIndex].transform.position + Vector3.left * _startOffset;


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
        _canvas.gameObject.SetActive(true);
    }

    private IEnumerator DoShowDown(GameObject vsTarget)
    {
        _canvas.gameObject.SetActive(false);
        List<StateManager> targets = GameObject.FindObjectsOfType<StateManager>().ToList();


        foreach (StateManager target in targets)
        {
            if (target.GetComponent<AIController>() != null)
            {
                var camComp = target.GetComponentInChildren<Camera>();
                if (camComp != null)
                    camComp.enabled = false;
            }
        }

        float time = 0;
        int index = 0;
        ////if(_previousArgs != null) index = _previousArgs.NewViewIndex;
        ///*else*/ index = _currentArgs.NewViewIndex;
        //index++;
        //int newIndex = index;

        GameObject playerPanel = _panels[1].gameObject;
        GameObject enemy = vsTarget;
        var cam = enemy.GetComponentInChildren<Camera>();
        if ( cam)
        {
            cam.enabled = true;
            cam.targetTexture = _renderTexture;
            cam.Render();
        }
        
        GameObject enemyPanel = _panels[2].gameObject;

        CharacterMovement enemyMove = enemy.GetComponent<CharacterMovement>();
        NavMeshAgent enemyNavMove = enemy.GetComponent<NavMeshAgent>();

        GameObject player = GameObject.Find("Player");
        CharacterMovement playerMove = player.GetComponent<CharacterMovement>();

        Vector3 playerToPos = playerPanel.transform.position;
        Vector3 enemyToPos = enemyPanel.transform.position;
        Vector3 vsToPos = _vsImage.transform.position;

        Vector3 playerStart = playerPanel.transform.position + (-Vector3.right * _startOffset);
        Vector3 enemyStart = enemyPanel.transform.position +(Vector3.right * _startOffset);
        Vector3 vsStart = _vsImage.transform.position + (Vector3.forward * _startOffset);

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

        _panelSwitchSound.Raise(this, EventArgs.Empty);

        while (Vector3.Distance(playerPanel.transform.position, playerToPos) > 0.2f)
        {
            time += Time.deltaTime;
            playerPanel.transform.position = Vector3.Lerp(playerStart, playerToPos, _showdownMoveSpeed * time);
            yield return null;
        }
        playerPanel.transform.position = playerToPos;
        time = 0;

        _panelSwitchSound.Raise(this, EventArgs.Empty);

        while (Vector3.Distance(enemyPanel.transform.position, enemyToPos) > 0.2f)
        {
            time += Time.deltaTime;
            enemyPanel.transform.position = Vector3.Lerp(enemyStart, enemyToPos, _showdownMoveSpeed * time);
            yield return null;
        }
        enemyPanel.transform.position = enemyToPos;
        time = 0;

        _vsSound.Raise(this, EventArgs.Empty);
        
        while (Vector3.Distance(_vsImage.transform.position, vsToPos) > 0.2f)
        {
            time += Time.deltaTime;
            _vsImage.transform.position = Vector3.Lerp(vsStart, vsToPos, _showdownMoveSpeed * time);
            yield return null;
        }
        _vsImage.transform.position = vsToPos;
        time = 0;

        yield return new WaitForSeconds(_showdownPauseTime);

        _panelSwitchSound.Raise(this, EventArgs.Empty);

        while (Vector3.Distance(playerPanel.transform.position, playerStart) > 0.2f)
        {
            time += Time.deltaTime;
            playerPanel.transform.position = Vector3.Lerp(playerToPos, playerStart, _showdownMoveSpeed * time);
            yield return null;
        }
        playerPanel.transform.position = playerStart;
        time = 0;

        _panelSwitchSound.Raise(this, EventArgs.Empty);

        while (Vector3.Distance(enemyPanel.transform.position, enemyStart) > 0.2f)
        {
            time += Time.deltaTime;
            enemyPanel.transform.position = Vector3.Lerp(enemyToPos, enemyStart, _showdownMoveSpeed * time);
            yield return null;
        }
        enemyPanel.transform.position = enemyStart ;
        time = 0;

        _vsSound.Raise(this, EventArgs.Empty);
        while (Vector3.Distance(_vsImage.transform.position, vsStart) > 0.2f)
        {
            time += Time.deltaTime;
            _vsImage.transform.position = Vector3.Lerp(vsToPos, vsStart, _showdownMoveSpeed * time);
            yield return null;
        }
        _vsImage.transform.position = vsStart;
        time = 0;

        playerPanel.transform.position = playerStart - (-Vector3.right * 10);
        enemyPanel.transform.position = enemyStart - (Vector3.right * 10);
        _vsImage.transform.position = vsStart - (Vector3.forward * 10);

        playerPanel.SetActive(false);
        enemyPanel.SetActive(false);
        _vsImage.SetActive(false);
        playerMove.enabled = true;
        enemyMove.enabled = true;
        enemyNavMove.enabled = true;
        enemyNavMove.isStopped = false;
        _canvas.gameObject.SetActive(true);
    }
}
