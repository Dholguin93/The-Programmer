/* Console.cs
 * Written By Diego Holguin
 * 
 *  Purpose : 
 * 
 * 
 */
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Console : MonoBehaviour 
{
	private GameObject DebugConsole, DebugText; // References to the in game console's textfield and child text object
	private GameManager gManager; // References the game's state manager
	public GameObject mainGUI, creditGUI, startGUI, puzzleGUI, puzzleSelectionGUI, puzzleHelpGUI; // References to the different GUI's
	private InputField inField; // The actual InputField Component of Debug Console
	private bool runningConCommand; // Determines if the "command" is being processed via runtime
	public bool executingCommand; // Flag that indicates when to execute multiple command Queue 
	public bool isSingleCommand; // Flag, that indicates if the current command is a single command 
	public bool isMultiCommand; 
	public int pID; 
	public Queue<string> commandsToExecute = new Queue<string>(); // Queue that will contain and execute multiple commands.. 

	#region - Error/Success Variables
	private float debugGraphDuration;
	private float debugGraphSmoothness;
	private int lerpCounter;
	private Color DebugNormelColor;
	#endregion

	#region - Valid Commands Dictionaries 
	private List<string> mainValidCommands = new List<string>
	{
		"clear", "help", "-sprogrammer", "-vcredits", "-vmain", "-spuzzleone", "-spuzzletwo", "-spuzzlethree"
	};

	private List<string> puzzleValidCommands = new List<string>
	{
		"-stepright", "-stepleft","-stepdown", "-stepup", "help"
	};
	#endregion

	private int transitionTimer, displayTimer;
		
	// Use this for initialization
	void Start () 
	{
		InitializeGameVariables ();
	}

	void InitializeGameVariables()
	{
		transitionTimer = 5; displayTimer = 4;
		runningConCommand = false;
		debugGraphDuration = 0.3f;
		lerpCounter = 2;
		debugGraphSmoothness = 0.005f;
		inField = GameObject.Find("Console_Input").GetComponent<InputField> ();
		DebugConsole= GameObject.Find ("Debug");
		DebugNormelColor = DebugConsole.GetComponent<Image> ().color;
		DebugText = DebugConsole.transform.GetChild (0).gameObject;	
		gManager = this.GetComponent<GameManager> ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (gManager.inMenu) 
		{
			CheckMainMenuCommands ();
		}
		else if (gManager.inGame)
		{
			// 
		}
		else if (gManager.inPuzzle)
		{
			CheckPuzzleCommands ();
			ExecuteMultipleCommands ();
		}
	}

	private void ExecuteMultipleCommands()
	{
		while(commandsToExecute.Count != 0 && executingCommand != false)
		{
			isMultiCommand = true;
			bool decrementMultiCommand = true;
			string singleCommand = commandsToExecute.Dequeue ();

			Debug.Log ("Executing the following command : " + singleCommand);

			// If the user doesn't have any more multi-commands (since not having any more "normel" command restarts scene
			if(gManager.numberMultiCommands == 0)
			{
					// Convey to the user that this is an error
					DebugText.GetComponent<Text> ().text = "[ERROR] : Can't Execute Anymore Multi-Commands";		

					// Set flag to true, indicating that the user total tally of multi commands shouldn't be decremented if 
					// they have already reached 0.. 
					decrementMultiCommand = false;

					// Set flag to true, meaning that to stop execution of the Queue, in turn the user "steps", immediately 
					executingCommand = false;

					// Clear the Queue, essentially ceasing execution
					commandsToExecute.Clear();

					// And then through graphics
					StartCoroutine (DebugGraphicalNotifier(false));
			}
			// If the command is a parameter command .. 
			else if(singleCommand.Contains("("))
			{
				// Perform the parameter puzzle command
				ParameterPuzzleCommand (singleCommand);
			}
			// If the command is just a normal command.. 
			else if (puzzleValidCommands.Contains(singleCommand) && gManager.numberCommands != 0)
			{
				// Perform the single puzzle command
				PerformSingleCommand (singleCommand);
			}

			// On the last series of the multi commands parsed within this Queue, where the user is enabled to perform a multi command .. 
			if(commandsToExecute.Count == 0 && decrementMultiCommand != false)
			{
				// Decrement the number of multi command that the user is able to use within the puzzle instance 
				gManager.numberMultiCommands--;
			}

			// Set flag to false, indicating that their are no commands being executed
			executingCommand = false;
		}
	}

	void CheckMainMenuCommands()
	{
		// When the user performs a command.. 
		if(inField.isFocused && inField.text != "" && (Input.GetKey(KeyCode.Return) || Input.GetMouseButton(1)) && runningConCommand != true)
		{
			// Set runtime flag to true, indicating that the console is running a command
			runningConCommand = true;

			// Get the input of the user
			string userCommand =  inField.text.ToLower();

			// Parse the user's input
			string parsedInput = ParseSpaces (userCommand);
					
			// If the command is valid..
			if(mainValidCommands.Contains(parsedInput))
			{
				// Perform the command of the user
				PerformMainCommand (parsedInput);

				// Reset Flag
				runningConCommand = false;
			}
			// If the command isn't valid.. 
			else
			{
				// Convey to the user that the command was not valid in text
				DebugText.GetComponent<Text> ().text = "[ERROR] : Non-recognizable Command";

				// And then through graphics
				StartCoroutine (DebugGraphicalNotifier(false));
			}

			// Set runtime flag to false, indicating that the console command is no longer running 
			runningConCommand = false;

			// Clear the console
			inField.text = "";
		}
	}

	private void PerformMainCommand(string input)
	{
		if(input == "clear")
		{
			inField.text = "";
		}
		else if (input == "help")
		{
			string allCommands = "";

			foreach (string command in mainValidCommands)
			{
				if (command.Equals (mainValidCommands [mainValidCommands.Count - 1]))
				{
					allCommands += command;
				}
				else 
				{
					allCommands += command + ", ";
				}
			}
			DebugText.GetComponent<Text> ().text = "[COMMANDS] : " + allCommands;
		}
		else if (input == "-vcredits")
		{
			mainGUI.SetActive (false);
			creditGUI.SetActive (true);
			DebugText.GetComponent<Text> ().text = "SUCCESS";
		}
		else if (input == "-vmain")
		{
			creditGUI.SetActive (false);
			mainGUI.SetActive (true);
			DebugText.GetComponent<Text> ().text = "SUCCESS";
		}
		else if (input == "-sprogrammer")
		{
			mainGUI.SetActive (false);
			puzzleSelectionGUI.SetActive (true);
		}
		else if (input == "-spuzzleone")
		{
			DebugText.GetComponent<Text> ().text = "SUCCESS";
			puzzleSelectionGUI.SetActive (false);
			startGUI.SetActive (true);
			StartCoroutine (StartGame());
			pID = 1;
		}
		else if (input == "-spuzzletwo")
		{
			DebugText.GetComponent<Text> ().text = "SUCCESS";
			puzzleSelectionGUI.SetActive (false);
			startGUI.SetActive (true);
			StartCoroutine (StartGame());
			pID = 2;
		}
		else if (input == "-spuzzlethree")
		{
			DebugText.GetComponent<Text> ().text = "SUCCESS";
			puzzleSelectionGUI.SetActive (false);
			startGUI.SetActive (true);
			StartCoroutine (StartGame());
			pID = 3;
		}

		StartCoroutine (DebugGraphicalNotifier(true));
	}

	void CheckPuzzleCommands()
	{
		// When the user performs a command.. 
		if(inField.isFocused && inField.text != "" && (Input.GetKey(KeyCode.Return) || Input.GetMouseButton(1)) && runningConCommand != true)
		{
			// Set runtime flag to true, indicating that the console is currently running a command
			runningConCommand = true;

			// Get the input of the user
			string userCommand =  inField.text.ToLower();

			// Parse the user's input
			string parsedInput = ParseSpaces (userCommand);

			// Reference the Game Manager script 
			GameManager gManager = GameObject.Find ("Input_Output").GetComponent<GameManager> ();

			// If the command is a mult-command input .. 
			if(parsedInput.Contains("*"))
			{
				// Split the commands up based upon the "multi-command" character.. 
				string[] commands = parsedInput.Split ('*');

				// Instance variable that will cycle through all commands and check if any aren't deemed a command.. 
				bool areCommandsValid = true;

				// For each command the user has inputted.. 
				for(int i = 0; i < commands.Length && areCommandsValid != false; i ++)
				{
					// Dynamically check if each command is valid
					areCommandsValid = areCommandsValid && CheckIfCommandIsValid (commands [i]);

					// If the current command is valid.. 
					if(areCommandsValid)
					{
						// Enter it into the queue.. 
						commandsToExecute.Enqueue (commands [i]);
					}
				}

				// Make sure the number of commands passed in allowed must be lower than 4
				areCommandsValid = areCommandsValid && commands.Length <= 3;

				// If all the user's commands check out and has a number greater  , execute each one via Queue
				if(areCommandsValid && gManager.numberMultiCommands != 0)
				{
					// Set flag to true, indicating the the Console 
					executingCommand = true;
				}
				// If the even of the user command's are invalid, don't execute any and provide appropriate feedback 
				else if (areCommandsValid!= true)
				{
						// Convey to the user that the command was not valid in text
						DebugText.GetComponent<Text> ().text = "[ERROR] : Multi-Command Not Recognized";				

						// And then through graphics
						StartCoroutine (DebugGraphicalNotifier(false));
				}

				// Clear the user's input field 
				inField.text = "";
			}
			// If the command is just a single command input .. 
			else if (parsedInput.Contains("*") != true)
			{
				// Set flag, indicating that this is a single command, enables communication between scripts
				isSingleCommand = true;

				// Execute the single command 
				PerformSingleCommand (parsedInput);
			}

			// Set runtime flag to false, indicating that the console command is no longer running 
			runningConCommand = false;
		}
	}

	private bool CheckIfCommandIsValid(string parsedInput)
	{
		// If the command is a command without any parameters and is an accepted command ... 
		if(puzzleValidCommands.Contains(parsedInput))
		{
			// return that this command is valid
			return true;
		}
		// If the command is a multiparameter command and satisfies specific requirements.. 
		else if ((parsedInput.Contains(")") && parsedInput.Contains("(")) && CheckInput(parsedInput))
		{
			// return that this command is also valid
			return true;
		}
		// If the command isn't recognized.. 
		else 
		{
			// return that this command isn't valid
			return false;
		}
	}
		
	void PerformSingleCommand(string parsedInput)
	{
		// If the command is valid..
		if(puzzleValidCommands.Contains(parsedInput) || (parsedInput.Contains(")") || parsedInput.Contains("(")))
		{				
			// If the command is a parameter command.. 
			if(parsedInput.Contains("(") && parsedInput.Contains(")"))
			{
				// Perform the parameter command of the user
				ParameterPuzzleCommand (parsedInput);
			}
			// If the command is a non-marameter command... 
			else 
			{
				if( gManager.numberCommands != 0)
				{
					// Decrement the number of commands enabled for the map
					gManager.numberCommands--;

					// Perform the non-parameter command of the user
					PerformPuzzleCommand (parsedInput, 1);
				}
				else 
				{
					if (gManager.numberMultiCommands == 0)
					{
						// Display error to user
						DebugText.GetComponent<Text> ().text = "[ERROR] : Can't Execute Anymore Multi-Commands";		

						// Start graphical error 
						StartCoroutine (DebugGraphicalNotifier(false));
					}
				}
			}
				
			// Reset Flag
			runningConCommand = false;
		}
		// If the command isn't valid.. 
		else
		{
			// Convey to the user that the command was not valid in text
			DebugText.GetComponent<Text> ().text = "[ERROR] : Non-recognizable Command";

			// And then through graphics
			StartCoroutine (DebugGraphicalNotifier(false));
		}
	}
		
	private bool CheckInput(string input)
	{
		int leftParam = input.IndexOf ("(");
		int rightParam = input.IndexOf (")");
		int beginning = input.IndexOf ("-");

		// If the input from the user isn't a parameter command .. 
		if (leftParam < 0 && rightParam < 0 && beginning < 0) 
		{
			// Return false if the input isn't a parameter input 
			return false;
		}
		// If the input from the user is a parameter command .. 
		else if (leftParam > 0 && rightParam > 0 && beginning >= 0)
		{
			string parameter = input.Substring (leftParam, rightParam - (leftParam - 1));
			string command = input.Substring (beginning, leftParam);
			Debug.Log (command);
			Debug.Log (parameter);	

			// If the command is deemed a valid command.. 
			if(command.Length-1 < leftParam && leftParam < rightParam)
			{
				// Pare out the string containing the parameter of the user's input
				string valueParam = parameter.Substring (parameter.IndexOf ("(")+1, parameter.IndexOf (")") - ((parameter.IndexOf ("(")+1)));

				int value; // Variable that had to be used because of C#'s TryParse method.. 

				// Instance variable that checks if the parameter is an integer or not.. 
				bool isInt = (int.TryParse(valueParam, out value)) ? true : false;

				// If the parameter is a int.. 
				if(isInt && value <= GameObject.Find ("Input_Output").GetComponent<GameManager> ().maxStep)
				{
					// Return that the parameter is an int, in addition to checking that the parameter is a valid command as well 
					return (true && puzzleValidCommands.Contains(command));
				}
				else
				{
					// Clear the user's input field 
					inField.text = "";
				}
			
				// Return false if the parameter isn't an int
				return false;
			}

			// Return false if the command isn't deemed valid.. 
			return false;		
		}

		// Return false is the parentheses aren't located properly 
		return false;
	}

	private void ParameterPuzzleCommand(string input)
	{
		PuzzleGame pGame = GameObject.Find ("PuzzleManager").transform.GetChild (0).gameObject.GetComponent<PuzzleGame> ();
		bool isParameter = CheckInput (input);
		Debug.Log (isParameter);
		if(isParameter)
		{
			int leftParam = input.IndexOf ("(");
			int rightParam = input.IndexOf (")");
			int beginning = input.IndexOf ("-");
			string parameter = input.Substring (leftParam, rightParam - (leftParam-1));
			string command = input.Substring (beginning, leftParam);
			string valueParam = parameter.Substring (parameter.IndexOf ("(")+1, parameter.IndexOf (")") - ((parameter.IndexOf ("(")+1)));
			// Decrement the number of multi command that the user is able to use within the puzzle instance 
			gManager.numberCommands--;
			int intParam = int.Parse(valueParam);
			PerformPuzzleCommand (command, intParam);
		}
		else if (!isParameter)
		{
			// Convey to the user that the command was not valid in text
			DebugText.GetComponent<Text> ().text = "[ERROR] : Non-recognizable Command";

			// And then through graphics
			StartCoroutine (DebugGraphicalNotifier(false));
		}
	}

	private void PerformPuzzleCommand(string input, int param)
	{ 
		PuzzleGame pGame = GameObject.Find ("PuzzleManager").transform.GetChild (0).gameObject.GetComponent<PuzzleGame> ();

		if(input == "-stepleft")
		{
			Vector3 desiredPosition = pGame.player.transform.localPosition + new Vector3 (1 * param, -0.75f, 0);
			bool validOperation =pGame.MovePlayer (desiredPosition);

			if(validOperation)
			{
				DebugText.GetComponent<Text> ().text = "SUCCESS";
				StartCoroutine (DebugGraphicalNotifier(true));
			}
			else if (validOperation != true)
			{
				DebugText.GetComponent<Text> ().text = "[ERROR] : MOVEMENT OUTSIDE SCOPE OF MAP";
				StartCoroutine (DebugGraphicalNotifier(false));
			}
		}
		else if (input == "help")
		{
			puzzleHelpGUI.SetActive (true);
			StartCoroutine (ShowHelpPuzzleGUI ());
		}
		else if (input == "-stepright")
		{
			Vector3 desiredPosition = pGame.player.transform.localPosition + new Vector3 (-1 * param, -0.75f, 0);
			bool validOperation =pGame.MovePlayer (desiredPosition);

			if(validOperation)
			{
				DebugText.GetComponent<Text> ().text = "SUCCESS";
				StartCoroutine (DebugGraphicalNotifier(true));
			}
			else if (validOperation != true)
			{
				DebugText.GetComponent<Text> ().text = "[ERROR] : MOVEMENT OUTSIDE SCOPE OF MAP";
				StartCoroutine (DebugGraphicalNotifier(false));
			}
		}
		else if (input == "-stepup")
		{
			Vector3 desiredPosition = pGame.player.transform.localPosition + new Vector3 (0, -0.75f, -1 * param);
			bool validOperation =pGame.MovePlayer (desiredPosition);

			if(validOperation)
			{
				DebugText.GetComponent<Text> ().text = "SUCCESS";
				StartCoroutine (DebugGraphicalNotifier(true));
			}
			else if (validOperation != true)
			{
				DebugText.GetComponent<Text> ().text = "[ERROR] : MOVEMENT OUTSIDE SCOPE OF MAP";
				StartCoroutine (DebugGraphicalNotifier(false));
			}
		}
		else if (input == "-stepdown")
		{
			Vector3 desiredPosition = pGame.player.transform.localPosition + new Vector3 (0, -0.75f, 1 * param);
			bool validOperation =pGame.MovePlayer (desiredPosition);

			if(validOperation)
			{
				DebugText.GetComponent<Text> ().text = "SUCCESS";
				StartCoroutine (DebugGraphicalNotifier(true));
			}
			else if (validOperation != true)
			{
				DebugText.GetComponent<Text> ().text = "[ERROR] : MOVEMENT OUTSIDE SCOPE OF MAP";
				StartCoroutine (DebugGraphicalNotifier(false));
			}
		}

		// Clear the console
		inField.text = "";
	}

	IEnumerator ShowHelpPuzzleGUI()
	{
		while (displayTimer > 0)
		{
			startGUI.transform.GetChild (0).GetComponent<Text> ().text = "Starting game in " + displayTimer + " seconds.....";
			yield return new WaitForSeconds (1f);
			displayTimer--;
		}

		displayTimer = 5;
		puzzleHelpGUI.SetActive (false);
	}

	IEnumerator StartGame()
	{
		while(transitionTimer > 0)
		{
			startGUI.transform.GetChild (0).GetComponent<Text> ().text = "Starting game in " + transitionTimer + " seconds.....";
			yield return new WaitForSeconds (1f);
			transitionTimer--;
		}

			transitionTimer = 5;
			this.transform.GetChild (0).gameObject.transform.GetChild (0).gameObject.SetActive (false);
			startGUI.gameObject.SetActive (false);
			gManager.inMenu = false;
			gManager.inPuzzle = true;
			puzzleGUI.SetActive (true);
			SceneManager.LoadScene ("Puzzle");
	}

	IEnumerator TransitionToInGameLevel(float timer, string nameOfLevel, bool isPuzzle)
	{
		if (isPuzzle) 
		{
			DebugText.GetComponent<Text> ().text = "[SUCCESS] : Starting the puzzle in " + timer + " seconds";
		}
		else if (!isPuzzle)
		{
			DebugText.GetComponent<Text> ().text = "[SUCCESS] : Starting the level in " + timer + " seconds";
		}

		yield return new WaitForSeconds (timer);
		SceneManager.LoadScene (nameOfLevel);
	}

	IEnumerator DebugGraphicalNotifier(bool success) 
	{
		float progress = 0;

		float increment = debugGraphSmoothness / debugGraphDuration;

		while(progress < 1 && lerpCounter != 0)
		{
			if((DebugConsole.GetComponent<Image> ().color == Color.red  || DebugConsole.GetComponent<Image> ().color == Color.cyan) && lerpCounter == 2)
			{
				lerpCounter--;
			}
			else if (DebugConsole.GetComponent<Image> ().color == Color.white && lerpCounter == 1)
			{
				lerpCounter--;
			}

			if (lerpCounter == 2) 
			{
				if (success) 
				{
					DebugConsole.GetComponent<Image> ().color = Color.Lerp (DebugConsole.GetComponent<Image> ().color, Color.cyan, progress);
				} 
				else if (!success)
				{
					DebugConsole.GetComponent<Image> ().color = Color.Lerp (DebugConsole.GetComponent<Image> ().color, Color.red, progress);
				}

				progress += increment;
				yield return null;

			}
			else if (lerpCounter == 1)
			{
				DebugConsole.GetComponent<Image> ().color = Color.Lerp (DebugConsole.GetComponent<Image> ().color, Color.white, progress);
				progress += increment;
				yield return null;
			}
		}

		lerpCounter = 2;

		DebugText.GetComponent<Text> ().text = "";
	}

	private string ParseSpaces(string userInput)
	{
		return string.Join("", userInput.Split(default(string[]), System.StringSplitOptions.RemoveEmptyEntries));
	}
}
