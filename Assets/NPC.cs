using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteraction
{
    [SerializeField] private List<string> dialogues = new();
    private int currentDialogueIndex = 0;
    [SerializeField] private string initialDialogue;
    [SerializeField] private AudioClip audioClip;

    public string InteractionMessage => initialDialogue;

    public void Interact()
    {
        if (currentDialogueIndex < dialogues.Count - 1)
        {
            currentDialogueIndex++;
        }
        else
        {
            currentDialogueIndex = 0;
        }
       
        Audio_Manager.instance.PlayInteractionAudio(audioClip);
     
    }
}
