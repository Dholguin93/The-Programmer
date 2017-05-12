using UnityEngine;
using System.Collections;

public class FloorAffecter : MonoBehaviour 
{
	public int floorType; 
	private bool movePlayerLeft, movePlayerRight, movePlayerUp, movePlayerDown, traversePlayer;
	public Vector3 traverseLocation;
	private PuzzleGame pGame; 
	public int moveScale; 
	private bool affectingPlayer;
	bool doOnce;

	// Use this for initialization
	void Start ()
	{
		doOnce = true;
		pGame =  GameObject.Find("PuzzleManager").transform.GetChild(0).gameObject.GetComponent<PuzzleGame>();
		DetermineFloorType();
	}

	private void SetColor(Color color)
	{
		this.gameObject.GetComponent<MeshRenderer> ().material.color = color;
	}

	public void DetermineFloorType()
	{
		if(floorType == 0)
		{
			movePlayerLeft = true;
			SetColor (Color.blue);
		}
		else if (floorType == 1)
		{
			movePlayerRight = true;
			SetColor (Color.red);
		}
		else if (floorType == 2)
		{
			movePlayerUp = true;
			SetColor (Color.green);
		}
		else if (floorType == 3)
		{
			movePlayerDown = true;
			SetColor (Color.yellow);
		}
		else if (floorType == 4)
		{
			traversePlayer = true;
			SetColor (Color.green);
		}


		affectingPlayer = false;
	}

	void OnTriggerEnter(Collider x)
	{
		// If the user just lands onto an affecter 
		if(x.gameObject.tag == "Player")
		{
			// Notify the game Manager, that this affecter is the on affecting the user
			pGame.pManager.affecterInUse = this.gameObject;
		}
	}
		
	void OnTriggerStay(Collider col)
	{
		// If the user just is ground (meaning has finished their step)
		if(col.gameObject.tag == "Player" && affectingPlayer != true)
		{
			// If the player is still moving .. stop movement!
			if(doOnce && pGame.movePlayer != false)
			{
				Debug.Log ("Clearing the Queue");

				// Clear all player steps and future commands due to the interrupt of the affecter 
				pGame.playerSteps.Clear ();
				pGame.console.commandsToExecute.Clear ();

				// Stop the player's movement
				pGame.StopStep ("PlayerStep");

				// Set flag to false, indicating not to perform this set of code again
				doOnce = false;
			}
			// On the next frame, move the player 
			else if (!pGame.movePlayer)
			{
				Debug.Log ("Moving player via Affecter");

				// Set the player to be the same color as the affecter
				col.GetComponent<MeshRenderer>().material.color = this.GetComponent<MeshRenderer>().material.color;

				// Move the player based upon the affecter 
				HandleFloorAffector (col);

			}
		}
	}

	void OnTriggerExit(Collider x)
	{
		if(x.gameObject.tag == "Player")
		{
			pGame.pManager.affecterInUse = null;
			affectingPlayer = false;
			doOnce = true;
		}
	}
		
	private void HandleFloorAffector(Collider x)
	{
		PuzzleGame pGame = GameObject.Find ("PuzzleManager").transform.GetChild (0).gameObject.GetComponent<PuzzleGame> ();

		if(true)
		{
			// Set flag to true, indicating that a user is being affected
			affectingPlayer = true;

			// Copy the movescale, in order to handle out of scope movement
			int movement = moveScale;

			if (movePlayerLeft || movePlayerRight)
			{
				if (movePlayerRight) {Debug.Log ("Moving Left");movement *= -1;}else if (movePlayerLeft){Debug.Log("Moving Right");}

				Vector3 desiredPosition = (pGame.player.transform.localPosition + new Vector3 (movement, -0.75f, 0));

				// First attempt to move the player, and see if it is within scope of the map
				bool attemptMovement = pGame.MovePlayer (desiredPosition);
				Debug.Log ("Attempted Movement : " + attemptMovement);
				Debug.Log ("Movement : " + movement);
				// Continually try to move the player in the same direction, but with less movement , as long
				// the desired position is within scope of the map 
				while(attemptMovement == false && movement != 0)
				{
					// Increment movement based upon which direction the affecter is trying to move the player 
					if(movePlayerRight){movement++;}else if (movePlayerLeft){movement--;}

					Debug.Log ("Dynamic Movement : " + movement);

					// Update the desired position based upon what is dynaimic value of movement
					desiredPosition = (pGame.player.transform.localPosition + new Vector3 (movement, -0.75f, 0));

					// Attempt to move the player again, and continue until the affecter can't (when the affecter won't move the player at all)
					if (movement  != 0) attemptMovement = pGame.MovePlayer (desiredPosition);

					Debug.Log("Dynamic Attempt Movement : " + attemptMovement);
				} 

				// If the affecter isn't able to move the player at all.. 
				if(movement == 0)
				{
					// Reset the color to white
					x.gameObject.GetComponent<MeshRenderer> ().material.color = Color.magenta;

					Debug.Log ("Can't Move player at all -- End of Map affecter case");

					GameObject.Find ("PuzzleManager").transform.GetChild (0).gameObject.GetComponent<PuzzleGame> ().PerformMapMovement ();
					affectingPlayer = false;
					GameObject.Find ("PuzzleManager").GetComponent<PuzzleManager> ().affecterInUse = null;
				}
			}
			else if (movePlayerUp || movePlayerDown)
			{

				if (movePlayerUp) {Debug.Log ("Moving Up");movement *= -1;}else if (movePlayerDown){Debug.Log ("Moving Down");}

				Vector3 desiredPosition = (pGame.player.transform.localPosition + new Vector3 (0, -0.75f, movement));

				// First attempt to move the player, and see if it is within scope of the map
				bool attemptMovement = pGame.MovePlayer (desiredPosition);

				// Continually try to move the player in the same direction, but with less movement , as long
				// the desired position is within scope of the map 
				while(attemptMovement == false && movement != 0)
				{
					// Increment movement based upon which direction the affecter is trying to move the player 
					if(movePlayerUp){movement++;}else if (movePlayerDown){movement--;}
					Debug.Log ("Dynamic Movement : " + movement);
					desiredPosition = (pGame.player.transform.localPosition + new Vector3 (0, -0.75f, movement));
					attemptMovement = pGame.MovePlayer (desiredPosition);
					Debug.Log("Dynamic Attempt Movement : " + attemptMovement);
				} 

				if(movement == 0)
				{
					Debug.Log ("Can't Move player at all -- End of Map affecter case");
					GameObject.Find ("PuzzleManager").transform.GetChild (0).gameObject.GetComponent<PuzzleGame> ().PerformMapMovement ();
					GameObject.Find ("PuzzleManager").GetComponent<PuzzleManager> ().affecterInUse = null;
				}
			}
			else if (traversePlayer)
			{
				pGame.MovePlayer (traverseLocation);
			}
		}
	}
}
