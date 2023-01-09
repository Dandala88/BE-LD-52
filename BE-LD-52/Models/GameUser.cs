using System.Text.Json.Serialization;

namespace BE_LD_52.Models
{
    public class GameUser
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        public int Currency { get; set; }
        public bool HasWater { get; set; }
        public string ConnectionId { get; set; }
        public bool PerformingAction { get; set; }
        public string Name { get; set; }
    }
}
