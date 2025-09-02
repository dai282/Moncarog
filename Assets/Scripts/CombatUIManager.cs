using UnityEngine;
using UnityEngine.UIElements;

public class CombatUIManager : MonoBehaviour
{
    public UIDocument uiDocument;
    public Moncarg moncarg;

    private VisualElement optionsContainer;
    private VisualElement fightContainer;

    private int option;

    private void OnEnable()
    {
        var rootVisualElement = uiDocument.rootVisualElement;

        // Get references to the containers
        optionsContainer = rootVisualElement.Q<VisualElement>("OptionsContainer");
        fightContainer = rootVisualElement.Q<VisualElement>("FightContainer");
        
        // Register button click handlers
        rootVisualElement.Q<Button>("FightButton").clicked += ShowFightPanel;
        rootVisualElement.Q<Button>("BackButton").clicked += ShowOptionsPanel;

        rootVisualElement.Q<Button>("FleeButton").clicked += FleeOption;
        rootVisualElement.Q<Button>("SwitchButton").clicked += SwitchOption;
        rootVisualElement.Q<Button>("InventoryButton").clicked += InventoryOption;

    }
    
    //Hides options display and just shows fight moves
    private void ShowFightPanel()
    {
        optionsContainer.style.display = DisplayStyle.None;
        fightContainer.style.display = DisplayStyle.Flex;

        option = 1;
        Debug.LogWarning("Option =" + option);
    }

    //Hides fight moves and just shows combat encounter options
    private void ShowOptionsPanel()
    {
        fightContainer.style.display = DisplayStyle.None;
        optionsContainer.style.display = DisplayStyle.Flex;
    }

    private void SwitchOption()
    {
        option = 2;
        Debug.LogWarning("Option =" + option);
    }

    private void InventoryOption()
    {
        option = 3;
        Debug.LogWarning("Option =" + option);
    }

    private void FleeOption()
    {
        option = 4;
        Debug.LogWarning("Option =" + option);
    }

    void Start()
    {
       
        var root = uiDocument.rootVisualElement;

        //Renaming fightContainer moves to the respective moves from the relevant moncarg

        // Find the button by its name 
        Button move0 = root.Q<Button>("Move0");
        Button move1 = root.Q<Button>("Move1");
        Button move2 = root.Q<Button>("Move2");
        Button move3 = root.Q<Button>("Move3");


        if (move0 != null)
        {
            move0.text = moncarg.skillset[0].name;
        }
        else
        {
            Debug.LogWarning("Button 'Move0' not found in the UI Document.");
        }

        if (move1 != null)
        {
            // Change the button's text
            move1.text = moncarg.skillset[1].name;
        }
        else
        {
            Debug.LogWarning("Button 'Move1' not found in the UI Document.");
        }

        if (move2 != null)
        {
            // Change the button's text
            move2.text = moncarg.skillset[2].name;
        }
        else
        {
            Debug.LogWarning("Button 'Move2' not found in the UI Document.");
        }

        if (move3 != null)
        {
            // Change the button's text
            move3.text = moncarg.skillset[3].name;
        }
        else
        {
            Debug.LogWarning("Button 'Move3' not found in the UI Document.");
        }
    }
}
