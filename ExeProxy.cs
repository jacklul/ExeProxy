/**
 * Made by Jack'lul (https://jacklul.github.io)
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace jacklul
{
	public class ExeProxy
	{
		private static string NAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

		public static void Main(string[] args)
		{
			string name = System.Diagnostics.Process.GetCurrentProcess().ProcessName; // No extension
			string dirname = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string arguments = args.Length > 0 ? GetArguments() : "";

			string config = Path.Combine(dirname, name) + ".ini";

			if (!File.Exists(config))
			{
				config = Path.Combine(dirname, name) + ".cfg";

				if (!File.Exists(config))
				{
					Error("Configuration file does not exist!");
					return;
				}
			}

			var ini = new IniFile(config);
			string ini_exe = ini.Read("exe", "MAIN");
			string ini_args = ini.Read("args", "MAIN");
			string ini_debug = ini.Read("debug", "MAIN");
			string ini_add_to_path = ini.Read("add_to_path", "MAIN");
			string ini_override_file = ini.Read("override_file", "MAIN");

			// If override file is set, read it and override settings if it contains defined ini section
			if (!String.IsNullOrEmpty(ini_override_file))
			{
				if (!Regex.IsMatch(ini_override_file, @"^[a-zA-Z0-9_\-\.]+$"))
				{
					Error("Invalid override file name!");
					return;
				}

				string override_file = Path.Combine(Directory.GetCurrentDirectory(), ini_override_file);

				if (File.Exists(override_file))
				{
					string override_file_contents = File.ReadAllText(override_file).Trim();

					if (!String.IsNullOrEmpty(override_file_contents))
					{
						ini_exe = ini.Read("exe", override_file_contents);
						ini_args = ini.Read("args", override_file_contents);
						ini_debug = ini.Read("debug", override_file_contents);
						ini_add_to_path = ini.Read("add_to_path", override_file_contents);
					}
				}
			}

			if (String.IsNullOrEmpty(ini_exe))
			{
				Error("Executable is not set!");
				return;
			}

			// Prepend with directory path of this executable
			if (ini_exe.StartsWith("/") || ini_exe.StartsWith("\\"))
			{
				ini_exe = dirname + ini_exe;
			}

			if (!File.Exists(ini_exe))
			{
				Error("Executable does not exist: " + ini_exe);
				return;
			}

			if (!String.IsNullOrEmpty(ini_args))
			{
				arguments = (ini_args + " " + arguments).Trim();
			}

			bool add_to_path = String.IsNullOrEmpty(ini_add_to_path) || ini_add_to_path.ToLower().Contains("true");

			if (ini_debug.ToLower().Contains("true"))
			{
				Console.WriteLine(NAME + " executable: " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				Console.WriteLine("Working directory: " + Directory.GetCurrentDirectory());
				Console.WriteLine("Executable: " + ini_exe);
				Console.WriteLine("Arguments: " + arguments);
				Console.WriteLine("Add to PATH: " + (add_to_path ? "YES" : "NO"));
				Console.WriteLine();
			}

			// Prepend PATH environment variable with directory path of target executable
			if (add_to_path)
			{
				string env_path = Environment.GetEnvironmentVariable("PATH");
				Environment.SetEnvironmentVariable("PATH", Path.GetDirectoryName(ini_exe) + ";" + env_path);
			}

			var proc = new Process();
			proc.StartInfo.FileName = ini_exe;
			proc.StartInfo.Arguments = arguments;
			proc.StartInfo.UseShellExecute = false;
			proc.Start();
			proc.WaitForExit();
			Environment.Exit(proc.ExitCode);
			proc.Close();
		}

		private static string GetArguments()
		{
			var exe = Environment.GetCommandLineArgs()[0];
			var cmd = Environment.CommandLine;

			return cmd.Remove(cmd.IndexOf(exe), exe.Length).TrimStart('"').Substring(1).Trim();
		}

		private static void Error(string text)
		{
			Console.WriteLine("[" + NAME + "] Error: " + text);
		}
	}
}