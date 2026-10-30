using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudio : MonoBehaviour
{
    private static GameAudio instance;
    private GameAudioConfig config;
    private AudioSource music;
    private AudioSource effects;
    private AudioListener fallbackListener;
    private AudioClip place;
    private AudioClip merge;
    private AudioClip wave;
    private AudioClip death;
    private AudioClip win;
    private AudioClip lose;
    private float nextShot;
    private TMP_Text soundLabel;
    private readonly List<RaycastResult> hits = new();
    public static bool Muted => PlayerPrefs.GetInt("audio.muted", 0) != 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatic() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var host = new GameObject("Game Audio");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<GameAudio>();
        instance.Initialize();
    }

    private void Initialize()
    {
        config = Resources.Load<GameAudioConfig>("GameAudio");
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0.22f;
        effects = gameObject.AddComponent<AudioSource>();
        effects.playOnAwake = false;
        effects.volume = 0.65f;
        place = Tone("Place", 360f, 520f, 0.12f);
        merge = Tone("Merge", 500f, 1000f, 0.24f);
        wave = Tone("Wave", 240f, 440f, 0.28f);
        death = Tone("Enemy defeat", 180f, 70f, 0.12f);
        win = Tone("Victory", 520f, 1040f, 0.6f);
        lose = Tone("Defeat", 280f, 90f, 0.5f);
        ApplyMute();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (instance == this) instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fallbackListener != null) fallbackListener.enabled = false;
        var listeners = FindObjectsByType<AudioListener>();
        AudioListener active = null;
        foreach (var listener in listeners)
        {
            if (listener == fallbackListener || !listener.enabled) continue;
            if (active == null) active = listener;
            else listener.enabled = false;
        }
        if (active == null)
        {
            if (fallbackListener == null) fallbackListener = gameObject.AddComponent<AudioListener>();
            fallbackListener.enabled = true;
        }
        if (config != null) SetMusic(scene.name == "MainMenu" ? config.menuMusic : config.combatMusic);
        if (scene.name != "MainMenu" && scene.name != "GameScene") return;
        var overlay = RuntimeUI.Overlay("Audio controls", null, 40);
        var buttonRect = RuntimeUI.Rect("Sound", overlay, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(210f, 64f), new Vector2(135f, -45f));
        var button = RuntimeUI.Button("Sound", buttonRect, "", TMP_Settings.defaultFontAsset, ToggleMute);
        soundLabel = button.GetComponentInChildren<TMP_Text>();
        UpdateLabel();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || EventSystem.current == null) return;
        hits.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = Input.mousePosition }, hits);
        foreach (var hit in hits)
        {
            var button = hit.gameObject.GetComponentInParent<Button>();
            if (button == null || !button.interactable) continue;
            if (config != null) Play(config.click, 0.6f);
            break;
        }
    }

    private void ToggleMute()
    {
        PlayerPrefs.SetInt("audio.muted", Muted ? 0 : 1);
        PlayerPrefs.Save();
        ApplyMute();
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (soundLabel != null) soundLabel.text = Muted ? "Sound: OFF" : "Sound: ON";
    }

    private void ApplyMute()
    {
        music.mute = Muted;
        effects.mute = Muted;
    }

    private void SetMusic(AudioClip clip)
    {
        if (music.clip == clip && music.isPlaying) return;
        music.clip = clip;
        if (clip != null) music.Play();
    }

    public static void PlayMusic(AudioClip clip)
    {
        if (instance != null && clip != null) instance.SetMusic(clip);
    }

    private void Play(AudioClip clip, float volume = 1f)
    {
        if (clip != null && !Muted) effects.PlayOneShot(clip, volume);
    }

    public static void PlayShot()
    {
        if (instance == null || instance.config == null || Time.unscaledTime < instance.nextShot) return;
        instance.nextShot = Time.unscaledTime + 0.08f;
        instance.Play(instance.config.shot, 0.35f);
    }

    public static void PlayPlace() { if (instance != null) instance.Play(instance.place); }
    public static void PlayMerge() { if (instance != null) instance.Play(instance.merge); }
    public static void PlayWave() { if (instance != null) instance.Play(instance.wave); }
    public static void PlayEnemyDeath() { if (instance != null) instance.Play(instance.death, 0.4f); }
    public static void PlayResult(bool victory) { if (instance != null) instance.Play(victory ? instance.win : instance.lose); }

    private static AudioClip Tone(string name, float from, float to, float duration)
    {
        const int rate = 22050;
        var samples = new float[Mathf.CeilToInt(rate * duration)];
        float phase = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / samples.Length;
            phase += 2f * Mathf.PI * Mathf.Lerp(from, to, t) / rate;
            samples[i] = Mathf.Sin(phase) * Mathf.Sin(Mathf.PI * t) * 0.25f;
        }
        var clip = AudioClip.Create(name, samples.Length, 1, rate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
