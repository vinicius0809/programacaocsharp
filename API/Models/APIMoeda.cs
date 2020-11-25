using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class APIMoeda
    {
        [Required]
        [JsonProperty("moeda")]
        public string Moeda { get; set; }

        [Required]
        [JsonProperty("data_inicio")]
        public string DataInicio { get; set; }

        [Required]
        [JsonProperty("data_fim")]
        public string DataFim { get; set; }
    }
}