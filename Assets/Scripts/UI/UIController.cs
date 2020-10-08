using UnityEngine;

[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject spawningUI = null;
    [SerializeField] private EndView endView = null;
    [SerializeField] private Board board = null;

    private void Awake()
    {
        board.CurrentGamePhase.ValueChanged += OnGamePhaseChanged;
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
        else
            endView.Enter(board.CurrentGameOutput);
    }
}
