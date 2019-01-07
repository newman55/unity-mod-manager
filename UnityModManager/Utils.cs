using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using dnlib.DotNet;

namespace UnityModManagerNet.Installer
{
    static class Utils
    {
        public static Version ParseVersion(string str)
        {
            var array = str.Split('.', ',');
            if (array.Length >= 3)
            {
                var regex = new Regex(@"\D");
                return new Version(int.Parse(regex.Replace(array[0], "")), int.Parse(regex.Replace(array[1], "")), int.Parse(regex.Replace(array[2], "")));
            }

            Log.Print($"Error parsing version '{str}'.");
            return new Version();
        }

        public static bool ParsePatchTarget(ModuleDefMD assembly, string patchTarget, out MethodDef foundMethod, out string insertionPlace)
        {
            foundMethod = null;
            insertionPlace = null;

            string className = null;
            string methodName = null;

            var pos = patchTarget.LastIndexOf('.');
            if (pos != -1)
            {
                className = patchTarget.Substring(0, pos);

                var pos2 = patchTarget.LastIndexOf(':');
                if (pos2 != -1)
                {
                    methodName = patchTarget.Substring(pos + 1, pos2 - pos - 1);
                    insertionPlace = patchTarget.Substring(pos2 + 1).ToLower();

                    if (insertionPlace != "after" && insertionPlace != "before")
                        Log.Print($"Parameter '{insertionPlace}' in '{patchTarget}' is unknown.");
                }
                else
                {
                    methodName = patchTarget.Substring(pos + 1);
                }

                if (methodName == "ctor")
                    methodName = ".ctor";
            }
            else
            {
                Log.Print($"Function name error '{patchTarget}'.");
                return false;
            }

            var targetClass = assembly.Types.FirstOrDefault(x => x.FullName == className);
            if (targetClass == null)
            {
                Log.Print($"Class '{className}' not found.");
                return false;
            }

            foundMethod = targetClass.Methods.FirstOrDefault(x => x.Name == methodName);
            if (foundMethod == null)
            {
                Log.Print($"Method '{methodName}' not found.");
                return false;
            }

            return true;
        }
        public static string ResolveOSXFileUrl(string url)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "osascript";
            p.StartInfo.Arguments = $"-e \"get posix path of posix file \\\"{url}\\\"\"";
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            return output.TrimEnd();
        }
    }
}
