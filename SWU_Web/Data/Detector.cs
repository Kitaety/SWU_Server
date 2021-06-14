﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SWU_Web.Data
{
    public class Detector
    {
        public int Id { get; set; }
        public int SystemId { get; set; }
        public int TypeDetectorId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public SwuSystem System { get; set; }
        public TypeDetector TypeDetector {get;set;}
    }
}
