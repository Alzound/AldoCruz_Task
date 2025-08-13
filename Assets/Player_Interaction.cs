using UnityEngine;

public class Player_Interaction : MonoBehaviour
{
    [Header("Interaction Properties")]
    public bool isInteractable;
    IInteraction interactable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("NPC"))
        {
            isInteractable = true;
            GetInfoOfContact(collision.GetComponent<IInteraction>());
        }
    }

    private void OnTriggerExit(Collider other)
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
