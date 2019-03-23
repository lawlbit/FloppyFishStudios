﻿using UnityEngine;

/// <summary>
/// Logic for game objective attached to the artifact prefab.
/// </summary>
public class ArtifactController : MonoBehaviour
{
    private bool isCarried;             // whether or not the artifact is currently being carried by a player
    private GameObject playerThatOwns;  // if artifact held, the player that is currently holding the artifact
    private int ownerID;                // if artifact held, the player id of the player that is currently holding the artifact
    private Vector3 ownerSpawn;         // the spawn location of the last player that held this artifact
    private RarityType rarity;          // rarity of this artifact

    Vector3 smallscale = new Vector3(1.25f, 1.25f, 1.25f);  // size used when carried (smaller)
    Vector3 normalscale = new Vector3(2f, 2f, 2f);	        // size used when artifact is on the ground (larger)

    /// <summary>
    /// Rarity of the artifact.
    /// </summary>
    public enum RarityType
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Initialize variables.
    /// </summary>
    void Start()
    {
        ownerID = -1;
        transform.localScale = normalscale;
        rarity = RarityType.Common;
    }


    void Update()
    {
        //if being carried, update location of artifact to where the carrier is
        if (isCarried)
        {
            transform.position = new Vector3(playerThatOwns.transform.position.x, playerThatOwns.transform.position.y + 3.5f, playerThatOwns.transform.position.z);
            if (playerThatOwns.GetComponent<HeroModel>().IsKnockedOut())
            {
                //player is knocked out, so he drops the artifact
                DroppedArtifact();
            }
        }
    }

    /// <summary>
    /// Checks the following cases through collision detection:
    ///     1. player picks up artifact
    ///     2. player returns artifact to spawn.
    /// </summary>
    /// <param name="col">Collider that artifact has collided with.</param>
    private void OnTriggerEnter(Collider col)
    {
        switch (col.gameObject.transform.tag)
        {
            case ("Player"):
                // Check case where player picks up artifact
                // Ensure that artifact is not already carried and player is not knocked out
                if (!isCarried && !col.GetComponent<HeroModel>().IsKnockedOut())
                {
                    // Make object float above character model's head
                    transform.localScale = smallscale;
                    playerThatOwns = col.gameObject;

                    // Set owner ID and spawn point
                    ownerID = playerThatOwns.GetComponent<HeroModel>().GetPlayerId();
                    ownerSpawn = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<HeroManager>().GetSpawnLocationOfPlayer(ownerID);
                    isCarried = true;

					// Slow down the player on pickup
					playerThatOwns.GetComponent<HeroController>().ArtifactPickup();

                    // Broadcast that player has acquired the artifact
                    GameObject.FindGameObjectWithTag("MatchManager").GetComponent<AnnouncementManager>().BroadcastAnnouncementAleAcquired(ownerID);
                }
                break;
            case ("SpawnRoom"):
                // Checks case where player enters scoring location (spawn point)
                if (isCarried)
                {
                    // Ensure that this spawn is the right one for the player carrying the artifact
                    if (Vector3.Distance(transform.position, ownerSpawn) <= 10)
                    {
						// Undo character slowdown
						playerThatOwns.GetComponent<HeroController>().ArtifactDrop();
						
                        // Broadcast that player has scored the artifact
                        GameObject.FindGameObjectWithTag("MatchManager").GetComponent<AnnouncementManager>().BroadcastAnnouncementAleScored(ownerID);

                        // Increase the player's score
                        playerThatOwns.GetComponent<HeroModel>().IncreaseScore(GetScore());

                        // Spawn another artifact
                        GameObject.FindGameObjectWithTag("EventSystem").GetComponent<ArtifactSpawn>().SpawnArtifactRandom();

                        // Destroy itself
                        Destroy(gameObject);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Called when the artifact is dropped.
    /// </summary>
    public void DroppedArtifact()
    {
        // Broadcast the announcement that artifact is dropped
        GameObject.FindGameObjectWithTag("MatchManager").GetComponent<AnnouncementManager>().BroadcastAnnouncementAleDropped(ownerID);

        // Set position of artifact on the ground where player has died
        transform.position = new Vector3(playerThatOwns.transform.position.x, playerThatOwns.transform.position.y + 1f, playerThatOwns.transform.position.z);

		// Undo character slowdown
		playerThatOwns.GetComponent<HeroController>().ArtifactDrop();

        // Reset owning player variables
        playerThatOwns = null;
        ownerID = -1;
        isCarried = false;

        // Scale size of artifact back up
        transform.localScale = normalscale;
    }

    /// <returns>
    /// Returns the player ID currently carrying the artifact.
    /// </returns>
    public int GetOwnerID()
    {
        return ownerID;
    }

    /// <returns>
    /// Returns the amount of score the artifact will reward based on its rarity.
    /// </returns>
    private int GetScore()
    {
        switch (rarity)
        {
            case RarityType.Common:
                return 100;
            case RarityType.Epic:
                return 250;
            case RarityType.Legendary:
                return 500;
            case RarityType.Rare:
                return 1000;
            default:
                Debug.Log("ArtifactController::GetScore() ERROR: Should not reach here!");
                return 0;
        }
    }
}