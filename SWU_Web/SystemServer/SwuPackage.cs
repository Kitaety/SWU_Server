using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.SystemServer
{
    public class SwuPackage
    {
        public int Id { get; set; }
        public string Date { get; set; }
        public int Operation { get; set; }
        public bool Error { get; set; }
        public string Data { get; set; }
    }
}

public enum Operations
{
    Authorization,
    Registration,
    ChangeListDetectors,
    UpdateState,
    Disconnect
}