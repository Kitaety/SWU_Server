using SWU_Web.SystemServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Hubs
{
    public class HubPackage
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public IEnumerable<PackageDetector> Detectors { get; set; }
    }
}
