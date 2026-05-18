using UnityEngine;

// ============================================================
// AUTO LEVEL UP SYSTEM (v6)
// Cuando el jugador sube de nivel automáticamente:
//   + HP máximo
//   + Velocidad de movimiento
//   + Rango de ataque a distancia (rangedAttackBonus)
// ============================================================
public class AutoLevelUpSystem : MonoBehaviour
{
    [Header("─── BONUS POR NIVEL ──")]
    [Tooltip("HP máximo que se suma por cada nivel")]
    public int hpPerLevel = 5;

    [Tooltip("Velocidad de movimiento que se suma por nivel")]
    public float speedPerLevel = 0.2f;

    [Tooltip("Rango de ataque a distancia que se suma por nivel")]
    public float rangedRangePerLevel = 0.5f;

    [Header("─── OPCIONES ──")]
    public bool healOnLevelUp = true;

    [Header("─── REFERENCIAS ──")]
    public PlayerControllerRogueLite player;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Start()
    {
        if (player == null) player = FindObjectOfType<PlayerControllerRogueLite>();

        if (GameManagerRogueLite.Instance == null)
        {
            Debug.LogError("[AutoLevelUp] GameManagerRogueLite no encontrado.");
            return;
        }

        GameManagerRogueLite.Instance.OnLevelUp += HandleLevelUp;
    }

    void OnDestroy()
    {
        if (GameManagerRogueLite.Instance != null)
            GameManagerRogueLite.Instance.OnLevelUp -= HandleLevelUp;
    }

    void HandleLevelUp(int newLevel)
    {
        if (player == null) return;

        // ── HP ──
        int oldMaxHp = player.maxHealth;
        player.maxHealth += hpPerLevel;
        if (healOnLevelUp) player.Heal(hpPerLevel);

        // ── VELOCIDAD ──
        player.moveSpeed += speedPerLevel;

        // ── RANGO RANGED ──
        player.rangedAttackBonus += rangedRangePerLevel;

        if (PlayerStatsManager.Instance != null)
            PlayerStatsManager.Instance.Recalculate();

        if (debugMode)
            Debug.Log($"[AutoLevelUp] Nivel {newLevel} → HP {oldMaxHp}→{player.maxHealth}, Vel: {player.moveSpeed:F2}, RangedBonus: {player.rangedAttackBonus:F2}");
    }
}