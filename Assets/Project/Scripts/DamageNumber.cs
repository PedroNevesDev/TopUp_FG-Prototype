using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 1f;
    public float fadeDuration = 1f;
    public TextMeshProUGUI text;

    private Color startColor;
    private Vector3 originalPosition;

    void Awake()
    {
        startColor = text.color;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        text.color = startColor;
        originalPosition = transform.position;
        transform.rotation = Quaternion.identity;

         if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        StartCoroutine(FadeAndRise());
    }

    public void Setup(string str)
    {
        text.text = str;
        gameObject.SetActive(true);
    }

    private System.Collections.IEnumerator FadeAndRise()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / fadeDuration;

            transform.position = originalPosition + Vector3.up * floatSpeed * percent;
            text.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0, percent));

            yield return null;
        }

        ObjectPool.Instance.ReturnObject(gameObject);
    }
}
