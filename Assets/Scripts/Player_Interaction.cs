using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    [Header("Interaction Properties")]
    public bool isInteractable;
    IInteraction interactable;

    [Header("Canvas PopUp")]
    [SerializeField] private Player_UIPopUp playerPopUp;

    private void Awake()
    {
        playerPopUp = GetComponentInChildren<Player_UIPopUp>();
    }


    //Is going to be more cost effective to use trigger colliders for interaction detection.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NPC"))
        { 
            playerPopUp.TooglePopUpActive();
            isInteractable = true;
            GetInfoOfContact(collision.GetComponent<IInteraction>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isInteractable = false;
        GetInfoOfContact(null);
        playerPopUp.TooglePopUpFalse();
    }

    public void GetInfoOfContact(IInteraction interactObject)
    {
        interactable = interactObject;
    }

    public void Interact()
    {
        interactable.Interact();
    }
}
