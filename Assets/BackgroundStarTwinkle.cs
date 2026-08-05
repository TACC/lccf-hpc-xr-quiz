using UnityEngine;
using UnityEngine.UI;

public class BackgroundStarTwinkle : MonoBehaviour
{
    public float speed = 1f;
    public float minimumAlpha = 0.25f;
    public float maximumAlpha = 1f;
    public float movementSpeed = 15f;

    private Image starImage;
    private RectTransform starRect;
    private RectTransform parentRect;
    private float offset;

    private void Awake()
    {
        starImage = GetComponent<Image>();
        starRect = GetComponent<RectTransform>();
        parentRect = transform.parent.GetComponent<RectTransform>();
        offset = Random.Range(0f, 10f);
        movementSpeed = Random.Range(0.01f, 0.1f);
    }

    private void Update()
    {
        if (starImage == null || starRect == null || parentRect == null)
        {
            return;
        }

        float amount = (Mathf.Sin((Time.time + offset) * speed) + 1f) * 0.5f;

        Color color = starImage.color;
        color.a = Mathf.Lerp(minimumAlpha, maximumAlpha, amount);
        starImage.color = color;

        starRect.anchoredPosition += Vector2.left * movementSpeed * Time.deltaTime;

        float leftEdge = -parentRect.rect.width * 0.5f;
        float rightEdge = parentRect.rect.width * 0.5f;

        if (starRect.anchoredPosition.x < leftEdge)
        {
            starRect.anchoredPosition = new Vector2(
                rightEdge,
                Random.Range(
                    -parentRect.rect.height * 0.5f,
                    parentRect.rect.height * 0.5f
                )
            );
        }
    }
}