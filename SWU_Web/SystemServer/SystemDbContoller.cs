using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SWU_Web.Data;
using SWU_Web.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.SystemServer
{
    public class SystemDbContoller
    {
        public static SystemDbContoller Current;

        IServiceProvider service;
        public SystemDbContoller(IServiceProvider service)
        {
            this.service = service;
            Current = this;
        }

        private ApplicationDbContext GetContext()
        {
            return service.GetRequiredService<ApplicationDbContext>();
        }
        public bool CheckSystem(SWU_Web.Data.SwuSystem checkSystem)
        {
            SWU_Web.Data.SwuSystem systemInDb = GetContext().Systems.Include(s => s.Detectors).FirstOrDefault(s => s.Id == checkSystem.Id);
            if (systemInDb != null)
            {
                if (checkSystem.Detectors.Count() == systemInDb.Detectors.Count())
                {
                    for (int i = 0; i < checkSystem.Detectors.Count(); i++)
                    {
                        Detector checkDetector = checkSystem.Detectors.ElementAt(i);
                        Detector detectorInDb = systemInDb.Detectors.FirstOrDefault(d => d.Id == checkDetector.Id && d.TypeDetectorId == checkDetector.TypeDetectorId);
                        if (detectorInDb == null)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public IEnumerable<SwuSystem> GetSystems()
        {
            return GetContext().Systems.Include(s => s.Detectors).ToList(); ;
        }

        public async Task UpdateValueDetectorsSystem(int idSystem, string dateTime, IEnumerable<PackageDetector> detectors)
        {
            var context = GetContext();
            foreach (PackageDetector detector in detectors)
            {
                string[] dateComponents = dateTime.Split(' ');
                LogDetector log = new LogDetector()
                {
                    DetectorId = detector.Id,
                    Value = detector.Value,
                    Date = dateComponents[0],
                    Time = dateComponents[1],
                };
                context.LogDetectors.Add(log);
            }

            context.SaveChanges();
            HubPackage package = new HubPackage()
            {
                Id = idSystem,
                Status = 1,
                Detectors = detectors
            };

            await NotificationHub.Current.Clients.All.SendAsync("UpdateSystemValue", package);
        }

        public async Task UpdateSystemStatus(int idSystem, int status)
        {
            var context = GetContext();
            SwuSystem system = context.Systems.FirstOrDefault(s => s.Id == idSystem);
            system.Status = status;
            context.SaveChanges();
            await NotificationHub.Current.Clients.All.SendAsync("UpdateSystemStatus", new { id = idSystem, status = status });
        }
    }
}
