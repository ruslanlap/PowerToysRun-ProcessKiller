using System.Diagnostics;
using Wox.Infrastructure;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.ProcessKiller;

internal partial class ProcessHelper
{
	private static readonly HashSet<string> SystemProcessList =
	[
		"conhost",
		"svchost",
		"idle",
		"system",
		"rundll32",
		"csrss",
		"lsass",
		"lsm",
		"smss",
		"wininit",
		"winlogon",
		"services",
		"spoolsv",
		"explorer",
	];

	private static bool IsSystemProcess(Process p) => SystemProcessList.Contains(p.ProcessName.ToLower());

	/// <summary>
	/// Returns a ProcessResult for every running non-system process whose name matches the given search
	/// </summary>
	public static List<ProcessResult> GetMatchingProcesses(string search)
	{
		var processes = Process.GetProcesses().Where(p => !IsSystemProcess(p)).ToList();

		if (string.IsNullOrWhiteSpace(search))
		{
			return processes.ConvertAll(p => new ProcessResult(p, 0));
		}

		List<ProcessResult> results = [];
		foreach (Process? p in processes)
		{
			var score = StringMatcher.FuzzySearch(search, p.ProcessName + p.Id).Score;
			if (score > 0)
			{
				results.Add(new ProcessResult(p, score));
			}
		}

		return results;
	}

	public static void TryKill(Process p)
	{
		try
		{
			if (!p.HasExited)
			{
				p.Kill();
				_ = p.WaitForExit(50);
			}
		}
		catch (Exception e)
		{
			Log.Exception($"Failed to kill process {p.ProcessName}", e, typeof(ProcessHelper));
		}
	}
}
