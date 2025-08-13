using TMPro;
using UnityEngine;

public class Player_UIPopUp : MonoBehaviour
{
    [Header("UI (Burbuja)")]
    [SerializeField] private Canvas bubbleCanvas; 
    [SerializeField] private float bubbleTime = 3f;

    private void Awake()
    {
        bubbleCanvas = GetComponentInChildren<Canvas>();
        if (bubbleCanvas) bubbleCanvas.gameObject.SetActive(false);
    }

    public void TooglePopUpActive()
    {
        bubbleCanvas.gameObject.SetActive(true);
    }

    public void TooglePopUpFalse()
    {
        bubbleCanvas.gameObject.SetActive(false);
    }
}
