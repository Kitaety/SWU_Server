using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Data
{
    public class SwuSystem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Status { get; set; }

        public IEnumerable<Detector> Detectors { get; set; }
    }
}
