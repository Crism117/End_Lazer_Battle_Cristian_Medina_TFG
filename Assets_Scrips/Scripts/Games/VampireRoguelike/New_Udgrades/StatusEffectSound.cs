using UnityEngine;

// ============================================================
// STATUS EFFECT SOUND (helper)
// Crea un AudioSource hijo del enemigo con el clip loop del efecto.
// Se destruye al terminar el efecto.
//
// Uso desde BleedEffect/BurnEffect/PoisonEffect:
//   _statusSound = StatusEffectSound.Create(gameObject, EffectType.Bleed);
//   ...
//   _statusSound?.Stop();
// ============================================================
public class StatusEffectSound : MonoBehaviour
{
    AudioSource _source;

    /// <summary>
    /// Crea un GameObject hijo del enemigo con un AudioSource loop.
    /// </summary>
    public static StatusEffectSound Create(GameObject enemyGO, EffectType type)
    {
        if (enemyGO == null || SoundManager.Instance == null) return null;

        var clip = SoundManager.Instance.GetStatusEffectClip(type);
        if (clip == null) return null;

        var go = new GameObject($"StatusSound_{type}");
        go.transform.SetParent(enemyGO.transform);
        go.transform.localPosition = Vector3.zero;

        var source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = SoundManager.Instance.GetStatusEffectVolume();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.priority = 64;

        // Asignar al grupo StatusEffects del Mixer
        var group = SoundManager.Instance.GetStatusEffectGroup();
        if (group != null) source.outputAudioMixerGroup = group;

        source.Play();

        var sfx = go.AddComponent<StatusEffectSound>();
        sfx._source = source;
        return sfx;
    }

    public void Stop()
    {
        if (_source != null) _source.Stop();
        Destroy(gameObject);
    }

    void OnDisable()
    {
        if (_source != null) _source.Stop();
    }
}
