using UnityEngine;
using System;

// ============================================================
// COIN SYSTEM
// Maneja las monedas del jugador.
// Otros scripts llaman CoinSystem.Instance.AddCoins(cantidad)
// para dar monedas, y SpendCoins(cantidad) para gastarlas.
// ============================================================
public class CoinSystem : MonoBehaviour
{
    public static CoinSystem Instance { get; private set; }

    // Cuántas monedas tiene el jugador ahora mismo
    public int CurrentCoins { get; private set; } = 0;

    // Evento: avisa al UI cada vez que cambian las monedas
    public event Action<int> OnCoinsChanged;

    void Awake()
    {
        // Solo puede existir una instancia (patrón Singleton)
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Dar monedas al jugador (se llama cuando muere un enemigo)
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        OnCoinsChanged?.Invoke(CurrentCoins); // Avisa al UI
    }

    // Intentar gastar monedas. Devuelve true si había suficientes.
    public bool SpendCoins(int amount)
    {
        if (CurrentCoins < amount) return false; // No hay suficientes
        CurrentCoins -= amount;
        OnCoinsChanged?.Invoke(CurrentCoins);
        return true;
    }

    // Resetea las monedas al reiniciar la partida
    public void Reset()
    {
        CurrentCoins = 0;
        OnCoinsChanged?.Invoke(CurrentCoins);
    }
}