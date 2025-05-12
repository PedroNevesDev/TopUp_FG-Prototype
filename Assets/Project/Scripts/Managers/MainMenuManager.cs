using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup menuButtonsGroup;
    public CanvasGroup howToPlayGroup;
    public CanvasGroup transitionFade;

    [Header("How To Play Pages")]
    public GameObject[] howToPlayPages;
    public Button nextPageButton;
    public Button prevPageButton;
    private int currentPage = 0;

    [Header("UI Elements")]
    public RectTransform[] menuButtons;
    public RectTransform title;

    [Header("Wobble Settings")]
    public float buttonWobbleSpeed = 1f;
    public float buttonWobbleAmount = 10f;
    public float titleWobbleSpeed = 2f;
    public float titleWobbleAmount = 20f;

    private Vector3[] originalButtonPositions;
    private Vector3 originalTitlePosition;
    [SerializeField] private CanvasGroup blackFadeCanvasGroup;
[SerializeField] private float blackFadeDuration = 1f;

    void Start()
    {
            if (blackFadeCanvasGroup != null)
        StartCoroutine(FadeOutBlackStart());
        // Cache original positions
        originalTitlePosition = title.anchoredPosition;
        originalButtonPositions = new Vector3[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
            originalButtonPositions[i] = menuButtons[i].anchoredPosition;

        // Setup initial UI state
        howToPlayGroup.gameObject.SetActive(false);
        transitionFade.gameObject.SetActive(false);
        UpdateHowToPlayPage();
    }
    IEnumerator FadeOutBlackStart()
{
    float t = 0f;
    while (t < blackFadeDuration)
    {
        t += Time.deltaTime;
        blackFadeCanvasGroup.alpha = 1f - (t / blackFadeDuration);
        yield return null;
    }
    blackFadeCanvasGroup.alpha = 0f;
    blackFadeCanvasGroup.gameObject.SetActive(false);
}


    void Update()
    {
        // Wobble title more aggressively
        title.anchoredPosition = originalTitlePosition + new Vector3(
            Mathf.Sin(Time.time * titleWobbleSpeed) * titleWobbleAmount,
            Mathf.Cos(Time.time * titleWobbleSpeed * 0.7f) * titleWobbleAmount,
            0f
        );

        // Wobble buttons uniquely
        for (int i = 0; i < menuButtons.Length; i++)
        {
            float offsetX = Mathf.Sin(Time.time * buttonWobbleSpeed + i) * buttonWobbleAmount;
            float offsetY = Mathf.Cos(Time.time * buttonWobbleSpeed * 1.2f + i) * buttonWobbleAmount * 0.5f;
            menuButtons[i].anchoredPosition = originalButtonPositions[i] + new Vector3(offsetX, offsetY, 0f);
        }
    }

    public void Play()
    {
        StartCoroutine(FadeAndStart());
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenHowToPlay()
    {
        StartCoroutine(FadeCanvasGroup(howToPlayGroup, true));               // fade in & enable HTP
        StartCoroutine(FadeCanvasGroup(menuButtonsGroup, false, true));      // fade out & disable buttons
    }

    public void CloseHowToPlay()
    {
        StartCoroutine(FadeCanvasGroup(howToPlayGroup, false, true));        // fade out & disable HTP
        StartCoroutine(FadeCanvasGroup(menuButtonsGroup, true));             // fade in & enable buttons
    }

    public void NextPage()
    {
        if (currentPage < howToPlayPages.Length - 1)
        {
            currentPage++;
            UpdateHowToPlayPage();
        }
    }

    public void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateHowToPlayPage();
        }
    }

void UpdateHowToPlayPage()
{
    for (int i = 0; i < howToPlayPages.Length; i++)
        howToPlayPages[i].SetActive(i == currentPage);

    nextPageButton.gameObject.SetActive(currentPage < howToPlayPages.Length - 1);
    prevPageButton.gameObject.SetActive(currentPage > 0);
}


    IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, bool disableOnFadeOut = false)
    {
        float duration = 0.4f;
        float timer = 0f;
        float startAlpha = cg.alpha;
        float endAlpha = fadeIn ? 1 : 0;

        if (fadeIn)
            cg.gameObject.SetActive(true);

        cg.interactable = fadeIn;
        cg.blocksRaycasts = fadeIn;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            yield return null;
        }

        cg.alpha = endAlpha;

        if (!fadeIn && disableOnFadeOut)
            cg.gameObject.SetActive(false);
    }

    IEnumerator FadeAndStart()
    {
        transitionFade.gameObject.SetActive(true);
        yield return FadeCanvasGroup(transitionFade, true);

        // wait 1 second and load next scene
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("SampleScene");
        Debug.Log("Game Started");
    }
}
