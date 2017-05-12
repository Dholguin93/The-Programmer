using UnityEngine;
using System.Collections;

public class GameManager:MonoBehaviour
{
	public bool inGame, inMenu, inPuzzle;
	public int numberMultiCommands, numberCommands, maxStep; 

	// Use this for initialization
	void Start ()
	{
		inMenu = true;
		inGame = false;
		inPuzzle = false;
	}

	public void InitLevelRestrictions(int totalMC, int totalC, int mxStep)
	{
		// Set the player's total multi commands
		numberMultiCommands= totalMC;

		// Set the total amount of commands 
		numberCommands = totalC;

		maxStep = mxStep;
	}
		




}
