using TMPro;
using UnityEngine;

public class EndView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI winnerText = null;

    public void Enter(Side gameOutput)
    {
        gameObject.SetActive(true);

        if (gameOutput == Side.A)
            winnerText.text = "Player won!";
        else
            winnerText.text = "Enemy AI won!";
    }

    public void Exit()
    {
        gameObject.SetActive(false);
    }
}
