using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject moncargSelectionPanel;
    public GameObject[] moncargCards = new GameObject[3]; // Drag 3 card UI objects here
    public TextMeshProUGUI[] moncargNameTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] moncargDescriptionTexts = new TextMeshProUGUI[3];
    public Image[] moncargImages = new Image[3];
    public Button[] selectButtons = new Button[3];

    private List<GameObject> availableMoncargs = new List<GameObject>();

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

        moncargSelectionPanel.SetActive(false);
    }


    public void ShowMoncargSelection()
    {
        // Get 3 random unique Moncargs
        availableMoncargs = MoncargDatabase.Instance.GetStarterMoncargs();

        // Populate the UI cards
        for (int i = 0; i < 3; i++)
        {
            if (i < availableMoncargs.Count)
            {
                StoredMoncarg storedMoncarg = availableMoncargs[i].GetComponent<StoredMoncarg>();
                if (storedMoncarg != null)
                {
                    moncargNameTexts[i].text = storedMoncarg.Details.FriendlyName;
                    moncargDescriptionTexts[i].text = $"Type: {storedMoncarg.Details.moncargData.type}\nHP: {storedMoncarg.Details.moncargData.maxHealth}\nAttack: {storedMoncarg.Details.moncargData.attack}";
                    moncargImages[i].sprite = storedMoncarg.Details.Icon;

                    // Set up button with correct index
                    int index = i; // Important: capture the index for the lambda
                    selectButtons[i].onClick.RemoveAllListeners();
                    selectButtons[i].onClick.AddListener(() => OnMoncargSelected(index));
                }
            }
        }

        //Disable movement buttons
        MovementUI.Instance.DisableAllButtons();

        moncargSelectionPanel.SetActive(true);
    }


    public void OnMoncargSelected(int index)
    {
        if (index < availableMoncargs.Count)
        {
            GameObject selectedMoncarg = availableMoncargs[index];
            AddMoncargToInventory(selectedMoncarg);
            ShowSelectedMoncargCard(selectedMoncarg);
        }

        MovementUI.Instance.EnableAllButtons();

    }

    private void AddMoncargToInventory(GameObject moncargPrefab)
    {
        GameObject moncargInstance = Instantiate(moncargPrefab);
        StoredMoncarg storedMoncarg = moncargInstance.GetComponent<StoredMoncarg>();

        if (storedMoncarg != null)
        {
            storedMoncarg.Details.moncargData.reset();
            storedMoncarg.AddToInventory();

            Debug.Log($"Added {storedMoncarg.Details.FriendlyName} to inventory as starter!");
        }

        AlertManager.Instance.ShowAlert($"Added {storedMoncarg.Details.FriendlyName} to inventory as starter!");

        Destroy(moncargInstance);

        // Mark chest as opened
        FindFirstObjectByType<ChestDetector>()?.SetOpened();
    }

    private void ShowSelectedMoncargCard(GameObject moncargPrefab)
    {
        moncargSelectionPanel.SetActive(false);

        // You could show a confirmation card here if needed
        Debug.Log($"Starter Moncarg selected: {moncargPrefab.GetComponent<StoredMoncarg>().Details.FriendlyName}");

        // Optional: Show brief confirmation message
        StartCoroutine(ShowBriefConfirmation(moncargPrefab));
    }

    private System.Collections.IEnumerator ShowBriefConfirmation(GameObject moncargPrefab)
    {
        StoredMoncarg storedMoncarg = moncargPrefab.GetComponent<StoredMoncarg>();

        // You could show a quick popup here
        Debug.Log($"Welcome {storedMoncarg.Details.FriendlyName} to your team!");

        yield return new WaitForSeconds(2f);
        // Hide any confirmation UI if you added it
    }

    public void CloseSelectionPanel()
    {
        moncargSelectionPanel.SetActive(false);
    }
}