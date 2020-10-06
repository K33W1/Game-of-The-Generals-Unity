using TMPro;
using UnityEngine;

public class EndView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StringValue winnerName = null;
    [SerializeField] private TextMeshProUGUI winnerText = null;

    public void Enter()
    {
        gameObject.SetActive(true);
        winnerText.text = winnerName.Value + " won!";
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
