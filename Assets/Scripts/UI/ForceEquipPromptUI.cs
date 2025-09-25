using UnityEngine;
using UnityEngine.UIElements;

public class ForceEquipPromptUI : MonoBehaviour
{
    [SerializeField] private UIDocument promptUI;
    private VisualElement root;
    private Label promptText;

    private void Awake()
    {
        if (promptUI != null)
        {
            root = promptUI.rootVisualElement;
            promptText = root.Q<Label>("PromptText");
            root.style.display = DisplayStyle.None;
        }
    }

    public void ShowPrompt(string message)
    {
        if (promptText != null)
        {
            promptText.text = message;
        }
        root.style.display = DisplayStyle.Flex;
    }

    public void HidePrompt()
    {
        root.style.display = DisplayStyle.None;
    }
}