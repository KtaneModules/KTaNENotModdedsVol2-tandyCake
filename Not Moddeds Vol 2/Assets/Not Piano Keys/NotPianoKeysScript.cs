using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KModkit;
using NotPianoKeys;

public class NotPianoKeysScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMSelectable[] buttons;
    public TextMesh display;
    public Material WhiteKey;

    private PianoSymbol[] displayedSymbols = new PianoSymbol[3];
    private int[] blackSequence = new int[3];
    private int[] whiteSequence;
    private int[] combinedSequence = new int[12];
    private static readonly string[] keyNames = { "W1", "W2", "W3", "W4", "W5", "W6", "W7", "B1", "B2", "B3", "B4", "B5" };
    private static readonly string[] quadNames = { "top-left", "top-right", "bottom-right", "bottom-left" };

    int chosen3x3;
    int[] chosenQuad;
    int intersectionStart;
    List<string> swaps = new List<string>(4);

    static int moduleIdCounter = 1;
    int moduleId;
    bool moduleSolved;
    int submissionPointer;
    bool[] pressed = new bool[12];

    void Awake()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < 12; i++)
        {
            int ix = i;
            buttons[ix].OnInteract += delegate () { ButtonPress(ix); return false; };
        }
    }

    void Start()
    {
        GenerateSymbols();
        GenerateBlackSequence();
        GenerateWhiteSequence();
        OrderSequences();
        DoLogging();
    }

    void ButtonPress(int pos)
    {
        if (pressed[pos])
            return;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pos].transform);
        buttons[pos].AddInteractionPunch(0.5f);
        if (pos == combinedSequence[submissionPointer])
        {
            Debug.LogFormat("[Not Piano Keys #{0}] Pressed key {1}.", moduleId, keyNames[pos]);
            pressed[pos] = true;
            submissionPointer++;
            StartCoroutine(KeyMove(buttons[pos].transform));
            buttons[pos].GetComponentInChildren<KMHighlightable>().gameObject.SetActive(false);
            if (submissionPointer == 12)
            {
                moduleSolved = true;
                Debug.LogFormat("[Not Piano Keys #{0}] Module solved.", moduleId);
                Audio.PlaySoundAtTransform("orderedSolve", transform);
                StartCoroutine(KeysFade());
                StartCoroutine(DisplayZero());
            }
        }
        else
        {
            Debug.LogFormat("[Not Piano Keys #{0}] Pressed key {1} when key {2} was expected. Strike!.", moduleId, keyNames[pos], keyNames[combinedSequence[submissionPointer]]);
            Module.HandleStrike();
        }
    }

    void GenerateSymbols()
    {
        do
        {
            for (int i = 0; i < 3; i++)
                displayedSymbols[i] = PianoSymbol.allSymbols[i].PickRandom();

            blackSequence[0] = NPKData.AstroIndex(displayedSymbols[0], displayedSymbols[1]);
            blackSequence[1] = NPKData.AstroIndex(displayedSymbols[0], displayedSymbols[2]);
            blackSequence[2] = NPKData.AstroIndex(displayedSymbols[1], displayedSymbols[2]);

            chosenQuad = NPKData.quadrants[displayedSymbols[0].GetValue()];
            int col = displayedSymbols[1].GetValue();
            int row = displayedSymbols[2].GetValue();
            chosen3x3 = 3 * (row / 3) + (col / 3);
            intersectionStart = Array.IndexOf(chosenQuad, 3 * (row % 3) + (col % 3));
        } while (blackSequence.Distinct().Count() != 3 || intersectionStart == -1);
    }
    void GenerateBlackSequence()
    {
        List<int> remainder = Enumerable.Range(0, 5).Where(x => !blackSequence.Contains(x)).ToList();
        if (Bomb.GetSerialNumberNumbers().Any(x => x % 5 == 0))
            remainder.Reverse();
        blackSequence = blackSequence.Concat(remainder).ToArray();
    }
    void GenerateWhiteSequence()
    {
        for (int i = 0; i < 4; i++)
        {
            swaps.Add(NPKData.bigGrid[chosen3x3][chosenQuad[intersectionStart]]);
            intersectionStart = (intersectionStart + 1) % 4;
        }
        whiteSequence = Enumerable.Range(0, 7).ToArray();
        foreach (string swap in swaps)
            whiteSequence = NPKData.DoSwap(whiteSequence, swap);
    }
    void OrderSequences()
    {
        List<int> presses = whiteSequence.Concat(blackSequence.Select(x => x + 7)).ToList();
        int[] order = NPKData.orders[Bomb.GetSerialNumberNumbers().Last()];
        for (int i = 0; i < 12; i++)
            combinedSequence[i] = presses[order[i]];
    }

    void DoLogging()
    {
        display.text = displayedSymbols.Select(x => x.symbol).Join("   ");
        Debug.LogFormat("[Not Piano Keys #{0}] The displayed symbols are: {1}.", moduleId, displayedSymbols.Select(x => x.name.ToString().Replace('_', ' ')).Join(", "));
        Debug.LogFormat("[Not Piano Keys #{0}] The black sequence is {1}.", moduleId, blackSequence.Select(x => x + 1).Join());
        Debug.LogFormat("[Not Piano Keys #{0}] The used 3×3 is #{1} in reading order. Use the {2} quadrant.", moduleId, chosen3x3 + 1, quadNames[displayedSymbols[0].GetValue()]);
        Debug.LogFormat("[Not Piano Keys #{0}] Perform the swaps {1}.", moduleId, swaps.Join(", "));
        Debug.LogFormat("[Not Piano Keys #{0}] The white sequence is {1}.", moduleId, whiteSequence.Select(x => x + 1).Join());
        Debug.LogFormat("[Not Piano Keys #{0}] The resulting ordered sequence is {1}.", moduleId, combinedSequence.Select(x => keyNames[x]).Join());
    }

    IEnumerator KeyMove(Transform tf)
    {
        float delta = 0;
        while (delta < 1)
        {
            delta += 6 * Time.deltaTime;
            tf.localEulerAngles = new Vector3(Mathf.Lerp(0, 5, delta), 0, 0);
            yield return null;
        }
    }
    IEnumerator KeysFade()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForSeconds(0.8f);
            buttons[i + 7].GetComponent<MeshRenderer>().material = WhiteKey;
        }
        Module.HandlePass();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
    }
    IEnumerator DisplayZero()
    {
        yield return new WaitForSeconds(0.8f);
        StringBuilder newDisplay = new StringBuilder(display.text);
        for (int i = 0; i < 3; i++)
        {
            newDisplay[4 * i] = '0';
            display.text = newDisplay.ToString();
            yield return new WaitForSeconds(1.6f);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} press W1 B1 W3 K5 W7 to press those keys.";
#pragma warning restore 414
    IEnumerator Press(KMSelectable btn, float wait)
    {
        btn.OnInteract();
        yield return new WaitForSeconds(wait);
    }
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parameters.First() == "PRESS" && parameters.Skip(1).All(x => keyNames.Contains(x)))
        {
            yield return null;
            foreach (string key in parameters.Skip(1))
                yield return Press(buttons[Array.IndexOf(keyNames, key)], 0.1f);
            if (moduleSolved)
                yield return "solve";
        }
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = submissionPointer; i < 12; i++)
            yield return Press(buttons[combinedSequence[submissionPointer]], 0.1f);
    }
}
