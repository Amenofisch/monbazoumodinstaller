using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace MonBazouModInstaller
{
    internal class Program
    {
        private static List<string> _droppedFilePaths = new List<string>();
        private static string _gamePath;

        private static string[] _unsupportedMods = {"FusionTranslationMod.dll", "MonBazouCustomAssets.dll", "CustomMap.dll", "CustomParts.dll"};

        // Define ExitCodes for easier debugging and usage
        [Flags]
        enum ExitCode : int
        {
            Success = 0,
            NoArgs = 1,
            FileNotExist = 2,
            FileNotDll = 4,
            GameNotFound = 8,
            BepInExNotFound = 16,
            ErrorWhileCopy = 32,
            Unsupported = 64
        }
        
        #region GetGameFolder Method
        /// <summary>
        /// Gets the game folder from registry.
        /// Returns string with path to game root folder.
        /// </summary>
        static string GetGameFolder()
        {
            try
            {
                using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    RegistryKey registryKey2;
                    try
                    {
                        registryKey2 = registryKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1520370");
                        object value = registryKey2.GetValue("InstallLocation");
                        return value.ToString();
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        #endregion

        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                foreach(string arg in args)
                {
                    if (!File.Exists(arg))
                    {
                        Console.WriteLine(arg + " isn't a file!");
                        Environment.Exit((int)ExitCode.FileNotExist);
                    };
                    FileInfo file = new FileInfo(arg);
                    if (file.Extension != ".dll")
                    {
                        Console.WriteLine(file.Name + " has wrong extension (only .dll allowed atm)");
                        Environment.Exit((int)ExitCode.FileNotDll);
                    }
                    for(int i = 0; i < _unsupportedMods.Length; i++)
                    {
                        if(file.Name == _unsupportedMods[i])
                        {
                            Console.WriteLine(file.Name + " is on the list of unsupported mods!");
                            Console.WriteLine("This is due to a limitation in the code atm!");
                            Environment.Exit((int)ExitCode.Unsupported);
                        }
                    }
                    _droppedFilePaths.Add(arg);
                    Console.WriteLine("Added " + file.Name + " to list"); 
                }
            } else
            {
                Environment.Exit(0);
            }

            _gamePath = GetGameFolder();

            if (String.IsNullOrEmpty(_gamePath))
            {
                Console.WriteLine("Couldn't get game location from registry");
                Environment.Exit((int)ExitCode.GameNotFound);
            }
            if (!Directory.Exists(_gamePath + "/BepInEx/plugins/"))
            {
                Console.WriteLine("BepInEx not installed!");
                Environment.Exit((int)ExitCode.BepInExNotFound);
            }
            
            foreach(string file in _droppedFilePaths)
            {
                FileInfo fileInfo = new FileInfo(file);
                try
                {
                    File.Copy(file, _gamePath + @"\BepInEx\plugins\" + fileInfo.Name, true);
                    Console.WriteLine("Copying " + fileInfo.Name + " to game folder!");
                } catch(Exception ex)
                {
                    Console.WriteLine("Error while copying " + fileInfo.Name + " to game folder!");
                    Console.WriteLine(ex.ToString());
                    Environment.Exit((int)ExitCode.ErrorWhileCopy);
                }
            }

            Console.WriteLine("Copied " + _droppedFilePaths.Count + " mods to game folder");
            Console.WriteLine("Thanks for using MonBazou Mod Installer! Made by Amenofisch#5368");
            Thread.Sleep(5000);
            Environment.Exit((int)ExitCode.Success);
        }
    }
}
