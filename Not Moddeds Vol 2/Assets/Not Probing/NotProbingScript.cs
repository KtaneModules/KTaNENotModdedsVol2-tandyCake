using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class NotProbingScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public KMSelectable[] wireSelectables;
    public GameObject[] blueClips;
    public GameObject[] redClips;
    public TextMesh screen;

    readonly string[] wordList = new string[] { "ARCHER", "ATTACK", "BANANA", "BLASTS", "BURSTS", "BUTTON", "CANNON", "CASING", "CHARGE", "DAMAGE", "DEFUSE", "DEVICE", "DISARM", "FLAMES", "KABOOM", "KEVLAR", "KEYPAD", "LETTER", "MODULE", "MORTAR", "NAPALM", "OTTAWA", "PERSON", "ROBOTS", "ROCKET", "SAPPER", "SEMTEX", "WEAPON", "WIDGET", "WIRING" };
    private bool bluePlaced;
    private int[] clipPlacements = {-1, -1};
    private string[] chosenWords;
    private string[] currentWords = new string[6];
    private WireInfo[] wires = new WireInfo[6];
    private int[] correctPairing = new int[2];
    private int[] associatedWires;
    private Coroutine submitDelay;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 6; i++)
        {
            int ix = i;
            wireSelectables[ix].OnInteract += delegate () { Connect(ix); return false; };
            blueClips[ix].SetActive(false);
            redClips[ix].SetActive(false);
        }
        screen.text = string.Empty;
    }

    void Start()
    {
        GetWires();
        CalculateAnswer();
        DoLogging();
    }
    void DisableClips()
    {
        screen.text = "";
        for (int ix = 0; ix < 6; ix++)
        {
            blueClips[ix].SetActive(false);
            redClips[ix].SetActive(false);
        }
        if (submitDelay != null)
            StopCoroutine(submitDelay);
    }

    void Connect(int pos)
    {
        if (clipPlacements.Contains(pos))
        {
            DisableClips();
            clipPlacements = new int[]{ -1, -1};
        }
        else if (clipPlacements[0] == -1)
        {
            clipPlacements[0] = pos;
            blueClips[pos].SetActive(true);
        }
        else if (clipPlacements[1] == -1)
        {
            clipPlacements[1] = pos;
            redClips[pos].SetActive(true);
        }
        else
        {
            DisableClips();
            blueClips[pos].SetActive(true);
            clipPlacements = new int[] { pos, -1 };
        }
        if (clipPlacements[0] != -1 && clipPlacements[1] != -1)
            ExecuteConnection();
    }
    void ExecuteConnection()
    {
        currentWords[clipPlacements[0]] = Swap(wires[clipPlacements[0]], wires[clipPlacements[1]]);
        wires[clipPlacements[0]].word = currentWords[clipPlacements[0]];
        screen.text = currentWords[clipPlacements[0]];
        submitDelay = StartCoroutine(CheckSubmit());
    }


    void GetWires()
    {
        chosenWords = wordList.ToArray().Shuffle().Take(6).ToArray();
        do
        {
            for (int i = 0; i < 6; i++)
                wires[i] = new WireInfo(chosenWords[i], Enumerable.Range(0, 6).ToArray().Shuffle().Take(2).ToArray());
        } while (wires.Select(x => x.AssociatedWire).Distinct().Count() != 5);
        for (int i = 0; i < 200; i++)
        {
            int[] nums = Enumerable.Range(0, 6).ToArray().Shuffle();
            currentWords[nums[0]] = Swap(wires[nums[0]], wires[nums[1]]);
            wires[nums[0]].word = currentWords[nums[0]];
        }
    }
    void CalculateAnswer()
    {
        associatedWires = wires.Select(x => x.AssociatedWire).ToArray();
        if (Bomb.GetSerialNumberNumbers().First() % 2 == Bomb.GetSerialNumberNumbers().Last() % 2)
            correctPairing[0] = Enumerable.Range(0, 6).First(x => associatedWires.Count(y => y == x) == 2);
        else correctPairing[0] = Enumerable.Range(0, 6).First(x => !associatedWires.Contains(x));
        int pointer = Bomb.GetSerialNumberNumbers().Sum() % 30;
        Debug.Log(pointer);
        while (!chosenWords.Contains(wordList[pointer]) || Array.IndexOf(chosenWords, wordList[pointer]) == correctPairing[0])
            pointer = (pointer + 1) % 30;
        correctPairing[1] = Array.IndexOf(chosenWords, wordList[pointer]);
    }
    void DoLogging()
    {
        Debug.LogFormat("[Not Probing #{0}] The displayed unscrambled words are {1}.", moduleId, chosenWords.Join(", "));
        Debug.LogFormat("[Not Probing #{0}] The displayed words are scrambled to {1}.", moduleId, currentWords.Join(", "));
        Debug.LogFormat("[Not Probing #{0}] The swap rules of the given wires are {1}.", moduleId, wires.Select(x => (x.swap1 + 1) + "/" + (x.swap2 + 1)).Join(", "));
        Debug.LogFormat("[Not Probing #{0}] The wires associated with each wire are {1}.", moduleId, associatedWires.Select(x => x + 1).Join());
        Debug.LogFormat("[Not Probing #{0}] The correct word from the table is {1}, which corresponds to wire {2}.", moduleId, chosenWords[correctPairing[1]], correctPairing[1] + 1);
        Debug.LogFormat("[Not Probing #{0}] The blue wire should be attached to wire {1}", moduleId, correctPairing[0] + 1);
        Debug.LogFormat("[Not Probing #{0}] The red wire should be attached to wire {1}", moduleId, correctPairing[1] + 1);
    }

    IEnumerator CheckSubmit()
    {
        if (moduleSolved)
            yield break;
        float delta = 0;
        while (delta < 6)
        {
            delta += Time.deltaTime;
            yield return null;
        }
        if (clipPlacements.SequenceEqual(correctPairing) && wordList.Contains(currentWords[clipPlacements[0]]) && wordList.Contains(currentWords[clipPlacements[1]]))
        {
            Debug.LogFormat("[Not Probing #{0}] Submitted with the blue wire connected to wire {1} and the red wire connected to wire {2}. The corresponding words are {3} and {4}. This is correct, module solved.",
                moduleId, clipPlacements[0] + 1, clipPlacements[1] + 1, currentWords[clipPlacements[0]], currentWords[clipPlacements[1]]);
            moduleSolved = true;
            Module.HandlePass();
        }
        else
        {
            Debug.LogFormat("[Not Probing #{0}] Submitted with the blue wire connected to wire {1} and the red wire connected to wire {2}. The corresponding words are {3} and {4}. This is incorrect, strike!.",
                moduleId, clipPlacements[0] + 1, clipPlacements[1] + 1, currentWords[clipPlacements[0]], currentWords[clipPlacements[1]]);
            Module.HandleStrike();
        }
    }
    string Swap(WireInfo word, WireInfo swap)
    {
        char[] wordArray = word.word.ToCharArray();
        char temp = wordArray[swap.swap1];
        wordArray[swap.swap1] = wordArray[swap.swap2];
        wordArray[swap.swap2] = temp;
        return new string(wordArray);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} connect 1 4 6 5 to connect the clips to those wires. Use !{0} cycle 1 3 4 5 to connect those wires and pause after every connection.";
#pragma warning restore 414
    IEnumerator Press(KMSelectable btn, float delay)
    {
        btn.OnInteract();
        Debug.LogFormat("PRESS");
        yield return new WaitForSeconds(delay);
    }
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (Regex.IsMatch(command, @"^CONNECT(\s+[1-6])+$"))
        {
            yield return null;
            foreach (string wire in parameters.Skip(1))
            {
                if (!clipPlacements.Any(x => x == -1))
                    yield return Press(wireSelectables[clipPlacements.PickRandom()], 0);
                yield return Press(wireSelectables[wire[0] - '1'], 0.25f);
            }
        }
        else
        {
            Match m = Regex.Match(command, @"^CYCLE\s*((?:[1-6]\s*)+)$");
            if (m.Success)
            {
                int[] allNums = m.Groups[1].Value.Where(x => !char.IsWhiteSpace(x)).Select(ch => ch - '1').ToArray();
                if (allNums.Length % 2 != 0)
                {
                    yield return "sendtochaterror Cycle cannot be performed with an odd number of arguments.";
                    yield break;
                }
                int[][] allPairs =
                    Enumerable.Range(0, allNums.Length / 2)
                    .Select(pos => allNums
                        .Skip(2 * pos)
                        .Take(2)
                        .ToArray()).ToArray();
                if (allPairs.Any(p => p[0] == p[1]))
                    yield return "sendtochaterror You cannot cycle with two of the same wire.";
                else
                {
                    yield return null;
                    foreach (int[] pair in allPairs)
                    {
                        if (!clipPlacements.Any(x => x == -1))
                            yield return Press(wireSelectables[clipPlacements.PickRandom()], 0);
                        yield return Press(wireSelectables[pair[0]], 0.1f);
                        yield return "trycancel";
                        yield return Press(wireSelectables[pair[1]], 1.5f);
                    }
                    if (!clipPlacements.Any(x => x == -1))
                        yield return Press(wireSelectables[clipPlacements.PickRandom()], 0);
                }
            }
        }
    }
}
