using UnityEngine;

[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject spawningUI = null;
    [SerializeField] private EndView endView = null;
    [SerializeField] private GamePhaseObject currentGamePhase = null;

    private void Awake()
    {
        currentGamePhase.GamePhaseChanged += OnGamePhaseChanged;
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
        endView.gameObject.SetActive(false);

        if (newGamePhase == GamePhase.MoveA || newGamePhase == GamePhase.MoveB)
            mainUI.SetActive(true);
        else if (newGamePhase == GamePhase.SpawnA || newGamePhase == GamePhase.SpawnB)
            spawningUI.SetActive(true);
        else
            endView.Enter();
    }
}
