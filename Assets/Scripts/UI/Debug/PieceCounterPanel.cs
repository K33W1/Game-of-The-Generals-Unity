using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class PieceCounterPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textPrefab = null;

    private readonly List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();

    private void Start()
    {
        for (PieceRank i = PieceRank.Spy; i <= PieceRank.Flag; i++)
        {
            TextMeshProUGUI text = Instantiate(textPrefab, transform);
            texts.Add(text);
            text.text = i.ToString() + " 21";
        }
    }

    public void UpdateText(List<int> piecePool)
    {
        for (PieceRank i = PieceRank.Spy; i <= PieceRank.Flag; i++)
        {
            TextMeshProUGUI text = texts[(int) i];
            text.text = i.ToString() + ' ' + piecePool[(int) i];
        }
    }
}
