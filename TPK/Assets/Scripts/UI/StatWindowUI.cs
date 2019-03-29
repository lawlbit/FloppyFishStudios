﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatWindowUI : MonoBehaviour
{
    public GameObject skillDescription;
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillDescriptionTitleText;
    public GameObject artifactComponent;

    private int playerId;
    private HeroModel heroModel;
    private HeroManager heroManager;

    /// <summary>
    /// Setup UI elements when stat window is active.
    /// </summary>
    void Awake()
    {
        // Get playerId
        MatchManager matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        playerId = matchManager.GetPlayerId();

        // Get networkHeroManager for hero stats
        heroManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<HeroManager>();
        heroModel = heroManager.GetHeroObject(playerId).GetComponent<HeroModel>();

        // Set UI elements
        skillDescription.SetActive(false);  // set to inactive by default
        SetupSkills();
    }

    private void OnEnable()
    {
        SetupStats();
    }

    void Update()
    {
        SetupAvatar();
        SetupArtifact();
    }

    /// <summary>
    /// Setup the skill UI elements to match player's equipped skills.
    /// </summary>
    private void SetupSkills()
    {
        AbilityManager abilityManager = heroManager.GetHeroObject(playerId).GetComponent<AbilityManager>();

        for (int i = 0; i < abilityManager.equippedSkills.Length; i++)
        {
            if (abilityManager.equippedSkills[i] != null)
            {
                GameObject skill = GameObject.Find("StatWindowSkill" + (i + 1));
                Image skillImg = skill.GetComponent<Image>();
                skillImg.sprite = abilityManager.equippedSkills[i].skillIcon;   // set sprite image
                skill.GetComponent<SkillHoverDescription>().skill = abilityManager.equippedSkills[i];   // Set skill description
            }
        }
    }

    /// <summary>
    /// Setup the stat UI elements to match player's actual stats.
    /// </summary>
    private void SetupStats()
    {
        // Get text elements
        TextMeshProUGUI atkText = GameObject.Find("AttackText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defText = GameObject.Find("DefenseText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI spdText = GameObject.Find("SpeedText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI healthText = GameObject.Find("MaxHealthText").GetComponent<TextMeshProUGUI>();

        // Get differences in current and base stats
        int speedDif = heroModel.GetBaseMoveSpeed() - heroModel.GetCurrentMoveSpeed();
        int atkDif = heroModel.GetBaseAttack() - heroModel.GetCurrentAttack();
        int defDif = heroModel.GetBaseDefense() - heroModel.GetCurrentDefense();

        // Set speed text
        if (speedDif == 0)
        {
            spdText.text = heroModel.GetBaseMoveSpeed().ToString();
        }
        else if (speedDif > 0)
        {
            // slow down
            spdText.text = "<color=#FF0000>" + heroModel.GetCurrentMoveSpeed().ToString() + "</color> (" + heroModel.GetBaseMoveSpeed().ToString() + ")";
        }
        else
        {
            // speed up
            spdText.text = "<color=#00FF00>" + heroModel.GetCurrentMoveSpeed().ToString() + "</color> (" + heroModel.GetBaseMoveSpeed().ToString() + ")";
        }

        // Set attack text
        if (atkDif == 0)
        {
            atkText.text = heroModel.GetBaseAttack().ToString();
        }
        else if (atkDif > 0)
        {
            // slow down
            atkText.text = "<color=#FF0000>" + heroModel.GetCurrentAttack().ToString() + "</color> (" + heroModel.GetBaseAttack().ToString() + ")";
        }
        else
        {
            // speed up
            atkText.text = "<color=#00FF00>" + heroModel.GetCurrentAttack().ToString() + "</color> (" + heroModel.GetBaseAttack().ToString() + ")";
        }

        // Set defense text
        if (defDif == 0)
        {
            defText.text = heroModel.GetBaseDefense().ToString();
        }
        else if (defDif > 0)
        {
            // slow down
            defText.text = "<color=#FF0000>" + heroModel.GetCurrentDefense().ToString() + "</color> (" + heroModel.GetBaseDefense().ToString() + ")";
        }
        else
        {
            // speed up
            defText.text = "<color=#00FF00>" + heroModel.GetCurrentDefense().ToString() + "</color> (" + heroModel.GetBaseDefense().ToString() + ")";
        }

        // Set health text
        healthText.text = heroModel.GetMaxHealth().ToString();
    }

    /// <summary>
    /// Setup the "currently carrying" and artifact image.
    /// </summary>
    private void SetupArtifact()
    {
        GameObject[] artifacts = GameObject.FindGameObjectsWithTag("Artifact"); // list of all artifacts in the game
        bool isCarryingArtifact = false;    // whether or not the player is currently carrying an artifact

        // Loop through all artifacts in the game and see if any of them are being carried by the current player
        foreach (GameObject artifact in artifacts)
        {
            ArtifactController artifactControl = artifact.GetComponent<ArtifactController>();
            if (artifactControl.GetOwnerID() == playerId)
            {
                isCarryingArtifact = true;
                break;
            }
        }

        // If player is carrying an artifact, activate the UI component
        if (isCarryingArtifact && !artifactComponent.activeSelf)
        {
            artifactComponent.SetActive(true);
        }
        else if (!isCarryingArtifact && artifactComponent.activeSelf)
        {
            // If player is not carrying an artifact, deactivate the UI component
            artifactComponent.SetActive(false);
        }
    }

    /// <summary>
    /// Setup the hero avatar in the stat window.
    /// </summary>
    private void SetupAvatar()
    {
        // Initialize variables
        HeroManager heroManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<HeroManager>();
        MatchManager matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        GameObject heroAvatar = heroManager.GetHeroObject(playerId);            // avatar of the player's hero
        GameObject heroAvatarCameraObj = GameObject.Find("HeroAvatarCamera");   // camera which will be focused on the hero avatar

        // Setup camera; follows player avatar
        Camera heroAvatarCamera = heroAvatarCameraObj.GetComponent<Camera>();
        heroAvatarCamera.transform.position = heroAvatar.transform.position + new Vector3(0.1f, 2.3f, 3.5f);
        heroAvatarCamera.transform.rotation = Quaternion.Euler(10, 180, 0);
    }
}
