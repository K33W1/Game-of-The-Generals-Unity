using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar = null;

    private CanvasGroup canvasGroup = null;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        canvasGroup.DOFade(1f, 1f);
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(HideCoroutine());
    }

    private IEnumerator HideCoroutine()
    {
        yield return canvasGroup.DOFade(0f, 1f).WaitForCompletion();
        gameObject.SetActive(false);
    }

    public void SetScrollbarValue(float value)
    {
        scrollbar.size = value;
    }
}
