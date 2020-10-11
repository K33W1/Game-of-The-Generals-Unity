using UnityEngine;

[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject spawningUI = null;
    [SerializeField] private EndView endView = null;
    [SerializeField] private GameManager gameManager = null;

    private void Awake()
    {
        gameManager.GamePhaseChanged += OnGamePhaseChanged;
        gameManager.GameEnded += OnGameEnd;
    }

    private void Start()
    {
        spawningUI.SetActive(true);
        mainUI.SetActive(false);
        endView.Exit();
    }

    private void OnGamePhaseChanged(GamePhase newGamePhase)
    {
        mainUI.gameObject.SetActive(false);
        spawningUI.gameObject.SetActive(false);
        endView.Exit();

        if (newGamePhase == GamePhase.Move)
            mainUI.SetActive(true);
        else if (newGamePhase == GamePhase.Spawn)
            spawningUI.SetActive(true);
    }

    private void OnGameEnd(GameOutput gameOutput)
    {
        endView.Enter(gameOutput);
    }
}
