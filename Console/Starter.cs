using System;
using System.IO;
using System.Reflection;

namespace UnityModManagerNet.Injection
{
    public class UnityModManagerStarter
    {
        public static void Start()
        {
            //Injector.Run();

            try
            {
                var file = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "UnityModManager", "UnityModManager.dll");
                Console.WriteLine("[Assembly] Loading UnityModManager by " + file);

                var assembly = Assembly.LoadFrom(file);
                var injector = assembly.GetType("UnityModManagerNet.Injector");
                injector.GetMethod("Run", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { false });
            }
            catch (Exception e) 
            { 
                Console.WriteLine(e.ToString());
            }
        }
    }
}
