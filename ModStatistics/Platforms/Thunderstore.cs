using System.Reflection;
using System.Text.Json;

namespace ModStatistics.Platforms
{
    public static class Thunderstore
    {
        public static Dictionary<string, Mod> GetThunderstoreMods()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith("thunderstore.json"));

            if (string.IsNullOrEmpty(resourceName))
            {
                throw new FileNotFoundException("Could not find embedded thunderstore.json");
            }

            using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return new Dictionary<string, Mod>();

                using (StreamReader reader = new StreamReader(stream))
                {
                    string jsonContent = reader.ReadToEnd();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    var mods = JsonSerializer.Deserialize<Dictionary<string, Mod>>(jsonContent, options)
                               ?? new Dictionary<string, Mod>();

                    foreach (var entry in mods)
                    {
                        entry.Value.name = entry.Key;
                    }

                    return mods;
                }
            }
        }
    }
}