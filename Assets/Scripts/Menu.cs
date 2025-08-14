using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    [Header("Menu")]
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartGame()
    {
        Debug.Log("Start Game");

        animator.SetBool("play", true);  

    }

    public void ActivatePlayer()
    {
        var PC = Player_Controller.instance;
        PC.gameObject.GetComponent<PlayerInput>().enabled = true;
        PC.gameObject.GetComponent<Player_Controller>().enabled = true;
        PC.gameObject.GetComponent<Toogle_Inventory>().ToggleInventory();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
