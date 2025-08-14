using UnityEngine;

public class Pause_Menu : MonoBehaviour
{
    public static Pause_Menu instance;

    [SerializeField] Inventory inventory;
    [SerializeField] Canvas pauseCanvas;

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        if (!pauseCanvas) pauseCanvas = GetComponentInChildren<Canvas>(true);

        // No desactives TODO el objeto; solo oculta el men� visual:
        if (pauseCanvas) pauseCanvas.gameObject.SetActive(false);
        // Importante: deja este GameObject ACTIVO para que Start() se ejecute.
    }

    void Start()
    {
        // Start corre despu�s de todos los Awake: aqu� s� suele existir el Inventory
        ResolveInventory();
    }

    void ResolveInventory()
    {
        if (!inventory) inventory = Object.FindFirstObjectByType<Inventory>();
        // (si tu Unity no tiene FindFirstObjectByType, usa FindObjectOfType<Inventory>())
    }

    public void ShowMenu()
    {
        if (pauseCanvas) pauseCanvas.gameObject.SetActive(true);
        // activa hijos por si acaso
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Delete()
    {
        PlayerPrefs.DeleteAll();
    }

    public void Resume()
    {
        if (pauseCanvas) pauseCanvas.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    public void Save()
    {
        if (!inventory) ResolveInventory();     // reintenta si a�n no estaba
        if (inventory) { inventory.SaveToPrefs(); Debug.Log("Game saved successfully."); }
        else Debug.LogWarning("No Inventory found to save.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
