using UnityEngine.Audio;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource musicSource;

    enum MusicType { None, Menu, Colony }
    private MusicType selectedMusicType = MusicType.None;

    [Header("Music")]
    [SerializeField] private List<AudioClip> menuMusic = new List<AudioClip>();
    private float musicVolume = 0.5f;
    private bool shouldBeMusicMute = true;
    private bool isMusicFadeing = false;
    private AudioClip musicToPlay = null;
    [SerializeField] private float timeToChangeMusic = 100f;

    [Header("Sounds")]
    [SerializeField] private List<AudioClip> colonyMusic = new List<AudioClip>();

    private void Awake()
    {
        if (instance != null) { return; }
        instance = this;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0f;
    }
    private void Update()
    {
        timeToChangeMusic -= Time.unscaledDeltaTime;
        if (timeToChangeMusic < 0f)
        {
            ChangeMusic();
        }
    }

    public void PlayMusicOfMenu()
    {
        selectedMusicType = MusicType.Menu;
        int n = Random.Range(0, menuMusic.Count);
        PlayMusic(menuMusic[n]);
    }
    public void PlayMusicOfColony()
    {
        selectedMusicType = MusicType.Colony;
        int n = Random.Range(0, colonyMusic.Count);
        PlayMusic(colonyMusic[n]);
    }

    private void PlayMusic(AudioClip musicClip)
    {
        if (isMusicFadeing) { musicToPlay = musicClip; return; }
        musicSource.clip = musicClip;
        musicToPlay = null;
        musicSource.Play();
        timeToChangeMusic = Random.Range(240, 600);
    }
    public void ChangeMusic()
    {
        timeToChangeMusic = 100f;
        Debug.Log("change music");
        if (selectedMusicType == MusicType.None) return;
        
        FadeDownMusic();
        switch (selectedMusicType)
        {
            case MusicType.Menu: PlayMusicOfMenu(); break;
            case MusicType.Colony: PlayMusicOfColony(); break;
        }
        FadeUpMusic();
    }

    public void FadeUpMusic()
    {
        shouldBeMusicMute = false;
        if (isMusicFadeing) return;

        if (musicToPlay != null) { PlayMusic(musicToPlay); }

        isMusicFadeing = true;
        musicSource.DOKill();
        musicSource.DOFade(musicVolume, 2f).SetEase(Ease.InQuad).SetUpdate(true).OnComplete(Fix);
        
        void Fix() { isMusicFadeing = false; if (shouldBeMusicMute == true) FadeDownMusic(); }
    }
    public void FadeDownMusic()
    {
        shouldBeMusicMute = true;
        if (isMusicFadeing) return;

        isMusicFadeing = true;
        musicSource.DOKill();
        musicSource.DOFade(0f, 0.5f).SetEase(Ease.InQuad).SetUpdate(true).OnComplete(Fix);

        void Fix() { isMusicFadeing = false; if (shouldBeMusicMute == false) FadeUpMusic(); }
    }
    public void SetMusicVolume(float volume) { musicSource.volume = volume; musicVolume = volume; }

    public void SetSoundsVolume(float volume) { Debug.Log("TODO: set sounds volume"); }
}
