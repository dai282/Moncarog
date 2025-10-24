using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TDD
{
    private GameObject moncargObject;
    private Moncarg moncarg;
    /*
    // A Test behaves as an ordinary method.
    [Test]
    public void TDDSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator TDDWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    } */

    [SetUp]
    public void SetUp()
    {
        // Arrange: Create a fresh Moncarg GameObject before each test
        moncargObject = new GameObject("TestMoncarg");
        moncarg = moncargObject.AddComponent<Moncarg>();

        moncarg.data = new MoncargData();
    }


    [Test]
    public void WhenMoncargTakesDamage_HealthDecreasesCorrectly()
    {
        // Arrange
        GameObject attackerObject = new GameObject("Attacker");
        Moncarg attacker = attackerObject.AddComponent<Moncarg>();
        attacker.data = new MoncargData();
        attacker.attack = 20f;

        moncarg.defense = 10f;

        SkillDefinition basicAttack = ScriptableObject.CreateInstance<SkillDefinition>();
        basicAttack.damage = 30f;

        CombatHandler combatHandler = new GameObject().AddComponent<CombatHandler>();

        // Act - Test the pure calculation method
        float damage = combatHandler.CalculateDamage(attacker, moncarg, basicAttack);

        // Assert
        Assert.AreEqual(40f, damage, 0.01f, "Damage should be calculated as: 30 + 20 - 10 = 40");

        // Cleanup
        Object.DestroyImmediate(attackerObject);
        Object.DestroyImmediate(combatHandler);
        Object.DestroyImmediate(basicAttack);
    }

    [Test]
    public void WhenMaxHealthSet_CurrentHealthDoesNotExceed()
    {
        // Arrange: Set up a Moncarg with specific max health
        moncarg.data.maxHealth = 100f;
        moncarg.data.health = 150f; ; // Intentionally set higher than max

        // Act: Trigger Awake to initialize the Moncarg
        moncarg.Awake();

        // Assert: Current health should equal max health (not exceed it)
        Assert.AreEqual(100f, moncarg.health,
            "Current health should be clamped to max health on initialization");
        Assert.AreEqual(100f, moncarg.maxHealth,
            "Max health should remain unchanged");
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up after each test
        Object.DestroyImmediate(moncargObject);
    }
}
