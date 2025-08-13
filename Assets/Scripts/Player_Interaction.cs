using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    [Header("Interaction Properties")]
    public bool isInteractable;
    IInteraction interactable;

    //Is going to be more cost effective to use trigger colliders for interaction detection.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NPC"))
        {
            Debug.Log("Player is in contact with an NPC or interactable object.");
            //I need to add the change of the UI text here.     
            isInteractable = true;
            GetInfoOfContact(collision.GetComponent<IInteraction>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isInteractable = false;
        GetInfoOfContact(null);
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
