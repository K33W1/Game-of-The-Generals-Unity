using UnityEngine;

[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject spawningUI = null;
    [SerializeField] private GameObject mainUI = null;
    [SerializeField] private GameObject endUI = null;
    [SerializeField] private GamePhaseObject currentGamePhase = null;

    private void Awake()
    {
        currentGamePhase.GamePhaseChanged += OnGamePhaseChanged;
    }

    private void OnGamePhaseChanged(GamePhase newGamePhase)
    {
        spawningUI.gameObject.SetActive(false);
        mainUI.gameObject.SetActive(false);
        endUI.gameObject.SetActive(false);

        if (newGamePhase == GamePhase.SpawnA || newGamePhase == GamePhase.SpawnB)
            spawningUI.SetActive(true);
        else if (newGamePhase == GamePhase.MoveA || newGamePhase == GamePhase.MoveB)
            mainUI.SetActive(true);
        else
            endUI.SetActive(true);
    }
}
