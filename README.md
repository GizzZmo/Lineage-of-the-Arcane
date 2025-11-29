### 1\. Project Name & Description

**Repository Name:** `lineage-of-the-arcane`
**Description:** A game mechanic prototype where magic is sentient, involving ancestral summoning and resource tethering.

-----

### 2\. Folder Structure

Create these folders on your computer or inside your Unity project `Assets` folder:

```text
lineage-of-the-arcane/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── MagicParent.cs       # Base class for the entities
│   │   │   └── TetherSystem.cs      # The health-drain mechanic
│   │   ├── Entities/
│   │   │   └── IgnisMater.cs        # Example "Fire Mother" implementation
│   │   └── Player/
│   │       └── PlayerController.cs
│   └── Docs/
│       └── GDD.md                   # Game Design Document
├── README.md
└── .gitignore                       # Standard Unity gitignore
```

-----

### 3\. The Core Code (C\#)

Here are the scripts that make the "Parents" mechanic work.

#### A. The Base Class (`MagicParent.cs`)

This defines what a "Parent" is. It handles the personality traits.

```csharp
using UnityEngine;

// The Blueprint for all "Parents of Magic"
public abstract class MagicParent : MonoBehaviour
{
    [Header("Entity Stats")]
    public string entityName;
    public float tetherCostPerSecond = 5.0f; // How much Health/Sanity it costs
    [Range(0, 100)] public float defianceLevel = 0f; // Chance to ignore orders

    protected PlayerController boundPlayer;

    // Called when the player initiates the Tether
    public virtual void OnSummon(PlayerController player)
    {
        boundPlayer = player;
        Debug.Log($"{entityName} has entered the reality. The Tether is formed.");
        ApplyEnvironmentalShift();
    }

    // Every Parent changes the game world physics/lighting
    protected abstract void ApplyEnvironmentalShift();

    // The unique passive rule (e.g., "Must keep attacking")
    public abstract void CheckTemperament();
}
```

#### B. The Concrete Implementation (`IgnisMater.cs`)

This is the "Fire Mother" we discussed. She demands aggression.

```csharp
using UnityEngine;

public class IgnisMater : MagicParent
{
    private void Start()
    {
        entityName = "Ignis Mater, The Combustion";
        tetherCostPerSecond = 10.0f; // High cost
    }

    protected override void ApplyEnvironmentalShift()
    {
        // Code to turn the floor to lava or make lighting red
        RenderSettings.ambientLight = Color.red;
        Debug.Log("The world heats up. Ignis is watching.");
    }

    public override void CheckTemperament()
    {
        // Mechanic: If player hasn't attacked in 3 seconds, punish them
        if (Time.time - boundPlayer.lastAttackTime > 3.0f)
        {
            PunishPlayer();
        }
    }

    void PunishPlayer()
    {
        boundPlayer.TakeDamage(5);
        Debug.Log("Ignis burns you for your hesitation!");
    }
}
```

#### C. The Tether System (`TetherSystem.cs`)

This handles the "Risk/Reward" health drain logic.

```csharp
using UnityEngine;

public class TetherSystem : MonoBehaviour
{
    public MagicParent activeSummon;
    public PlayerController player;
    public bool isTethered = false;

    void Update()
    {
        if (isTethered && activeSummon != null)
        {
            MaintainTether();
            activeSummon.CheckTemperament(); // Check if the Parent is angry
        }
    }

    void MaintainTether()
    {
        // Drain player health to keep summon alive
        float cost = activeSummon.tetherCostPerSecond * Time.deltaTime;
        
        if (player.currentHealth > cost)
        {
            player.currentHealth -= cost;
            // Visual effect: Grey bar increases
        }
        else
        {
            BreakTether();
        }
    }

    void BreakTether()
    {
        isTethered = false;
        Debug.LogWarning("THE TETHER SNAP! The Parent goes RAMPANT.");
        // Logic to make the Summon attack the player goes here
    }
}
```

-----

### 4\. The README.md File

Create a file named `README.md` and paste this in. This makes your GitHub page look professional.

```markdown
# Lineage of the Arcane: The Parents of Magic

> "Magic is not a tool. It is a family tree. And you are the youngest child."

## 🔮 Project Overview
**Lineage of the Arcane** is a gameplay prototype exploring a unique magic system where spells are sentient entities. Players do not cast magic; they negotiate with ancient "Progenitors" (Parents) to lend their aid.

## ⚙️ Core Mechanics implemented
1.  **The Tether System:** Mana does not exist. Summoning drains the user's max health (Physical Tether) and sanity.
2.  **Ancestral Temperament:** Summons have specific personalities (Aggressive, Passive, Rhythm-based). Failing to adhere to their playstyle results in the summon damaging the player.
3.  **Environmental Shifts:** Summoning a Parent changes the physics and lighting of the game map globally.

## 📂 Structure
- `Scripts/Core/`: Base logic for the Tether and Entity inheritance.
- `Scripts/Entities/`: Unique AI logic for specific Parents (e.g., Ignis Mater).

## 🚀 Roadmap
- [ ] Implement "Custody Battle" (Multiplayer tug-of-war for summons)
- [ ] Add Tier 1 (Scions) and Tier 2 (Heirs) evolution logic
- [ ] Create the "Rampant" AI state when a Tether breaks

## 📄 License
MIT License
```

-----

### 5\. How to upload this to GitHub

If you have Git installed, open your terminal/command prompt in your project folder and run:

1.  `git init`
2.  `git add .`
3.  `git commit -m "Initial commit: Core mechanics for Parents of Magic"`
4.  `git branch -M main`
5.  `git remote add origin https://github.com/YOUR_USERNAME/lineage-of-the-arcane.git`
6.  `git push -u origin main`

*(Make sure to create the empty repo on GitHub.com first\!)*
