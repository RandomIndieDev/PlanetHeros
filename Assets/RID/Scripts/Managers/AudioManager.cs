using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kadinche.Kassets.Variable;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum SoundType
{
    OnButtonClick_Default = 1,
    OnButtonClick_Low = 2,
    OnButtonClick_High = 3,
    
    MainMenuMusic = 20,
    ContestModeMusic = 21,
    PracticeModeMusic = 22,
    
    popup_Fact = 40,
    popup_Fact_Out = 41,
    panel_Show = 42,
    
    
    icon_Popin= 60,
    timer_Completed= 61,
    dino_Show_Happy =62,
    
    grid_WordSelected = 80,
    grid_CorrectWordFound = 81,
    grid_WrongWord = 82,
    
    Effect_Experience_Gained = 90,

    
    
    
    NONE = 999,
}

[System.Serializable]
public class SoundCategory
{
    [FoldoutGroup("$CategoryName", expanded: false)]
    [HideLabel, LabelWidth(120)]
    public string CategoryName;

    // Optional header line for visual spacing
    [FoldoutGroup("$CategoryName")]
    [PropertySpace]

    [FoldoutGroup("$CategoryName"), TableList]
    public List<Sound> Sounds = new();
}

[System.Serializable]
public class Sound
{
    [HorizontalGroup("Header")] [HideLabel]
    public SoundType soundType;

    [BoxGroup("a", false)] public bool ChooseFromMultiple;

    [BoxGroup("a", false), ShowIf("ChooseFromMultiple")]
    public List<AudioClip> Clips;

    [BoxGroup("a", false)] [PreviewField, HideLabel, HideIf("ChooseFromMultiple")]
    public AudioClip clip;

    [FoldoutGroup("General Settings"), Range(0f, 1f)]
    public float volume = 1f;

    [FoldoutGroup("General Settings"), Range(0.1f, 3f)]
    public float pitch = 1f;

    [FoldoutGroup("General Settings")] public bool loop;

    [FoldoutGroup("Advanced Settings")] public bool EnableRandomPitchVariance;

    [FoldoutGroup("Advanced Settings"), ShowIf("EnableRandomPitchVariance")]
    public Vector2 RandomPitchVariance;

    [FoldoutGroup("Advanced Settings")] public bool allowMultiple = false;

    [FoldoutGroup("Advanced Settings"), ShowIf("allowMultiple")]
    public float pitchVariance = 0.05f;

    [FoldoutGroup("Advanced Settings"), ShowIf("allowMultiple")]
    public float pitchStackStep = 0.02f;

    [HideInInspector] public AudioSource source;
    [HideInInspector] public int playCount = 0;

    public void SetRandomClip()
    {
        if (!ChooseFromMultiple) return;
        if (Clips.Count <= 0) return;

        var randomClip = Clips[Random.Range(0, Clips.Count)];

        clip = randomClip;
        source.clip = randomClip;
    }
}


public class AudioManager : MonoBehaviour, ICoreSystemManager
{
    [Header("Music Settings")]
    [SerializeField] private float musicFadeDuration = 1.5f;
    private Sound currentBackgroundSound;
    private Coroutine musicTransitionCoroutine;

    [InfoBox("Organize your sounds into categories for clean inspector management.")]
    [SerializeField, ListDrawerSettings(Expanded = true)]
    public List<SoundCategory> soundCategories = new();


    // 🔊 Global toggles
    [Header("Global Toggles")]
    [SerializeField] private bool allowSound = true;
    [SerializeField] private bool allowMusic = true;

    public bool AllowSound => allowSound;
    public bool AllowMusic => allowMusic;

    public void Init()
    {
        foreach (Sound s in soundCategories.SelectMany(category => category.Sounds))
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // =========================
    // 🔧 Global Toggle Methods
    // =========================

    public void ToggleSound(bool enabled)
    {
        allowSound = enabled;
        PlayerPrefs.SetInt("AllowSound", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleMusic(bool enabled)
    {
        allowMusic = enabled;
        PlayerPrefs.SetInt("AllowMusic", enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!allowMusic)
            StopBackgroundMusic();
        else if (currentBackgroundSound != null)
        {
            currentBackgroundSound.source.Play();
            
            PlayBackgroundMusic(currentBackgroundSound.soundType);
        }

    }

    public void LoadAudioPrefs()
    {
        allowSound = PlayerPrefs.GetInt("AllowSound", 1) == 1;
        allowMusic = PlayerPrefs.GetInt("AllowMusic", 1) == 1;
    }
    
    public void Play(SoundType type)
    {
        if (!allowSound) return; // 🚫 sound muted

        var s = FindSound(type);
        if (s == null)
        {
            Debug.LogWarning($"⚠️ Sound '{type}' not found!");
            return;
        }

        if (s.ChooseFromMultiple)
            s.SetRandomClip();

        if (!s.allowMultiple)
        {
            s.source.volume = s.volume;
            s.source.pitch = s.pitch + Random.Range(-s.pitchVariance, s.pitchVariance);
            s.source.loop = s.loop;
            s.source.Play();
        }
        else
        {
            var tempGO = new GameObject($"TempAudio_{type}");
            var tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = s.clip;
            tempSource.volume = s.volume;

            float pitchOffset = Random.Range(-s.pitchVariance, s.pitchVariance);
            float stackedPitch = s.pitch + pitchOffset + (s.playCount * s.pitchStackStep);
            tempSource.pitch = stackedPitch;

            tempSource.loop = false;
            tempSource.Play();

            s.playCount++;
            Destroy(tempGO, s.clip.length / Mathf.Abs(tempSource.pitch));
            StartCoroutine(ResetCountAfterDelay(s, 0.2f));
        }
    }

    public void PlayBackgroundMusic(SoundType type)
    {
        if (!allowMusic) return;
        
        var newSound = FindSound(type);
        if (newSound == null)
        {
            Debug.LogWarning($"⚠️ Background sound '{type}' not found!");
            return;
        }
        
        if (currentBackgroundSound != null && currentBackgroundSound.soundType == type && currentBackgroundSound.source.isPlaying)
            return;

        if (musicTransitionCoroutine != null)
            StopCoroutine(musicTransitionCoroutine);

        musicTransitionCoroutine = StartCoroutine(TransitionMusic(newSound));
    }

    public void StopBackgroundMusic()
    {
        if (currentBackgroundSound == null) return;
        currentBackgroundSound.source.Stop();
    }

    public void PlayOneShot(SoundType type, float? volume = null, float? pitch = null, float delay = 0f)
    {
        if (!allowSound) return;
        
        StartCoroutine(PlayOneShotRoutine(type, volume, pitch, delay));
    }

    IEnumerator PlayOneShotRoutine(SoundType type, float? volume, float? pitch, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        var s = FindSound(type);
        if (s == null)
        {
            yield break;
        }

        if (s.ChooseFromMultiple)
        {
            if (s.Clips.Count <= 0)
            {
                yield break;
            }

            if (s.ChooseFromMultiple) s.SetRandomClip();
        }

        if (s.clip == null)
        {
            yield break;
        }

        float finalVolume = volume ?? s.volume;
        float finalPitch = pitch ?? s.pitch;
        if (s.EnableRandomPitchVariance) finalPitch += Random.Range(s.RandomPitchVariance.x, s.RandomPitchVariance.y);
        if (!s.allowMultiple)
        {
            var tempGO = new GameObject($"TempOneShot_{type}");
            var tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = s.clip;
            tempSource.volume = finalVolume;
            tempSource.pitch = finalPitch;
            tempSource.Play();
            Destroy(tempGO, s.clip.length / Mathf.Abs(finalPitch));
        }
        else
        {
            float pitchOffset = Random.Range(-s.pitchVariance, s.pitchVariance);
            float stackedPitch = finalPitch + pitchOffset + (s.playCount * s.pitchStackStep);
            var tempGO = new GameObject($"TempOneShot_{type}");
            var tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = s.clip;
            tempSource.volume = finalVolume;
            tempSource.pitch = stackedPitch;
            tempSource.loop = false;
            tempSource.Play();
            s.playCount++;
            Destroy(tempGO, s.clip.length / Mathf.Abs(stackedPitch));
            StartCoroutine(ResetCountAfterDelay(s, 0.2f));
        }
    }

    private IEnumerator TransitionMusic(Sound newSound)
    {
        float fadeTime = musicFadeDuration;
        
        if (currentBackgroundSound != null && currentBackgroundSound.source != null)
        {

            if (currentBackgroundSound != null && currentBackgroundSound.source.isPlaying)
            {
                float startVolume = currentBackgroundSound.source.volume;
                float t = 0;
                while (t < fadeTime)
                {
                    t += Time.deltaTime;
                    currentBackgroundSound.source.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                    yield return null;
                }

                currentBackgroundSound.source.Stop();
                currentBackgroundSound.source.volume = startVolume;
            }
        }

        currentBackgroundSound = newSound;
        if (newSound.ChooseFromMultiple)
            newSound.SetRandomClip();

        newSound.source.clip = newSound.clip;
        newSound.source.loop = true;
        newSound.source.volume = 0f; 
        newSound.source.Play();
        
        float targetVolume = newSound.volume;
        
        float t2 = 0;
        while (t2 < fadeTime)
        {
            t2 += Time.deltaTime;
            newSound.source.volume = Mathf.Lerp(0f, targetVolume, t2 / fadeTime);
            yield return null;
        }

        newSound.source.volume = targetVolume;
        musicTransitionCoroutine = null;
    }


    IEnumerator ResetCountAfterDelay(Sound s, float delay)
    {
        yield return new WaitForSeconds(delay);
        s.playCount = Mathf.Max(0, s.playCount - 1);
    }
    
    Sound FindSound(SoundType type)
    {
        foreach (var cat in soundCategories)
        {
            var found = cat.Sounds.Find(s => s.soundType == type);
            if (found != null)
                return found;
        }

        return null;
    }
}