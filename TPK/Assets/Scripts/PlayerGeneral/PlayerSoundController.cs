﻿using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Plays all sound effects related to player attacks.
/// </summary>
public class PlayerSoundController : NetworkBehaviour
{
    private AudioSource source;
    private AudioClip aoe;
    private AudioClip[] projectileSounds;
    private AbilityCaster caster;
    private AudioClip basicAttack;
    private AudioClip artifactSound;
    private AudioClip itemSoundEffect;
    private AudioClip potionSoundEffect;

    void Start()
    {
        source = GetComponent<AudioSource>();
        caster = GetComponent<AbilityCaster>();

        // Loads in corresponding sound effect for each skill
        projectileSounds = new AudioClip[caster.projectiles.Length];
        for (int i = 0; i < caster.projectiles.Length; i++)
        {
            projectileSounds[i] = Resources.Load("SoundEffects/" + caster.projectiles[i].name) as AudioClip;
        }
        aoe = Resources.Load("SoundEffects/aoe") as AudioClip;
        basicAttack = Resources.Load("SoundEffects/BasicAttack") as AudioClip;
        artifactSound = Resources.Load("SoundEffects/GameplaySoundEffects/ArtifactSound") as AudioClip;
        itemSoundEffect = Resources.Load("SoundEffects/GameplaySoundEffects/Buff") as AudioClip;
        potionSoundEffect = Resources.Load("SoundEffects/GameplaySoundEffects/Potion") as AudioClip;
    }
    
    /// <summary>
    /// Play sound for AOE skill.
    /// </summary>
    [ClientRpc]
    public void RpcPlayAOESound()
    {
        source.PlayOneShot(aoe);
    }

    /// <summary>
    /// Play sound for basic attack.
    /// </summary>
    [ClientRpc]
    public void RpcPlayBasicAttackSound()
    {
        source.PlayOneShot(basicAttack);
    }

    /// <summary>
    /// Play sound for corresponding projectile skill.
    /// </summary>
    [ClientRpc]
    public void RpcPlaySoundEffect(string projectileName)
    {
        source.PlayOneShot(GetSoundEffectWithName(projectileName));
    }

    /// <summary>
    /// Play sound for artifact pickup
    /// </summary>
    [ClientRpc]
    public void RpcPlayArtifactSound()
    {
        if (!isLocalPlayer) return;
        source.PlayOneShot(artifactSound, 0.5f);
    }

    /// <summary>
    /// Play sound for item buff pickup
    /// </summary>
    [ClientRpc]
    public void RpcPlayItemBuffSound()
    {
        if (!isLocalPlayer) return;
        source.PlayOneShot(itemSoundEffect, 0.5f);
    }

    /// <summary>
    /// Play sound for health potion pickup
    /// </summary>
    [ClientRpc]
    public void RpcPlayPotionSound()
    {
        if (!isLocalPlayer) return;
        source.PlayOneShot(potionSoundEffect);
    }

    /// <summary>
    /// Return projectile sound effect corresponding to projectile name.
    /// </summary>
    private AudioClip GetSoundEffectWithName(string projectileName)
    {
        for (int i = 0; i < projectileSounds.Length; i++)
        {
            if (string.Compare(projectileSounds[i].name, projectileName) == 0)
            {
                return projectileSounds[i];
            }
        }
        return null;
    }
}
