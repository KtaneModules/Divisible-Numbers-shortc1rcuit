/*MESSAGE TO ANY FUTURE CODERS:
 PLEASE COMMENT YOUR WORK
 I can't stress how important this is especially with bomb types such as boss modules.
 If you don't it makes it realy hard for somone like me to find out how a module is working so I can learn how to make my own.
 Please comment your work.
 Short_c1rcuit*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Linq;
using KModkit;

public class DivisableNumbers : MonoBehaviour
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	//The two Yea and Nay buttons
	public KMSelectable Yea;
	public KMSelectable Nay;

	//Text on the display
	public TextMesh displaytext;

	//The number you check that another number is divisible by
	int divisor;

	//The current round you are on
	int round;

	//The number checked to be divisible
	int number;

	//Fields used in the successful solve of the module (used for Souv)
	int[] finalNumbers = new int[3];
	string[] finalAnswers = new string[3];

	//Storage for the string form of the number 
	string numbertext;

	//The starting time of the bomb
	float startingTime;

	//Bool used to see wether the number is divisible or not
	bool divisible;

	//Colours for the stage counter. 0 = white, 1 = gray, 2 = green
	public Material[] Stagecolours;

	//Object that show the stage number
	public Renderer[] stages;

	//logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	//Twitch help message
#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Submit “YEA” with “!{0} Y/VoteYea”. Submit “NAY” with “!{0} N/VoteNay”.";
#pragma warning restore 414

	//This part takes the command and sees if it says yes or no then presses the correct button
	public KMSelectable[] ProcessTwitchCommand(string command)
	{
		command = command.ToLowerInvariant().Trim();
		if (command == "voteyea" | command == "y")
		{
			return new[] { Yea };
		}
		else if (command == "votenay" | command == "n")
		{
			return new[] { Nay };
		}
		return null;
	}


	void Awake()
	{
		moduleId = moduleIdCounter++;
		displaytext.text = "";
		Yea.OnInteract += delegate () { ButtonPress(Yea); return false; };
		Nay.OnInteract += delegate () { ButtonPress(Nay); return false; };
		GetComponent<KMBombModule>().OnActivate += OnActivate;
	}

	// Use this for initialization
	void OnActivate()
	{
		//Gets the starting time of the bomb
		startingTime = bomb.GetTime() / 60;

		//Generates the number for the next round
		number = UnityEngine.Random.Range(0, 10000);
		numbertext = number.ToString();

		//Puts the number on the display and adds any needed 0s on the front for continuity.
		while (numbertext.Length < 4)
		{
			numbertext = "0" + numbertext;
		}
		displaytext.text = numbertext;

		finalNumbers[0] = number;

		//Makes this the first round
		round = 1;

		//Sets the first stage counter to white
		stages[0].material = Stagecolours[0];

		//Works out the divisor
		divisor = Divisor();

		Debug.LogFormat("[Divisible Numbers #{0}] Your number must be divisible by {1}.", moduleId, divisor);

		//Works out if the number is divisible by the divisor
		divisible = (number % divisor == 0) & (number != 0);

		if (divisible)
		{
			Debug.LogFormat("[Divisible Numbers #{0}] {1} is divisible by {2}.", moduleId, number, divisor);
			finalAnswers[0] = "Yea";
		}
		else
		{
			Debug.LogFormat("[Divisible Numbers #{0}] {1} isn't divisible by {2}.", moduleId, number, divisor);
			finalAnswers[0] = "Nay";
		}

	}

	//When a button is pressed
	void ButtonPress(KMSelectable button)
	{
		if (moduleSolved)
		{
			return;
		}

		//Makes the bomb move when you press it
		button.AddInteractionPunch();

		//Makes a sound when you press the button.
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

		if ((button == Yea) == divisible)
		{
			GenerateNewRound();
		}
		else
		{
			//Gives a strike and resets the module
			GetComponent<KMBombModule>().HandleStrike();
			Debug.LogFormat("[Divisible Numbers #{0}] Incorrect, resetting.", moduleId);
			ResetModule();
		}
	}

	void GenerateNewRound()
	{
		if (round == 3)
		{
			//Solves the module
			stages[2].material = Stagecolours[2];
			StartCoroutine(Swaptext("DONE"));
			moduleSolved = true;
			GetComponent<KMBombModule>().HandlePass();
			Debug.LogFormat("[Divisible Numbers #{0}] Module solved.", moduleId);
		}
		else
		{
			//Generates the number for the next round
			number = UnityEngine.Random.Range(0, 10000);
			numbertext = number.ToString();

			//Puts the number on the display and adds any needed 0s on the front for continuity.
			while (numbertext.Length < 4)
			{
				numbertext = "0" + numbertext;
			}
			StartCoroutine(Swaptext(numbertext));

			//changes the colours of the stage counters
			stages[round - 1].material = Stagecolours[2];
			stages[round].material = Stagecolours[0];

			//Increments the round counter by one
			round += 1;

			finalNumbers[round - 1] = number;

			//Works out if the number is divisible by the divisor
			divisible = (number % divisor == 0) & (number != 0);

			if (divisible)
			{
				Debug.LogFormat("[Divisible Numbers #{0}] {1} is divisible by {2}.", moduleId, number, divisor);
				finalAnswers[round - 1] = "Yea";
			}
			else
			{
				Debug.LogFormat("[Divisible Numbers #{0}] {1} isn't divisible by {2}.", moduleId, number, divisor);
				finalAnswers[round - 1] = "Nay";
			}
		}
	}

	void ResetModule()
	{
		round = 1;

		foreach (Renderer stage in stages)
		{
			stage.material = Stagecolours[1];
		}
		stages[0].material = Stagecolours[0];

		//Generates the number for the next round
		number = UnityEngine.Random.Range(0, 10000);
		numbertext = number.ToString();

		//Puts the number on the display and adds any needed 0s on the front for continuity.
		while (numbertext.Length < 4)
		{
			numbertext = "0" + numbertext;
		}
		StartCoroutine(Swaptext(numbertext));

		finalNumbers[0] = number;

		//Works out the divisor
		divisor = Divisor();

		//Works out if the number is divisible by the divisor
		divisible = (number % divisor == 0) & (number != 0);

		if (divisible)
		{
			Debug.LogFormat("[Divisible Numbers #{0}] {1} is divisible by {2}.", moduleId, number, divisor);
			finalAnswers[0] = "Yea";
		}
		else
		{
			Debug.LogFormat("[Divisible Numbers #{0}] {1} isn't divisible by {2}.", moduleId, number, divisor);
			finalAnswers[0] = "Nay";
		}
	}

	IEnumerator Swaptext(string endString)
	{
		//Removes the digits in a right to left fashion
		for (int i = 1; i < 5; i++)
		{
			displaytext.text = displaytext.text.Substring(0, 4 - i);
			yield return new WaitForSeconds(0.15f);
		}

		for (int i = 1; i < 5; i++)
		{
			displaytext.text = endString.Substring(0, i);
			yield return new WaitForSeconds(0.15f);
		}
	}

	int Divisor()
	{
		//If else tree that asks the questions in the manual.
		if (bomb.GetBatteryCount() >= 3)
		{
			return 3;
		}
		else if (bomb.GetOnIndicators().Count() > bomb.GetOffIndicators().Count())
		{
			return 9;
		}
		else if (number < 1000)
		{
			return 6;
		}
		else if (startingTime < 10)
		{
			return 4;
		}
		else if (bomb.GetSerialNumberNumbers().Last() % 2 == 0)
		{
			return 2;
		}
		else if (bomb.GetModuleNames().Count() > 10)
		{
			return 5;
		}
		else
		{
			return 10;
		}
	}
}
