using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // ← para Text

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

    [Header("UI (Burbuja)")]
    [SerializeField] private Canvas bubbleCanvas;  // Canvas (World Space) hijo del NPC
    [SerializeField] private TextMeshProUGUI bubbleText;      // Texto dentro del Canvas
    [SerializeField] private float bubbleTime = 3f;

    public string InteractionMessage => initialDialogue;

    private void Awake()
    {
        if (bubbleCanvas) bubbleCanvas.gameObject.SetActive(false); // oculta al inicio
    }

    public void Interact()
    {
        ChangeAnimationState(Player_Controller.instance.transform.rotation.x > 0 ? INTERACT_RIGHT : INTERACT_LEFT);

        //Shows the dialogues, then loops back to the start
        if (currentDialogueIndex < dialogues.Count - 1) currentDialogueIndex++;
        else currentDialogueIndex = 0;

        //Show dialogue in bubble
        if (bubbleCanvas)
        {
            if (bubbleText)
            {
                string msg = (dialogues != null && dialogues.Count > 0) ? dialogues[currentDialogueIndex] : initialDialogue;
                bubbleText.text = msg;
            }
            bubbleCanvas.gameObject.SetActive(true);
        }

        // Sonido
        Audio_Manager.instance.PlayInteractionAudio(audioClip);

        //Stops coroutine if it's already running
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
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
        yield return new WaitForSeconds(bubbleTime); // mismo tiempo para ocultar
        if (bubbleCanvas) bubbleCanvas.gameObject.SetActive(false);
        ChangeAnimationState(Player_Controller.instance.transform.rotation.x > 0 ? IDLE_RIGHT : IDLE_LEFT);
    }
    #endregion
}
