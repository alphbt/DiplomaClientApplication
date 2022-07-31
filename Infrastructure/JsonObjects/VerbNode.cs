using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication.Infrastructure.JsonObjects
{
    public class VerbNode
    {
        [JsonProperty(Order= -2)]
        public string Verb { get; set; }
        [JsonProperty(Order = -2)]
        public string Prep { get; set; }
        [JsonProperty(Order = -2)]
        public double NounFrequency { get; set; }
        [JsonProperty(Order = -2)]
        public double VerbFrequency { get; set; }
        [JsonProperty(Order = -2)]
        public double CombinationFrequency { get; set; }
        [JsonProperty(Order = -2)]
        public double LogDice { get; set; }
        [JsonProperty(Order = -2)]
        public double MinimumSensitivity { get; set; }
    }
}
