﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets the values in the scoreboard.
/// Attached to the scoreboard prefab.
/// This assumes that there are exactly 2 players - Player 1 and Player 2; otherwise, the scoreboard will not be shown.
/// </summary>
public class ScoreboardUI : MonoBehaviour
{
    // Hero icons
    private Sprite king;
    private Sprite rogue;
    private Sprite wizard;
    private Sprite knight;

    // Managers
    private HeroManager heroManager;
    private MatchManager matchManager;

    // UI elements
    private TextMeshProUGUI player1Name;
    private TextMeshProUGUI player2Name;
    private TextMeshProUGUI player1Score;
    private TextMeshProUGUI player2Score;
    private Image player1Icon;
    private Image player2Icon;

    /// <summary>
    /// Initialize variables.
    /// Note: Awake->Enable->Start.
    /// </summary>
    void Awake()
    {
        // Initialize managers
        matchManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<MatchManager>();
        heroManager = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<HeroManager>();

        // Get scoreboard UI elements
        player1Name = GameObject.Find("Player1Text").GetComponent<TextMeshProUGUI>();
        player2Name = GameObject.Find("Player2Text").GetComponent<TextMeshProUGUI>();
        player1Score = GameObject.Find("Player1ScoreText").GetComponent<TextMeshProUGUI>();
        player2Score = GameObject.Find("Player2ScoreText").GetComponent<TextMeshProUGUI>();
        player1Icon = GameObject.Find("Player1Icon").GetComponent<Image>();
        player2Icon = GameObject.Find("Player2Icon").GetComponent<Image>();

        // Get icon resources
        king = Resources.Load<Sprite>("UI Resources/king");
        rogue = Resources.Load<Sprite>("UI Resources/thief");
        wizard = Resources.Load<Sprite>("UI Resources/mage");
        knight = Resources.Load<Sprite>("UI Resources/knight");
    }

    /// <summary>
    /// Called whenever scoreboard is opened by the player.
    /// Checks that there are exactly 2 players - Player 1 and Player 2; otherwise, does not open.
    /// Sets the values in the scoreboard.
    /// </summary>
    void OnEnable()
    {
        // Player objects
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        GameObject player1 = null, player2 = null;

        // Get the player objects
        foreach (GameObject player in playerObjects)
        {
			if (player.GetComponent<HeroModel>().GetPlayerId() == 1 || player.GetComponent<HeroModel>().GetPlayerId() == 0)
            {
                player1 = player;
            }
            else if (player.GetComponent<HeroModel>().GetPlayerId() == 2)
            {
                player2 = player;
            }
        }

        // Check that both player objects are set properly
		if ( (player1 == null || player2 == null) && !(matchManager.GetMaxPlayers() == 1) )
        {
            gameObject.SetActive(false);
            return;
        }

        // Set the player name text and colour
		player1Name.text = "<color=#" + heroManager.GetPlayerColourHexCode(player1.GetComponent<HeroModel>().GetPlayerId()) + ">You</color>";

        // Set the player score
        player1Score.text = player1.GetComponent<HeroModel>().GetScore().ToString();
        

		if (matchManager.GetMaxPlayers () != 1) {
			player2Name.text = "<color=#" + heroManager.GetPlayerColourHexCode (player2.GetComponent<HeroModel> ().GetPlayerId ()) + ">Player 2</color>";
			player2Score.text = player2.GetComponent<HeroModel> ().GetScore ().ToString ();
		} else {
			try{
				GameObject.Find("Player2").SetActive(false);
			}catch(NullReferenceException e){
			}

		}
			

        // Set the player icons
        switch (player1.GetComponent<HeroModel>().GetHeroIndex())
        {
            case 0:
                player1Icon.sprite = king;
                break;
            case 1:
                player1Icon.sprite = rogue;
                break;
            case 2:
                player1Icon.sprite = wizard;
                break;
            case 3:
                player1Icon.sprite = knight;
                break;
            default:
                Debug.Log("ERROR: given hero index does not match any known hero type!");
                player1Icon.sprite = king;
                break;
        }
		if (matchManager.GetMaxPlayers () != 1) {
			switch (player2.GetComponent<HeroModel> ().GetHeroIndex ()) {
			case 0:
				player2Icon.sprite = king;
				break;
			case 1:
				player2Icon.sprite = rogue;
				break;
			case 2:
				player2Icon.sprite = wizard;
				break;
			case 3:
				player2Icon.sprite = knight;
				break;
			default:
				Debug.Log ("ERROR: given hero index does not match any known hero type!");
                //player2Icon.sprite = king;
				break;
			}
		}
    }
}
