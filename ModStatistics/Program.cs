using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModStatistics;
using ModStatistics.Platforms;

string gistId = Environment.GetEnvironmentVariable("GIST_ID") ?? "";
string githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "";

string steamApiKey = "";
string nexusApiKey = "";

using HttpClient client = new HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Compatible; ModStats/1.0)");

bool getThunderstore = true;
bool getSteam = false;
bool getNexus = false;

Console.WriteLine("/// --- /// MOD STATISTICS /// --- ///");

try
{
    var thunderstoreMods = Thunderstore.GetThunderstoreMods();
    var steamMods = new Dictionary<string, Mod>();
    var nexusMods = new Dictionary<string, Mod>();

    var packageData = new Dictionary<string, object>();

    ulong totalDownloads = 0;
    ulong totalRatings = 0;
    ulong totalRatingsBad = 0;

    if (getThunderstore)
    {
        foreach (var entry in thunderstoreMods)
        {
            var match = Regex.Match(entry.Value.link, @"/p/([^/]+)/([^/]+)/?$");
            var match2 = Regex.Match(entry.Value.link, @"/package/([^/]+)/([^/]+)/?$");
            
            if (match.Success || match2.Success)
            {
                if (match2.Success) { match = match2; }
                string team = match.Groups[1].Value;
                string pkg = match.Groups[2].Value;
                string apiUrl = $"https://thunderstore.io/api/v1/package-metrics/{team}/{pkg}/";

                await Task.Delay(500);

                var response = await client.GetStringAsync(apiUrl);
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;

                entry.Value.Downloads = root.GetProperty("downloads").GetUInt64();
                entry.Value.Ratings = (ulong)root.GetProperty("rating_score").GetInt32();
                entry.Value.Version = root.GetProperty("latest_version").GetString() ?? "1.0.0";

                totalDownloads += entry.Value.Downloads;
                totalRatings += entry.Value.Ratings;
                packageData[entry.Key] = entry.Value;

                Console.WriteLine($"[Thunderstore] {entry.Value.name} || Downloads: {entry.Value.Downloads}");
            }
        }
    }

    if (!string.IsNullOrEmpty(steamApiKey) && getSteam)
    {
    }

    if (getNexus && !string.IsNullOrEmpty(nexusApiKey))
    {
    }

    var finalData = new Dictionary<string, object>
    {
        { "total_downloads", totalDownloads },
        { "total_ratings", totalRatings },
        { "total_ratings_bad", totalRatingsBad },
        { "last_checked", DateTimeOffset.UtcNow.ToUnixTimeSeconds() }
    };

    foreach (var pkg in packageData) finalData.Add(pkg.Key, pkg.Value);

    var jsonOutput = JsonSerializer.Serialize(finalData, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(jsonOutput);

    if (!string.IsNullOrEmpty(githubToken) && !string.IsNullOrEmpty(gistId))
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);

        var gistFiles = new Dictionary<string, object>
        {
            { "prev.json", new { content = jsonOutput } }
        };

        var gistPayload = new { files = gistFiles };
        var patchContent = new StringContent(JsonSerializer.Serialize(gistPayload), Encoding.UTF8, "application/json");

        var result = await client.PatchAsync($"https://api.github.com/gists/{gistId}", patchContent);
        Console.WriteLine(result.IsSuccessStatusCode ? "Success! Gist Updated" : $"Error: Gist Failed: {result.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
