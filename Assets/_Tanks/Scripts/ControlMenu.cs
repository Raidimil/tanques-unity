using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlMenu : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelControles;

    public void Jugar()
    {
        SceneManager.LoadScene("Principal");
    }
    // Método para regresar al Menú Principal
    public void IrAlMenu()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void AbrirControles()
    {
        if (panelControles != null)
            panelControles.SetActive(true);
    }

    public void CerrarControles()
    {
        if (panelControles != null)
            panelControles.SetActive(false);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}