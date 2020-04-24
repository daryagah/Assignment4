using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Assignment4.Models;
using Assignment4.APIHandlerManager;
using Assignment4.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Assignment4.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;
        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public ActionResult Index()
        {
            return View();
        }

        public IActionResult DataLoad()
        {
            APIHandler webHandler = new APIHandler();
            HospitalData result = webHandler.GetHospitals();
            
            foreach(var item in result.data)
            {
                dbContext.Hospitals.Add(item);
            }
            dbContext.SaveChanges();
            return View(result);
        }

        public ActionResult Table()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .Where(h => h.provider_state == "FL");

            return View(Hosp);
        }

        public ActionResult About()
        {
            return View();
        }


        public ActionResult Chart()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .GroupBy(h => h.provider_state)
                                                 .Select(cl => new Hospital
                                                 {
                                                     provider_state = cl.Key,
                                                     total_discharges = cl.Sum(c => c.total_discharges)
                                                 })
                                                 .OrderBy(h => h.total_discharges);

            List<string> State = new List<string>();
            foreach (var item in Hosp)
            {
                State.Add(item.provider_state);
            }
            List<int> TotalDischarges = new List<int>();
            foreach (var item in Hosp)
            {
                TotalDischarges.Add(item.total_discharges);
            }
            ViewBag.Data = String.Join(",", TotalDischarges.Select(d => d));
            ViewBag.Labels = String.Join(",", State.Select(d => "\"" + d + "\""));
            ViewBag.Label = "Total Discharges by State";

            return View("Chart", Hosp);
        }

        public ActionResult AveragePaymentsByDRGs()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .GroupBy(h => h.drg_definition)
                                                 .Select(cl => new Hospital
                                                 {
                                                     drg_definition = cl.Key,
                                                     average_medicare_payments = cl.Sum(c => c.average_medicare_payments)
                                                 })
                                                 .OrderBy(h => h.average_medicare_payments);

            List<string> DRG = new List<string>();
            foreach (var item in Hosp)
            {
                DRG.Add(item.drg_definition);
            }
            List<float> TotalPayments = new List<float>();
            foreach (var item in Hosp)
            {
                TotalPayments.Add(item.average_medicare_payments);
            }
            ViewBag.Data = String.Join(",", TotalPayments.Select(d => d));
            ViewBag.Labels = String.Join(",", DRG.Select(d => "\"" + d + "\""));

            return View("Chart", Hosp);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
