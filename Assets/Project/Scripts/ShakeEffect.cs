using DG.Tweening;
using UnityEngine;

public class ShakeEffect : MonoBehaviour
{
    public AnimationCurve shakeCurve;
    public float duration = 0.5f;
    public float power = 1f;

    private Vector3 originalPos;
    private Tween currentShakeTween;



    public void Shake()
    {
        originalPos = transform.localPosition;
        // Cancel ongoing shake
        if (currentShakeTween != null && currentShakeTween.IsActive())
        {
            currentShakeTween.Kill();
            transform.localPosition = originalPos;
        }

        float elapsed = 0f;

        currentShakeTween = DOTween.To(() => elapsed, x => elapsed = x, duration, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                float strength = shakeCurve.Evaluate(elapsed / duration) * power;
                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f
                ) * strength;

                transform.localPosition = originalPos + offset;
            })
            .OnComplete(() =>
            {
                transform.localPosition = originalPos;
            });
    }
}
