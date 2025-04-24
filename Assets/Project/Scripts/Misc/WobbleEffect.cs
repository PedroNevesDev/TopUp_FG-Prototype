using UnityEngine;

public class WobbleEffect : MonoBehaviour
{
    float strength = 0.5f;
    float speed = 10f;
    Vector3 originalPos;

    public void Init(float s, float sp)
    {
        strength = s;
        speed = sp;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        float wobbleX = Mathf.Sin(Time.time * speed) * strength;
        float wobbleY = Mathf.Cos(Time.time * speed * 1.3f) * strength;
        transform.localPosition = originalPos + new Vector3(wobbleX, wobbleY, 0);
    }
}
