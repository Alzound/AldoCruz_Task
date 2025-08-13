using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    Animator animator;
    AnimationClip animationClip; 

    public void StartGame()
    {
        Debug.Log("Start Game");
        Player_Controller.instance.gameObject.GetComponent<PlayerInput>().enabled = true;
        //animator.Play(); 
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
