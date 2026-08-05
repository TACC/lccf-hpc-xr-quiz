using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundStars : MonoBehaviour
{
    public RectTransform starArea;
    public Image starPrefab;
    public int starCount = 80;
    public Vector2 starSizeRange = new Vector2(2f, 7f);
    public Color starColorOne = Color.white;
    public Color starColorTwo = new Color(0.35f, 0.75f, 1f);
    public Color starColorThree = new Color(0.55f, 1f, 0.7f);
    public bool twinkle = true;
    public float minimumTwinkleSpeed = 0.5f;
    public float maximumTwinkleSpeed = 1.5f;

    private IEnumerator Start()
    {
        yield return null;

        for (int i = 0; i < starCount; i++)
        {
            CreateStar();
        }
    }

    private void CreateStar()
    {
        Image star = Instantiate(starPrefab, starArea);
        RectTransform starRect = star.rectTransform;

        float width = starArea.rect.width;
        float height = starArea.rect.height;

        starRect.anchoredPosition = new Vector2(
            Random.Range(-width * 0.5f, width * 0.5f),
            Random.Range(-height * 0.5f, height * 0.5f)
        );

        float size = Random.Range(starSizeRange.x, starSizeRange.y);
        starRect.sizeDelta = new Vector2(size, size);

        int colorChoice = Random.Range(0, 3);

        if (colorChoice == 0)
        {
            star.color = starColorOne;
        }
        else if (colorChoice == 1)
        {
            star.color = starColorTwo;
        }
        else
        {
            star.color = starColorThree;
        }

        Color starColor = star.color;
        starColor.a = Random.Range(0.25f, 1.5f);
        star.color = starColor;

        star.raycastTarget = false;
        star.gameObject.SetActive(true);

        if (twinkle)
        {
            BackgroundStarTwinkle twinkleScript =
                star.gameObject.AddComponent<BackgroundStarTwinkle>();

            twinkleScript.speed =
                Random.Range(minimumTwinkleSpeed, maximumTwinkleSpeed);

            twinkleScript.minimumAlpha = Random.Range(0.15f, 0.4f);
            twinkleScript.maximumAlpha = Random.Range(0.65f, 1f);
        }
    }
}