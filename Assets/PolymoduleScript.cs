using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class PolymoduleScript : MonoBehaviour {
    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    public KMSelectable[] Buttons;

    int[] InitialValues = new int[6];
    int[] FinalValues = new int[6];
    int[] SecondToLastValues = new int[6];

    int InputNumber = 0;

    string SERIALNUMBER;
    readonly string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly int DAY = (int)System.DateTime.Now.DayOfWeek + 1;
    readonly int DATE = System.DateTime.Now.Day;
    readonly int MONTH = System.DateTime.Now.Month;
    readonly int YEAR = System.DateTime.Now.Year;

    int[] RULES = new int[18];
    int RULENUMBER;
    int INPUTMETHOD;
    int[] InputArray;

    static ulong LCG(ulong a, ulong c, ulong m, ulong x)
    {
        return (a * x + c) % m;
    }

  

    void Awake () {
        RULENUMBER = 15 + (DAY % 3) - (DATE % 3);
       
        INPUTMETHOD = (YEAR + DATE + DAY + MONTH) % 3;
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        //Rule generation
        var Seed = (ulong)(YEAR * 10000 + MONTH * 100 + DAY);
        ulong a = 1664525, c = 1013904223, m = (ulong)Math.Pow(2, 32);

        for(int i = 0; i < RULES.Length; i++)
        {
            Seed = LCG(a, c, m, Seed);
            RULES[i] = (int)(Seed % 16);
        }
        //Make sure the first rules aren't redundant
        if (RULES[0] == 5||RULES[0]==6) { RULES[0] = 8;}
        //Make sure rule 5 doesn't appear twice in a row
        bool WasLastValue5 = false;

        for (int i = 0; i < RULES.Length; i++) {
            if (WasLastValue5 && RULES[i] == 5) { RULES[i] = 6;WasLastValue5 = false;continue; }

            if (RULES[i] == 5) { WasLastValue5 = true; }
            else { WasLastValue5 = false; }
        }

        foreach (KMSelectable Button in Buttons)
        {
            Button.OnInteract += delegate () { ButtonPress(Button); return false; };
        }     
    }

    void ButtonPress(KMSelectable Button){
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Button.AddInteractionPunch(1);
        
        if (InputNumber==0){
            Debug.LogFormat("[Polymodule] Pressed button at minute {0}", Mathf.FloorToInt(Bomb.GetTime() / 60f));
            UpdateFinalValues();}
        InputArray = (int[])FinalValues.Clone();
        Array.Sort(InputArray);

       

        switch (INPUTMETHOD) {
            case 0:
                if (FinalValues[Button.GetComponent<NumberScript>().ButtonIndex] == InputArray[5]) { Solve(); }
                else {Strike();}
                        
                break;
            case 1:
                if (FinalValues[Button.GetComponent<NumberScript>().ButtonIndex] == InputArray[InputNumber])
                {
                    InputNumber++;
                    if (InputNumber == 6) { Solve(); }
                }
                else { InputNumber = 0;Strike();}
                break;
            case 2:
                if (FinalValues[Button.GetComponent<NumberScript>().ButtonIndex] == InputArray[5] && FinalValues[Button.GetComponent<NumberScript>().ButtonIndex] % 10 == Math.Floor(Bomb.GetTime() % 10)) { Solve(); }
                else { Strike(); }
                break;
        }
    }

    void Activate() {
    }

    void  ValueChanger(int n) {
        switch (n)
        {
            case 0:
                for (int i = 0; i < FinalValues.Length; i++) {
                    if (char.IsDigit(SERIALNUMBER[FinalValues[i] % 6])) { FinalValues[i] += int.Parse(SERIALNUMBER[FinalValues[i] % 6].ToString()); }
                    else { FinalValues[i] += 1+ALPHABET.IndexOf(SERIALNUMBER[FinalValues[i] % 6]); }
                }
                break;
            case 1:
                for (int i = 0; i < FinalValues.Length; i++)
                {
                    if (FinalValues[i] < 2) { FinalValues[i] = 0;continue; }
                    List<int> Factors = PrimeFactors(FinalValues[i]);
                    int Sum = 0;
                    foreach (int h in Factors) {
                        Sum += h;
                    }
                    FinalValues[i] = Sum;
                }
                break;
            case 2:
                int AvarageSum = 0;
                foreach (int h in FinalValues) {
                    if (h == FinalValues.Max()) { continue; }
                    AvarageSum += h;
                }
                FinalValues[Array.IndexOf(FinalValues, FinalValues.Max())] = Mathf.FloorToInt(AvarageSum/5f);
                break;
            case 3:
                int SmallestNumber = FinalValues.Min();
                int SmallestIndex = Array.IndexOf(FinalValues,SmallestNumber);
                int BiggestNumber = FinalValues.Max();
                int BiggestIndex = Array.IndexOf(FinalValues, BiggestNumber);
                FinalValues[SmallestIndex] = BiggestNumber;
                FinalValues[BiggestIndex] = SmallestNumber;
                break;
            case 4:
                for (int i = 0; i < FinalValues.Length; i++)
                {
                    FinalValues[i] += InitialValues[i];
                }
                break;
            case 5:
                int MaxIndex = Array.IndexOf(FinalValues, FinalValues.Max());
                FinalValues[MaxIndex] = SecondToLastValues[MaxIndex];
                break;
            case 6:
                int TempInt = Array.IndexOf(FinalValues, FinalValues.Max());
                FinalValues[TempInt] = InitialValues[TempInt];
                break;
            case 7:
                for (int i = 0; i < FinalValues.Length; i++) {
                    FinalValues[i] = Mathf.FloorToInt((float)Math.Sqrt(FinalValues[i]));                   
                }
                break;
            case 8:
                for (int i = 0; i < FinalValues.Length; i++) {
                    if (FinalValues[i] % 2 == 0) { FinalValues[i] /= 2;continue;}
                    FinalValues[i] *= 3;
                    FinalValues[i] += 1;
                }

                break;
            case 9:
                for(int i = 0; i < FinalValues.Length; i++) {
                    FinalValues[i] = FinalValues[i] * FinalValues[i];
                }
                break;
            case 10:
                for (int i = 0; i < FinalValues.Length; i++) {
                    FinalValues[i] += Bomb.GetSolvedModuleNames().Count;
                } 
                break;
            case 11:
                for (int i = 0; i < FinalValues.Length; i++) {
                    if (FinalValues[i] <= Bomb.GetSolvedModuleNames().Count) { FinalValues[i] += 20; }
                }
                break;
            case 12:
                for (int i = 0; i < FinalValues.Length; i++) {
                    if (FinalValues[i] < 10) { FinalValues[i] += 20; }
                }
                break;
            case 13:
                for (int i = 0; i < FinalValues.Length; i++) {
                    if (FinalValues.Min() != 0 && FinalValues[i] != FinalValues.Min()&&FinalValues[i]%FinalValues.Min()==0) { FinalValues[i] *= 2; }
                }
                break;
            case 14:
                FinalValues[Array.IndexOf(FinalValues, FinalValues.Max())] += Mathf.FloorToInt(Bomb.GetTime() / 60f);
                FinalValues[Array.IndexOf(FinalValues, FinalValues.Min())] += Mathf.FloorToInt(Bomb.GetTime() / 60f);
                break;
            case 15:
                int TempSum = 0;
                foreach (int i in FinalValues) {
                    TempSum += i;
                }
                if (Mathf.FloorToInt(Bomb.GetTime() / 60f) < TempSum) { FinalValues[Array.IndexOf(FinalValues, FinalValues.Min())] += 10; }
                else { FinalValues[Array.IndexOf(FinalValues, FinalValues.Min())] += 30; }
                break;
        }
    }

    void ResolveDuplicate(int Duplicate) {
        List<int> DuplicateIndices = new List<int>();

        for (int i = 0; i < FinalValues.Length; i++)
        {
            if (FinalValues[i]==Duplicate)
            { DuplicateIndices.Add(i);
            };
        }
        int HighestValue = -1;
        int HighestIndex = -1;
        foreach (int i in DuplicateIndices) {
            if (InitialValues[i] > HighestValue) { HighestValue = InitialValues[i]; HighestIndex = i; }
        }
        FinalValues[HighestIndex] += 1;
    }

    bool CheckForDuplicates() {
        int[] CheckingArray = new int[6];

        for (int i = 0; i < CheckingArray.Length; i++)
        {
            CheckingArray[i] = -1;
        }
        

        int Duplicate = -1;
        for (int i = 0; i < FinalValues.Length; i++)
        {
            if (CheckingArray.Contains(FinalValues[i]))
            {
                //Debug.Log("Duplicate found at index " + i);
                ResolveDuplicate(FinalValues[i]);
                return false;
            };
            CheckingArray[i] = FinalValues[i];
        }

        return true;
    }



    void Start () {
        SERIALNUMBER = Bomb.GetSerialNumber();

        string TempString = "";
        for (int i = 0; i < RULES.Length; i++) {
            TempString += RULES[i]+" ";
        }


        //Debug.Log("Rules are" + TempString);
        //Debug.Log("Day is " + DAY);
        //Debug.Log("Date is " + DATE);
        //Debug.Log("Month is " + MONTH);
        //Debug.Log("Year is " + YEAR);

        for (int i = 0; i < InitialValues.Length; i++)
        {
            InitialValues[i] = -1;
        }

        for (int i = 0; i < InitialValues.Length; i++)
        {
            int x = UnityEngine.Random.Range(0, 10);
            if (InitialValues.Contains<int>(x) == true) { i--; continue; }
            InitialValues[i] = x;
            Buttons[i].GetComponentInChildren<TextMesh>().text = InitialValues[i].ToString();
            FinalValues[i] = x;
        }

        for (int i = 0; i < InitialValues.Length; i++)
        {

            //Debug.Log("Initial value " + i + " is " + InitialValues[i]);
        }
        //UpdateFinalValues();

        /*for (int i = 0; i < InitialValues.Length; i++)
        {
            Debug.Log("Final value " + i + " is " + FinalValues[i]);
        }*/
    }

    void UpdateFinalValues() {

        FinalValues = (int[])InitialValues.Clone();

        for (int i = 0; i < RULENUMBER; i++)
        {
            if (RULES[i] != 5) { SecondToLastValues = (int[])FinalValues.Clone(); }
            ValueChanger(RULES[i]);
            while (true) {
                if (CheckForDuplicates() == true) { break; }
            }

            string TempString = "";
            foreach (int n in FinalValues) {
                TempString += n+" ";
            }
            Debug.LogFormat("[Polymodule] Changed the array into {0}",TempString);
        }
    }

    void Solve()
    {
        Debug.LogFormat("[Polymodule] Module solved");
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {

        string TempString = "";
        foreach (int n in FinalValues) {
            TempString += n + " ";
        }
        //Debug.Log("The last digit of the timer is "+ Math.Floor(Bomb.GetTime()%10));
        //Debug.Log("The current minute is " + Mathf.FloorToInt(Bomb.GetTime() / 60f));
        Debug.LogFormat("[Polymodule] Striked, expected values were {0}",TempString);
        GetComponent<KMBombModule>().HandleStrike();
    }

    static List<int> PrimeFactors(int number)
    {
        var factors = new List<int>();

        while (number % 2 == 0)
        {
            factors.Add(2);
            number /= 2;
        }

        for (int i = 3; i <= Math.Sqrt(number); i += 2)
        {
            while (number % i == 0)
            {
                factors.Add(i);
                number /= i;
            }
        }

        if (number > 2)
            factors.Add(number);

        return factors;
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use{0} Chain commands via spaces.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string Command) {
        Command = Command.Trim();
        yield return null;
        string[] Commands = Command.Split(' ');
        for (int i = 0; i < Commands.Length; i++) {

            if (Commands[i].Length != 1 || !char.IsDigit(Commands[i][0])) {
                yield return "sendtochaterror Incorrect syntax.";
                yield break;
            }

        }

        
        for (int i = 0; i < Commands.Length; i++)
        {
            if (Array.IndexOf(InitialValues,Int32.Parse(Commands[i][0].ToString()))==-1)
            {
                yield return "sendtochaterror Value not found.";
                yield break;
            }

        }


        for (int i = 0; i < Commands.Length; i++) {
            Buttons[Array.IndexOf(InitialValues, Int32.Parse(Commands[i][0].ToString()))].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

    }

    IEnumerator TwitchHandleForcedSolve() {
        if (INPUTMETHOD != 1)
        {
            Buttons[Array.IndexOf(FinalValues, FinalValues.Max())].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else {
            int[] TempArray = (int[])FinalValues.Clone();
            Array.Sort(TempArray);
            foreach (int n in TempArray) {
                Buttons[Array.IndexOf(FinalValues,n)].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

}
