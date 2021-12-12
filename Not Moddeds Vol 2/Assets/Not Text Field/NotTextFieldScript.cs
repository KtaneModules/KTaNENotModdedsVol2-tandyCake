using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using NotTextField;
using Rnd = UnityEngine.Random;
public class NotTextFieldScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public NTFButton[] buttons;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private Dictionary<char, int> arrowLookup;
    private static readonly string alphabet = "ABCDEF";
    private static readonly string[] arrowColors = { "red", "green", "blue" };
    private static readonly string[] ordinals = { "1st", "2nd", "3rd" };
    private char bgChar;
    private char[] otherChars;
    private int[] otherPositions = new int[3];
    private char[] solution = new char[3];
    private int submissionPointer;
    private bool animating = true;
    private string availableLetters;

    void Awake () {
        moduleId = moduleIdCounter++;
    }
    void InitializeButtons()
    {
        for (int i = 0; i < 12; i++)
        {
            int ix = i;
            buttons[i].selectable.OnInteract += delegate () { ButtonPress(ix); return false; };
        }
    }

    void Start ()
    {
        InitializeButtons();
        bgChar = alphabet.PickRandom();
        Log("The letter which appears 9 times is {0}", bgChar);
        availableLetters = "ABCDEF".Replace(bgChar.ToString(), "");

        GenerateLetters();
        OrderFirstStage();
        GetArrows();
        Module.OnActivate += () => StartCoroutine(ShowLetters(true));
    }
    void ButtonPress(int pos)
    {
        if (availableLetters.Contains(buttons[pos].displayedLetter))
            Audio.PlaySoundAtTransform("bloop" + availableLetters.IndexOf(buttons[pos].displayedLetter), buttons[pos].transform);
        else Audio.PlaySoundAtTransform("bloop" + Rnd.Range(0, 5), buttons[pos].transform);
        buttons[pos].selectable.AddInteractionPunch();
        if (buttons[pos].submitted)
            return;
        if (solution[submissionPointer] == buttons[pos].displayedLetter)
        {
            submissionPointer++;
            buttons[pos].SetChar('✓');
            buttons[pos].submitted = true;
            if (submissionPointer == 3)
            {
                submissionPointer = 0;
                if (buttons.All(x => x.submitted))
                {
                    moduleSolved = true;
                    Module.HandlePass();
                    Log("All buttons pressed; module solved");
                }
                else
                {
                    GenerateLetters();
                    StartCoroutine(ShowLetters());
                    GenerateAnswer();
                }
            }
        }
        else
        {
            Log("Pressed button {0} ({2}) while expected {1} ({3}). Strike!", pos + 1, Enumerable.Range(0, 12).First(x => buttons[x].displayedLetter == solution[submissionPointer]) + 1,
                buttons[pos].displayedLetter, solution[submissionPointer]);
            for (int i = 0; i < 12; i++)
                buttons[i].submitted = false;
            submissionPointer = 0;
            GenerateLetters();
            OrderFirstStage();
            StartCoroutine(ShowLetters());
            Module.HandleStrike();
        }
    }
    void GenerateLetters()
    {
        var shuffle = alphabet.ToCharArray();
        do otherChars = shuffle.Shuffle().Take(3).ToArray();
        while (otherChars.Contains(bgChar));
        int[] order = Enumerable.Range(0, 12).Where(x => !buttons[x].submitted).ToArray().Shuffle().Take(3).ToArray();
        for (int i = 0; i < 12; i++)
            if (!buttons[i].submitted)
                buttons[i].displayedLetter = bgChar;
        for (int i = 0; i < 3; i++)
            buttons[order[i]].displayedLetter = otherChars[i];
        Log("The generated other letters are {0}", otherChars.Join());
    }
    void OrderFirstStage()
    {
        solution = otherChars.OrderBy(x => x).ToArray();
        Log("You should press the letters in alphabetical order ({0})", solution.Join());
    }
    void GenerateAnswer()
    {
        char[] prevSolution = (char[])solution.Clone();
        char[] dispCharsInOrder = otherChars.OrderBy(x => x).ToArray();
        char[] mappedChars = new char[3];
        for (int i = 0; i < 3; i++)
        {
            int arrowUsed = arrowLookup[prevSolution[i]];
            Log("For letter {0} (previous answer {1}), we should take the {2} arrow.", dispCharsInOrder[i], prevSolution[i], arrowColors[arrowUsed]);
            mappedChars[i] = Data.diagrams[bgChar][dispCharsInOrder[i]][arrowUsed];
            Log("Letter {0} maps to {1}.", dispCharsInOrder[i], mappedChars[i]);
        }
        solution = dispCharsInOrder.OrderBy(x => mappedChars[Array.IndexOf(dispCharsInOrder, x)]).ThenBy(x => x).ToArray();
        Log("You should press the letters in order {0}", solution.Join());
    }
    void GetArrows()
    {
        arrowLookup = new Dictionary<char, int>()
        {
            { 'A', Bomb.GetBatteryCount() % 2 == 0                  ? 0 : 1 },
            { 'B', Bomb.GetPortCount() % 2 == 0                     ? 0 : 2 },
            { 'C', Bomb.GetOnIndicators().Count() % 2 == 0          ? 1 : 0 },
            { 'D', Bomb.GetOffIndicators().Count() % 2 == 0         ? 1 : 2 },
            { 'E', Bomb.GetSerialNumberNumbers().First() % 2 == 0   ? 2 : 0 },
            { 'F', Bomb.GetSerialNumberNumbers().Last() % 2 == 0    ? 2 : 1 },
        };
    }
    
    IEnumerator ShowLetters(bool start = false)
    {
        animating = true;
        for (int i = 0; i < 12; i++)
            if (!buttons[i].submitted)
                buttons[i].label.text = "-";
        if (!start)
            yield return new WaitForSeconds(1);
        int[] order = Enumerable.Range(0, 12).ToArray().Shuffle();
        for (int i = 0; i < 12; i++)
        {
            buttons[order[i]].UpdateDisp();
            yield return new WaitForSeconds(0.05f);
        }
        animating = false;
    }
    void Log(string msg, params object[] formatArgs)
    {
        Debug.LogFormat("[Not Text Field #{0}] {1}", moduleId, string.Format(msg, formatArgs));
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use [!{0} press ABC] to press those letters.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command) 
    {
        command = command.Trim().ToUpperInvariant();
        Match m = Regex.Match(command, @"^(?:PRESS|SUBMIT)?\s*((?:[A-F]\s*)+)$");
        if (m.Success)
        {
            var submission = m.Groups[1].Value.Where(x => x != ' ');
            if (submission.Any(x => !otherChars.Contains(x) && x != bgChar))
            {
                yield return "sendtochaterror letter " + submission.First(x => !otherChars.Contains(x) && x != bgChar) + " cannot be found in the grid.";
                yield break;
            }
            yield return null;
            foreach (char letter in m.Groups[1].Value.Where(x => x != ' '))
            {
                buttons.First(x => x.displayedLetter == letter).selectable.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve () 
    {
        while (!moduleSolved)
        {
            while (animating)
                yield return true;
            buttons.First(x => x.displayedLetter == solution[submissionPointer]).selectable.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
