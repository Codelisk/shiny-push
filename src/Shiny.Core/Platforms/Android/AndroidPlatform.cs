using System;
using System.IO;
using Android.OS;

namespace Shiny;


public class AndroidPlatform : IPlatform
{
    public DirectoryInfo AppData => AndroidShinyHost.AppData;


    readonly Handler handler = new(Looper.MainLooper);
    public void InvokeOnMainThread(Action action)
    {
        if (Looper.MainLooper!.IsCurrentThread)
            action();
        else
            this.handler.Post(action);
    }
}