using System;
using System.IO;

namespace Shiny;


public interface IPlatform
{
    // TODO: DirectoryInfo Temp { get; } caches or is Path.GetTempPath() enough?
    // TODO: what if connectivity & battery are here?
    // TODO: app & device info
    
    DirectoryInfo AppData { get; }
    void InvokeOnMainThread(Action action);
}