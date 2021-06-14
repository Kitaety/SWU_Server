using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SWU_Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SwuSystem> Systems { get; set; }
        public DbSet<TypeDetector> TypeDetectors { get; set; }
        public DbSet<Detector> Detectors { get; set; }
        public DbSet<LogDetector> LogDetectors { get; set; }
    }
}
