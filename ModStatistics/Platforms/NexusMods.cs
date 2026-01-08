using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModStatistics.Platforms
{
    public class NexusMods
    {
        public static Dictionary<string, Mod> GetNexusMods()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames()
                .FirstOrDefault(r => r.EndsWith("nexusmods.json"));

            if (string.IsNullOrEmpty(resourceName))
            {
                throw new FileNotFoundException("Could not find embedded nexusmods.json");
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
                        entry.Value.platform = "Nexus";
                        entry.Value.link = $"https://www.nexusmods.com/{entry.Value.community}/mods/{entry.Value.nexusModId}";
                    }

                    return rawData;
                }
            }
        }
    }
}
