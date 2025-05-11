using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class UIDeathPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup panelGroup;        // Death message group
    public CanvasGroup hudGroup;          // HUD to fade out
    public CanvasGroup blackFadeGroup;    // Black screen overlay group (just the image)
    public TextMeshProUGUI deathText;
    public string deathMessage = "The dungeon claims your soul once more.";

    [Header("Timing")]
    public float fadeInDuration = 1.5f;
    public float displayDuration = 3f;
    public float fadeOutDuration = 1f;
    public float hudFadeDuration = 0.5f;
    public float blackFadeDuration = 1f;
    public float restartDelay = 5f;

    [Header("Post Processing")]
    public Volume postProcessingVolume;
    private DepthOfField blurEffect;

    IEnumerator Start()
    {
        panelGroup.alpha = 0f;
        hudGroup.alpha = 1f;
        blackFadeGroup.alpha = 1f;
        deathText.text = deathMessage;

        if (postProcessingVolume.profile.TryGet(out blurEffect))
        {
            blurEffect.active = false;
        }

        // ✅ Wait for the first frame to be rendered
        yield return new WaitForEndOfFrame();

        // 🔄 Start the fade-from-black
        StartCoroutine(FadeFromBlack());
    }

    public void ShowDeathPanel()
    {
        StartCoroutine(AnimateDeathSequence());
    }

    IEnumerator FadeFromBlack()
    {
        float t = 0;
        while (t < blackFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            blackFadeGroup.alpha = Mathf.Lerp(1f, 0f, t / blackFadeDuration);
            yield return null;
        }
        blackFadeGroup.alpha = 0f;
    }

    IEnumerator AnimateDeathSequence()
    {
        float t = 0;

        // 1. Fade out HUD
        while (t < hudFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            hudGroup.alpha = Mathf.Lerp(1f, 0f, t / hudFadeDuration);
            yield return null;
        }

        hudGroup.alpha = 0f;

        // 2. Enable and animate blur
        if (blurEffect != null)
        {
            blurEffect.active = true;
            blurEffect.gaussianStart.value = 0f;
            blurEffect.gaussianEnd.value = 0f;
        }

        // 3. Fade in black
        t = 0;
        while (t < blackFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            blackFadeGroup.alpha = Mathf.Lerp(0f, 1f, t / blackFadeDuration);
            if (blurEffect != null)
                blurEffect.gaussianEnd.value = Mathf.Lerp(0f, 10f, t / blackFadeDuration);
            yield return null;
        }

        // 4. Show death text
        t = 0;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            panelGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        // 5. Wait with full text
        yield return new WaitForSecondsRealtime(displayDuration);

        // 6. Fade out death text
        t = 0;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            panelGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        panelGroup.alpha = 0f;

        // 7. Restart scene
        yield return new WaitForSecondsRealtime(restartDelay);
        LevelManager.Instance.Restart();
    }
}
