namespace Shiny;


public abstract class ShinyLifecycleTask : IShinyStartupTask, IApplicationLifecycle
{
    public virtual void Start() { }

    /// <summary>
    /// This will be null during Start, meaning that full start has taken place
    /// </summary>
    public bool? IsInForeground { get; private set; }
    protected virtual void OnStateChanged(bool backgrounding) { }


    public void OnBackground()
    {
        this.IsInForeground = false;
        this.OnStateChanged(true);
    }


    public virtual void OnForeground()
    {
        this.IsInForeground = true;
        this.OnStateChanged(false);
    }

}