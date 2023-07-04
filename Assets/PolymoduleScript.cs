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

    int[] InitialValues = new int[5];
    int[] FinalValues = new int[5];
    int[] SecondToLastValues = new int[5];



    string SERIALNUMBER;
    readonly string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    readonly int DAY = (int)System.DateTime.Now.DayOfWeek + 1;
    readonly int DATE = System.DateTime.Now.Day;
    readonly int MONTH = System.DateTime.Now.Month;
    readonly int YEAR = System.DateTime.Now.Year;

    List<int> RULES = new List<int>();
    readonly int MODULUS = 11;
    int RULENUMBER;

    void Awake () {
        RULENUMBER = 10 + (DAY % 3) - (DATE % 3);
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        SERIALNUMBER = Bomb.GetSerialNumber();

        foreach (KMSelectable Button in Buttons)
        {

            Button.OnInteract += delegate () { ButtonPress(Button); return false; };
        }

        RULES.Add((DAY * (YEAR - DATE)) % MODULUS);
        RULES.Add((MONTH * (YEAR - DAY)) % MODULUS);
        RULES.Add((DAY * 3 * YEAR) % MODULUS);
        RULES.Add((YEAR + MONTH + 8 - DAY) % MODULUS);
        RULES.Add(DAY);
        RULES.Add((MONTH + DATE) % MODULUS);
        RULES.Add((YEAR * MONTH * DAY) % MODULUS);
        RULES.Add((MONTH + DAY + YEAR) % MODULUS);
        RULES.Add((DAY * 9 * DATE) % MODULUS);
        RULES.Add(((MONTH + YEAR) * DATE) % MODULUS);
        RULES.Add((DAY * DAY * (DAY + MONTH)) % MODULUS);
        RULES.Add((MONTH + (MONTH * DAY) + (MONTH * YEAR)) % MODULUS);
    }
    void ButtonPress(KMSelectable Button){
        Debug.Log("Test");
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
                    if (h == InitialValues.Max()) { continue; }
                    AvarageSum += h;
                }
                FinalValues[Array.IndexOf(FinalValues, FinalValues.Max())] = Mathf.FloorToInt(AvarageSum/4f);
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
            //TODO make sure this doesn't accour twice in a row
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
        }






    }


    void Start () {
        string TempString = "";
        for (int i = 0; i < RULES.Count; i++) {
            TempString += RULES[i]+" ";
        }
        Debug.Log("Rules are" + TempString);

        Debug.Log("Day is " + DAY);
        Debug.Log("Date is " + DATE);
        Debug.Log("Month is " + MONTH);
        Debug.Log("Year is " + YEAR);
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

            Debug.Log("Initial value " + i + " is " + InitialValues[i]);
        }


        for (int i = 0; i < RULENUMBER; i++) {
            if(RULES[i]!=5) { SecondToLastValues = (int[])FinalValues.Clone(); }
            ValueChanger(RULES[i]);
        }

        

        for (int i = 0; i < InitialValues.Length; i++)
        {

            Debug.Log("Final value " + i + " is " + FinalValues[i]);
        }



    }

    int[] UpdateFinalValues() {

        FinalValues = (int[])InitialValues.Clone();

        for (int i = 0; i < RULENUMBER; i++)
        {
            if (RULES[i] != 5) { SecondToLastValues = (int[])FinalValues.Clone(); }
            ValueChanger(RULES[i]);

        }


        return FinalValues;
    }

    void CheckForSameInstances() {



    }

    void Update()
    {

    }

    void Solve()
    {
        GetComponent<KMBombModule>().HandlePass();
    }

    void Strike()
    {
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
}
