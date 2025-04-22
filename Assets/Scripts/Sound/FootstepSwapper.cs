using UnityEngine;

public class FootstepSwapper : MonoBehaviour
{
private TerrainChecker _checker;
private PlayerController _playerCon;
private string _currentLayerName;
    // void Start()
    // {
    //     _checker = new TerrainChecker();
    //     _playerCon = FindObjectOfType<PlayerController>();
    // }
    //
    // public string CheckLayers()
    // {
    //     RaycastHit _hit = default;
    //     if (Physics.Raycast(transform.position, Vector3.down, 3))
    //     {
    //         Terrain terrain = _hit.transform.GetComponent<Terrain>();
    //         if (_currentLayerName != _checker.GetLayerName(transform.position, terrain))
    //         {
    //             _currentLayerName = _checker.GetLayerName(transform.position, terrain);
    //         }
    //     }
    //     return _currentLayerName;
    // }
}
