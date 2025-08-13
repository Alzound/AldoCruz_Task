using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteraction
{
    [Header("NPC Properties")]
    [SerializeField] private List<string> dialogues = new();
    private int currentDialogueIndex = 0;
    [SerializeField] private string initialDialogue;
    [SerializeField] private AudioClip audioClip;


    [Header("Animator")]
    [SerializeField] private Animator player_Animator;
    public string currentState;
    private const string IDLE_RIGHT = "NPC_IdleRight", IDLE_LEFT = "NPC_IdleLeft";
    private const string INTERACT_RIGHT = "NPC_InteractRight", INTERACT_LEFT = "NPC_InteractLeft";
    private Coroutine idleCoroutine;

    public string InteractionMessage => initialDialogue;

    public void Interact()
    {
        ChangeAnimationState(Player_Controller.instance.transform.rotation.x > 0 ? INTERACT_RIGHT : INTERACT_LEFT);
        if (currentDialogueIndex < dialogues.Count - 1)
        {
            currentDialogueIndex++;
        }
        else
        {
            currentDialogueIndex = 0;
        }
       
        Audio_Manager.instance.PlayInteractionAudio(audioClip);
        //I added this to stop acummulation of coroutines. 
        if(idleCoroutine != null)
        {
            StopCoroutine(idleCoroutine);
        }
        idleCoroutine = StartCoroutine(PlayIDLE());
    }

    #region Animation
    private void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        player_Animator.CrossFade(newState, 0.1f);
        currentState = newState;
    }

    private IEnumerator PlayIDLE()
    {
        yield return new WaitForSeconds(3);
        ChangeAnimationState(Player_Controller.instance.transform.rotation.x > 0 ? IDLE_RIGHT : IDLE_LEFT);
    }
    #endregion
}
