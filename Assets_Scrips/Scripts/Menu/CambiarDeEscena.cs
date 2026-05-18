using UnityEngine;
using UnityEngine.SceneManagement;

public class CambiarDeEscena : MonoBehaviour
{
    public void IrAlTresEnRaya()
    {
        SceneManager.LoadScene("TresEnRaya");
    }

    public void IrAlMenuPrincipal()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void IrAlRogueLitePrototype()
    {
        SceneManager.LoadScene("RogueLitePrototype");
    }
}