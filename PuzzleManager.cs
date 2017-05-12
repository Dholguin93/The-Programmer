using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PuzzleManager : MonoBehaviour 
{
	public GameObject[] puzzleTypes;
	public GameObject affecterInUse; 
	private GameManager gManager;
	private GameObject puzzleGUI;
	private PuzzleGame pGame;
	private AudioClip PlayerMovement;
	private AudioSource source;
	public int puzzleID; 

	void Start()
	{
		//
		puzzleGUI = GameObject.Find ("PuzzleGUI");

		// Reference the Game Manager Script
		gManager = GameObject.Find ("Input_Output").GetComponent<GameManager> ();

		// Start the level, based upon a specific choice of what puzzle to load
		StartLevel (GameObject.Find("Input_Output").GetComponent<Console>().pID);

		source = this.GetComponent<AudioSource> ();

		source.clip = Resources.Load ("CubeSound") as AudioClip;
	}

	void Update()
	{
		puzzleGUI.transform.GetChild (1).GetComponent<Text> ().text = "Multi-Commands Remaining : " + gManager.numberMultiCommands;
		puzzleGUI.transform.GetChild (2).GetComponent<Text> ().text = "Commands Remaining : " + gManager.numberCommands;
		puzzleGUI.transform.GetChild (3).GetComponent<Text> ().text = "Max Step : " + gManager.maxStep;
	}
		
	public void RestartLevel()
	{
		SceneManager.LoadScene ("Puzzle");
	}

	public void playMovementSound()
	{
		source.Play ();
	}

	void StartLevel(int p_id)
	{
		puzzleID = p_id;

		if(puzzleID == 1)
		{
			// The number of max multi commands that can be used for puzzle one
			int puzzleOneMCommands = 1;

			// The number of commands that can be executed for puzzle one
			int puzzleOneCommands = 10;

			int maxStep = 3;

			// Instantiate prefab of level one
			GameObject puzzleInstance = Instantiate(puzzleTypes[0]) as GameObject;

			// Initialize the puzzle one's number of Multi and Single commands allowed for the level 
			pGame = puzzleInstance.GetComponent<PuzzleGame> ();

			puzzleInstance.GetComponent<PuzzleGame> ().InitPuzzleGame (puzzleOneCommands, puzzleOneMCommands, maxStep);

			// Set the parent of the instantiated puzzle to this object 
			puzzleInstance.transform.SetParent (this.transform);

			// Set the puzzle location to start at the origin 
			puzzleInstance.transform.localPosition = Vector3.zero;

		}
		else if (puzzleID == 2)
		{
			// The number of max multi commands that can be used for puzzle one
			int puzzleOneMCommands = 1;

			// The number of commands that can be executed for puzzle one
			int puzzleOneCommands = 7;

			int maxStep = 3;

			// Instantiate prefab of level one
			GameObject puzzleInstance = Instantiate(puzzleTypes[1]) as GameObject;

			// Initialize the puzzle one's number of Multi and Single commands allowed for the level 
			pGame = puzzleInstance.GetComponent<PuzzleGame> ();

			puzzleInstance.GetComponent<PuzzleGame> ().InitPuzzleGame (puzzleOneCommands, puzzleOneMCommands, maxStep);

			// Set the parent of the instantiated puzzle to this object 
			puzzleInstance.transform.SetParent (this.transform);

			// Set the puzzle location to start at the origin 
			puzzleInstance.transform.localPosition = Vector3.zero;
		}
		else if (puzzleID == 3)
		{
			// The number of max multi commands that can be used for puzzle one
			int puzzleOneMCommands = 1;

			// The number of commands that can be executed for puzzle one
			int puzzleOneCommands = 10;

			int maxStep = 3;

			// Instantiate prefab of level one
			GameObject puzzleInstance = Instantiate(puzzleTypes[2]) as GameObject;

			// Initialize the puzzle one's number of Multi and Single commands allowed for the level 
			pGame = puzzleInstance.GetComponent<PuzzleGame> ();

			puzzleInstance.GetComponent<PuzzleGame> ().InitPuzzleGame (puzzleOneCommands, puzzleOneMCommands,maxStep);

			// Set the parent of the instantiated puzzle to this object 
			puzzleInstance.transform.SetParent (this.transform);

			// Set the puzzle location to start at the origin 
			puzzleInstance.transform.localPosition = Vector3.zero;
		}
	}
}
