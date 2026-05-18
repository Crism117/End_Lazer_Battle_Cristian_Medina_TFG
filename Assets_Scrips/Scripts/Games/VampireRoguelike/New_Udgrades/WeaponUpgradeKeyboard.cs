using UnityEngine;

// ============================================================
// WEAPON UPGRADE KEYBOARD
// Soporte para teclas 1, 2, 3 como alternativa al ratón
// para subir nivel a las armas del jugador.
//
// Funciona en PARALELO al sistema de clicks del ratón.
// NO funciona si el juego está pausado.
//
// USO:
// 1. Añadir este componente al GameObject del Player
//    (donde está WeaponSystemV2)
// 2. Opcional: asignar el WeaponSystem manualmente,
//    si no lo busca solo
// ============================================================
public class WeaponUpgradeKeyboard : MonoBehaviour
{
    [Header("─── REFERENCIA ──")]
    [Tooltip("Si lo dejas vacío, se busca en el propio GameObject o sus padres")]
    public WeaponSystemV2 weaponSystem;

    [Header("─── TECLAS ──")]
    public KeyCode key1 = KeyCode.Alpha1;
    public KeyCode key2 = KeyCode.Alpha2;
    public KeyCode key3 = KeyCode.Alpha3;

    [Header("─── OPCIONES ──")]
    [Tooltip("También permitir teclas del numpad (1, 2, 3)")]
    public bool alsoUseNumpad = true;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Start()
    {
        if (weaponSystem == null)
        {
            weaponSystem = GetComponent<WeaponSystemV2>();
            if (weaponSystem == null)
                weaponSystem = GetComponentInParent<WeaponSystemV2>();
            if (weaponSystem == null)
                weaponSystem = GetComponentInChildren<WeaponSystemV2>();
        }

        if (weaponSystem == null)
            Debug.LogWarning("[WeaponUpgradeKeyboard] No se encontró WeaponSystemV2.");
    }

    void Update()
    {
        if (weaponSystem == null) return;

        // No procesar si juego pausado
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused)
            return;

        // Tecla 1 → slot 0
        if (Input.GetKeyDown(key1) || (alsoUseNumpad && Input.GetKeyDown(KeyCode.Keypad1)))
            TryUpgrade(0);

        // Tecla 2 → slot 1
        if (Input.GetKeyDown(key2) || (alsoUseNumpad && Input.GetKeyDown(KeyCode.Keypad2)))
            TryUpgrade(1);

        // Tecla 3 → slot 2
        if (Input.GetKeyDown(key3) || (alsoUseNumpad && Input.GetKeyDown(KeyCode.Keypad3)))
            TryUpgrade(2);
    }

    void TryUpgrade(int index)
    {
        if (debugMode)
            Debug.Log($"[KeyboardUpgrade] Tecla → upgrade arma {index}");

        bool ok = weaponSystem.TryUpgradeWeapon(index);

        if (debugMode)
            Debug.Log($"[KeyboardUpgrade] Resultado: {(ok ? "ÉXITO" : "FALLO")}");

        // El sonido (success / buzz) ya se reproduce desde TryUpgradeWeapon
    }
}
