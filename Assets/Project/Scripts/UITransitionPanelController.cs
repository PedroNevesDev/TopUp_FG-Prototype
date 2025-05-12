using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using System.Collections.Generic;
using System;

public class UITransitionPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup panelGroup;        // Message panel group
    public CanvasGroup hudGroup;          // HUD to fade out
    public CanvasGroup blackFadeGroup;    // Black screen overlay group (just the image)
    public TextMeshProUGUI messageText;

    [Header("Timing")]
    public float fadeInDuration = 1.5f;
    public float displayDuration = 4f;    // Time each message will be displayed
    public float fadeOutDuration = 1f;
    public float hudFadeDuration = 0.5f;
    public float blackFadeDuration = 1f;
    public float restartDelay = 5f;

    [Header("Post Processing")]
    public Volume postProcessingVolume;
    private DepthOfField blurEffect;

    private List<string> messagesToShow;

    void OnDisable()
    {
        if (panelGroup != null)
            panelGroup.gameObject.SetActive(false);
        if (blackFadeGroup != null)
            blackFadeGroup.gameObject.SetActive(false);
    }

    IEnumerator Start()
    {
        // Initial UI states
        panelGroup.alpha = 0f;
        hudGroup.alpha = 1f;
        blackFadeGroup.alpha = 1f;

        // Only enable black overlay at start, text panel will be activated later
        panelGroup.gameObject.SetActive(false);
        blackFadeGroup.gameObject.SetActive(true);

        if (postProcessingVolume.profile.TryGet(out blurEffect))
        {
            blurEffect.active = false;
        }

        yield return new WaitForEndOfFrame();

        StartCoroutine(FadeFromBlack());
    }

    public void ShowTransition(List<string> messages, Action onEnd = null)
    {
        messagesToShow = messages;
        StartCoroutine(AnimateTransitionSequence(onEnd));
    }

    public IEnumerator FadeFromBlack()
    {
        float t = 0f;

        float initialBlackAlpha = blackFadeGroup.alpha;
        float initialHudAlpha = hudGroup.alpha;
        float initialBlurValue = (blurEffect != null && blurEffect.active) ? blurEffect.gaussianEnd.value : 0f;

        while (t < blackFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedTime = t / blackFadeDuration;

            blackFadeGroup.alpha = Mathf.Lerp(initialBlackAlpha, 0f, normalizedTime);
            hudGroup.alpha = Mathf.Lerp(initialHudAlpha, 1f, normalizedTime);

            if (blurEffect != null && blurEffect.active)
            {
                blurEffect.gaussianEnd.value = Mathf.Lerp(initialBlurValue, 0f, normalizedTime);
            }

            yield return null;
        }

        blackFadeGroup.alpha = 0f;
        hudGroup.alpha = 1f;

        if (blurEffect != null)
        {
            blurEffect.gaussianEnd.value = 0f;
            blurEffect.active = false;
        }

        // Disable black overlay after fade out
        blackFadeGroup.gameObject.SetActive(false);
    }

    IEnumerator AnimateTransitionSequence(Action onEnd)
    {
        // 1. Fade out HUD
        float t = 0f;
        while (t < hudFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            hudGroup.alpha = Mathf.Lerp(1f, 0f, t / hudFadeDuration);
            yield return null;
        }
        hudGroup.alpha = 0f;

        // 2. Enable blur
        if (blurEffect != null)
        {
            blurEffect.active = true;
            blurEffect.gaussianStart.value = 0f;
            blurEffect.gaussianEnd.value = 0f;
        }

        // 3. Enable and fade in black screen
        blackFadeGroup.gameObject.SetActive(true);
        t = 0f;
        while (t < blackFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float norm = t / blackFadeDuration;
            blackFadeGroup.alpha = Mathf.Lerp(0f, 1f, norm);
            if (blurEffect != null)
                blurEffect.gaussianEnd.value = Mathf.Lerp(0f, 10f, norm);
            yield return null;
        }
        blackFadeGroup.alpha = 1f;

        // 4. Enable and fade in message panel
        panelGroup.gameObject.SetActive(true);
        panelGroup.alpha = 1f;

        // 5. Show messages
        foreach (string message in messagesToShow)
        {
            messageText.text = message;
            messageText.alpha = 0f;

            // Fade in text
            t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.unscaledDeltaTime;
                messageText.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }
            messageText.alpha = 1f;

            yield return new WaitForSecondsRealtime(displayDuration);

            // Fade out text
            t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                messageText.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
                yield return null;
            }
            messageText.alpha = 0f;
        }

        // 6. Hide message panel
        panelGroup.alpha = 0f;
        panelGroup.gameObject.SetActive(false);

        // Final state: black screen remains
        onEnd?.Invoke();
    }
}
