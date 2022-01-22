using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using System.Threading;

public class NotForgetMeNotScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable[] buttons;
    public KMSelectable zeroButton;
    public MeshRenderer[] leds;
    public TextMesh bottomDisplay, rightDisplay, submissionScreen;

    private readonly int[] table = new int[]
    {
        1, 7, 3, 2, 6, 5, 0, 4, 1, 8,
        0, 8, 1, 5, 2, 6, 2, 3, 4, 7,
        2, 1, 6, 3, 8, 4, 5, 7, 0, 4,
        7, 3, 4, 2, 4, 1, 8, 6, 8, 0,
        8, 5, 7, 4, 5, 0, 1, 3, 2, 6,
        4, 2, 5, 6, 1, 8, 5, 0, 7, 3,
        6, 0, 8, 7, 3, 2, 4, 1, 6, 5,
        3, 4, 0, 1, 3, 7, 0, 8, 5, 2,
        4, 5, 6, 0, 7, 8, 6, 2, 3, 1,
        5, 6, 2, 8, 0, 3, 7, 5, 1, 4,
    };
    private int selectedDigit;
    private int[] solution;
    private int[] givenPuzzle;
    private int[] currentPuzzle;
    private int[] rightNums;
    private int[] bottomNums = new int[9];

    private bool inputting;
    private bool regrab;
    private string inputtedCode = "";
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 9; i++)
        {
            int ix = i;
            buttons[ix].OnInteract += delegate () { ButtonPress(ix); return false; };
        }
        zeroButton.OnInteract += delegate () { ZeroPress(); return false; };

    }
    void Start()
    {
        GetSolution();
        GeneratePuzzle();
        GetPairs();
        SetSelected(0);
        DoLogging();
    }

    void ButtonPress(int pos)
    {
        buttons[pos].AddInteractionPunch(0.2f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[pos].transform);
        if (moduleSolved)
            return;
        int squarePos = Array.IndexOf(currentPuzzle, pos + 1);
        int zeroPos = Array.IndexOf(currentPuzzle, 0);
        if (!inputting)
            SetSelected(pos);
        else if (GetAdjacents(3, 3, squarePos).Contains(zeroPos) && pos != 8) //pressing 9 will always strike
        {
            currentPuzzle = Swap(currentPuzzle, squarePos, zeroPos);
            Debug.LogFormat("[Forget Me #{0}] Pressed {1}.", moduleId, pos + 1);
            inputtedCode += "12345678"[pos];
            SetInputScreen();
            regrab = false;
            leds[9].material.color = Color.black;
        }
        else
        {
            Debug.LogFormat("[Forget Me #{0}] Attempted to press {1}. Strike!", moduleId, pos + 1);
            Module.HandleStrike();
            regrab = true;
            leds[9].material.color = Color.green;
        }
    }
    void ZeroPress()
    {
        zeroButton.AddInteractionPunch(0.4f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, zeroButton.transform);
        if (moduleSolved)
            return;
        if (!inputting)
        {
            inputting = true;
            SetInputScreen();
        }
        else if (regrab)
            GoBack();
        else if (currentPuzzle.SequenceEqual(solution))
        {
            moduleSolved = true;
            Debug.LogFormat("[Forget Me #{0}] Pressed 0 with the grid {1}. This matches the solution, module solved!", moduleId, givenPuzzle.Select(x => x == 0 ? "-" : x.ToString()).Join());
            Module.HandlePass();
            Audio.PlaySoundAtTransform("wedidit", transform);
        }
        else
        {
            Debug.LogFormat("[Forget Me #{0}] Pressed 0 with the grid {1}. This is incorrect, strike!", moduleId, givenPuzzle.Select(x => x == 0 ? "-" : x.ToString()).Join());
            Audio.PlaySoundAtTransform("incorrect", transform);
            Module.HandleStrike();
            GoBack();
        }
    }

    void GetSolution()
    {
        bool[] visited = new bool[100];
        List<int> currentCells = new List<int>() { 10 * (Bomb.GetSerialNumber()[5] - '0') + (Bomb.GetSerialNumber()[2] - '0') };
        List<int> output = new List<int>();
        List<int> toBeCurrentCells = new List<int>();
        while (output.Count < 9)
        {
            foreach (int pos in currentCells)
            {
                if (!output.Contains(table[pos]))
                    output.Add(table[pos]);
                visited[pos] = true;
                toBeCurrentCells.AddRange(GetAdjacents(10, 10, pos));
            }
            currentCells = toBeCurrentCells.Where(x => !visited[x]).Distinct().OrderBy(x => x).ToList();
        }
        solution = output.ToArray();
    }
    void GoBack()
    {
        regrab = false;
        inputting = false;
        leds[9].material.color = Color.black;
        submissionScreen.gameObject.SetActive(false);
        bottomDisplay.gameObject.SetActive(true);
        inputtedCode = string.Empty;
        currentPuzzle = givenPuzzle.ToArray();
        SetSelected(0);
    }
    void GeneratePuzzle()
    {
        do
        {
            givenPuzzle = Enumerable.Range(0, 9).ToArray().Shuffle();
        } while (GetPermuations(givenPuzzle) % 2 != GetPermuations(solution) % 2);
        currentPuzzle = givenPuzzle.ToArray();
    }
    void GetPairs()
    {
        rightNums = Enumerable.Range(1, 99).ToArray().Shuffle().Take(9).ToArray();
        int[] sortedOrder = Enumerable.Range(0, 9).OrderBy(x => rightNums[x]).ToArray();
        for (int i = 0; i < 9; i++)
            bottomNums[sortedOrder[i]] = givenPuzzle[i];

    }
    void SetInputScreen()
    {
        leds[selectedDigit].material.color = Color.black;
        rightDisplay.text = "--";
        bottomDisplay.gameObject.SetActive(false);
        submissionScreen.gameObject.SetActive(true);
        submissionScreen.text = string.Empty;
        int numbersDisplayed = inputtedCode.Length < 24 ? inputtedCode.Length : (inputtedCode.Length % 12) + 12;
        string displayedsection = inputtedCode.TakeLast(numbersDisplayed).Join("").PadRight(24, '-');
        for (int i = 0; i < 24; i++)
        {
            submissionScreen.text += displayedsection[i];
            if (i % 3 == 2)
                submissionScreen.text += ' ';
            if (i == 11)
                submissionScreen.text += '\n';
        }
    }
    void DoLogging()
    {
        Debug.LogFormat("[Forget Me #{0}] Generated pairs are {1}.", moduleId, Enumerable.Range(0, 9).Select(ix => string.Format("[{0} {1}]", rightNums[ix], bottomNums[ix])).Join(", "));
        Debug.LogFormat("[Forget Me #{0}] Module generated with grid {1}.", moduleId, givenPuzzle.Select(x => x == 0 ? "-" : x.ToString()).Join());
        Debug.LogFormat("[Forget Me #{0}] The solution grid is {1}.", moduleId, solution.Select(x => x == 0 ? "-" : x.ToString()).Join());

    }
    void SetSelected(int sel)
    {
        leds[selectedDigit].material.color = Color.black;
        selectedDigit = sel;
        leds[selectedDigit].material.color = Color.green;
        rightDisplay.text = rightNums[selectedDigit].ToString().PadLeft(2, '0');
        bottomDisplay.text = bottomNums[selectedDigit].ToString();
    }

    IEnumerable<int> GetAdjacents(int width, int height, int pos)
    {
        if (pos > width - 1)
            yield return pos - width;
        if (pos % width != 0)
            yield return pos - 1;
        if (pos % width != width - 1)
            yield return pos + 1;
        if (pos < width * height - width)
            yield return pos + width;
    }
    int[] Swap(int[] array, int p1, int p2)
    {
        int temp = array[p1];
        array[p1] = array[p2];
        array[p2] = temp;
        return array;
    }
    int GetPermuations(int[] field)
    {
        int permutations = 0;
        for (int i = 0; i <= 8; i++)
            for (int j = 0; j < i; j++)
                if (field[i] != 0 && field[j] != 0 && (field[i] < field[j]))
                    permutations++;
        return permutations;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} cycle to cycle through all 9 pairs. Use !{0} submit 123456789 to press those number buttons.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.Trim().ToUpperInvariant();
        List<string> parameters = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (command == "CYCLE")
        {
            if (inputting)
                yield return "sendtochaterror You cannot cycle the numbers now.";
            else
            {
                yield return null;
                for (int i = 0; i < 9; i++)
                {
                    buttons[i].OnInteract();
                    yield return "trycancel";
                    yield return new WaitForSeconds(1.5f);
                }
            }
        }
        else if (parameters.Count == 2 && (parameters[0] == "PRESS" || parameters[0] == "SUBMIT") && parameters[1].All(x => "1234567890".Contains(x)))
        {
            yield return null;
            foreach (char num in parameters[1])
            {
                if (num == '0')
                    zeroButton.OnInteract();
                else buttons[num - '1'].OnInteract();
            }

        }

    }
    IEnumerator TwitchHandleForcedSolve()
    {
        if (!inputting)
        {
            zeroButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        List<int> path = null;
        List<Movement> allMoves = new List<Movement>();
        int[] startConfig = currentPuzzle.ToArray();
        var thread = new Thread(() =>
        {
            string stack = "";
            try
            {

                Queue<int[]> q = new Queue<int[]>();
                HashSet<string> visitedSets = new HashSet<string>();
                q.Enqueue(startConfig);
                int count = 0;
                while (q.Count > 0)
                {
                    int[] cur = q.Dequeue();
                    if (cur.SequenceEqual(solution))
                        break;
                    int zeroIx = Array.IndexOf(cur, 0);
                    foreach (int pressablePos in GetAdjacents(3, 3, zeroIx))
                    {
                        if ((pressablePos == 0 && (cur[0] == solution[0])) 
                            || ((pressablePos == 1 || pressablePos == 2) && cur[1] == solution[1] && cur[2] == solution[2]) 
                            || ((pressablePos == 3 || pressablePos == 6) && cur[3] == solution[3] && cur[6] == solution[6])) //shortcuts if certain tiles are already in place.
                            continue;
                        int[] newPos = Swap(cur.ToArray(), zeroIx, pressablePos);
                        if (visitedSets.Add(newPos.Join("")))
                        {
                            count++;
                            stack += newPos.Join("") + "\n";
                            if (count % 500 == 0)
                                Debug.Log(stack);
                            q.Enqueue(newPos);
                            allMoves.Add(new Movement(cur, newPos, cur[pressablePos]));
                        }
                    }
                }
                Debug.Log("Found path");
                Movement lastMove = allMoves.First(x => x.start.SequenceEqual(solution));
                List<int> pressPath = new List<int>() { lastMove.movedItem };
                while (!lastMove.start.SequenceEqual(startConfig))
                {
                    lastMove = allMoves.First(x => x.end.SequenceEqual(lastMove.start));
                    pressPath.Add(lastMove.movedItem);
                }
                pressPath.Reverse();
                path = pressPath;
            }
            catch (Exception e)
            {
                Debug.Log(stack);
                Debug.Log(e.Message);
                throw;
            }
        });
        thread.Start();
        while (path == null)
            yield return true;
        Debug.Log("Thread done!");
        foreach (int ix in path)
        {
            buttons[ix].OnInteract();
            yield return new WaitForSeconds(0.075f);
        }
    }
    struct Movement
    {
        public int[] start;
        public int[] end;
        public int movedItem;
        public Movement(int[] s, int[] e, int m)
        {
            start = s;
            end = e;
            movedItem = m;
        }
    }
}
