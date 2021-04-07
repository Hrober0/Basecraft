using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Language : MonoBehaviour
{
    private const char Separator = '=';
    private static readonly Dictionary<string, string> allEngWords = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> allWords = new Dictionary<string, string>();
    private static SystemLanguage usingLanguage;

    private static readonly Dictionary<Res, string> resNames = new Dictionary<Res, string>();
    private static readonly Dictionary<Obj, string> objNames = new Dictionary<Obj, string>();
    private static readonly Dictionary<Technologies, string> techNames = new Dictionary<Technologies, string>();

    private void Awake()
    {
        var file = Resources.Load<TextAsset>("Languages/" + SystemLanguage.English.ToString());
        foreach (var line in file.text.Split('\n'))
        {
            var prop = line.Split(Separator);
            if (prop.Length < 2) { continue; }
            allEngWords[prop[0]] = prop[1];
        }
    }

    public static void SetLanguage(SystemLanguage language)
    {
        if (usingLanguage == language) { Debug.Log(language + " was alredy set."); return; }

        usingLanguage = language;
        ReadAllWords();
        SetResNames();
        SetObjNames();
        SetTechNames();
    }

    private static void ReadAllWords()
    {
        var file = Resources.Load<TextAsset>("Languages/" + usingLanguage.ToString());
        if (file == null)
        {
            file = Resources.Load<TextAsset>("Languages/" + SystemLanguage.English.ToString());
            Debug.Log("ERROR! Dont found " + usingLanguage + " file!");
            usingLanguage = SystemLanguage.English;
        }
        foreach (var line in file.text.Split('\n'))
        {
            var prop = line.Split(Separator);
            if (prop.Length < 2) { continue; }
            allWords[prop[0]] = prop[1];
        }
    }
    private static void SetResNames()
    {
        string[] names = Enum.GetNames(typeof(Res));
        foreach (string sName in names)
        {
            Enum.TryParse(sName, out Res res);
            string dName = "Res." + sName;
            if (allWords.ContainsKey(dName))
            {
                resNames[res] = allWords[dName];
                allWords.Remove(dName);
                continue;
            }

            if (allEngWords.ContainsKey(dName))
            {
                Debug.Log("ERROR!: Dont found Res." + res + " in " + usingLanguage + " file! Setting English version.");
                resNames[res] = allEngWords[dName];
                continue;
            }

            Debug.Log("ERROR!: Dont found Res." + res + " in " + usingLanguage + " and English file! Setting place holder.");
            resNames[res] = string.Format("<{0}>", res);
        }
    }
    private static void SetObjNames()
    {
        string[] names = Enum.GetNames(typeof(Obj));
        foreach (string sName in names)
        {
            Enum.TryParse(sName, out Obj obj);
            string dName = "Obj." + sName;
            if (allWords.ContainsKey(dName))
            {
                objNames[obj] = allWords[dName];
                allWords.Remove(dName);
                continue;
            }

            if (allEngWords.ContainsKey(dName))
            {
                Debug.Log("ERROR!: Dont found Obj." + obj + " in " + usingLanguage + " file! Setting English version.");
                objNames[obj] = allEngWords[dName];
                continue;
            }

            Debug.Log("ERROR!: Dont found Obj." + obj + " in " + usingLanguage + " file! Setting place holder.");
            objNames[obj] = string.Format("<{0}>", obj);
        }
    }
    private static void SetTechNames()
    {
        string[] names = Enum.GetNames(typeof(Technologies));
        foreach (string sName in names)
        {
            Enum.TryParse(sName, out Technologies tech);
            string dName = "Tech." + sName;
            if (allWords.ContainsKey(dName))
            {
                techNames[tech] = allWords[dName];
                allWords.Remove(dName);
                continue;
            }

            if (allEngWords.ContainsKey(dName))
            {
                Debug.Log("ERROR!: Dont found Tech." + tech + " in " + usingLanguage + " file! Setting English version.");
                techNames[tech] = allEngWords[dName];
                continue;
            }

            Debug.Log("ERROR!: Dont found Tech." + tech + " in " + usingLanguage + " file! Setting place holder.");
            techNames[tech] = string.Format("<{0}>", tech);
        }
    }


    public static string NameOfRes(Res res) => resNames[res];
    public static string NameOfTech(Technologies tech) => techNames[tech];
    public static string NameOfObj(Obj obj) => objNames[obj];
    public static string GetText(string key)
    {
        if (allWords.ContainsKey(key))
        {
            string s = allWords[key].Replace("\\n", "\n");
            return s;
        }

        if (allEngWords.ContainsKey(key))
        {
            Debug.LogWarning("Missing text of kay: " + key + " in " + usingLanguage);
            string s = allEngWords[key].Replace("\\n", "\n");
            return s;
        }

        Debug.LogWarning("Missing text of kay: " + key);
        return "null";
    }
    public static SystemLanguage GetUsingLangeage() => usingLanguage;
}
