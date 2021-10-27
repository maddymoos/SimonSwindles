using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using System.Text;

public class Swindlem : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public Light[] lights;
    public Light blacklight;
    public KMSelectable[] buttons;
    public KMSelectable MuteSel;
    public Transform UWU;
    private string[] color = { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" };
    private string[] xorn = { "Black", "Red", "Green", "Yellow", "Blue", "Magenta", "Cyan", "White" };
    private string color2 = "KRGBCMYW";
    private string xorcolor = "KRGYBMCW";
    private string answer, constnt, input, output;
    private bool inputting, solved, tpsolved;
    private int x, count;
    private bool _isMuted;
    // Use this for initialization
    void Start()
    {
        constnt = "";
        answer = "";
        for (int i = 0; i < 6; i++)
        {
            answer += color2[Rnd.Range(0, 8)];
            constnt += color2[Rnd.Range(0, 8)];
            inputting = false;

        }
        output = "";
        input = "";
        count = 0;
        Debug.LogFormat("[Simon Swindles #{0}]: GENERATION PHASE: Intial answer is {1} and the constant is {2}", _moduleId, answer, constnt);
    }
    static private int _moduleIdCounter = 1;
    private int _moduleId;
    void Awake()
    {
        _moduleId = _moduleIdCounter++;
        for (byte i = 0; i < buttons.Length; i++)
        {
            KMSelectable btn = buttons[i];
            btn.OnInteract += delegate
            {
                HandlePress(btn);
                return false;
            };
        }
        MuteSel.OnInteract += delegate ()
        {
            _isMuted = !_isMuted;
            return false;
        };
    }
    void HandlePress(KMSelectable btn)
    {
        if (!solved)
        {
            int aly = Array.IndexOf(buttons, btn);
            buttons[aly].AddInteractionPunch();
            if (input.Length != 6)
            {
                if (!inputting)
                {
                    StopAllCoroutines();
                    blacklight.enabled = false;
                    lights[0].enabled = false;
                    lights[1].enabled = false;
                    lights[2].enabled = false;
                    StartCoroutine(stupid());
                    inputting = true;
                }
                if (aly != 3)
                {
                    lights[aly].enabled = !lights[aly].enabled;
                }
                else
                {
                    blacklight.enabled = false;
                    lights[0].enabled = false;
                    lights[1].enabled = false;
                    lights[2].enabled = false;
                }
                switch (aly)
                {
                    case 0: if (x % 2 == 0) x = (x + 1) % 8; else x = (x - 1) % 8; break;
                    case 1: if (x % 4 == 0 || x % 4 == 1) x = (x + 2) % 8; else x = (x - 2) % 8; break;
                    case 2: if (x % 8 == 0 || x % 8 == 1 || x % 8 == 2 || x % 8 == 3) x = (x + 4) % 8; else x = (x - 4) % 8; break;
                    case 3:
                        input += xorcolor[x];
                        if (!_isMuted)
                            Audio.PlaySoundAtTransform(xorn[x], UWU.transform);
                        x = 0;
                        break;
                }
            }
            else
            {
                switch (aly)
                {
                    case 0: input = ""; break;
                    case 1: Subit(); break;
                    case 2: Querey(); break;
                    case 3: Module.HandleStrike(); Start(); break;

                }
                x = 0;
                inputting = false;
            }
        }
    }
    // Update is called once per frame
    IEnumerator stupid()
    {
        while (true)
        {
            if (count != 0 && !inputting)
            {
                if (!solved) yield return new WaitForSeconds(2f);
                for (int i = 0; i < output.Length; i++)
                {
                    if (inputting) break;
                    int X = Array.IndexOf(xorcolor.ToCharArray(), output[i]);
                    blacklight.enabled = true;
                    if (X % 2 != 0) lights[0].enabled = true;
                    if (X % 4 != 0 && X % 4 != 1) lights[1].enabled = true;
                    if (X % 8 != 0 && X % 8 != 1 && X % 8 != 2 && X % 8 != 3) lights[2].enabled = true;
                    if (!solved && !_isMuted) Audio.PlaySoundAtTransform(xorn[X], UWU);
                    if (count == 1 && solved && !tpsolved && !_isMuted) Audio.PlaySoundAtTransform(xorn[X], UWU);
                    if (inputting) break;
                    if (!solved) yield return new WaitForSeconds(.5f);
                    if (solved) yield return new WaitForSeconds(.05f);
                    if (inputting) break;
                    blacklight.enabled = false;
                    lights[0].enabled = false;
                    lights[1].enabled = false;
                    lights[2].enabled = false;
                    if (inputting) break;
                    if (!solved) yield return new WaitForSeconds(.2f);
                }
            }
            if (solved) count++;
            yield return new WaitForSeconds(.001f);
        }
    }
    void Subit()
    {
        if (input == answer)
        {
            solved = true;
            Module.HandlePass();
            count = 1;
            output = "RRRRRGGGGGBBBBBRRRRGGGGBBBBRRRGGGBBBRRGGBBRGBRGBRGBRGBRGBRRGGBBRRRGGGBBBRRRRGGGGBBBB";
            Debug.LogFormat("[Simon Swindles #{0}]: Well done! You got me!", _moduleId);
        }
        else
        {
            Module.HandleStrike(); Start();
            Debug.LogFormat("[Simon Swindles #{0}]: F.", _moduleId);
        }
    }
    void Querey()
    {
        string death = constnt;
        for (int i = 0; i < 6; i++)
        {
            switch (input[i])
            {
                case 'K': death = ReverseString(death.ToCharArray()); break;
                case 'R':
                    death = death.Select(x => x.ToString().Replace("K", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("R", "K")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "R")).Join("");
                    death = death.Select(x => x.ToString().Replace("G", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("Y", "G")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "Y")).Join("");
                    death = death.Select(x => x.ToString().Replace("B", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("M", "B")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "M")).Join("");
                    death = death.Select(x => x.ToString().Replace("C", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("W", "C")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "W")).Join(""); break;
                case 'G':
                    death = death.Select(x => x.ToString().Replace("K", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("G", "K")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "G")).Join("");
                    death = death.Select(x => x.ToString().Replace("R", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("Y", "R")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "Y")).Join("");
                    death = death.Select(x => x.ToString().Replace("B", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("C", "B")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "C")).Join("");
                    death = death.Select(x => x.ToString().Replace("M", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("W", "M")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "W")).Join(""); break;
                case 'B':
                    death = death.Select(x => x.ToString().Replace("K", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("B", "K")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "B")).Join("");
                    death = death.Select(x => x.ToString().Replace("G", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("C", "G")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "C")).Join("");
                    death = death.Select(x => x.ToString().Replace("R", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("M", "R")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "M")).Join("");
                    death = death.Select(x => x.ToString().Replace("Y", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("W", "Y")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "W")).Join(""); break;
                case 'C': death = leftrotate(death, 1); break;
                case 'M': death = leftrotate(death, 5); break;
                case 'Y':
                    death = death.Select(x => x.ToString().Replace("K", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("W", "K")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "W")).Join("");
                    death = death.Select(x => x.ToString().Replace("G", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("M", "G")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "M")).Join("");
                    death = death.Select(x => x.ToString().Replace("R", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("C", "R")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "C")).Join("");
                    death = death.Select(x => x.ToString().Replace("Y", "-")).Join("");
                    death = death.Select(x => x.ToString().Replace("B", "Y")).Join("");
                    death = death.Select(x => x.ToString().Replace("-", "B")).Join(""); break;
                case 'W': death = ReverseTwoHalves(death); break;
            }
        }
        string death2 = "";
        for (int i = 0; i < 6; i++)
        {
            death2 += xorcolor[Array.IndexOf(xorcolor.ToCharArray(), death[i]) ^ Array.IndexOf(xorcolor.ToCharArray(), input[i])];
            if (input[i] == answer[i])
            {
                char h = xorcolor[Array.IndexOf(xorcolor.ToCharArray(), death2[i]) ^ 7];
                death2 = ReplaceLastChar(death2, h);
            }
        }
        output = death2;
        string uwu = "";
        for (int i = 0; i < 6; i++)
        {
            uwu += xorcolor[Array.IndexOf(xorcolor.ToCharArray(), output[i]) ^ Array.IndexOf(xorcolor.ToCharArray(), answer[i])];
        }
        switch ((Array.IndexOf(xorcolor.ToCharArray(), input[count % 6]) ^ 1) % 2)
        {
            case 0: uwu = uwu.Substring(0, 3).Reverse().Join("") + uwu.Substring(3, 3).Join(""); break;
            case 1: uwu = uwu.Substring(0, 3).Join("") + uwu.Substring(3, 3).Reverse().Join(""); break;
        }
        switch ((Array.IndexOf(xorcolor.ToCharArray(), input[count % 6]) ^ 2) % 4)
        {
            case 0: uwu = invert(uwu.Substring(0, 3)) + uwu.Substring(3, 3); break; //K B
            case 1: uwu = invert(uwu.Substring(0, 3)) + uwu.Substring(3, 3); break; //R M
            case 2: uwu = uwu.Substring(0, 3) + invert(uwu.Substring(3, 3)); break; //G C
            case 3: uwu = uwu.Substring(0, 3) + invert(uwu.Substring(3, 3)); break; //W Y
        }
        switch ((Array.IndexOf(xorcolor.ToCharArray(), input[count % 6]) ^ 4) % 8)
        {
            case 4: uwu = leftrotate(uwu, 5); break; //K
            case 5: uwu = leftrotate(uwu, 5); break; //R
            case 6: uwu = leftrotate(uwu, 5); break; //G
            case 7: uwu = leftrotate(uwu, 5); break; //Y
            case 0: uwu = leftrotate(uwu, 1); break; //B
            case 1: uwu = leftrotate(uwu, 1); break; //M
            case 2: uwu = leftrotate(uwu, 1); break; //C
            case 3: uwu = leftrotate(uwu, 1); break; //Y
        }
        answer = uwu;
        Debug.LogFormat("[Simon Swindles #{0}]: GUESS #{1} - You guessed {2}. I respond with {3}, and make the next answer {4}", _moduleId, count, input, output, answer);
        count++;
        input = "";
    }
    public static string leftrotate(string t, int x)
    {       //AWKBRA -> WKBRAA
        return t.Substring(x, t.Length - x) + t.Substring(0, x);
    }
    private string ReverseTwoHalves(string str)
    {
        return str.Substring(0, str.Length / 2).Reverse().Join("") + str.Substring(str.Length / 2, (str.Length / 2)).Reverse().Join("");
    }
    private string ReplaceLastChar(string str, char c)
    {
        return str.Substring(0, str.Length - 1) + c;
    }
    private string invert(string str)
    {
        string death = str;
        death = death.Select(x => x.ToString().Replace("K", "-")).Join("");
        death = death.Select(x => x.ToString().Replace("W", "K")).Join("");
        death = death.Select(x => x.ToString().Replace("-", "W")).Join("");
        death = death.Select(x => x.ToString().Replace("G", "-")).Join("");
        death = death.Select(x => x.ToString().Replace("M", "G")).Join("");
        death = death.Select(x => x.ToString().Replace("-", "M")).Join("");
        death = death.Select(x => x.ToString().Replace("R", "-")).Join("");
        death = death.Select(x => x.ToString().Replace("C", "R")).Join("");
        death = death.Select(x => x.ToString().Replace("-", "C")).Join("");
        death = death.Select(x => x.ToString().Replace("Y", "-")).Join("");
        death = death.Select(x => x.ToString().Replace("B", "Y")).Join("");
        death = death.Select(x => x.ToString().Replace("-", "B")).Join("");
        return death;
    }
    private string ReverseString(char[] s)
    {

        for (int i = 0; i < s.Length / 2; i++)
        {

            char temp = s[i];
            s[i] = s[s.Length - 1 - i];
            s[s.Length - 1 - i] = temp;

        }
        return s.Join("");
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} query KWYCRB (Queries blacK White Yellow Cyan Red Blue), !{0} submit RGBCMY (Submits Red Green Blue Cyan Magenta Yellow)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        bool Valid = true;
        var m = Regex.Match(command, @"^\s*(query|submit)\s+(?:(.+))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            var type = m.Groups[1].Value;
            var input = m.Groups[2].Value;
            Debug.Log(type);
            Debug.Log(input);
            if (input.Length != 6)
            {
                Valid = false;
            }
            else
            {
                for (int i = 0; i < 6; i++)
                {
                    if (Array.IndexOf(xorcolor.ToCharArray(), input[i].ToString().ToUpper().ToCharArray()[0]) == -1)
                        Valid = false;
                }
            }
            if (!Valid)
            {
                yield return "sendtochaterror Incorrect Syntax. Valid colors are K,R,G,B,C,M,Y, and W. Make sure your input is length 6!";
                yield break;
            }
            yield return null;  // acknowledge to TP that the command was valid

            for (var i = 0; i < 6; i++)
            {
                int X = Array.IndexOf(xorcolor.ToCharArray(), input[i].ToString().ToUpper().ToCharArray()[0]);
                if (X % 2 == 1) buttons[0].OnInteract();
                yield return new WaitForSeconds(.05f);
                if (X % 4 == 2 || X % 4 == 3) buttons[1].OnInteract();
                yield return new WaitForSeconds(.1f);
                if (X % 8 == 4 || X % 8 == 5 || X % 8 == 6 || X % 8 == 7) buttons[2].OnInteract();
                yield return new WaitForSeconds(.2f);
                buttons[3].OnInteract();
            }
            if (type.ToLowerInvariant() == "query")
            {
                buttons[2].OnInteract();
            }
            else buttons[1].OnInteract();
            if (solved)
            {
                yield return "solve";
            }
        }

        var n = Regex.Match(command, @"^\s*(?:mute)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (n.Success)
        {
            yield return null;
            MuteSel.OnInteract();
            yield break;
        }

        yield break;
    }
    IEnumerator TwitchHandleForcedSolve()
    {
        tpsolved = true;
        for (int i = 0; i < 6; i++)
        {
            int X = Array.IndexOf(xorcolor.ToCharArray(), answer[i]);
            if (X % 2 == 1) buttons[0].OnInteract();
            yield return new WaitForSeconds(.0125f);
            if (X % 4 == 2 || X % 4 == 3) buttons[1].OnInteract();
            yield return new WaitForSeconds(.025f);
            if (X % 8 == 4 || X % 8 == 5 || X % 8 == 6 || X % 8 == 7) buttons[2].OnInteract();
            yield return new WaitForSeconds(.05f);
            buttons[3].OnInteract();
        }
        buttons[1].OnInteract();
    }

}
