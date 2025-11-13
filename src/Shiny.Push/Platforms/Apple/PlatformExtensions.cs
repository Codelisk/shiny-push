using System;
using Foundation;
using UIKit;

namespace Shiny.Push;


public static class PlatformExtensions
{
    public static string ToPushTokenString(this NSData deviceToken)
    {
        string token = null!;
        if (deviceToken.Length > 0)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                var data = deviceToken.ToArray();
                token = BitConverter
                    .ToString(data)
                    .Replace("-", "")
                    .Replace("\"", "");
            }
            else if (!String.IsNullOrWhiteSpace(deviceToken.Description))
            {
                token = deviceToken.Description.Trim('<', '>');
            }
        }
        return token;
    }
}