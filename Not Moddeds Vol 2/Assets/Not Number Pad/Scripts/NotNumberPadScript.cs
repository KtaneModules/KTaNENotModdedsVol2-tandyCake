using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using NotNumberPad;
using Rnd = UnityEngine.Random;

public class NotNumberPadScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMBombModule Module;
    public Button[] numButtons;
    public Button clear, submit;
    public Material diffuseMat, unlitMat;
    public TextMesh displayMesh;

    private Coroutine[] buttonMovements = new Coroutine[12];

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    [HideInInspector]
    public bool initState = true;
    private int stage;
    private int displayVal;

    private List<Flash> flashes = new List<Flash>();
    private int submissionPointer;
    private List<int> answers = new List<int>();
    private List<int> submittedNumbers = new List<int>();
    private Coroutine flashAnim;
    private int[] chosenPriorityList;

    private int chosenCol;

    void Awake () {
        moduleId = moduleIdCounter++;
       
    }
    void InitializeButtons()
    {
        for (int i = 0; i < 10; i++)
        {
            int ix = i;
            numButtons[ix].selectable.OnInteract += delegate ()
            { ButtonPress(ix); return false; };
        }
        clear.selectable.OnInteract += delegate () { Clear(); return false; };
        submit.selectable.OnInteract += delegate () { Submit(); return false; };
    }

    void Start ()
    {
        InitializeButtons();
        SetColors();
    }
    void SetColors()
    {
        do
        {
            submit.color = (ButtonColor)Rnd.Range(0, 4);
            clear.color = (ButtonColor)Rnd.Range(0, 4);
        } while (submit.color == ButtonColor.Green && submit.color == ButtonColor.Red);
        submit.UpdateAppearance();
        clear.UpdateAppearance();
        for (int i = 0; i < 10; i++)
        {
            numButtons[i].color = (ButtonColor)Rnd.Range(0, 4);
            numButtons[i].UpdateAppearance();
            numButtons[i].value = i;
        }
        Log("The button colors are:");
        Log(numButtons[7].ToString() + numButtons[8].ToString() + numButtons[9].ToString());
        Log(numButtons[4].ToString() + numButtons[5].ToString() + numButtons[6].ToString());
        Log(numButtons[1].ToString() + numButtons[2].ToString() + numButtons[3].ToString());
        Log(" " + numButtons[0].ToString());

    }
    void ButtonPress(int ix)
    {
        if (initState || displayVal.ToString().Length == 4)
            return;
        if (flashAnim != null)
        {
            StopCoroutine(flashAnim);
            flashAnim = null;
            for (int i = 0; i < 10; i++)
                numButtons[i].SetState(false);
        }
        if (!moduleSolved)
        {
            displayVal *= 10;
            displayVal += ix;
            UpdateDisplay();
        }
    }
    void Clear()
    {
        displayVal = 0;
        if (flashAnim == null && !moduleSolved)
            flashAnim = StartCoroutine(FlashAnimations());
        for (int i = 0; i < 10; i++)
            numButtons[i].SetState(false);
        UpdateDisplay();
    }
    void Submit()
    {
        if (initState)
            GenerateStage();
        else if (moduleSolved)
            return;
        else if (answers[submissionPointer] == displayVal)
        {
            submissionPointer++;
            if (submissionPointer == flashes.Count)
            {
                stage++;
                if (stage == 3)
                {
                    moduleSolved = true;
                    Module.HandlePass();
                }
                else GenerateStage();
                submissionPointer = 0;
            }
            Log(string.Format("Submitted {0}, that is correct.", displayVal));
            Audio.PlaySoundAtTransform(new Flash(numButtons.Where(x => x.isLit)).ToString(), transform);
            foreach (Button button in numButtons.Where(x => x.isLit))
                button.SetState(false);
            displayVal = 0;
            UpdateDisplay();
        }
        else
        {
            Module.HandleStrike();
            Log(string.Format("Submitted {0} while expected {1}. Strike!", displayVal, answers[submissionPointer]));
        }
        
    }

    void GenerateStage()
    {
        initState = false;
        chosenPriorityList = Data.priorities[((int)submit.color + stage + 1) % 4];
        int count = Rnd.Range(1, 5);
        AddFlash:
        flashes.Add(new Flash(numButtons.Shuffle().Take(count)));
        if (flashes.Last().GetValue(chosenPriorityList) == 0)
        {
            flashes.RemoveAt(flashes.Count - 1);
            goto AddFlash;
        }
        Log(string.Format("The numbers {0} flash, corresponding to the colors {1}.", flashes.Last().Select(x => x.value).Join(), flashes.Select(x => x.ToString()).Join()));
        answers.Add(CalculateAnswer(flashes.Last(), stage));

        flashAnim = StartCoroutine(FlashAnimations());
    }
    int CalculateAnswer(Flash flash, int stage)
    {
        int val = flash.GetValue(chosenPriorityList);
        int initRow = (val - 1) % 9;
        int row = initRow;
        Log("The number obtained from the priority list is " + val);
        Log("Using starting row #" + (initRow + 1));
        int[] primes = { 2, 3, 5, 7 };
        Func<bool>[][] rules = new Func<bool>[][]
        {
            new Func<bool>[]
            {
                () => flash.OrderBy(x => x.value).First().color == ButtonColor.Red,
                () => flash.Sum(x => x.value) % 6 == 0,
                () => flash.Count(x => x.color == ButtonColor.Red) == 2,
                () => flash.OrderBy(x => x.value).First().color == ButtonColor.Green,
                () => flash.Any(x => x.color == submit.color),
                () => flash.OrderBy(x => x.value).First().color == ButtonColor.Blue,
                () => flash.Count() == 4,
                () => flash.Sum(x => x.value) % 5 == 0,
                () => flash.OrderBy(x => x.value).First().color == ButtonColor.Yellow,
            },
            new Func<bool>[]
            {
                () => flash.Select(x => x.color).Distinct().Count() == 3,
                () => flash.Count(x => x.value.EqualsAny(7,8,9)) <= 1 && flash.Count(x => x.value.EqualsAny(4,5,6)) <= 1 && flash.Count(x => x.value.EqualsAny(1,2,3)) <= 1,
                () => flash.Any(x => x.color == clear.color),
                () => flash.Select(x => x.value % 2).Distinct().Count() == 1,
                () => !flash.Sum(x => x.value).ToString().Select(x => x - '0').Any(x => flash.Select(y => y.value).Contains(x)),
                () => flash.Count(x => primes.Contains(x.value.Value)) >= 2,
                () => flash.Any(x => x.color == ButtonColor.Red) && flash.Any(x => x.color == ButtonColor.Green),
                () => numButtons.Count(x => x.color == ButtonColor.Red) >= 2 && flash.Any(x => x.color == ButtonColor.Red),
                () => flash.Count() == 2 || flash.Count() == 4,
            },
            new Func<bool>[]
            {
                () => answers.Join("").Select(x => x - '0').Any(x => flash.Select(y => y.value.Value).Contains(x)),
                () => Enumerable.Range(0,10).Any(x => flashes[0].Select(y => y.value.Value).Contains(x) && flashes[1].Select(y => y.value.Value).Contains(x) && flashes[2].Select(y => y.value.Value).Contains(x)),
                () => flash.Any(x => (int)x.color == ((int)submit.color + 1 + stage) % 4),
                () => flashes[0].Concat(flashes[1]).Concat(flashes[2]).Sum(x => x.value) % 4 == 0 || flashes[0].Concat(flashes[1]).Concat(flashes[2]).Sum(x => x.value) % 5 == 0,
                () => Enumerable.Range(0,10).Count(x => flashes[0].Select(y => y.value.Value).Contains(x) && flashes[2].Select(y => y.value.Value).Contains(x)) >= 2,
                () => flashes[1].Sum(x => x.value) % 2 == flashes[2].Sum(x => x.value) % 2,
                () => flashes[0].Concat(flashes[1]).Concat(flashes[2]).Count(x => x.color == ButtonColor.Yellow) <= 2,
                () => flashes.All(x => x.Select(y => y.color).Contains(ButtonColor.Green)),
                () => flashes[0].Concat(flashes[1]).Concat(flashes[2]).Count(x => x.color == ButtonColor.Red) >= 6,
            },
        };
        for (int i = 0; i < 9; i++)
        {
            if (rules[stage][row]())
                break;
            else row = (row + 1) % 9;
        }
        if (rules[stage].All(x => !x()))
        {
            row = initRow;
            Log("No statements are true. Using the starting row.");
        }
        else Log("The first true row is row #" + (row + 1));
        int multiplier = Data.multiplierTable[(int)clear.color, row];
        Log("Using multiplier " + multiplier);
        Log("The correct answer for this stage is " + (val * multiplier) % 10000);
        return (val * multiplier) % 10000;
    }

    IEnumerator FlashAnimations()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            foreach (Flash flash in flashes)
            {
                yield return new WaitForSeconds(0.5f);
                Audio.PlaySoundAtTransform(flash.ToString(), transform);
                foreach (Button button in flash)
                    button.SetState(true);
                yield return new WaitForSeconds(0.5f);
                foreach (Button button in flash)
                    button.SetState(false);
            }
            yield return null;
        }
    }
    void UpdateDisplay()
    {
        if (displayVal == 0)
            displayMesh.text = string.Empty;
        else displayMesh.text = displayVal.ToString();
    }
    void Log(string msg)
    {
        Debug.LogFormat("[Not Number Pad #{0}] {1}", moduleId, msg);
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use [!{0} start] to press submit. Use [!{0} submit 123 456 789] to submit those numbers into the module.";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand (string command) {
        command = command.Trim().ToUpperInvariant();
        if (command == "START")
        {
            yield return null;
            submit.selectable.OnInteract();
            yield return new WaitForSeconds(0.1f);
            yield break;
        }
        Match m = Regex.Match(command, @"^SUBMIT\s+([0-9\s]+)$");
        if (m.Success)
        {
            string[] submission = m.Groups[1].Value.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            yield return null;
            foreach (string num in submission)
            {
                if (num.StartsWith(displayMesh.text) && displayMesh.text.Length != 0)
                {
                    clear.selectable.OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                foreach (int digit in num.Select(x => x - '0').Skip(displayMesh.text.Length))
                {
                    numButtons.First(x => x.value == digit).selectable.OnInteract();
                    yield return new WaitForSeconds(0.25f);
                }
                yield return new WaitForSeconds(0.1f);
                submit.selectable.OnInteract();
                yield return new WaitForSeconds(0.75f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve () 
    {
        if (initState)
        {
            submit.selectable.OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
        while (!moduleSolved)
        {
            if (answers[submissionPointer].ToString().StartsWith(displayMesh.text) && displayMesh.text.Length != 0)
            {
                clear.selectable.OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
            foreach (int digit in answers[submissionPointer].ToString().Select(x => x - '0'))
            {
                numButtons.First(x => x.value == digit).selectable.OnInteract();
                yield return new WaitForSeconds(0.25f);
            }
            yield return new WaitForSeconds(0.1f);
            submit.selectable.OnInteract();
            yield return new WaitForSeconds(0.75f);
        }
    }
}
