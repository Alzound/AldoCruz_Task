using TMPro;
using UnityEngine;

public class UI_Manager : MonoBehaviour
{
    public static UI_Manager instance;

    [Header("UI Interactions")]
    public GameObject panelUI;
    public TextMeshProUGUI text;

    [Header("UI Money")]
    public TextMeshProUGUI moneyText;
    public int money = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #region Interactions
    public void ChangeText(string newText)
    {
        text.text = newText;
    }

    public void ToggleUI()
    {
        panelUI.SetActive(!panelUI.activeSelf);
    }
    #endregion
}
