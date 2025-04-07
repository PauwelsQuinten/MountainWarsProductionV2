using System;
using UnityEngine;

public class GameWonTrigger : MonoBehaviour
{
    [SerializeField]
    private GameEvent _gameWon;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name != "Player") return;

        _gameWon.Raise(this, EventArgs.Empty);

    }
}
