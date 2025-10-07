using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject chestConfirmationPanel;
    public GameObject moncargCardPanel;
    public TextMeshProUGUI confirmationText;
    public TextMeshProUGUI moncargNameText;
    public TextMeshProUGUI moncargDescriptionText;
    public Image moncargImage;

    private ChestDetector currentChest;

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
        // Hide panels at start
        chestConfirmationPanel.SetActive(false);
        moncargCardPanel.SetActive(false);
    }

    public void ShowChestConfirmation(ChestDetector chest)
    {
        currentChest = chest;
        confirmationText.text = "Open this chest to get your starting Moncarg?";
        chestConfirmationPanel.SetActive(true);
    }

    public void OnConfirmOpen()
    {
        chestConfirmationPanel.SetActive(false);
        currentChest.OpenChest();
    }

    public void OnCancelOpen()
    {
        chestConfirmationPanel.SetActive(false);
        currentChest = null;
    }

    public void ShowMoncargCard(GameObject moncargPrefab)
    {
        StoredMoncarg storedMoncarg = moncargPrefab.GetComponent<StoredMoncarg>();

        if (storedMoncarg != null)
        {
            moncargNameText.text = storedMoncarg.Details.FriendlyName;
            moncargDescriptionText.text = $"Type: {storedMoncarg.Details.moncargData.type}\nHP: {storedMoncarg.Details.moncargData.maxHealth}";
            // You can set the image if you have sprites
            moncargImage.sprite = storedMoncarg.Details.Icon;

            moncargCardPanel.SetActive(true);
        }
    }

    public void CloseMoncargCard()
    {
        moncargCardPanel.SetActive(false);
        currentChest = null;
    }
}