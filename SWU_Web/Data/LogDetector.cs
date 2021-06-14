using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SWU_Web.Data
{
    public class LogDetector
    {
        public int Id { get; set; }
        public int DetectorId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public float Value { get; set; }
        [JsonIgnore]
        public Detector Detector { get; set; }

    }
}
