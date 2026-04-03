using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;

await MainAsync(args);

static async Task<int> MainAsync(string[] args)
{
	var baseUrl = "http://localhost:5000";
	string rootArg = null;
	int concurrency = 50;
	int timeoutSeconds = 10;
	string outputJson = null;

	foreach (var a in args)
	{
		if (a.StartsWith("--host=")) baseUrl = a.Substring("--host=".Length);
		else if (a.StartsWith("--root=")) rootArg = a.Substring("--root=".Length);
		else if (a.StartsWith("--concurrency=") && int.TryParse(a.Substring("--concurrency=".Length), out var c)) concurrency = c;
		else if (a.StartsWith("--timeout=") && int.TryParse(a.Substring("--timeout=".Length), out var t)) timeoutSeconds = t;
		else if (a.StartsWith("--output=")) outputJson = a.Substring("--output=".Length);
	}

	string repoRoot = null;
	if (!string.IsNullOrEmpty(rootArg) && Directory.Exists(rootArg))
		repoRoot = Path.GetFullPath(rootArg);
	else
	{
		var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (dir != null)
		{
			if (File.Exists(Path.Combine(dir.FullName, "ZKEACMS.sln")))
			{
				repoRoot = dir.FullName;
				break;
			}
			dir = dir.Parent;
		}
		if (repoRoot == null) repoRoot = Directory.GetCurrentDirectory();
	}

	Console.WriteLine($"Repository root: {repoRoot}");
	Console.WriteLine($"Host: {baseUrl}");

	// find main wwwroot (prefer ZKEACMS.WebHost)
	string chosenWwwroot = null;
	var candidate = Path.Combine(repoRoot, "src", "ZKEACMS.WebHost", "wwwroot");
	if (Directory.Exists(candidate)) chosenWwwroot = candidate;
	else
	{
		var all = Directory.EnumerateDirectories(repoRoot, "wwwroot", SearchOption.AllDirectories);
		chosenWwwroot = all.OrderByDescending(d =>
		{
			try { return Directory.EnumerateFiles(d, "*", SearchOption.AllDirectories).Count(); } catch { return 0; }
		}).FirstOrDefault();
	}

	if (chosenWwwroot != null) Console.WriteLine($"Using wwwroot: {chosenWwwroot}");
	else Console.WriteLine("No wwwroot found. Skipping wwwroot scan.");

	var pluginsDir = Path.Combine(repoRoot, "src", "Plugins");
	var hasPlugins = Directory.Exists(pluginsDir);
	if (hasPlugins) Console.WriteLine($"Using Plugins dir: {pluginsDir}");
	else Console.WriteLine("No Plugins directory found. Skipping plugins scan.");

	var exts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{ ".css",".js",".jpg",".jpeg",".png",".gif",".svg",".webp",".woff",".woff2",".ttf",".eot",".map",".json",".html",".htm",".ico",".txt",".pdf",".zip",".mp3",".mp4" };

	var resources = new ConcurrentBag<(string local, string url)>();

	if (chosenWwwroot != null)
	{
		foreach (var f in Directory.EnumerateFiles(chosenWwwroot, "*", SearchOption.AllDirectories))
		{
			try
			{
				var parts = f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				if (parts.Any(p => string.Equals(p, "bin", StringComparison.OrdinalIgnoreCase) || string.Equals(p, "obj", StringComparison.OrdinalIgnoreCase))) continue;
				if (!exts.Contains(Path.GetExtension(f))) continue;
				var rel = Path.GetRelativePath(chosenWwwroot, f).Replace('\\', '/');
				var url = "/" + rel;
				resources.Add((f, url));
			}
			catch { }
		}
	}

	if (hasPlugins)
	{
		foreach (var plugin in Directory.EnumerateDirectories(pluginsDir))
		{
			var pluginName = Path.GetFileName(plugin);
			foreach (var f in Directory.EnumerateFiles(plugin, "*", SearchOption.AllDirectories))
			{
				try
				{
					var parts = f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
					if (parts.Any(p => string.Equals(p, "bin", StringComparison.OrdinalIgnoreCase) || string.Equals(p, "obj", StringComparison.OrdinalIgnoreCase))) continue;
					if (!exts.Contains(Path.GetExtension(f))) continue;
					var rel = Path.GetRelativePath(plugin, f).Replace('\\', '/');
					var url = "/Plugins/" + pluginName + "/" + rel;
					resources.Add((f, url));
				}
				catch { }
			}
		}
	}

	var totalToCheck = resources.Count;
	Console.WriteLine($"Found {totalToCheck} resources to check.");
	if (totalToCheck == 0) return 0;

	var results = new ConcurrentBag<ResourceCheckResult>();

	var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
	using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds) };
	var baseUri = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/");

	var sem = new SemaphoreSlim(concurrency);
	var tasks = resources.Select(async r =>
	{
		await sem.WaitAsync();
		try
		{
			Uri target;
			try { target = new Uri(baseUri, r.url.TrimStart('/')); }
			catch { target = new Uri(baseUri, Uri.EscapeUriString(r.url.TrimStart('/'))); }
			try
			{
				using var req = new HttpRequestMessage(HttpMethod.Get, target);
				using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
				var ok = ((int)resp.StatusCode) < 400;
				results.Add(new ResourceCheckResult { LocalPath = r.local, Url = target.ToString(), Ok = ok, StatusCode = (int)resp.StatusCode, Failure = ok ? null : resp.ReasonPhrase });
			}
			catch (Exception ex)
			{
				results.Add(new ResourceCheckResult { LocalPath = r.local, Url = target.ToString(), Ok = false, Failure = ex.Message });
			}
		}
		finally { sem.Release(); }
	}).ToArray();

	await Task.WhenAll(tasks);

	var allResults = results.ToArray();
	var okCount = allResults.Count(r => r.Ok);
	var failed = allResults.Where(r => !r.Ok).OrderBy(r => r.Url).ToList();

	Console.WriteLine($"Checked: {allResults.Length}, OK: {okCount}, Failed: {failed.Count}");
	if (failed.Count > 0)
	{
		Console.WriteLine("Failed resources:");
		foreach (var f in failed) Console.WriteLine($"- {f.Url} => {f.Failure} (status: {f.StatusCode ?? 0}) Local: {f.LocalPath}");
	}

	if (!string.IsNullOrEmpty(outputJson))
	{
		var j = JsonSerializer.Serialize(failed, new JsonSerializerOptions { WriteIndented = true });
		File.WriteAllText(outputJson, j);
		Console.WriteLine($"Failed-only results written to {outputJson} ({failed.Count} entries)");
	}

	return failed.Count > 0 ? 1 : 0;
}

class ResourceCheckResult
{
    public string LocalPath { get; set; }
    public string Url { get; set; }
    public bool Ok { get; set; }
    public int? StatusCode { get; set; }
    public string Failure { get; set; }
}