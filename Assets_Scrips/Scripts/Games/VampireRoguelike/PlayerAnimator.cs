using UnityEngine;

// ============================================================
// PLAYER ANIMATOR
// Controla las 5 animaciones del jugador:
//   - Idle        (parado, sin moverse)
//   - Walk_Down   (frente, mirando hacia abajo)
//   - Walk_Up     (espalda, mirando hacia arriba)
//   - Walk_Left   (lado izquierdo)
//   - Walk_Right  (lado derecho)
//
// SETUP EN UNITY:
// 1. Adjunta este script al Player
// 2. Crea un Animator Controller con 5 estados
// 3. Añade los parámetros:
//      - MoveX    (Float)
//      - MoveY    (Float)
//      - IsMoving (Bool)
// 4. Conecta las transiciones según la tabla de abajo
// ============================================================
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    Animator _anim;
    Rigidbody2D _rb;

    // Última dirección antes de pararse (para que quede mirando
    // hacia donde iba cuando se detiene)
    Vector2 _lastDir = Vector2.down;

    // Hashes de los parámetros del Animator (más eficiente que strings)
    static readonly int MoveX = Animator.StringToHash("MoveX");
    static readonly int MoveY = Animator.StringToHash("MoveY");
    static readonly int IsMoving = Animator.StringToHash("IsMoving");

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Lee la velocidad actual del Rigidbody2D
        Vector2 vel = _rb != null ? _rb.velocity : Vector2.zero;
        bool moving = vel.magnitude > 0.1f;

        // Guarda la última dirección solo cuando el jugador se mueve
        // Así al pararse queda mirando hacia donde iba
        if (moving) _lastDir = vel.normalized;

        // Actualiza los parámetros del Animator
        _anim.SetFloat(MoveX, _lastDir.x);
        _anim.SetFloat(MoveY, _lastDir.y);
        _anim.SetBool(IsMoving, moving);

        // Cuando IsMoving = false el Animator va al estado Idle
        // Cuando IsMoving = true va a Walk_Down/Up/Left/Right según MoveX y MoveY
    }
}

// ============================================================
// TABLA DE TRANSICIONES EN EL ANIMATOR CONTROLLER
//
// Parámetros necesarios:
//   IsMoving (Bool)
//   MoveX    (Float)
//   MoveY    (Float)
//
// Transiciones desde "Any State":
//
//  → Idle       : IsMoving = false
//  → Walk_Down  : IsMoving = true  AND  MoveY < -0.5
//  → Walk_Up    : IsMoving = true  AND  MoveY >  0.5
//  → Walk_Left  : IsMoving = true  AND  MoveX < -0.5
//  → Walk_Right : IsMoving = true  AND  MoveX >  0.5
//
// En TODAS las transiciones:
//   - Has Exit Time: DESACTIVADO
//   - Transition Duration: 0
//
// Estado por defecto (Entry): Idle
// ============================================================
