﻿using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Contains all data/state variables regarding the player character.
/// These variables are synced across the network and across all players.
/// </summary>
public class HeroModel : NetworkBehaviour
{
    // Stats
    [SyncVar] private int maxHealth;
    [SyncVar] private int baseMoveSpeed;
    [SyncVar] private int baseDefense;
    [SyncVar] private int baseAttack;

    [SyncVar] private int currentHealth;
    [SyncVar] private int currentDefense;
    [SyncVar] private int currentAttack;
    [SyncVar] private int currentMoveSpeed;

    // Managers
    private MatchManager matchManager;
    private CharacterMovement characterMovement;

    // Other state variables
    [SyncVar] private int playerId;         // playerId controlling this hero
    [SyncVar] private HeroType heroType;
    [SyncVar] private bool isKnockedOut;
    [SyncVar] private int score;
    [SyncVar] private int heroIndex;        // current hero model the player has; 0 = king, 1 = rogue, 2 = wizard, 3 = armoured

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Start()
    {
        if (!hasAuthority) return;

        characterMovement = new CharacterMovement(0);
        score = 0;
        isKnockedOut = false;
        heroIndex = 0;
        matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        SetPlayerId(matchManager.GetPlayerId());
    }

    /// <summary>
    /// Causes this hero to take a specified amount of damage.
    /// Calculates how much damage the hero should take based on stats, then decrements the hero's health by the calculated amount.
    /// This function is run on the server using data from the client which called this function.
    /// </summary>
    /// <param name="amount">Amount of damage this hero should take.</param>
    [Command]
    public void CmdTakeDamage(int amount)
    {
		if (isKnockedOut)
			return;
        // Calculate final damage taken based on stats
        float finalDamage = 0;
        float dmgReduce = (float)currentDefense;
        dmgReduce = dmgReduce / 50;
        finalDamage = amount * (1 - dmgReduce);

        finalDamage = Mathf.Clamp(finalDamage, 0, int.MaxValue);    // restrict damage to [0, int.MaxValue]
        currentHealth = currentHealth - (int)Mathf.Round(finalDamage);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);   // restrict health to [0, maxHealth]
    }

    /// <summary>
    /// Heals the player by a specified amount.
    /// Can only be called from the player owning this object.
    /// </summary>
    /// <param name="amount">Amount to heal the player.</param>
    public void Heal(int amount)
    {
        if (!hasAuthority) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (!isServer)
        {
            CmdHeal(amount);
        }
    }

    /// <summary>
    /// Heals the player by a specified amount.
    /// Can be called from either server or client.
    /// </summary>
    /// <param name="amount">Amount to heal the player.</param>
    [Command]
    public void CmdHeal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    /// <returns>
    /// Returns true if character's current attack differs from their base atack.
    /// </returns>
    public bool IsAttackBuffed()
    {
        if (currentAttack > baseAttack)
        {
            return true;
        }

        return false;
    }

    /// <returns>
    /// Returns true if character's current defense differs from their base defense.
    /// </returns>
    public bool IsDefBuffed()
    {
        if (currentDefense > baseDefense)
        {
            return true;
        }

        return false;
    }

    /// <returns>
    /// Returns true if character's current speed differs from their base speed.
    /// </returns>
    public bool IsSpeedBuffed()
    {
        if (currentMoveSpeed > baseMoveSpeed)
        {
            return true;
        }

        return false;
    }

    /// ----------------------------------------
    /// -               SETTERS                -
    /// ----------------------------------------
    /// Setters are implemented to synchronize the variables across all players.
    /// Synchronization is done using SyncVar and Command:
    ///     - SyncVar synchronizes the value from server to clients.
    ///     - Command synchronizes the value from client to server.
    /// Only the player owning this script can change its variables, hence a check on hasAuthority before any variable is set.

    /// <summary>
    /// Setter for player id.
    /// </summary>
    private void SetPlayerId(int id)
    {
        if (!hasAuthority) return;  // only the player owning this hero should change its player id

        playerId = id;

        if (!isServer)
        {
            CmdSetPlayerId(id);
        }
    }
    [Command]
    private void CmdSetPlayerId(int id)
    {
        playerId = id;
    }

    /// <summary>
    /// Setter for hero type.
    /// </summary>
    public void SetHeroType(HeroType heroType)
    {
        if (!hasAuthority) return;

        this.heroType = heroType;

        if (!isServer)
        {
            CmdSetHeroType(heroType);
        }
    }
    [Command]
    private void CmdSetHeroType(HeroType heroType)
    {
        this.heroType = heroType;
    }

    /// <summary>
    /// Setter for base move speed.
    /// </summary>
    public void SetBaseMoveSpeed(int val)
    {
        if (!hasAuthority) return;

        baseMoveSpeed = val;
        currentMoveSpeed = val;
        characterMovement.SetSpeed(val);

        if (!isServer)
        {
            CmdSetBaseSpeed(val);
        }
    }
    [Command]
    private void CmdSetBaseSpeed(int val)
    {
        baseMoveSpeed = val;
        currentMoveSpeed = val;
    }

    /// <summary>
    /// Setter for current move speed.
    /// </summary>
    public void SetCurrentMoveSpeed(int val)
    {
        currentMoveSpeed = val;
        if (!hasAuthority) {
            RpcSetcurr(val);
            return;
        }
        characterMovement.SetSpeed(val);

        if (!isServer)
        {
            CmdSetCurrentMoveSpeed(val);
        }
    }
    [ClientRpc]
    private void RpcSetcurr(int val) {
        if (!hasAuthority) return; // So not run on server.
        characterMovement.SetSpeed(val);
    }

    [Command]
    private void CmdSetCurrentMoveSpeed(int val)
    {
        currentMoveSpeed = val;
    }

    /// <summary>
    /// Setter for base defense.
    /// </summary>
    public void SetBaseDefense(int val)
    {
        if (!hasAuthority) return;

        baseDefense = val;
        currentDefense = val;

        if (!isServer)
        {
            CmdSetBaseDefense(val);
        }
    }
    [Command]
    private void CmdSetBaseDefense(int val)
    {
        baseDefense = val;
        currentDefense = val;
    }

    /// <summary>
    /// Setter for current defense.
    /// </summary>
    public void SetCurrentDefense(int val)
    {
        if (!hasAuthority) return;

        currentDefense = val;

        if (!isServer)
        {
            CmdSetCurrentDefense(val);
        }
    }
    [Command]
    private void CmdSetCurrentDefense(int val)
    {
        currentDefense = val;
    }

    /// <summary>
    /// Setter for base attack.
    /// </summary>
    public void SetBaseAttack(int val)
    {
        if (!hasAuthority) return;

        baseAttack = val;
        currentAttack = val;

        if (!isServer)
        {
            CmdSetBaseAttack(val);
        }
    }
    [Command]
    private void CmdSetBaseAttack(int val)
    {
        baseAttack = val;
        currentAttack = val;
    }

    /// <summary>
    /// Setter for current attack.
    /// </summary>
    public void SetCurrentAttack(int val)
    {
        if (!hasAuthority) return;

        currentAttack = val;

        if (!isServer)
        {
            CmdSetCurrentAttack(val);
        }
    }
    [Command]
    private void CmdSetCurrentAttack(int val)
    {
        currentAttack = val;
    }

    /// <summary>
    /// Setter for max health.
    /// </summary>
    public void SetMaxHealth(int val)
    {
        if (!hasAuthority) return;

        maxHealth = val;

        if (!isServer)
        {
            CmdSetMaxHealth(val);
        }
    }
    [Command]
    private void CmdSetMaxHealth(int val)
    {
        maxHealth = val;
    }

    /// <summary>
    /// Sets the hero's health back to full.
    /// </summary>
    public void SetFullHealth()
    {
        if (!hasAuthority) return;

        currentHealth = maxHealth;

        if (!isServer)
        {
            CmdSetFullHealth();
        }
    }
    [Command]
    private void CmdSetFullHealth()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Sets the knocked out status of the hero.
    /// </summary>
    public void SetKnockedOut(bool isKnockedOut)
    {
        if (!hasAuthority) return;

        this.isKnockedOut = isKnockedOut;

        if (!isServer)
        {
            CmdSetKnockedOut(isKnockedOut);
        }
    }
    [Command]
    private void CmdSetKnockedOut(bool isKnockedOut)
    {
        this.isKnockedOut = isKnockedOut;
    }

    /// <summary>
    /// Increment the score by a value.
    /// </summary>
    /// <param name="amount">Value to increment the score by.</param>
    public void IncreaseScore(int amount)
    {
        //if (!hasAuthority) return;

        score += amount;
    }
    [ClientRpc]
    private void RpcIncreaseScore(int amount)
    {
        score += amount;
    }
		
	/// <summary>
	/// Activates drunk effect
	/// </summary>
	public void drunk(bool isDrunk){

		if (!hasAuthority) {
			RpcDrunk(isDrunk);
			return;
		}
		GameObject.FindGameObjectWithTag("HeroCamera" + playerId.ToString()).GetComponent<HeroCameraController> ().drunkEffect (isDrunk);

		if (!isServer)
		{
			CmdDrunk(isDrunk);
		}
	}
	[Command]
	private void CmdDrunk(bool isDrunk)
	{
		GameObject.FindGameObjectWithTag("HeroCamera" + playerId.ToString()).GetComponent<HeroCameraController> ().drunkEffect (isDrunk);
	}
	[ClientRpc]
	private void RpcDrunk(bool isDrunk) {
		if (!hasAuthority) return; // So not run on server.
		GameObject.FindGameObjectWithTag("HeroCamera" + playerId.ToString()).GetComponent<HeroCameraController> ().drunkEffect (isDrunk);
	}

    /// <summary>
    /// This function sets the model of the hero to the given hero.
    /// Reference: https://answers.unity.com/questions/1414490/set-active-over-network.html
    /// </summary>
    /// <param name="myHero">The hero to set the model to.</param>
    public void SetModel(Hero myHero)
    {
        if (!hasAuthority) return;  // only the player owning this hero should change its model

        MatchManager matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        HeroManager heroManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<HeroManager>();
        GameObject hero = heroManager.GetHeroObject(matchManager.GetPlayerId());

        int oldHeroIndex = heroIndex;
        int childIndex = myHero.childIndex;
        HeroType heroType = myHero.heroType;

        // Update the player's model locally
        LocalSetModel(hero, childIndex, heroType, oldHeroIndex);

        // Send update of player's model to all other players
        if (isServer)
        {
            RpcSetModel(hero, childIndex, heroType, oldHeroIndex);
        }
        else
        {
            CmdSetModel(hero, childIndex, heroType, oldHeroIndex);
        }
    }

    /// <summary>
    /// Tell server to change the model of the specified player on the client.
    /// </summary>
    /// <param name="hero">Hero object to change the model of.</param>
    /// <param name="myHero">Hero to change to.</param>
    /// <param name="oldHeroIndex">Index of the hero previously selected.</param>
    [Command]
    private void CmdSetModel(GameObject hero, int childIndex, HeroType type, int oldHeroIndex)
    {
        LocalSetModel(hero, childIndex, heroType, oldHeroIndex);
        RpcSetModel(hero, childIndex, heroType, oldHeroIndex);
    }

    /// <summary>
    /// From server, tell all clients to change the model of the specified player.
    /// </summary>
    /// <param name="hero">Hero object to change the model of.</param>
    /// <param name="myHero">Hero to change to.</param>
    /// <param name="oldHeroIndex">Index of the hero previously selected.</param>
    [ClientRpc]
    private void RpcSetModel(GameObject hero, int childIndex, HeroType type, int oldHeroIndex)
    {
        if (isLocalPlayer) return;  // prevent receiving the notification you started
        LocalSetModel(hero, childIndex, heroType, oldHeroIndex);
    }

    /// <summary>
    /// Locally sets the model of the player.
    /// </summary>
    /// <param name="hero">Hero object to change the model of.</param>
    /// <param name="myHero">Hero to change to.</param>
    /// <param name="oldHeroIndex">Index of the hero previously selected.</param>
    public void LocalSetModel(GameObject hero, int childIndex, HeroType type, int oldHeroIndex)
    {
        hero.transform.GetChild(oldHeroIndex).gameObject.SetActive(false);
        heroIndex = childIndex;
        heroType = type;
        hero.transform.GetChild(heroIndex).gameObject.SetActive(true);
    }

    /// <summary>
    /// Set a basic attack to be active.
    /// TODO: make basic attacks.
    /// </summary>
    public void SetBasicAttack()
    {

    }

    /// ----------------------------------------
    /// -               GETTERS                -
    /// ----------------------------------------
    public HeroType GetHeroType()
    {
        return heroType;
    }

    public int GetBaseMoveSpeed()
    {
        return baseMoveSpeed;
    }

    public int GetBaseDefense()
    {
        return baseDefense;
    }

    public int GetBaseAttack()
    {
        return baseAttack;
    }

    public int GetCurrentMoveSpeed()
    {
        return currentMoveSpeed;
    }

    public int GetCurrentDefense()
    {
        return currentDefense;
    }

    public int GetCurrentAttack()
    {
        return currentAttack;
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsKnockedOut()
    {
        return isKnockedOut;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetHeroIndex()
    {
        return heroIndex;
    }

    public CharacterMovement GetCharacterMovement()
    {
        return characterMovement;
    }
}
