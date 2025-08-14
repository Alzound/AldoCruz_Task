using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SlotUI : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, ICancelHandler,
    IPointerEnterHandler, IPointerExitHandler //Hover interface
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private GameObject playerRef;

    [Header("Tooltip Description")]
    [SerializeField] private RectTransform tooltipRoot;        //tooltip parent object.
    [SerializeField] private TextMeshProUGUI tooltipText;      //Text mesh pro description. 
    [SerializeField] private Vector2 tooltipOffset = new (72, -8);
    [SerializeField] private float tooltipDelay = 2f;          // delay for the hoover to activate the description. (2s)

    private Coroutine hoverRoutine;    //Hoover countdown coroutine.
    private bool tooltipVisible;       //If its already visible, avoid re-showing it.

    private Inventory inv;
    private int index;

    //Drag and Drop static variables
    private static SlotUI dragging;
    private static GameObject ghost;
    private static bool dropSucceeded;

    //Block click events while dragging
    private bool suppressClick = false;

    public void Bind(Inventory inventory, int idx)
    {
        inv = inventory; index = idx;
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnClickUse);

        HideTooltip(); // <- asegura que el tooltip arranque oculto
    }

    public void Render(ItemStack s)
    {
        bool has = !s.IsEmpty;
        if (icon) { icon.enabled = has; icon.sprite = has ? s.item.icon : null; }
        if (amountText) amountText.text = (has && s.item.stackable && s.amount > 1) ? s.amount.ToString() : "";
        GetComponent<Button>().interactable = has;

        if (!has) HideTooltip(); // <- si el slot queda vacío, oculta el tooltip
    }

    public void SetIndex(int idx) { index = idx; }

    //If the Drag happens, we suppress the click to avoid triggering it.
    void OnClickUse()
    {
        if (suppressClick) return;
        var player = playerRef ? playerRef : GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        var pc = player.GetComponent<Player_Controller>();
        var s = inv.Get(index);
        if (pc && !s.IsEmpty && s.item.actionIndex != 0)
            pc.TriggerItemEvent(s.item.actionIndex);

        inv.UseAt(index, player);
    }

    private static void DestroyGhost()
    {
        if (!ghost) return;
        var g = ghost; ghost = null;
        GameObject.Destroy(g);
    }

    private static void ResetDrag()
    {
        dropSucceeded = false;
        dragging = null;
        //Clears the current selection in the EventSystem to avoid issues with UI selection.
        var es = EventSystem.current;
        if (es) es.SetSelectedGameObject(null);
    }

    private IEnumerator ClearSuppressClickAtEndOfFrame()
    {
        yield return null; //To ensure that it doesnt click till the end of the frame
        suppressClick = false;
    }

    void LateUpdate()
    {
        if (ghost && dragging != null && !Input.GetMouseButton(0))
        {
            DestroyGhost();
            ResetDrag();
            //Releases the click at the end of the frame to avoid other selection issues.
            if (!suppressClick) return;
            StartCoroutine(ClearSuppressClickAtEndOfFrame());
        }
    }

    // ---------------- DRAG & DROP ----------------
    public void OnBeginDrag(PointerEventData e)
    {
        //Start the drag of UI slot.
        DestroyGhost();
        ResetDrag();
        suppressClick = true;

        // Cancel any pending hover/tooltip when dragging starts
        CancelHoverRoutine(); // <- nuevo
        HideTooltip();        // <- nuevo

        var s = inv.Get(index);
        if (s.IsEmpty) { StartCoroutine(ClearSuppressClickAtEndOfFrame()); return; }

        dragging = this;
        dropSucceeded = false;

        //Create the ghost icon.
        ghost = new GameObject("GhostIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        var canvas = GetComponentInParent<Canvas>();
        ghost.transform.SetParent(canvas.transform, false);
        ghost.transform.SetAsLastSibling();

        var img = ghost.GetComponent<Image>();
        img.sprite = icon ? icon.sprite : null;
        img.raycastTarget = false;

        var cg = ghost.GetComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.85f;

        var rt = ghost.GetComponent<RectTransform>();
        rt.sizeDelta = (transform as RectTransform).sizeDelta;

        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {
        //To ensure the ghost follows the mouse
        if (ghost) ghost.transform.position = e.position;
    }

    public void OnDrop(PointerEventData e)
    {
        // Al soltar, siempre cancela hover/tooltip para no dejarlo pegado
        CancelHoverRoutine(); // <- nuevo
        HideTooltip();        // <- nuevo

        if (dragging == null)
        {
            DestroyGhost(); //Just in case.
            return;
        }

        if (dragging == this)
        {
            //Same slot, no movement.
            dropSucceeded = true;
            DestroyGhost();
            ResetDrag();
            StartCoroutine(ClearSuppressClickAtEndOfFrame());
            return;
        }

        dropSucceeded = true;
        inv.MoveOrMerge(dragging.index, this.index);

        DestroyGhost();
        ResetDrag();
        StartCoroutine(ClearSuppressClickAtEndOfFrame());
    }

    public void OnEndDrag(PointerEventData e)
    {
        // Al terminar el drag, garantizamos limpiar tooltip/hover
        CancelHoverRoutine(); // <- nuevo
        HideTooltip();        // <- nuevo

        //This is to ensure that the ghost is destroyed even if the drop was not successful
        DestroyGhost();

        //If the drop was not successful, we can try to move the item to the nearest slot
        if (dragging == this && !dropSucceeded)
        {
            int to = inv.GetNearestIndex(e.position);
            if (to != index) inv.MoveOrMerge(index, to);
        }

        ResetDrag();
        StartCoroutine(ClearSuppressClickAtEndOfFrame());
    }

    public void OnCancel(BaseEventData e)
    {
        //Just cancel the drag
        CancelHoverRoutine(); // <- nuevo
        HideTooltip();        // <- nuevo

        DestroyGhost();
        ResetDrag();
        StartCoroutine(ClearSuppressClickAtEndOfFrame());
    }

    private void OnDisable()
    {
        //If this slot is being dragged, clean up
        if (dragging == this)
        {
            DestroyGhost();
            ResetDrag();
        }

        // Limpia cualquier estado de hover al desactivarse
        CancelHoverRoutine(); // <- nuevo
        HideTooltip();        // <- nuevo

        suppressClick = false;
    }

    // ---------------- HOVER (con delay 2s y sin repetir) ----------------
    public void OnPointerEnter(PointerEventData e)
    {
        // No tooltip while dragging; no repetir si ya está visible o ya hay rutina corriendo
        if (dragging != null) return;
        if (tooltipVisible) return;
        if (hoverRoutine != null) return;

        hoverRoutine = StartCoroutine(HoverDelay()); // comienza la cuenta de 2s
    }

    public void OnPointerExit(PointerEventData e)
    {
        CancelHoverRoutine(); // cancela el conteo si aún no mostró
        HideTooltip();        // oculta si estaba visible
    }

    private IEnumerator HoverDelay()
    {
        yield return new WaitForSeconds(tooltipDelay);
        hoverRoutine = null;
        ShowTooltip(); // al cumplir 2s, mostrar una sola vez
    }

    private void CancelHoverRoutine()
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }
    }

    private void ShowTooltip()
    {
        if (!tooltipRoot || !tooltipText) return;
        var s = inv.Get(index);
        if (s.IsEmpty || dragging != null) return;

        // Construye el texto del tooltip (nombre + cantidad + descripción)
        string title = string.IsNullOrEmpty(s.item.displayName) ? s.item.name : s.item.displayName;
        string desc = !string.IsNullOrWhiteSpace(s.item.description) ? s.item.description : s.item.category.ToString();
        string txt = (s.item.stackable && s.amount > 1) ? $"{title} x{s.amount}\n{desc}" : $"{title}\n{desc}";

        tooltipText.text = txt;

        // Posición simple relativa al Slot
        tooltipRoot.anchoredPosition = tooltipOffset;
        tooltipRoot.gameObject.SetActive(true);
        tooltipVisible = true; // evita re-entradas
    }

    private void HideTooltip()
    {
        if (tooltipRoot) tooltipRoot.gameObject.SetActive(false);
        tooltipVisible = false;
    }
}
