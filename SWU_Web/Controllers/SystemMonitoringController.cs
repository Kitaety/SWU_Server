using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWU_Web.Data;
using SWU_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWU_Web.Controllers
{
    [Authorize]
    public class SystemMonitoringController : Controller
    {
        private readonly ILogger<SystemMonitoringController> _logger;
        private ApplicationDbContext _context;

        public SystemMonitoringController(ILogger<SystemMonitoringController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            MainPageModel model = new MainPageModel();
            Dictionary<int, float> lastValues = new Dictionary<int, float>();
            List<SWU_Web.Data.SwuSystem> systems = _context.Systems.Include(s => s.Detectors).ThenInclude(d => d.TypeDetector).ToList();

            foreach (List<Detector> detectors in systems.Select(s => s.Detectors.ToList()))
            {
                foreach (Detector detector in detectors)
                {
                    LogDetector log = _context.LogDetectors.OrderByDescending(l => l.Id).FirstOrDefault(l => l.DetectorId == detector.Id);

                    float lastValue = 0;
                    
                    if (log != null)
                    {
                        lastValue = log.Value;
                    }

                    lastValues.Add(detector.Id, lastValue);
                }
            }

            model.Systems = systems;
            model.LastDetectorsValue = lastValues;

            return View(model);
        }
        [HttpGet]
        public IActionResult ControlPanel()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetSystems()
        {
            List<SWU_Web.Data.SwuSystem> systems = _context.Systems.Include(s => s.Detectors).ThenInclude(d => d.TypeDetector).ToList();
            return Json(systems);
        }
    }
}
