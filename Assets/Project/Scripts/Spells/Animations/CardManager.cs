using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Linq;
public class CardManager : MonoBehaviour
{
    [Header("Card References")]
    public RectTransform[] cards;

    [Header("Position References")]
    public RectTransform handFanCenter;      // Cards fan out here in hand
    public RectTransform deckStackPosition;  // Cards collapse here in deck state

    public GameObject pressSpace;

    [Header("Fan Layout Settings (Hand State)")]
    public float fanRadius = 250f;
    public float maxFanAngle = 30f;

    [Header("Animation Settings")]
    public float animationDuration = 0.4f;
    public Ease animationEase = Ease.OutQuad;

    [Header("Idle Sway Settings")]
    public float swayAngle = 2f;         // Max rotation in either direction
    public float swayTime = 1.2f;        // Average duration for a sway

    private Coroutine idleSwayCoroutine;
    private bool isHandOpen = false;

    [Header("Card Scale")]
    public float deckScale = 0.75f;
    public float handScale = 1f;

    [Header("Selection State")]
    public RectTransform selectionCenter;
    public float selectionSpacing = 110f;

    bool isSelecting = false;

    void Start()
    {
        MoveToDeckState(); // Start with cards collapsed in deck
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            ToggleCardView();
        if(Input.GetKeyDown(KeyCode.Escape))
            ToggleCardSelection();

        if(cards.Length!=transform.childCount)
        {
        cards = transform.Cast<Transform>()
                        .Select(t => t.GetComponent<RectTransform>())
                        .Where(rt => rt != null)
                        .ToArray();

            MoveToHandState();
        }        
    }

    public void ToggleCardSelection()
    {
        if(isSelecting)
        {
            ReturnFromSelectionToHand();
        }
        else
        {
            MoveToSelectionState();
        }

        isSelecting = !isSelecting;
    }

    public void ToggleCardView()
    {
        if (isHandOpen)
            MoveToDeckState();
        else
            MoveToHandState();

        isHandOpen = !isHandOpen;
    }

    public void MoveToHandState()
    {
        pressSpace.SetActive(false);
        int count = cards.Length;
        if (count == 0) return;

        float cardWidth = cards[0].rect.width;
        float idealSpacing = cardWidth * 0.9f;
        float maxTotalWidth = 600f;
        float totalWidth = Mathf.Min((count - 1) * idealSpacing, maxTotalWidth);
        float spacing = totalWidth / Mathf.Max(1, count - 1);
        float startX = -totalWidth / 2f;

        float rippleOffset = 8f; // how far up/down the ripple goes

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * spacing;

            // Alternate Y offset: up/down/up/down
            float y = ((i % 2 == 0) ? 1f : -1f) * rippleOffset;

            Vector2 targetPos = handFanCenter.anchoredPosition + new Vector2(x, y);
            float randomRot = Random.Range(-2f, 2f); // Slight random tilt still cool

            cards[i].DOAnchorPos(targetPos, animationDuration).SetEase(animationEase);
            cards[i].DORotate(Vector3.forward * randomRot, animationDuration).SetEase(animationEase);
            cards[i].DOScale(handScale, animationDuration).SetEase(animationEase);
        }

        if (idleSwayCoroutine != null) StopCoroutine(idleSwayCoroutine);
        idleSwayCoroutine = StartCoroutine(SwayIdle());
    }

    public void MoveToSelectionState()
    {
        pressSpace.SetActive(false);
        int count = cards.Length;
        if (count == 0 || selectionCenter == null) return;

        float totalWidth = (count - 1) * selectionSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float x = startX + i * selectionSpacing;
            Vector2 targetPos = selectionCenter.anchoredPosition + new Vector2(x, 0f);

            float randomRot = Random.Range(-2f, 2f); // Small wobble if you want

            cards[i].DOAnchorPos(targetPos, animationDuration).SetEase(animationEase);
            cards[i].DORotate(Vector3.forward * randomRot, animationDuration).SetEase(animationEase);
            cards[i].DOScale(handScale, animationDuration).SetEase(animationEase); // same as hand
        }

        if (idleSwayCoroutine != null) StopCoroutine(idleSwayCoroutine);
        idleSwayCoroutine = StartCoroutine(SwayIdle()); // optional
    }

    public void MoveToDeckState()
    {
        pressSpace.SetActive(true);
        foreach (RectTransform card in cards)
        {
            card.DOAnchorPos(deckStackPosition.anchoredPosition, animationDuration)
                .SetEase(animationEase);
            card.DORotate(Vector3.zero, animationDuration)
                .SetEase(animationEase);
                    card.DOScale(deckScale, animationDuration).SetEase(animationEase);
        }

        if (idleSwayCoroutine != null)
        {
            StopCoroutine(idleSwayCoroutine);
            idleSwayCoroutine = null;
        }
    }
    public void ReturnFromSelectionToHand()
    {
        MoveToHandState(); // or Toggle to hand layout
    }
    IEnumerator SwayIdle()
    {
        while (true)
        {
            foreach (RectTransform card in cards)
            {
                float sway = Random.Range(-swayAngle, swayAngle);
                float time = Random.Range(0.8f * swayTime, 1.2f * swayTime);

                Quaternion swayRot = Quaternion.Euler(0, 0, sway);

                card.DORotateQuaternion(swayRot, time).SetEase(Ease.InOutSine);
            }

            yield return new WaitForSeconds(Random.Range(1.2f, 2f));
        }
    }
}