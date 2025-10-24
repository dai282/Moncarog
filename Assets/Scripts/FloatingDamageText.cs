using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float floatSpeed = 50f;
    public float fadeDuration = 0.8f;

    private CanvasGroup canvasGroup;

    public void Initialize(float damage)
    {
        if (text == null) text = GetComponentInChildren<TextMeshProUGUI>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        text.text = Mathf.RoundToInt(damage).ToString();
        StartCoroutine(FadeAndMove());
    }

    private System.Collections.IEnumerator FadeAndMove()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 50f; // move up 50px

        while (elapsed < fadeDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / fadeDuration);
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
