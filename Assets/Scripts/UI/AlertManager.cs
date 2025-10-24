using UnityEngine;
using TMPro;
using System.Collections;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    [Header("Alert UI")]
    public TextMeshProUGUI alertText;
    public GameObject alertPanel;
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("Alert SoundFx")]
    [SerializeField] private AudioClip alertSoundFX;
    [SerializeField] private AudioClip notificationSoundFX;

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
        notificationPanel.SetActive(false);
    }

    public void ShowAlert(string message, float duration = 2f)
    {
        SoundFxManager.Instance.PlaySoundFXClip(alertSoundFX, transform, 1f);
        if (alertText != null)
        {
            alertText.text = message.ToUpper();
        }

        if (alertPanel != null)
        {
            alertPanel.SetActive(true);
            StartCoroutine(HideAlertAfterDelay(alertPanel, duration));
        }
    }

    public void ShowNotification(string message, float duration = 2f)
    {
        SoundFxManager.Instance.PlaySoundFXClip(notificationSoundFX, transform, 1f);
        if (notificationText != null)
        {
            notificationText.text = message.ToUpper();
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
            StartCoroutine(HideAlertAfterDelay(notificationPanel, duration));
        }
    }

    private IEnumerator HideAlertAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }
}