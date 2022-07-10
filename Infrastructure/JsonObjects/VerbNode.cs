using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaClientApplication.Infrastructure.JsonObjects
{
    public class VerbNode
    {
        public string Verb { get; set; }
        public string Prep { get; set; }
        public double NounFrequency { get; set; }
        public double VerbFrequency { get; set; }
        public double CombinationFrequency { get; set; }
        public double LogDice { get; set; }
        public double MinimumSensitivity { get; set; }
    }
}
