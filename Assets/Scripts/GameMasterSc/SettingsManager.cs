using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    private readonly string resImageInBuilding = "resImageInBuilding";
    private readonly string fullscreen = "fullscreen";
    private readonly string autoSaveFreq = "autoSaveFreq";
    private readonly string usingLanguage = "usingLanguage";

    private readonly string musicVolume = "musicVolume";
    private readonly string soundsVolume = "soundsVolume";

    public static SettingsManager instance;
    private void Awake()
    {
        if (instance == null) { instance = this; }
    }
    private void Start()
    {
        ReadAndSetAllSettings();
    }

    [Header("Settings")]
    public readonly string gameVersion = "0.7.2";

    public void ReadAndSetAllSettings()
    {
        SetResImageInBuilding(ReadResImageInBuilding());
        SetFullscreen(GetFullscreen());
        SetAutoSaveFreq(GetAutoSaveValue());
        SetUsingLanguage(GetUsingLanguage());
        SetMusicVolume(GetMusicVolume());
        SetSoundsVolume(GetSoundscVolume());
    }

    private bool resImageInBuildingValue;
    public void SetResImageInBuilding(bool active)
    {
        resImageInBuildingValue = active;

        if (active) { PlayerPrefs.SetInt(resImageInBuilding, 1); } else { PlayerPrefs.SetInt(resImageInBuilding, 0); }

        if (DronControler.instance != null)
        {
            foreach (PlatformBehavior PBSC in DronControler.instance.GetAllPBSc)
            {
                PBSC.SetVisableImage(active, true);
            }
        }
    }
    private bool ReadResImageInBuilding()
    {
        if (PlayerPrefs.GetInt(resImageInBuilding, 1) == 1) { return true; }
        return false;
    }
    public bool GetResImageInBuilding() => resImageInBuildingValue;

    public void SetFullscreen(bool active)
    {
        if (active) { PlayerPrefs.SetInt(fullscreen, 1); } else { PlayerPrefs.SetInt(fullscreen, 0); }
        Screen.fullScreen = active;
    }
    public bool GetFullscreen()
    {
        if (PlayerPrefs.GetInt(fullscreen, 1) == 1) { return true; }
        return false;
    }

    public void SetAutoSaveFreq(int value)
    {
        PlayerPrefs.SetInt(autoSaveFreq, value);
        float time = -1;
        switch (value)
        {
            case 0: time = -1; break;
            case 1: time = 60; break;
            case 2: time = 120; break;
            case 3: time = 300; break;
        }
        SpaceBaseMainSc.instance.colonyAutoSaveDelay = time;
    }
    public int GetAutoSaveValue()
    {
        return PlayerPrefs.GetInt(autoSaveFreq, 2);
    }

    public void SetUsingLanguageFromInt(int value)
    {
        switch (value)
        {
            case 0: SetUsingLanguage(SystemLanguage.English); return;
            case 1: SetUsingLanguage(SystemLanguage.Polish); return;
            default: Debug.Log("Error! " + value + " this language number is undefined so cant select it"); return;
        }
    }
    public int GetUsingLanguageAsInt()
    {
        switch (GetUsingLanguage())
        {
            case SystemLanguage.English: return 0;
            case SystemLanguage.Polish: return 1;
            default: return 0;
        }
    }
    public void SetUsingLanguage(SystemLanguage language)
    {
        PlayerPrefs.SetString(usingLanguage, language.ToString());
        Language.SetLanguage(language);
    }
    public SystemLanguage GetUsingLanguage()
    {
        string lang = PlayerPrefs.GetString(usingLanguage, "null");
        if (Enum.TryParse(lang, out SystemLanguage language)) { return language; }
        
        return Application.systemLanguage;
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(musicVolume, volume);
        AudioManager.instance.SetMusicVolume(volume);
    }
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(musicVolume, 0.25f);
    }

    public void SetSoundsVolume(float volume)
    {
        PlayerPrefs.SetFloat(soundsVolume, volume);
        AudioManager.instance.SetSoundsVolume(volume);
    }
    public float GetSoundscVolume()
    {
        return PlayerPrefs.GetFloat(soundsVolume, 0.5f);
    }
}