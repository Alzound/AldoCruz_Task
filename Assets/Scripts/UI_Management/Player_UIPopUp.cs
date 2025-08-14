using TMPro;
using UnityEngine;

public class Player_UIPopUp : MonoBehaviour
{
    [Header("UI (Burbuja)")]
    [SerializeField] private Canvas bubbleCanvas; 

    private void Awake()
    {
        bubbleCanvas = GetComponentInChildren<Canvas>();
        if (bubbleCanvas) bubbleCanvas.gameObject.SetActive(false);
    }
    //Activates or deactivates the pop up dialogue bubble. 
    public void TooglePopUpActive()
    {
        bubbleCanvas.gameObject.SetActive(true);
    }

    public void TooglePopUpFalse()
    {
        bubbleCanvas.gameObject.SetActive(false);
    }
}
