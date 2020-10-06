using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStringValue", menuName = "String Value")]
public class StringValue : ScriptableObject
{
    public event Action<string> ValueChanged;

    public string Value { get; set; }

    private void OnEnable()
    {
        Value = "";
    }
}
