using System;

namespace Shiny;

public class PermissionException : Exception
{
    public PermissionException(string module, AccessState badStatus) : base($"{module} had status of {badStatus}") { }
    
    // /// <summary>
    // /// Asserts that AccessState is available (or allows restricted)
    // /// </summary>
    // /// <param name="state"></param>
    // /// <param name="message"></param>
    // /// <param name="allowRestricted"></param>
    // /// <exception cref="ArgumentException"></exception>
    // public static void Assert(this AccessState state, string? message = null, bool allowRestricted = false)
    // {
    //     if (state == AccessState.Available)
    //         return;
    //
    //     if (allowRestricted && state == AccessState.Restricted)
    //         return;
    //
    //     throw new InvalidOperationException(message ?? $"Invalid State " + state);
    // }
}
