namespace Shiny;


public interface IApplicationLifecycle
{
    void OnForeground();
    void OnBackground();
}