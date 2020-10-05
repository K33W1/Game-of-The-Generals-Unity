using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGamePhaseObject", menuName = "Game Phase Object")]
public class GamePhaseObject : ScriptableObject
{
    public event Action<GamePhase> GamePhaseChanged;

    public GamePhase Value
    {
        get => gamePhase;
        set
        {
            gamePhase = value;
            GamePhaseChanged.Invoke(value);
        }
    }

    private GamePhase gamePhase = GamePhase.SpawnA;

    private void OnEnable()
    {
        gamePhase = GamePhase.SpawnA;
    }
}
