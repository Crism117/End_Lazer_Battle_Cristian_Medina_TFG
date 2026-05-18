// Guarda y carga los ajustes entre escenas y sesiones
// Usa PlayerPrefs (se guarda en disco automáticamente)

using UnityEngine;

public static class GuardarAjustes
{
    // Guardar valores
    public static void GuardarBrillo(float valor)
    {
        PlayerPrefs.SetFloat("Brillo", valor);
        PlayerPrefs.Save();
    }

    public static void GuardarVolumen(float valor)
    {
        PlayerPrefs.SetFloat("Volumen", valor);
        PlayerPrefs.Save();
    }

    public static void GuardarMusica(bool encendida)
    {
        PlayerPrefs.SetInt("Musica", encendida ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void GuardarCRT(bool encendido)
    {
        PlayerPrefs.SetInt("CRT", encendido ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Cargar valores (con valores por defecto)
    public static float CargarBrillo()
    {
        return PlayerPrefs.GetFloat("Brillo", 1f);
    }

    public static float CargarVolumen()
    {
        return PlayerPrefs.GetFloat("Volumen", 1f);
    }

    public static bool CargarMusica()
    {
        return PlayerPrefs.GetInt("Musica", 1) == 1;
    }

    public static bool CargarCRT()
    {
        return PlayerPrefs.GetInt("CRT", 1) == 1;
    }
}