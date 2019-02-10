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
        static Utils()
        {

        }

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

        public static bool IsDirectoryWritable(string dirpath)
        {
            try
            {
                if (Directory.Exists(dirpath))
                    using (FileStream fs = File.Create(Path.Combine(dirpath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                    {; }
                return true;
            }
            catch
            {
                Log.Print($"Directory '{dirpath}' does not have write permission.");
                return false;
            }
        }

        public static bool IsFileWritable(string filepath)
        {
            try
            {
                if (File.Exists(filepath))
                    using (FileStream fs = File.OpenWrite(filepath))
                    {; }
                return true;
            }
            catch
            {
                Log.Print($"File '{filepath}' does not have write permission.");
                return false;
            }
        }

        public static bool TryParseEntryPoint(string str, out string assembly)
        {
            assembly = string.Empty;
            return TryParseEntryPoint(str, out assembly, out _, out _, out _);
        }

        public static bool TryParseEntryPoint(string str, out string assembly, out string @class, out string method, out string insertionPlace)
        {
            assembly = string.Empty;
            @class = string.Empty;
            method = string.Empty;
            insertionPlace = string.Empty;

            var regex = new Regex(@"(?:(?<=\[)(?'assembly'.+(?>\.dll))(?=\]))|(?:(?'class'[\w|\.]+)(?=\.))|(?:(?<=\.)(?'func'\w+))|(?:(?<=\:)(?'mod'\w+))", RegexOptions.IgnoreCase);
            var matches = regex.Matches(str);
            var groupNames = regex.GetGroupNames();

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    foreach (var group in groupNames)
                    {
                        if (match.Groups[group].Success)
                        {
                            switch (group)
                            {
                                case "assembly":
                                    assembly = match.Groups[group].Value;
                                    break;
                                case "class":
                                    @class = match.Groups[group].Value;
                                    break;
                                case "func":
                                    method = match.Groups[group].Value;
                                    if (method == "ctor")
                                        method = ".ctor";
                                    else if (method == "cctor")
                                        method = ".cctor";
                                    break;
                                case "mod":
                                    insertionPlace = match.Groups[group].Value.ToLower();
                                    break;
                            }
                        }
                    }
                }
                //Log.Print(assembly);
                //Log.Print(@class);
                //Log.Print(method);
                //Log.Print(insertionPlace);
            }

            var hasError = false;

            if (string.IsNullOrEmpty(assembly))
            {
                hasError = true;
                Log.Print("Assembly name not found.");
            }

            if (string.IsNullOrEmpty(@class))
            {
                hasError = true;
                Log.Print("Class name not found.");
            }

            if (string.IsNullOrEmpty(method))
            {
                hasError = true;
                Log.Print("Method name not found.");
            }

            if (hasError)
            {
                Log.Print($"Error parsing EntryPoint '{str}'.");
                return false;
            }

            return true;
        }

        public static bool TryGetEntryPoint(ModuleDefMD assemblyDef, string str, out MethodDef foundMethod, out string insertionPlace, bool createConstructor = false)
        {
            foundMethod = null;

            if (!TryParseEntryPoint(str, out var assembly, out var className, out var methodName, out insertionPlace))
            {
                return false;
            }

            var targetClass = assemblyDef.Types.FirstOrDefault(x => x.FullName == className);
            if (targetClass == null)
            {
                Log.Print($"Class '{className}' not found.");
                return false;
            }

            foundMethod = targetClass.Methods.FirstOrDefault(x => x.Name == methodName);
            if (foundMethod == null)
            {
                if (createConstructor && methodName == ".cctor")
                {
                    //var m = new MethodDefUser(".cctor", assemblyDef.CorLibTypes.Void, MethodAttributes.Private | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.Static);
                    var typeDef = ModuleDefMD.Load(typeof(Utils).Module).Types.FirstOrDefault(x => x.FullName == typeof(Utils).FullName);
                    var method = typeDef.Methods.FirstOrDefault(x => x.Name == ".cctor");
                    if (method != null)
                    {
                        typeDef.Methods.Remove(method);
                        targetClass.Methods.Add(method);
                        foundMethod = method;
                        
                        return true;
                    }
                }
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

        public static bool IsUnixPlatform()
        {
            int p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }

        public static bool MakeBackup(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Copy(path, $"{path}.backup_", true);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }

        public static bool MakeBackup(string[] arr)
        {
            try
            {
                foreach (var path in arr)
                {
                    if (File.Exists(path))
                    {
                        File.Copy(path, $"{path}.backup_", true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }

        public static bool RestoreBackup(string path)
        {
            try
            {
                var backup = $"{path}.backup_";
                if (File.Exists(backup))
                {
                    File.Copy(backup, path, true);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }

        public static bool RestoreBackup(string[] arr)
        {
            try
            {
                foreach (var path in arr)
                {
                    var backup = $"{path}.backup_";
                    if (File.Exists(backup))
                    {
                        File.Copy(backup, path, true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }

        public static bool DeleteBackup(string path)
        {
            try
            {
                var backup = $"{path}.backup_";
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }

        public static bool DeleteBackup(string[] arr)
        {
            try
            {
                foreach (var path in arr)
                {
                    var backup = $"{path}.backup_";
                    if (File.Exists(backup))
                    {
                        File.Delete(backup);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Print(e.Message);
                return false;
            }

            return true;
        }
    }
}
