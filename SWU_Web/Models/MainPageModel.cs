using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Models
{
    public class MainPageModel
    {
        public List<SWU_Web.Data.SwuSystem> Systems { get; set; } = new List<Data.SwuSystem>();
        public Dictionary<int, float> LastDetectorsValue { get; set; } = new Dictionary<int, float>();
    }
}
