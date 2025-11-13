// using System;
// using System.IO;
// using System.Reflection;
// using System.Threading;
// using System.Threading.Tasks;
//
// namespace Shiny;
//
//
// public static class PlatformExtensions
// {

//
//
//     public static string ResourceToFilePath(this IPlatform platform, Assembly assembly, string resourceName)
//     {
//         var path = Path.Combine(platform.AppData.FullName, resourceName);
//         if (!File.Exists(path))
//         {
//             using var stream = assembly.GetManifestResourceStream(resourceName);
//             using var fs = File.Create(path);
//
//             stream!.CopyTo(fs);
//         }
//         return path;
//     }
// }
