public interface IInteraction
{
    public string UiText { get; set; }
    public bool CanInteract { get; set; }

    void InteractAction();

    void InteractText(string text); 

}