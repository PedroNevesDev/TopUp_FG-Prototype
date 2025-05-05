using UnityEngine;
using TMPro; // Make sure you're using TextMeshPro

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 1f;         // How fast it moves up
    public float fadeDuration = 1f;       // How long it takes to fade out
    public  TextMeshProUGUI text;         // TMP text component
    private Color startColor;

    void OnEnable()
    {
        startColor = text.color;
        StartCoroutine(FadeAndRise());
    }

    void OnServerInitialized()
    {
        
    }

    private System.Collections.IEnumerator FadeAndRise()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fadeDuration;

            // Move upward
            transform.position = startPos + Vector3.up * floatSpeed * percent;

            // Fade out
            text.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0, percent));

            yield return null;
        }

        ObjectPool.Instance.ReturnObject(gameObject); 
    }
}