using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModStatistics.Platforms;

namespace ModStatistics
{
    public class ModStatsLogic
    {
        public static async Task Run(Action<string> log)
        {
            string gistId = Environment.GetEnvironmentVariable("GIST_ID") ?? "";
            string githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") ?? "";
            string steamApiKey = Environment.GetEnvironmentVariable("STEAM_API_KEY") ?? "";
            string nexusApiKey = Environment.GetEnvironmentVariable("NEXUS_API_KEY") ?? "";

            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Compatible; ModStats/1.0)");

            bool getThunderstore = true;
            bool getSteam = false;
            bool getNexus = false;

            log("/// --- /// MOD STATISTICS STARTED /// --- ///");

            try
            {
                var thunderstoreMods = Thunderstore.GetThunderstoreMods();
                var packageData = new Dictionary<string, object>();

                ulong totalDownloads = 0;
                ulong totalRatings = 0;
                ulong totalRatingsBad = 0;

                if (getThunderstore)
                {
                    log($"Fetching Thunderstore Data for {thunderstoreMods.Count} mods...");
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

                            await Task.Delay(150);

                            try 
                            {
                                var response = await client.GetStringAsync(apiUrl);
                                using var doc = JsonDocument.Parse(response);
                                var root = doc.RootElement;

                                entry.Value.Downloads = root.GetProperty("downloads").GetUInt64();
                                entry.Value.Ratings = (ulong)root.GetProperty("rating_score").GetInt32();
                                entry.Value.Version = root.GetProperty("latest_version").GetString() ?? "1.0.0";

                                totalDownloads += entry.Value.Downloads;
                                totalRatings += entry.Value.Ratings;
                                packageData[entry.Key] = entry.Value;

                                log($"[Thunderstore] {entry.Value.name} || DLs: {entry.Value.Downloads:N0}");
                            }
                            catch (Exception ex)
                            {
                                log($"[Error] Failed to fetch {entry.Value.name}: {ex.Message}");
                            }
                        }
                    }
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
                
                log("\n" + new string('=', 30));
                log($" FINAL STATS SUMMARY");
                log(new string('=', 30));
                log($" TOTAL DOWNLOADS : {totalDownloads:N0}");
                log($" TOTAL RATINGS   : {totalRatings:N0}");
                log(new string('=', 30) + "\n");

                if (!string.IsNullOrEmpty(githubToken) && !string.IsNullOrEmpty(gistId))
                {
                    log("Updating Gist...");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);

                    var gistFiles = new Dictionary<string, object>
                    {
                        { "prev.json", new { content = jsonOutput } }
                    };

                    var gistPayload = new { files = gistFiles };
                    var patchContent = new StringContent(JsonSerializer.Serialize(gistPayload), Encoding.UTF8, "application/json");

                    var result = await client.PatchAsync($"https://api.github.com/gists/{gistId}", patchContent);
                    log(result.IsSuccessStatusCode ? "Success! Gist Updated" : $"Error: Gist Failed: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                log($"CRITICAL ERROR: {ex.Message}");
            }
        }
    }
}