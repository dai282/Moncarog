using UnityEngine;
using TMPro; 

public class StatsUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject statsScreenPanel;

    [Header("Lifetime Stats UI")]
    [SerializeField] private TextMeshProUGUI lifetimeStepsText;
    [SerializeField] private TextMeshProUGUI lifetimeDamageDealtText;
    [SerializeField] private TextMeshProUGUI lifetimeMoncargsDefeatedText;
    [SerializeField] private TextMeshProUGUI lifetimeHpRecoveredText;
    [SerializeField] private TextMeshProUGUI lifetimeHpLostText;
    [SerializeField] private TextMeshProUGUI lifetimeManaRecoveredText;
    [SerializeField] private TextMeshProUGUI lifetimeManaSpentText;
    [SerializeField] private TextMeshProUGUI lifetimePotionsUsedText;
    [SerializeField] private TextMeshProUGUI lifetimeAbilitiesUsedText;
    [SerializeField] private TextMeshProUGUI lifetimeMoncargsCollectedText;

    [Header("Session Stats UI")]
    [SerializeField] private TextMeshProUGUI sessionStepsText;
    [SerializeField] private TextMeshProUGUI sessionDamageDealtText;
    [SerializeField] private TextMeshProUGUI sessionMoncargsDefeatedText;
    [SerializeField] private TextMeshProUGUI sessionHpRecoveredText;
    [SerializeField] private TextMeshProUGUI sessionHpLostText;
    [SerializeField] private TextMeshProUGUI sessionManaRecoveredText;
    [SerializeField] private TextMeshProUGUI sessionManaSpentText;
    [SerializeField] private TextMeshProUGUI sessionPotionsUsedText;
    [SerializeField] private TextMeshProUGUI sessionAbilitiesUsedText;
    [SerializeField] private TextMeshProUGUI sessionMoncargsCollectedText;

    private void Start()
    {
        // Ensure the panel is hidden on start
        if (statsScreenPanel != null)
        {
            statsScreenPanel.SetActive(false);
        }
    }

    // This method is called by the "Stats" button on your pause menu
    public void ShowStatsScreen()
    {
        if (statsScreenPanel == null) return;
        
        statsScreenPanel.SetActive(true);
        UpdateStatsDisplay();
    }

    // This method is called by the "Return" button on this stats screen
    public void HideStatsScreen()
    {
        if (statsScreenPanel == null) return;

        statsScreenPanel.SetActive(false);
    }

    // Fetches data from StatsCollector and updates all UI text fields
    private void UpdateStatsDisplay()
    {
        if (StatsCollector.Instance == null)
        {
            Debug.LogError("StatsCollector instance not found! Cannot display stats.");
            return;
        }

        GameStats lifetimeStats = StatsCollector.Instance.GetLifetimeRecord();
        GameStats sessionStats = StatsCollector.Instance.GetCurrentSessionStats();

        // Populate Lifetime UI
        lifetimeStepsText.text = $"Steps Taken: {lifetimeStats.StepsTaken}";
        lifetimeDamageDealtText.text = $"Damage Dealt: {lifetimeStats.DamageDealt:F0}";
        lifetimeMoncargsDefeatedText.text = $"Moncargs Defeated: {lifetimeStats.moncargsDefeated}";
        lifetimeHpRecoveredText.text = $"HP Recovered: {lifetimeStats.HPRecovered:F0}";
        lifetimeHpLostText.text = $"HP Lost: {lifetimeStats.HPLost:F0}";
        lifetimeManaRecoveredText.text = $"Mana Recovered: {lifetimeStats.ManaRecovered:F0}";
        lifetimeManaSpentText.text = $"Mana Spent: {lifetimeStats.ManaSpent:F0}";
        lifetimePotionsUsedText.text = $"Potions Used: {lifetimeStats.PotionsUsed}";
        lifetimeAbilitiesUsedText.text = $"Abilities Used: {lifetimeStats.AbilitiesUsed}";
        lifetimeMoncargsCollectedText.text = $"Moncargs Collected: {lifetimeStats.moncargsCollected}";

        // Populate Session UI
        sessionStepsText.text = $"Steps Taken: {sessionStats.StepsTaken}";
        sessionDamageDealtText.text = $"Damage Dealt: {sessionStats.DamageDealt:F0}";
        sessionMoncargsDefeatedText.text = $"Moncargs Defeated: {sessionStats.moncargsDefeated}";
        sessionHpRecoveredText.text = $"HP Recovered: {sessionStats.HPRecovered:F0}";
        sessionHpLostText.text = $"HP Lost: {sessionStats.HPLost:F0}";
        sessionManaRecoveredText.text = $"Mana Recovered: {sessionStats.ManaRecovered:F0}";
        sessionManaSpentText.text = $"Mana Spent: {sessionStats.ManaSpent:F0}";
        sessionPotionsUsedText.text = $"Potions Used: {sessionStats.PotionsUsed}";
        sessionAbilitiesUsedText.text = $"Abilities Used: {sessionStats.AbilitiesUsed}";
        sessionMoncargsCollectedText.text = $"Moncargs Collected: {sessionStats.moncargsCollected}";
    }
}