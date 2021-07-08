using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private readonly string[] keyNames = { "W1", "W2", "W3", "W4", "W5", "W6", "W7", "B1", "B2", "B3", "B4", "B5" };

    static int moduleIdCounter = 1;
    int moduleId;
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

    void GenerateBlackSequence()
    {
        do
        {
            for (int i = 0; i < 3; i++)
                displayedSymbols[i] = PianoSymbol.allSymbols[i].PickRandom();
            blackSequence[0] = NPKTables.Index(displayedSymbols[0], displayedSymbols[1]);
            blackSequence[1] = NPKTables.Index(displayedSymbols[0], displayedSymbols[2]);
            blackSequence[2] = NPKTables.Index(displayedSymbols[1], displayedSymbols[2]);
        } while (blackSequence.Distinct().Count() != 3);
        var remainder = Enumerable.Range(0, 5).Where(x => !blackSequence.Contains(x));
        if (Bomb.GetSerialNumberNumbers().Any(x => x % 5 == 0))
            remainder = remainder.Reverse();
        blackSequence = blackSequence.Concat(remainder).ToArray();
    }
    void GenerateWhiteSequence()
    {
        whiteSequence = Enumerable.Range(0, 7).ToArray();
        for (int i = 0; i < 6; i++)
        {
            whiteSequence = Swap.PerformSwap(whiteSequence, NPKTables.Swaps[Bomb.GetSerialNumber()[i]]);
            Debug.LogFormat("[Not Piano Keys #{0}] Performing the swap corresponding to {1} results in the white sequence {2}.", moduleId, Bomb.GetSerialNumber()[i], whiteSequence.Select(x => x + 1).Join());
        }
    }
    void OrderSequences()
    {
        List<int> presses = whiteSequence.Concat(blackSequence.Select(x => x + 7)).ToList();
        int[] order = NPKTables.orders[Bomb.GetSerialNumberNumbers().Last()];
        for (int i = 0; i < 12; i++)
            combinedSequence[i] = presses[order[i]];
    }
    
    void DoLogging()
    {
        display.text = displayedSymbols.Select(x => x.symbol).Join("   ");
        Debug.LogFormat("[Not Piano Keys #{0}] The displayed symbols are: {1}.", moduleId, displayedSymbols.Select(x => x.name.ToString().Replace('_', ' ')).Join(", "));
        Debug.LogFormat("[Not Piano Keys #{0}] The black sequence is {1}.", moduleId, blackSequence.Select(x => x + 1).Join());
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
        char[] newDisplay = display.text.ToCharArray();
        for (int i = 0; i < 3; i++)
        {
            newDisplay[4 * i] = '0';
            display.text = newDisplay.Join("");
            yield return new WaitForSeconds(1.6f);
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} press W1 B1 W3 K5 W7 to press those keys.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (parameters.First() == "PRESS" && parameters.Skip(1).All(x => keyNames.Contains(x)))
            foreach (string key in parameters.Skip(1))
            {
                buttons[Array.IndexOf(keyNames, key)].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = submissionPointer; i < 12; i++)
        {
            buttons[combinedSequence[submissionPointer]].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
    }
}
