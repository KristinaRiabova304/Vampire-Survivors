using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundFX : MonoBehaviour
{
    public enum Sfx { Shoot, Hit, Death, Pickup, LevelUp, PlayerHurt }

    static SoundFX instance;

    [SerializeField, Range(0f, 1f)] float volume = 0.5f;
    [SerializeField] AudioClip shoot, hit, death, pickup, levelUp, playerHurt;

    AudioSource source;

    void Awake()
    {
        instance = this;
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;

        if (shoot == null) shoot = Tone("shoot", 0.06f, 900f, 500f, 0f, 0.25f);
        if (hit == null) hit = Tone("hit", 0.05f, 300f, 200f, 0.7f, 0.5f);
        if (death == null) death = Tone("death", 0.25f, 400f, 60f, 0.5f, 0.8f);
        if (pickup == null) pickup = Tone("pickup", 0.08f, 700f, 1400f, 0f, 0.4f);
        if (levelUp == null) levelUp = Tone("levelup", 0.4f, 400f, 1200f, 0f, 0.6f);
        if (playerHurt == null) playerHurt = Tone("hurt", 0.15f, 200f, 90f, 0.3f, 0.8f);
    }

    public static void Play(Sfx sfx)
    {
        if (instance == null) return;
        instance.source.PlayOneShot(instance.Clip(sfx), instance.volume);
    }

    AudioClip Clip(Sfx sfx) => sfx switch
    {
        Sfx.Shoot => shoot,
        Sfx.Hit => hit,
        Sfx.Death => death,
        Sfx.Pickup => pickup,
        Sfx.LevelUp => levelUp,
        _ => playerHurt,
    };

    static AudioClip Tone(string name, float duration, float freqFrom, float freqTo, float noise, float gain)
    {
        const int rate = 44100;
        int n = Mathf.CeilToInt(rate * duration);
        var data = new float[n];
        var rnd = new System.Random(name.GetHashCode());
        double phase = 0;

        for (int i = 0; i < n; i++)
        {
            float t = (float)i / n;
            phase += 2.0 * Mathf.PI * Mathf.Lerp(freqFrom, freqTo, t) / rate;
            float square = Mathf.Sin((float)phase) >= 0 ? 1f : -1f;
            float white = (float)(rnd.NextDouble() * 2.0 - 1.0);
            float env = (1f - t) * (1f - t);
            data[i] = Mathf.Lerp(square, white, noise) * env * gain * 0.4f;
        }

        var clip = AudioClip.Create(name, n, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
