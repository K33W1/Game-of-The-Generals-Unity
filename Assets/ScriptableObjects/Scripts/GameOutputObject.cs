using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGameOutputObject", menuName = "Game Output Object")]
public class GameOutputObject : ScriptableObject
{
    public GameOutput Value { get; set; }

    private void OnEnable()
    {
        Value = GameOutput.None;
    }
}
