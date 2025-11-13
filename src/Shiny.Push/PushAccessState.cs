namespace Shiny.Push;

public record PushAccessState(
    AccessState Status,
    string? RegistrationToken
)
{
    public static PushAccessState Denied { get; } = new(AccessState.Denied, null);

    public void Assert()
    {
        if (this.Status != AccessState.Available)
            throw new PermissionException("Push registration fail", this.Status);
    }
}
