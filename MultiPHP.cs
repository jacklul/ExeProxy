
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace jacklul
{
	public class MultiPHP
	{
		private static string NAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

		public static void Main(string[] args)
		{
			string name = System.Diagnostics.Process.GetCurrentProcess().ProcessName; // No extension
			string dirname = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			string cwd = Directory.GetCurrentDirectory();
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
			string ini_default_exe = ini.Read("default_exe", "MAIN");
			string ini_multi_path = ini.Read("multi_path", "MAIN");
			string ini_config_path = ini.Read("config_path", "MAIN");
			string ini_args = ini.Read("args", "MAIN");
			string ini_addpath = ini.Read("addpath", "MAIN");
			string ini_debug = ini.Read("debug", "MAIN");

			if (String.IsNullOrEmpty(ini_multi_path))
			{
				Error("Target directory is not set!");
				return;
			}

			if (String.IsNullOrEmpty(ini_default_exe))
			{
				Error("Target executable is not set!");
				return;
			}

			if (ini_multi_path.StartsWith("/") || ini_multi_path.StartsWith("\\"))
			{
				ini_multi_path = dirname + ini_multi_path;
			}

			if (!Directory.Exists(ini_multi_path))
			{
				if (!File.Exists(Path.Combine(ini_multi_path, "php.exe")))
				{
					Error("Target directory does not exist: " + ini_multi_path);
					return;
				}
			}
			
			if (ini_default_exe.StartsWith("/") || ini_default_exe.StartsWith("\\"))
			{
				ini_default_exe = dirname + ini_default_exe;
			}

			if (!File.Exists(ini_default_exe))
			{
				Error("Target executable does not exist: " + ini_default_exe);
				return;
			}

			if (!String.IsNullOrEmpty(ini_args))
			{
				arguments = (ini_args + " " + arguments).Trim();
			}

			string target_exe = ini_default_exe;
			string override_file = Path.Combine(cwd, ".phpversion");

			if (File.Exists(override_file))
			{
				string override_file_contents = File.ReadAllText(override_file).Trim();

				if (String.IsNullOrEmpty(override_file_contents))
				{
					Error("Override file is empty!");
					return;
				}

				string override_path = Path.Combine(ini_multi_path, override_file_contents);

				if (!Directory.Exists(override_path))
				{
					Error("Override directory does not exist: " + override_path);
					return;
				}

				target_exe = Path.Combine(override_path, "php.exe");
			}

			if (ini_config_path.StartsWith("/") || ini_config_path.StartsWith("\\"))
			{
				ini_config_path = dirname + ini_config_path;
			}
			
			if (ini_debug.ToLower().Contains("true"))
			{
				Console.WriteLine(NAME + " executable: " + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				Console.WriteLine("Default executable: " + ini_default_exe);
				Console.WriteLine("Multi path: " + ini_multi_path);
				Console.WriteLine("Config path: " + ini_config_path);
				Console.WriteLine("Target executable: " + target_exe);
				Console.WriteLine("Target arguments: " + arguments);
				Console.WriteLine("Working directory: " + cwd);
				Console.WriteLine();
			}

			if (ini_addpath.ToLower().Contains("true"))
			{
				string env_path = Environment.GetEnvironmentVariable("PATH");
				Environment.SetEnvironmentVariable("PATH", Path.GetDirectoryName(target_exe) + ";" + env_path);
			}

			if (!String.IsNullOrEmpty(ini_config_path) && String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PHP_INI_SCAN_DIR")))
			{
				Environment.SetEnvironmentVariable("PHP_INI_SCAN_DIR", Path.GetDirectoryName(ini_config_path));
			}
			
			if (ini_default_exe != target_exe)
			{
				Console.WriteLine("Local override forces PHP " + Path.GetFileName(Path.GetDirectoryName(target_exe)) + " (.phpversion)");
			}

			var proc = new Process();
			proc.StartInfo.FileName = target_exe;
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