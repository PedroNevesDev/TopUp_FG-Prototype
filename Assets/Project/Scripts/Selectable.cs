using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Selectable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Normal and hover scale values
    public Vector3 normalScale = new Vector3(1f, 1f, 1f);
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    
    // Optional: scale duration (how fast the scaling happens)
    public float scaleDuration = 0.2f;
    
    private RectTransform rectTransform;


    Action OnClick;
    public Selectable Setup(Action onClick)
    {
        OnClick = onClick;
        return this;
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = normalScale;
    }

    // When the pointer enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();  // Stop any ongoing scaling
        StartCoroutine(ScaleTo(hoverScale));
    }

    // When the pointer exits the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();  // Stop any ongoing scaling
        StartCoroutine(ScaleTo(normalScale));
    }

    // When the UI element is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick?.Invoke();
    }

    // Coroutine to scale smoothly
    private System.Collections.IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 initialScale = rectTransform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            rectTransform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.localScale = targetScale;  // Ensure the target scale is set after the lerp
    }
}
