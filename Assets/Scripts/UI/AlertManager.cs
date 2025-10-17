using UnityEngine;
using TMPro;
using System.Collections;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Header("Alert UI")]
    public TextMeshProUGUI alertText;
    public GameObject alertPanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Hide alert at start
        if (alertPanel != null)
            alertPanel.SetActive(false);
    }

    public void ShowAlert(string message, float duration = 2f)
    {
        if (alertText != null)
        {
            alertText.text = message.ToUpper();
        }

        if (alertPanel != null)
        {
            alertPanel.SetActive(true);
            StartCoroutine(HideAlertAfterDelay(duration));
        }
    }

    private IEnumerator HideAlertAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (alertPanel != null)
        {
            alertPanel.SetActive(false);
        }
    }
}