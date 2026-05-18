using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

// ============================================================
// SOUND MANAGER
// Buzz denegado, Victoria, Derrota, Save Confirm
// usa mechanicsSource (separado de SFX armas)
// ============================================================

[System.Serializable]
public class WeaponSoundEntry
{
    [Tooltip("ID del arma (debe coincidir EXACTAMENTE con WeaponDefinition.id)")]
    public string weaponId;

    [Tooltip("Sonido al disparar (ranged) o golpear (melee)")]
    public AudioClip fireClip;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitchMin = 0.95f;
    [Range(0.5f, 1.5f)] public float pitchMax = 1.05f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("─── AUDIO MIXER GROUPS ──")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup statusEffectGroup;

    [Header("─── AUDIO SOURCES ──")]
    public AudioSource sfxSource;
    public AudioSource uiSource;
    public AudioSource musicSource;
    public AudioSource itemsSource;
    public AudioSource mechanicsSource;
    public AudioSource alertsSource;

    // ═══ CLIPS ═══════════════════════════════════════════════════════════

    [Header("─── PLAYER ──")]
    public AudioClip playerWalkClip;
    [Range(0f, 1f)] public float playerWalkVolume = 0.4f;
    public AudioClip playerHurtClip;
    [Range(0f, 1f)] public float playerHurtVolume = 0.7f;

    [Header("─── ENEMIGOS ──")]
    [Tooltip("Sonido genérico de daño compartido")]
    public AudioClip enemyHurtClip;
    [Range(0f, 1f)] public float enemyHurtVolume = 0.6f;

    [Header("─── HIT GENÉRICO ──")]
    [Tooltip("Sonido corto de impacto bala/melee")]
    public AudioClip hitGenericClip;
    [Range(0f, 1f)] public float hitGenericVolume = 0.5f;

    [Header("─── ARMAS RANGED ──")]
    public List<WeaponSoundEntry> rangedWeaponSounds = new List<WeaponSoundEntry>();

    [Header("─── ARMAS MELEE ──")]
    public List<WeaponSoundEntry> meleeWeaponSounds = new List<WeaponSoundEntry>();

    [Header("─── ITEMS ──")]
    public AudioClip itemSpawnClip;
    [Range(0f, 1f)] public float itemSpawnVolume = 0.5f;
    public AudioClip itemPickupClip;
    [Range(0f, 1f)] public float itemPickupVolume = 0.7f;

    [Header("─── MECÁNICAS ──")]
    public AudioClip coinPickupClip;
    [Range(0f, 1f)] public float coinPickupVolume = 0.4f;

    public AudioClip playerLevelUpClip;
    [Range(0f, 1f)] public float playerLevelUpVolume = 0.8f;

    public AudioClip weaponLevelUpClip;
    [Range(0f, 1f)] public float weaponLevelUpVolume = 0.7f;

    public AudioClip healRegenClip;
    [Range(0f, 1f)] public float healRegenVolume = 0.6f;

    [Header("─── NUEVOS v9: BUZZ / VICTORIA / DERROTA / SAVE ──")]
    [Tooltip("Buzz denegado al intentar comprar sin monedas")]
    public AudioClip deniedBuzzClip;
    [Range(0f, 1f)] public float deniedBuzzVolume = 0.7f;

    [Tooltip("Sonido al ganar la partida")]
    public AudioClip victoryClip;
    [Range(0f, 1f)] public float victoryVolume = 0.9f;

    [Tooltip("Sonido al perder la partida")]
    public AudioClip defeatClip;
    [Range(0f, 1f)] public float defeatVolume = 0.8f;

    [Tooltip("Sonido al guardar puntuación")]
    public AudioClip saveConfirmClip;
    [Range(0f, 1f)] public float saveConfirmVolume = 0.7f;

    [Header("─── ALERTAS / OLEADAS ──")]
    public AudioClip waveWarningClip;
    [Range(0f, 1f)] public float waveWarningVolume = 0.6f;
    public AudioClip waveEndBellClip;
    [Range(0f, 1f)] public float waveEndBellVolume = 0.7f;
    public AudioClip waveStartClip;
    [Range(0f, 1f)] public float waveStartVolume = 0.6f;

    [Header("─── STATUS EFFECTS (loop) ──")]
    public AudioClip bleedLoopClip;
    public AudioClip burnLoopClip;
    public AudioClip poisonLoopClip;
    [Range(0f, 1f)] public float statusEffectVolume = 0.3f;

    [Header("─── COUNTDOWN ──")]
    public AudioClip countdownAudioClip;
    [Range(0f, 1f)] public float countdownVolume = 0.9f;

    [Header("─── MÚSICA ──")]
    public AudioClip musicBackgroundClip;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("─── UI ──")]
    public AudioClip uiClickClip;
    [Range(0f, 1f)] public float uiClickVolume = 0.6f;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    Dictionary<string, WeaponSoundEntry> _weaponSoundsCache;

    // ═══════════════════════════════════════════════════════════════════
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        BuildWeaponCache();
        ConfigureSources();
    }

    void Start()
    {
        StartMusic();
    }

    void BuildWeaponCache()
    {
        _weaponSoundsCache = new Dictionary<string, WeaponSoundEntry>();
        foreach (var entry in rangedWeaponSounds)
            if (!string.IsNullOrEmpty(entry.weaponId))
                _weaponSoundsCache[entry.weaponId] = entry;
        foreach (var entry in meleeWeaponSounds)
            if (!string.IsNullOrEmpty(entry.weaponId))
                _weaponSoundsCache[entry.weaponId] = entry;

        if (debugMode)
            Debug.Log($"[SoundManager] Cache armas: {_weaponSoundsCache.Count} entradas.");
    }

    void ConfigureSources()
    {
        ConfigSource(sfxSource, sfxGroup, false);
        ConfigSource(uiSource, uiGroup, false);
        ConfigSource(musicSource, musicGroup, true);
        ConfigSource(itemsSource, sfxGroup, false);
        ConfigSource(mechanicsSource, sfxGroup, false);
        ConfigSource(alertsSource, uiGroup, false);
    }

    void ConfigSource(AudioSource src, AudioMixerGroup group, bool loop)
    {
        if (src == null) return;
        if (group != null) src.outputAudioMixerGroup = group;
        src.playOnAwake = false;
        src.loop = loop;
        src.priority = 0;
        src.spatialBlend = 0f;
    }

    // ═══════════════════════════════════════════════════════════════════
    // MÚSICA
    // ═══════════════════════════════════════════════════════════════════
    public void StartMusic()
    {
        if (musicSource == null || musicBackgroundClip == null) return;
        musicSource.clip = musicBackgroundClip;
        musicSource.volume = musicVolume;
        musicSource.loop = true;
        if (!musicSource.isPlaying) musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ARMAS
    // ═══════════════════════════════════════════════════════════════════
    public void PlayWeaponFire(string weaponId)
    {
        if (_weaponSoundsCache == null || sfxSource == null) return;

        if (!_weaponSoundsCache.TryGetValue(weaponId, out var entry))
        {
            if (debugMode) Debug.LogWarning($"[SoundManager] Arma no encontrada: '{weaponId}'");
            return;
        }
        if (entry.fireClip == null)
        {
            if (debugMode) Debug.LogWarning($"[SoundManager] '{weaponId}' sin clip.");
            return;
        }

        sfxSource.pitch = Random.Range(entry.pitchMin, entry.pitchMax);
        sfxSource.PlayOneShot(entry.fireClip, entry.volume);
    }

    /// <summary>
    /// FIX v9: usa mechanicsSource para evitar saturación con SFX armas.
    /// </summary>
    public void PlayHit()
    {
        PlayOneShot(mechanicsSource, hitGenericClip, hitGenericVolume);
    }

    // ═══════════════════════════════════════════════════════════════════
    // PLAYER
    // ═══════════════════════════════════════════════════════════════════
    public void PlayPlayerHurt()
    {
        PlayOneShot(sfxSource, playerHurtClip, playerHurtVolume);
    }

    public void PlayPlayerWalkStep()
    {
        if (sfxSource == null || playerWalkClip == null) return;
        sfxSource.pitch = Random.Range(0.92f, 1.08f);
        sfxSource.PlayOneShot(playerWalkClip, playerWalkVolume);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ENEMIGOS
    // ═══════════════════════════════════════════════════════════════════
    public void PlayEnemyHurt()
    {
        PlayOneShot(sfxSource, enemyHurtClip, enemyHurtVolume);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ITEMS
    // ═══════════════════════════════════════════════════════════════════
    public void PlayItemSpawn() => PlayOneShot(itemsSource, itemSpawnClip, itemSpawnVolume);
    public void PlayItemPickup() => PlayOneShot(itemsSource, itemPickupClip, itemPickupVolume);

    // ═══════════════════════════════════════════════════════════════════
    // MECÁNICAS
    // ═══════════════════════════════════════════════════════════════════
    public void PlayCoinPickup() => PlayOneShot(mechanicsSource, coinPickupClip, coinPickupVolume);
    public void PlayPlayerLevelUp() => PlayOneShot(mechanicsSource, playerLevelUpClip, playerLevelUpVolume);
    public void PlayWeaponLevelUp() => PlayOneShot(mechanicsSource, weaponLevelUpClip, weaponLevelUpVolume);
    public void PlayHealRegen() => PlayOneShot(mechanicsSource, healRegenClip, healRegenVolume);

    // ═══════════════════════════════════════════════════════════════════
    // NUEVOS v9
    // ═══════════════════════════════════════════════════════════════════
    public void PlayDeniedBuzz() => PlayOneShot(uiSource, deniedBuzzClip, deniedBuzzVolume);
    public void PlayVictory() => PlayOneShot(alertsSource, victoryClip, victoryVolume);
    public void PlayDefeat() => PlayOneShot(alertsSource, defeatClip, defeatVolume);
    public void PlaySaveConfirm() => PlayOneShot(uiSource, saveConfirmClip, saveConfirmVolume);

    // ═══════════════════════════════════════════════════════════════════
    // ALERTAS DE OLEADA
    // ═══════════════════════════════════════════════════════════════════
    public void PlayWaveWarning() => PlayOneShot(alertsSource, waveWarningClip, waveWarningVolume);
    public void PlayWaveEndBell() => PlayOneShot(alertsSource, waveEndBellClip, waveEndBellVolume);
    public void PlayWaveStart() => PlayOneShot(alertsSource, waveStartClip, waveStartVolume);

    // ═══════════════════════════════════════════════════════════════════
    // COUNTDOWN
    // ═══════════════════════════════════════════════════════════════════
    public void PlayCountdown() => PlayOneShot(uiSource, countdownAudioClip, countdownVolume);
    public float GetCountdownDuration() => countdownAudioClip != null ? countdownAudioClip.length : 4f;

    // ═══════════════════════════════════════════════════════════════════
    // UI
    // ═══════════════════════════════════════════════════════════════════
    public void PlayUIClick() => PlayOneShot(uiSource, uiClickClip, uiClickVolume);

    // ═══════════════════════════════════════════════════════════════════
    // STATUS EFFECTS (loop)
    // ═══════════════════════════════════════════════════════════════════
    public AudioClip GetStatusEffectClip(EffectType type) => type switch
    {
        EffectType.Bleed => bleedLoopClip,
        EffectType.Burn => burnLoopClip,
        EffectType.Poison => poisonLoopClip,
        _ => null,
    };

    public float GetStatusEffectVolume() => statusEffectVolume;
    public AudioMixerGroup GetStatusEffectGroup() => statusEffectGroup;

    // ═══════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════
    void PlayOneShot(AudioSource src, AudioClip clip, float vol)
    {
        if (src == null || clip == null) return;
        src.pitch = 1f;
        src.PlayOneShot(clip, vol);
    }
}