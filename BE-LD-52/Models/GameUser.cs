using System.Text.Json.Serialization;

namespace BE_LD_52.Models
{
    public class GameUser
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        public string Name { get; set; }
        public int Currency { get; set; }
        public bool HasWater { get; set; }
    }
}
