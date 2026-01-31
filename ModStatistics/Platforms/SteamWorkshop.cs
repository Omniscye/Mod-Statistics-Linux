using System.Reflection;
using System.Text.Json;

namespace ModStatistics.Platforms
{
    public static class SteamWorkshop
    {
        public static Dictionary<string, Mod> GetSteamWorkshop()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith("steam.json"));

            if (string.IsNullOrEmpty(resourceName))
            {
                throw new FileNotFoundException("Could not find embedded steam.json");
            }

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return new Dictionary<string, Mod>();

                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonContent = reader.ReadToEnd();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var rawData = JsonSerializer.Deserialize<Dictionary<string, Mod>>(jsonContent, options)
                               ?? new Dictionary<string, Mod>();

                    foreach (var entry in rawData)
                    {
                        entry.Value.platform = "Steam";
                        entry.Value.link = $"https://steamcommunity.com/sharedfiles/filedetails/?id={entry.Key}";
                    }

                    return rawData;
                }
            }
        }
    }
}