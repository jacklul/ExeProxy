
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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
			string ini_addpath = ini.Read("addpath", "MAIN");
			string ini_debug = ini.Read("debug", "MAIN");

			if (String.IsNullOrEmpty(ini_exe))
			{
				Error("Target executable is not set!");
				return;
			}

			// Prepend with directory path of this executable
			if (ini_exe.StartsWith("/") || ini_exe.StartsWith("\\"))
			{
				ini_exe = dirname + ini_exe;
			}

			if (!File.Exists(ini_exe))
			{
				Error("Target executable does not exist: " + ini_exe);
				return;
			}

			if (!String.IsNullOrEmpty(ini_args))
			{
				arguments = (ini_args + " " + arguments).Trim();
			}

			if (ini_debug.ToLower().Contains("true"))
			{
				Console.WriteLine(NAME + " executable: " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				Console.WriteLine("Target executable: " + ini_exe);
				Console.WriteLine("Target arguments: " + arguments);
				Console.WriteLine("Working directory: " + Directory.GetCurrentDirectory());
				Console.WriteLine();
			}

			// Prepend PATH environment variable with directory path of target executable
			if (ini_addpath.ToLower().Contains("true"))
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
			Console.WriteLine(NAME + " error: " + text);
		}
	}
}