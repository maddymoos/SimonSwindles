using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

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
    private string answer, constant, input, output;
    private bool inputting, solved, tpsolved;
    private int x, count;
    private bool _isMuted;
    // Use this for initialization
    void Start()
    {
        _moduleId = _moduleIdCounter++;
        for(byte i = 0; i < buttons.Length; i++)
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
        float scalar = transform.lossyScale.x;
        for(var i = 0; i < lights.Length; i++)
            lights[i].range *= scalar;
        blacklight.range *= scalar;
        GenerateRuleSeed();
        Generate();
    }

    private static string ColorToBinary(string s)
    {
        string str = "";
        foreach(char c in s)
        {
            switch(c)
            {
                case 'K':
                    str += "000";
                    break;
                case 'R':
                    str += "100";
                    break;
                case 'G':
                    str += "010";
                    break;
                case 'B':
                    str += "001";
                    break;
                case 'C':
                    str += "011";
                    break;
                case 'M':
                    str += "101";
                    break;
                case 'Y':
                    str += "110";
                    break;
                case 'W':
                    str += "111";
                    break;
            }
        }
        return str;
    }
    private static string BinaryToColor(string s)
    {
        string str = "";
        while(s.Length >= 3)
        {
            string cur = s.Substring(0, 3);
            s = s.Substring(3);
            switch(cur)
            {
                case "000":
                    str += 'K';
                    break;
                case "100":
                    str += 'R';
                    break;
                case "010":
                    str += 'G';
                    break;
                case "001":
                    str += 'B';
                    break;
                case "011":
                    str += 'C';
                    break;
                case "101":
                    str += 'M';
                    break;
                case "110":
                    str += 'Y';
                    break;
                case "111":
                    str += 'W';
                    break;
            }
        }
        return str;
    }

    // RULE SEED
    private bool _nor1 = false, _nor2 = false, _revform = false, _bit1 = false, _bit2 = false, _bit3 = false;
    private int _rule1 = 0, _rule2 = 0, _rule3 = 0;

    private List<Func<string, string>> _rules = new List<Func<string, string>>
    {
        // Vanilla
        s => ReverseString(s.ToCharArray()),
        s =>
        {
            s = s.Replace("K", "-");
            s = s.Replace("R", "K");
            s = s.Replace("-", "R");
            s = s.Replace("G", "-");
            s = s.Replace("Y", "G");
            s = s.Replace("-", "Y");
            s = s.Replace("B", "-");
            s = s.Replace("M", "B");
            s = s.Replace("-", "M");
            s = s.Replace("C", "-");
            s = s.Replace("W", "C");
            s = s.Replace("-", "W");
            return s;
        },
        s =>
        {
            s = s.Replace("K", "-");
            s = s.Replace("G", "K");
            s = s.Replace("-", "G");
            s = s.Replace("R", "-");
            s = s.Replace("Y", "R");
            s = s.Replace("-", "Y");
            s = s.Replace("B", "-");
            s = s.Replace("C", "B");
            s = s.Replace("-", "C");
            s = s.Replace("M", "-");
            s = s.Replace("W", "M");
            s = s.Replace("-", "W");
            return s;
        },
        s =>
        {
            s = s.Replace("K", "-");
            s = s.Replace("B", "K");
            s = s.Replace("-", "B");
            s = s.Replace("G", "-");
            s = s.Replace("C", "G");
            s = s.Replace("-", "C");
            s = s.Replace("R", "-");
            s = s.Replace("M", "R");
            s = s.Replace("-", "M");
            s = s.Replace("Y", "-");
            s = s.Replace("W", "Y");
            s = s.Replace("-", "W");
            return s;
        },
        s => LeftRotate(s, 1),
        s => LeftRotate(s, 5),
        s => ReverseString(s.ToCharArray()),
        s => ReverseTwoHalves(s),
        // Rule-Seeded
        s => new char[] { s[1], s[0], s[3], s[2], s[5], s[4] }.Join(""),
        s => s.Select(sub => BinaryToColor(LeftRotate(ColorToBinary(sub.ToString()), 1))).Join(""),
        s => s.Select(sub => BinaryToColor(LeftRotate(ColorToBinary(sub.ToString()), 2))).Join(""),
        s => BinaryToColor(LeftRotate(ColorToBinary(s), 1)),
        s => BinaryToColor(LeftRotate(ColorToBinary(s), 17))
    };
    private List<List<Func<string, string>>> _rulesFunni = new List<List<Func<string, string>>>
    {
        new List<Func<string, string>> { s => new char[] { s[2], s[1], s[0], s[3], s[4], s[5] }.Join(""), s => new char[] { s[0], s[1], s[2], s[5], s[4], s[3] }.Join("") },
        new List<Func<string, string>> { s => Invert(s.Substring(0, 3)) + s.Substring(3, 3), s => s.Substring(0, 3) + Invert(s.Substring(3, 3)) },
        new List<Func<string, string>> { s => LeftRotate(s, 1), s => LeftRotate(s, 5), s => LeftRotate(s, 2), s => LeftRotate(s, 4) },
        new List<Func<string, string>> { s => InvertEven(s), s => Invert(InvertEven(s)) }
    };
    private List<int> _commandColors = new List<int> { 0, 1, 2 };

    private void GenerateRuleSeed()
    {
        MonoRandom rnd = GetComponent<KMRuleSeedable>().GetRNG();

        if(rnd.Seed == 1)
            return;

        Debug.LogFormat("[Simon Swindles #{0}] GENERATION PHASE: Ruleseed is {1}", _moduleId, rnd.Seed);

        rnd.ShuffleFisherYates(_rules);
        rnd.ShuffleFisherYates(_rulesFunni);
        rnd.ShuffleFisherYates(_commandColors);

        _nor1 = rnd.Next(2) == 1;
        _nor2 = rnd.Next(2) == 1;
        _revform = rnd.Next(2) == 1;
        _bit1 = rnd.Next(2) == 1;
        _bit2 = rnd.Next(2) == 1;
        _bit3 = rnd.Next(2) == 1;

        _rule1 = rnd.Next(_rulesFunni[0].Count);
        _rule2 = rnd.Next(_rulesFunni[1].Count);
        _rule3 = rnd.Next(_rulesFunni[2].Count);
    }

    static private int _moduleIdCounter = 1;
    private int _moduleId;
    private void Generate()
    {
        constant = "";
        answer = "";
        for(int i = 0; i < 6; i++)
        {
            answer += color2[Rnd.Range(0, 8)];
            constant += color2[Rnd.Range(0, 8)];
            inputting = false;
        }
        output = "";
        input = "";
        count = 0;
        Debug.LogFormat("[Simon Swindles #{0}] GENERATION PHASE: Intial answer is {1} and the constant is {2}", _moduleId, answer, constant);
    }
    void HandlePress(KMSelectable btn)
    {
        if(solved)
            return;

        int aly = Array.IndexOf(buttons, btn);
        buttons[aly].AddInteractionPunch();
        if(input.Length != 6)
        {
            if(!inputting)
            {
                StopAllCoroutines();
                blacklight.enabled = false;
                lights[0].enabled = false;
                lights[1].enabled = false;
                lights[2].enabled = false;
                StartCoroutine(Stupid());
                inputting = true;
            }
            if(aly != 3)
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
            switch(aly)
            {
                case 0: x ^= 1; break;
                case 1: x ^= 2; break;
                case 2: x ^= 4; break;
                case 3:
                    input += xorcolor[x];
                    if(!_isMuted)
                        Audio.PlaySoundAtTransform(xorn[x], UWU.transform);
                    x = 0;
                    break;
            }
        }
        else
        {
            if(aly == _commandColors[0])
                input = "";
            else if(aly == _commandColors[1])
                Submit();
            else if(aly == _commandColors[2])
                Query();
            else
            {
                Module.HandleStrike();
                Generate();
            }
            x = 0;
            inputting = false;
        }
    }
    // Update is called once per frame
    IEnumerator Stupid()
    {
        while(true)
        {
            if(count != 0 && !inputting)
            {
                if(!solved) yield return new WaitForSeconds(2f);
                for(int i = 0; i < output.Length; i++)
                {
                    if(inputting) break;
                    int X = Array.IndexOf(xorcolor.ToCharArray(), output[i]);
                    blacklight.enabled = true;
                    if(X % 2 != 0) lights[0].enabled = true;
                    if(X % 4 != 0 && X % 4 != 1) lights[1].enabled = true;
                    if(X % 8 != 0 && X % 8 != 1 && X % 8 != 2 && X % 8 != 3) lights[2].enabled = true;
                    if(!solved && !_isMuted) Audio.PlaySoundAtTransform(xorn[X], UWU);
                    if(count == 1 && solved && !tpsolved && !_isMuted) Audio.PlaySoundAtTransform(xorn[X], UWU);
                    if(inputting) break;
                    if(!solved) yield return new WaitForSeconds(.5f);
                    if(solved) yield return new WaitForSeconds(.05f);
                    if(inputting) break;
                    blacklight.enabled = false;
                    lights[0].enabled = false;
                    lights[1].enabled = false;
                    lights[2].enabled = false;
                    if(inputting) break;
                    if(!solved) yield return new WaitForSeconds(.2f);
                }
            }
            if(solved) count++;
            yield return new WaitForSeconds(.001f);
        }
    }
    void Submit()
    {
        if(input == answer)
        {
            solved = true;
            count = 1;
            output = "RRRRRGGGGGBBBBBRRRRGGGGBBBBRRRGGGBBBRRGGBBRGBRGBRGBRGBRGBRRGGBBRRRGGGBBBRRRRGGGGBBBB";
            Debug.LogFormat("[Simon Swindles #{0}] Well done! You got me!", _moduleId);
            Module.HandlePass();
        }
        else
        {
            Debug.LogFormat("[Simon Swindles #{0}] F.", _moduleId);
            Debug.LogFormat("[Simon Swindles #{0}] (You submitted {1}.)", _moduleId, input);
            Module.HandleStrike();
            Generate();
        }
    }
    void Query()
    {
        string death = constant;
        for(int i = 0; i < 6; i++)
        {
            switch(input[i])
            {
                case 'K':
                    death = _rules[0](death);
                    break;
                case 'R':
                    death = _rules[1](death);
                    break;
                case 'G':
                    death = _rules[2](death);
                    break;
                case 'B':
                    death = _rules[3](death);
                    break;
                case 'C':
                    death = _rules[4](death);
                    break;
                case 'M':
                    death = _rules[5](death);
                    break;
                case 'Y':
                    death = _rules[6](death);
                    break;
                case 'W':
                    death = _rules[7](death);
                    break;
            }
        }
        string death2 = "";
        for(int i = 0; i < 6; i++)
        {
            death2 += xorcolor[Array.IndexOf(xorcolor.ToCharArray(), death[i]) ^ Array.IndexOf(xorcolor.ToCharArray(), input[i]) ^ (_nor1 ? 7 : 0)];
            if(input[i] == answer[i])
            {
                char h = xorcolor[Array.IndexOf(xorcolor.ToCharArray(), death2[i]) ^ 7];
                death2 = ReplaceLastChar(death2, h);
            }
        }
        output = death2;
        string uwu = "";
        for(int i = 0; i < 6; i++)
        {
            uwu += xorcolor[Array.IndexOf(xorcolor.ToCharArray(), output[i]) ^ Array.IndexOf(xorcolor.ToCharArray(), answer[i]) ^ (_nor2 ? 7 : 0)];
        }

        int calced = _revform ? 5 - (count % 6) : count % 6;

        switch((Array.IndexOf(xorcolor.ToCharArray(), input[calced]) & 1) ^ (_bit1 ? 1 : 0))
        {
            case 0: uwu = _rulesFunni[0][_rule1 ^ 1](uwu); break;
            case 1: uwu = _rulesFunni[0][_rule1](uwu); break;
        }
        switch((Array.IndexOf(xorcolor.ToCharArray(), input[calced]) & 2) ^ (_bit1 ? 2 : 0))
        {
            case 0: uwu = _rulesFunni[1][_rule2 ^ 1](uwu); break;
            case 2: uwu = _rulesFunni[1][_rule2](uwu); break;
        }
        switch((Array.IndexOf(xorcolor.ToCharArray(), input[calced]) & 4) ^ (_bit1 ? 4 : 0))
        {
            case 0: uwu = _rulesFunni[2][_rule3 ^ 1](uwu); break;
            case 4: uwu = _rulesFunni[2][_rule3](uwu); break;
        }
        answer = uwu;
        Debug.LogFormat("[Simon Swindles #{0}] GUESS #{1} - You guessed {2}. I respond with {3}, and make the next answer {4}", _moduleId, count, input, output, answer);
        count++;
        input = "";
    }
    public static string LeftRotate(string t, int x)
    {       //AWKBRA -> WKBRAA
        return t.Substring(x, t.Length - x) + t.Substring(0, x);
    }
    private static string ReverseTwoHalves(string str)
    {
        return str.Substring(0, str.Length / 2).Reverse().Join("") + str.Substring(str.Length / 2, (str.Length / 2)).Reverse().Join("");
    }
    private static string ReplaceLastChar(string str, char c)
    {
        return str.Substring(0, str.Length - 1) + c;
    }
    private static string Invert(string str)
    {
        string death = str;
        death = death.Replace("K", "-");
        death = death.Replace("W", "K");
        death = death.Replace("-", "W");
        death = death.Replace("G", "-");
        death = death.Replace("M", "G");
        death = death.Replace("-", "M");
        death = death.Replace("R", "-");
        death = death.Replace("C", "R");
        death = death.Replace("-", "C");
        death = death.Replace("Y", "-");
        death = death.Replace("B", "Y");
        death = death.Replace("-", "B");
        return death;
    }
    private static string InvertEven(string s)
    {
        char[] str = s.ToCharArray();

        for(int i = 0; i < str.Length; i++)
        {
            if(i % 2 == 1)
                continue;
            if(str[i] == 'K')
                str[i] = 'W';
            else if(str[i] == 'R')
                str[i] = 'C';
            else if(str[i] == 'G')
                str[i] = 'M';
            else if(str[i] == 'B')
                str[i] = 'Y';
            else if(str[i] == 'C')
                str[i] = 'R';
            else if(str[i] == 'M')
                str[i] = 'G';
            else if(str[i] == 'Y')
                str[i] = 'B';
            else if(str[i] == 'W')
                str[i] = 'K';
        }

        return str.Join("");
    }
    private static string ReverseString(char[] s)
    {
        for(int i = 0; i < s.Length / 2; i++)
        {
            char temp = s[i];
            s[i] = s[s.Length - 1 - i];
            s[s.Length - 1 - i] = temp;
        }
        return s.Join("");
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} query KWYCRB (Queries blacK White Yellow Cyan Red Blue), !{0} submit RGBCMY (Submits Red Green Blue Cyan Magenta Yellow), !{0} mute (shuts up)";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        bool Valid = true;
        var m = Regex.Match(command, @"^\s*(query|submit)\s+(?:(.+))$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if(m.Success)
        {
            var type = m.Groups[1].Value;
            var input = m.Groups[2].Value;
            Debug.Log(type);
            Debug.Log(input);
            if(input.Length != 6)
            {
                Valid = false;
            }
            else
            {
                for(int i = 0; i < 6; i++)
                {
                    if(Array.IndexOf(xorcolor.ToCharArray(), input[i].ToString().ToUpper().ToCharArray()[0]) == -1)
                        Valid = false;
                }
            }
            if(!Valid)
            {
                yield return "sendtochaterror Incorrect Syntax. Valid colors are K,R,G,B,C,M,Y, and W. Make sure your input is length 6!";
                yield break;
            }
            yield return null;  // acknowledge to TP that the command was valid

            for(var i = 0; i < 6; i++)
            {
                int X = Array.IndexOf(xorcolor.ToCharArray(), input[i].ToString().ToUpper().ToCharArray()[0]);
                if(X % 2 == 1) buttons[0].OnInteract();
                yield return new WaitForSeconds(.05f);
                if(X % 4 == 2 || X % 4 == 3) buttons[1].OnInteract();
                yield return new WaitForSeconds(.1f);
                if(X % 8 == 4 || X % 8 == 5 || X % 8 == 6 || X % 8 == 7) buttons[2].OnInteract();
                yield return new WaitForSeconds(.2f);
                buttons[3].OnInteract();
            }
            if(type.ToLowerInvariant() == "query")
            {
                buttons[_commandColors[2]].OnInteract();
            }
            else buttons[_commandColors[1]].OnInteract();
            if(solved)
            {
                yield return "solve";
            }
        }

        var n = Regex.Match(command, @"^\s*(?:mute|shut\sup)?\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if(n.Success)
        {
            yield return null;
            MuteSel.OnInteract();
            yield break;
        }
        yield break;
    }

    // Implemented by Quinn Wuest.
    IEnumerator TwitchHandleForcedSolve()
    {
        for(int i = 0; i < input.Length; i++)
        {
            if(input[i] != answer[i])
            {
                for(int j = input.Length; j < 6; j++)
                {
                    buttons[3].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                buttons[_commandColors[2]].OnInteract();
                goto inputSolution;
            }
        }
        inputSolution:
        for(int i = input.Length; i < 6; i++)
        {
            var list = GetAutosolveBtns(answer[i], x);
            for(int j = 0; j < list.Count; j++)
            {
                list[j].OnInteract();
                yield return new WaitForSeconds(0.05f);
            }
            buttons[3].OnInteract();
            yield return new WaitForSeconds(0.2f);
        }
        buttons[_commandColors[1]].OnInteract();
    }

    private List<KMSelectable> GetAutosolveBtns(char inp, int num)
    {
        var list = new List<KMSelectable>();
        if(inp == 'K')
        {
            if(num % 2 != 0)
                list.Add(buttons[0]);
            if(num % 4 != 0)
                list.Add(buttons[1]);
            if(num % 8 != 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'R')
        {
            if(num % 2 == 0)
                list.Add(buttons[0]);
            if(num % 4 != 0)
                list.Add(buttons[1]);
            if(num % 8 != 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'G')
        {
            if(num % 2 != 0)
                list.Add(buttons[0]);
            if(num % 4 == 0)
                list.Add(buttons[1]);
            if(num % 8 != 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'Y')
        {
            if(num % 2 == 0)
                list.Add(buttons[0]);
            if(num % 4 == 0)
                list.Add(buttons[1]);
            if(num % 8 != 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'B')
        {
            if(num % 2 != 0)
                list.Add(buttons[0]);
            if(num % 4 != 0)
                list.Add(buttons[1]);
            if(num % 8 == 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'M')
        {
            if(num % 2 == 0)
                list.Add(buttons[0]);
            if(num % 4 != 0)
                list.Add(buttons[1]);
            if(num % 8 == 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'C')
        {
            if(num % 2 != 0)
                list.Add(buttons[0]);
            if(num % 4 == 0)
                list.Add(buttons[1]);
            if(num % 8 == 0)
                list.Add(buttons[2]);
        }
        else if(inp == 'W')
        {
            if(num % 2 == 0)
                list.Add(buttons[0]);
            if(num % 4 == 0)
                list.Add(buttons[1]);
            if(num % 8 == 0)
                list.Add(buttons[2]);
        }
        return list;
    }
}
