using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PuzzleGame : MonoBehaviour 
{
	// The step delay for the player's movement 
	private float stepDelay = 0.5f;

	// The movement speed of the player 
	private float movementSpeed = 2f; 

	// The collection of Rows and Columns within the puzzle (handled in inspecter via tags)
	public GameObject[] movementRowFloors, movementRows;

	// A reference to the player object within the game 
	public GameObject player;

	public Material normelBlockMat;

	// A reference to the start and end location of the puzzle (handled in inspecter)
	private GameObject startLocation, endLocation;

	public Console console;

	public bool affecterCollided;

	public int collisionCount = 0;
	public Vector3 startPosition, endPosition;
	public Vector3 movementVector;
	public bool movePlayer;
	private bool moveRight, moveLeft, moveDown, moveUp;
	public bool run;
	public Queue<Vector3> playerSteps = new Queue<Vector3> ();
	private Vector3 lastStep ; 
	public GameManager gManager;
	public PuzzleManager pManager;

	private bool isAffectorMoved = false;


	// Use this for initialization
	void Start () 
	{
		// Reference the Game Manager Script 
		gManager = GameObject.Find ("Input_Output").GetComponent<GameManager> ();

		// Reference the Console Script
		console = GameObject.Find ("Input_Output").GetComponent<Console> ();

		// Reference the Puzzle Manager Script
		pManager = this.transform.parent.gameObject.GetComponent<PuzzleManager> ();

		// Get all of the movement tiles of the a specific puzzle map
		movementRowFloors = GameObject.FindGameObjectsWithTag ("FloorMovement");

		// Get each row within the puzzle
		movementRows = GameObject.FindGameObjectsWithTag ("RowMovement");

		// Reference the starting and ending locaiton of the puzzle
		startLocation = GameObject.Find ("Start"); endLocation = GameObject.Find ("End");

		// Mark the starting and end points of the map
		MarkEndPoints ();

		// Create the player, starting at the start position
		CreatePlayer (startLocation.transform.position);
		StartCoroutine ("PlayerBreadth");

	}

	// Update is called once per frame
	void Update () 
	{
		CheckPlayerMovement();
		PlayerReachEnd ();
	}

	private bool PlayerReachEnd()
	{
		// Return true, in the case that the player has reached the end of the puzzle
		if(player.transform.position == (endLocation.transform.position + new Vector3(0,0.75F,0)))
		{
			return true;
		}

		return false;
	}

	public void PerformMapMovement()
	{
		int numOfColumnForEachRow = 11;

		// For each row.. 
		for(int j = 0; j < movementRows.Length; j++)
		{
			// Reference the last column for each row.. 
			GameObject lastColumnForEachRow = movementRows[j].GetComponent<RowInfo>().lastColumn;

			// For each column within that row.. 
			for(int i =0; i <  movementRows [j].transform.childCount; i++)
			{
				//Debug.Log ("Child count for " + movementRows [j].name + " is " + movementRows [j].transform.childCount + " and the i value is " + i );
				GameObject currentColumn = null;

				// If the column is non-wrap column... i.e column that is either the starting column of the row or another column within that row that isn't the last column... 
				if(i != movementRows [j].transform.childCount - 1)
				{
					currentColumn = movementRows [j].transform.GetChild (i).gameObject;
				}
				// if the column is just an ordinary column that isn't the 
				else if (i == movementRows [j].transform.childCount - 1)
				{
					currentColumn = movementRows [j].transform.GetChild (movementRows [j].transform.childCount - 1).gameObject;
				}

				// Destroy's any left over Arrow
				if (currentColumn.transform.childCount > 0) foreach (Transform childTransform in currentColumn.transform) Destroy(childTransform.gameObject);

				// Determines if the column being evaluated is the first column of the row 
				bool isLastColumn = (i+1%numOfColumnForEachRow == movementRows [j].transform.childCount) ? true : false;

				// Determines if the column being evaluated is the first column of the row 
				bool isFirstColumn = (i == 0) ? true : false;

				// Determines whether the current column being evaluated is an Affecter.. 
				bool isAffector = (currentColumn.GetComponent<FloorAffecter> () != null) ? true : false;

				// If the current floor has an effector before an affector has been swapped over the row ..
				if(isAffector && !isAffectorMoved)
				{
					// Reference, in memory, the Floor Affecter Script
					FloorAffecter currentColumnAffector = currentColumn.GetComponent<FloorAffecter> ();

					// Reference the color of the current column -- Since affecters are colored differently.. 
					Color affecterColor = currentColumn.GetComponent<MeshRenderer> ().material.color;

					Material affecterMaterial = currentColumn.GetComponent<MeshRenderer> ().material;

					int[] variables = new int [2] {currentColumnAffector.floorType, currentColumnAffector.moveScale};

					// Set the current column to be the natural color... think : un affecter !
					currentColumn.GetComponent<MeshRenderer> ().material = normelBlockMat;
					currentColumn.GetComponent<MeshRenderer>().material.color = Color.white;
					currentColumn.transform.eulerAngles = Vector3.zero;

					// Destroy the Floor Affecter Script on the current column 
					Destroy (currentColumn.GetComponent<FloorAffecter>());

					// If the column is the first column for the row.. 
					if (isFirstColumn) 
					{
						// Reference the previous column 
						GameObject previousColumn = lastColumnForEachRow;
					
						// Attach the Floor Affecter Script from the current column onto the next column.. 
						previousColumn.AddComponent (typeof(FloorAffecter));
						previousColumn.GetComponent<MeshRenderer> ().material = affecterMaterial;
						previousColumn.GetComponent<FloorAffecter> ().floorType = variables [0];
						previousColumn.GetComponent<FloorAffecter> ().moveScale = variables [1];
						previousColumn.GetComponent<MeshRenderer> ().material.color = affecterColor;
						isAffectorMoved = true;
					} 
					// If the column isn't the last column for the row.. 
					else if (!isFirstColumn && !isLastColumn)
					{
						
						// Reference the previous column 
						GameObject previousColumn = movementRows [j].transform.GetChild (i - 1).gameObject;

						// Attach the Floor Affecter Script from the current column onto the next column.. 
							previousColumn.AddComponent (typeof(FloorAffecter));
						    previousColumn.GetComponent<MeshRenderer> ().material = affecterMaterial;
							previousColumn.GetComponent<FloorAffecter> ().floorType = variables [0];
							previousColumn.GetComponent<FloorAffecter> ().moveScale = variables [1];
							previousColumn.GetComponent<MeshRenderer> ().material.color = affecterColor;
						}
					else if (isLastColumn)
					{
						if(!isAffectorMoved)
						{
							// Reference the previous column 
							GameObject previousColumn = movementRows [j].transform.GetChild (i - 1).gameObject;

							// Attach the Floor Affecter Script from the current column onto the next column.. 
							previousColumn.AddComponent (typeof(FloorAffecter));
							previousColumn.GetComponent<MeshRenderer> ().material = affecterMaterial;
							previousColumn.GetComponent<FloorAffecter> ().floorType = variables [0];
							previousColumn.GetComponent<FloorAffecter> ().moveScale = variables [1];
							previousColumn.GetComponent<MeshRenderer> ().material.color = affecterColor;
						}
					}
				}
				else if (isAffector && isAffectorMoved && !isLastColumn)
				{
					// Reference, in memory, the Floor Affecter Script
					FloorAffecter currentColumnAffector = currentColumn.GetComponent<FloorAffecter> ();

					// Reference the color of the current column -- Since affecter are colored differently.. 
					Color affecterColor = currentColumn.GetComponent<MeshRenderer> ().material.color;

					Material affecterMaterial = currentColumn.GetComponent<MeshRenderer> ().material;

					// Set the current column to be the natural color... think : un affecter !
					currentColumn.GetComponent<MeshRenderer> ().material = normelBlockMat;
					currentColumn.GetComponent<MeshRenderer>().material.color = Color.white;
					currentColumn.transform.eulerAngles = Vector3.zero;

					int[] variables = new int [2] {currentColumnAffector.floorType, currentColumnAffector.moveScale};

					// Destroy the Floor Affecter Script on the current column 
					Destroy (currentColumn.GetComponent<FloorAffecter>());

					// Reference the previous column 
					GameObject previousColumn = movementRows [j].transform.GetChild (i - 1).gameObject;

					// Attach the Floor Affecter Script from the current column onto the next column.. 
					previousColumn.AddComponent (typeof(FloorAffecter));
					previousColumn.GetComponent<MeshRenderer> ().material = affecterMaterial;
					previousColumn.GetComponent<FloorAffecter> ().floorType = variables [0];
					previousColumn.GetComponent<FloorAffecter> ().moveScale = variables [1];
					previousColumn.GetComponent<MeshRenderer> ().material.color = affecterColor;
					previousColumn.GetComponent<FloorAffecter> ().DetermineFloorType ();
				}
			}
				
			isAffectorMoved = false;
		}
	}

	private void CheckPlayerMovement()
	{
		// If the Queue has a movement that needs to be executed .. 
		while(playerSteps.Count != 0 && movePlayer != true)
		{
			Debug.Log ("Playersteps Count : " + playerSteps.Count);

			// If the command thats being executed is the last command.. 
			if(playerSteps.Count == 1)
			{
				// Remove and reference this last step 
				Vector3 singleStep = playerSteps.Dequeue ();

				// Start the Coroutine to move the player to desired location, passing in the flag true to 
				// indicate that this is the last step.. 
				StartCoroutine (PerformStep (singleStep, true));

				// Set flag to true, indicating that the player is moving.. 
				movePlayer = true;
			}
			// If the command thats going to be executed isn't the last command.. 
			else if (playerSteps.Count != 1)
			{
				// Remove and reference this last step 
				Vector3 singleStep = playerSteps.Dequeue ();

				// Start the Coroutine to move the player to desired location, passing in the flag false to 
				// indicate that this isn't the last step.. 
				StartCoroutine (PerformStep (singleStep, false));

				// Set flag to true, indicating that the player is moving.. 
				movePlayer = true;	
			}

			pManager.playMovementSound ();
		}
	}

	public void StopStep(string nameOfCoroutine)
	{
		StopCoroutine(nameOfCoroutine);

		Debug.Log ("Stoped Coroutine : " + nameOfCoroutine);
	}

	IEnumerator PlayerBreadth()
	{
		bool colorChanged = false; 
		int multiplier = 1; 
		while(run)
		{
			if (player.transform.localScale.x > 0.55f) multiplier = -1; 
			else if (player.transform.localScale.x < 0.35f) multiplier = 1;

			Debug.Log (player.transform.localScale);
			Debug.Log (multiplier);
			float rateOfGrowth = Time.deltaTime * multiplier * 0.75f;

			player.transform.localScale += new Vector3 (rateOfGrowth, rateOfGrowth, rateOfGrowth);

			yield return new WaitForSeconds(0.05f);
		}

		yield return null;
	}

	IEnumerator PerformStep(Vector3 stepLocation, bool lastStep)
	{
		// Loop condition, to set true when the loop, and in turn the IEnumerator, needs to cease execution 
		bool exitLoop = false;

		// While the player is being moved to their desired location.. 
		while(exitLoop != true)
		{
			// Constantly check the position between the player and the desired location, in this case the stepLocation the player needs to move towards.. 
			float distanceFromPosition = Vector3.Distance (player.transform.position, stepLocation);

			// Movement cases
			if (moveRight) 
			{
				player.transform.Translate (Vector3.right * Time.deltaTime * movementSpeed);
			}
			else if (moveLeft)
			{
				player.transform.Translate (Vector3.left * Time.deltaTime * movementSpeed);
			}
			else if (moveDown)
			{
				player.transform.Translate (-Vector3.forward * Time.deltaTime * movementSpeed); 
			}
			else if (moveUp)
			{
				player.transform.Translate (Vector3.forward * Time.deltaTime * movementSpeed); 
			}

			// Once the player's object is close enough to the desired location 
			if(Mathf.Abs(distanceFromPosition) <= 0.03f)
			{
				// Move the player to the exact position 
				player.transform.position = stepLocation;

				// Set the movement vector, reflecting where the player intends on moving, to reference nothing  (CANT BECAUSE OF UNITY)
				movementVector = Vector3.one; 

				// After every successful step, check game cases
				CheckGameCases ();

				// Once the player is moved to their desired location, have a step delay 
				yield return  new WaitForSeconds(stepDelay);

				// Set flag to false, indicating that the player won't be moved any longer 
				movePlayer = false;

				// Exit the loop, ceasing any movement of the player 
				exitLoop = true;

				// If this happens to be the last step.. 
				if(lastStep)
				{
					// Communicate with the Console script, indicating that the console can now execute another command, since the last command of the previous
					// command has finished executing.. 
					console.executingCommand = true;

					// Set the player's color to white
					player.GetComponent<MeshRenderer> ().material.color = Color.magenta;

					// If the console is executing a multi-command and is on the last multi-command inputted into the system.. 
					if(console.commandsToExecute.Count == 0 && console.isMultiCommand)
					{
						// If the user doesn't land on an affecter .. 
						if(pManager.affecterInUse == null)
						{
							// Perform map movement, altering the entire map 
							PerformMapMovement ();

							// Reset flags
							console.isMultiCommand = false;
							console.isSingleCommand = false;
						}
					}

					// If the console is just executing a single command 
					if (console.isSingleCommand)
					{
						Debug.Log ("Is A Single Command");

						// If the user doesn't land on an affecter .. 
						if(pManager.affecterInUse == null)
						{
							// Perform map movement, altering the entire map 
							PerformMapMovement ();

							// Reset flags
							console.isMultiCommand = false;
							console.isSingleCommand = false;
						}
					}
				}
			}

			// Return nothing
			yield return null;
		}
	}

	private void CheckGameCases()
	{
		// First, we'll check if the user has no more command to execute i.e end game
		if (gManager.numberCommands == 0)
		{
			pManager.RestartLevel ();
		}
		// Next, we check if the user has reached the end point
		else if (player.transform.position == (endLocation.transform.localPosition + new Vector3(0,0.75f,0)))
		{
			// Set the appropriete gameobjects active or not
			console.puzzleGUI.SetActive (false);
			console.mainGUI.SetActive (true);
			console.gameObject.transform.GetChild (0).gameObject.transform.GetChild (0).gameObject.SetActive (true);

			// Set flags to reflect that the user is in the main menu
			gManager.inMenu = true;
			gManager.inPuzzle = false;

			// Load the actual scene 
			SceneManager.LoadScene ("MainMenu");
		}
	}

	void CreatePlayer(Vector3 start)
	{
		// The offset needed to instantiate the player above the starting location 
		Vector3 instaintedOffset = new Vector3 (0, 0.75f, 0);

		// Crate a single Cube to represent the player 
		player = GameObject.CreatePrimitive (PrimitiveType.Cube);

		// Set player detials
		player.name = "Player";player.tag = "Player";
		player.GetComponent<MeshRenderer> ().material.color = Color.magenta;

		// Set the player collider to be a trigger
		player.GetComponent<BoxCollider> ().isTrigger = true;

		// Render the player above the starting location 
		player.transform.localPosition = start + instaintedOffset;

		// Scale the object 
		player.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
	}

	public bool MovePlayer(Vector3 movementPosition)
	{
		// Check if the input is valid within this system or not
		bool validMovement = CheckMovement (movementPosition);

		// Calculate number of horizontal "steps" the player needs to take 
		int numXSteps = (int) Mathf.Abs((player.transform.localPosition - movementPosition).x);

		// Boolean indicating if we're moving the player in the negative x axis or not
		bool xStepNeg = ((player.transform.localPosition - movementPosition).x < 0) ? true : false;

		// Calculate the numebr of vertical "steps" the player needs to take
		int numZSteps = (int) Mathf.Abs((player.transform.localPosition - movementPosition).z);

		// Boolean indicating if we're moving the player in the negative z axis or not
		bool zStepNeg = ((player.transform.localPosition - movementPosition).z < 0) ? true : false;

		// If the player is moving their character left or right.. 
		if(numXSteps > 0 && validMovement)
		{
			// Instance variable to indicate which direction the player is moving in 
			int movementDirection = (xStepNeg) ? 1 : -1;
				
			// Variable to reference the player's starting position 
			Vector3 startPosition = player.transform.localPosition;

			// Create the "movement string" via iterations
			for(int i = 1; i <= numXSteps; i++)
			{
				// Instance variable to contain the each "step" the user will be taking 
				Vector3 stepMovement = startPosition + new Vector3 (i * movementDirection,0,0);

				// Add this "step" to a queue of commands that will be executed 
				playerSteps.Enqueue(stepMovement);
			}
		}
		else if (numZSteps > 0 && validMovement)
		{
			// Instance variable to indicate which direction the player is moving in 
			int movementDirection = (zStepNeg) ? 1 : -1;

			// Variable to reference the player's starting position 
			Vector3 startPosition = player.transform.localPosition; 

			// Create the "movement string" via iterations
			for(int i = 1; i <= numZSteps; i++)
			{
				// Instance variable to contain the each "step" the user will be taking 
				Vector3 stepMovement = startPosition + new Vector3 (0, 0, i * movementDirection);

				// Add this "step" to a queue of commands that will be executed 
				playerSteps.Enqueue(stepMovement);
			}
		}

		return validMovement;
	}

	public bool CheckMovement(Vector3 desiredLocation)
	{
		// Initially, set flag to false
		bool canMove = false;

		// Check if the desired location is within scope of the map
		for (int j = 0; j < movementRowFloors.Length && canMove != true; j++)
		{
			// If one of the locations in the map matches the desired location
			if(desiredLocation == movementRowFloors[j].transform.localPosition)
			{
				// Update the movement vector to reflect the new position
				movementVector = movementRowFloors [j].transform.localPosition;

				// Determine which direction the movement of the player needs to head towards in
				DetermineMovementDirection ();

				// Set flag to true, enabling the player to move
				canMove = true;
			}
		}

		// Return whether the desired location is within scope of the map or not
		return canMove;
	}

	private void DetermineMovementDirection()
	{
		Debug.Log ("Movement Vector : " + movementVector);
		Vector3 differenceInVectors = player.transform.localPosition - movementVector;

		if(differenceInVectors.x < 0)
		{
			moveRight = true;
			moveLeft = false;
			moveUp = false;
			moveDown = false;
		}
		else if (differenceInVectors.x > 0)
		{
			moveRight = false;
			moveLeft = true;
			moveUp = false;
			moveDown = false;		
		}

		if (differenceInVectors.z < 0)
		{
			moveRight = false;
			moveLeft = false;
			moveUp = true;
			moveDown = false;		
		}
		else if (differenceInVectors.z > 0)
		{
			moveRight = false;
			moveLeft = false;
			moveUp = false;
			moveDown = true;
		}
	}
		
	void MarkEndPoints()
	{
		startLocation.GetComponent<MeshRenderer> ().material.color = Color.white;
		endLocation.GetComponent<MeshRenderer> ().material.color = Color.magenta;
	}

	public void InitPuzzleGame(int numOfCommands, int numOfMCommands, int _maxStep)
	{
		// Reference the Game Manager 
		GameManager gManager = GameObject.Find ("Input_Output").GetComponent<GameManager> ();

		// Initialize the level restrictions 
		gManager.InitLevelRestrictions (numOfMCommands, numOfCommands, _maxStep);

		// Set flag to true, indicating to the Puzzle Manager script to start executing it's commands 
		run = true;
	}
}
