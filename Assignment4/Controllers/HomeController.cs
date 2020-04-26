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

        public IActionResult Index()
        {
            return View();
        }

        public async Task<ViewResult> DataLoad()
        {
            APIHandler webHandler = new APIHandler();
            HospitalData result = webHandler.GetHospitals();

            foreach (var item in result.data)
            {
                if (dbContext.Hospitals.Count() == 0)
                {
                    dbContext.Hospitals.Add(item);
                }
            }
            foreach(var item in dbContext.Hospitals)
            {
                Provider provider = new Provider();
                Location location = new Location();
                provider.provider_id = item.provider_id;
                provider.provider_name = item.provider_name;
                provider.provider_street_address = item.provider_street_address;
                provider.provider_zip_code = item.provider_zip_code;
                provider.total_discharges = item.total_discharges;
                provider.drg_definition = item.drg_definition;
                provider.average_covered_charges = item.average_covered_charges;
                provider.average_medicare_payments = item.average_medicare_payments;
                provider.average_medicare_payments_2 = item.average_medicare_payments_2;
                location.provider_city = item.provider_city;
                location.provider_state = item.provider_state;
                location.hospital_referral_region_description = item.hospital_referral_region_description;
                dbContext.Locations.Add(location);
                provider.Location = location;
                dbContext.Providers.Add(provider);
            }
            await dbContext.SaveChangesAsync();
            return View("Index", result);
        }

        public IActionResult Table(string searchProvState, string searchProvCity, string sortOrder)
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals;
            var HospsProvNames = Hosp;
            var HospsProvCities = Hosp;
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "namesDesc" : "namesAsc";
            ViewBag.CitySortParam = String.IsNullOrEmpty(sortOrder) ? "cityDesc" : "cityAsc";
            ViewBag.StateSortParam = String.IsNullOrEmpty(sortOrder) ? "states" : "";
            ViewBag.DischargesSortParam = sortOrder == "MinDis" ? "discharges" : "MinDis";
            ViewBag.CovChargesSortParam = sortOrder == "MinCovCharge" ? "covCharges" : "MinCovCharge";
            ViewBag.TotalPmtsSortParam = sortOrder == "MinTotalPmts" ? "totalPmts" : "MinTotalPmts";
            ViewBag.MedicSortParam = sortOrder == "MinMedic" ? "medicare" : "MinMedic";
            switch(sortOrder)
            {
                case "namesDesc":
                    Hosp = Hosp.OrderByDescending(h => h.provider_name);
                    break;
                case "namesAsc":
                    Hosp = Hosp.OrderBy(h => h.provider_name);
                    break;
                case "cityDesc":
                    Hosp = Hosp.OrderByDescending(h => h.provider_city);
                    break;
                case "cityAsc":
                    Hosp = Hosp.OrderBy(h => h.provider_city);
                    break;
                case "states":
                    Hosp = Hosp.OrderByDescending(h => h.provider_state);
                    break;
                case "MinDis":
                    Hosp = Hosp.OrderBy(h => h.total_discharges);
                    break;
                case "discharges":
                    Hosp = Hosp.OrderByDescending(h => h.total_discharges);
                    break;
                case "MinCovCharge":
                    Hosp = Hosp.OrderBy(h => h.average_covered_charges);
                    break;
                case "covCharges":
                    Hosp = Hosp.OrderByDescending(h => h.average_covered_charges);
                    break;
                case "MinTotalPmts":
                    Hosp = Hosp.OrderBy(h => h.average_medicare_payments);
                    break;
                case "totalPmts":
                    Hosp = Hosp.OrderByDescending(h => h.average_medicare_payments);
                    break;
                case "MinMedic":
                    Hosp = Hosp.OrderBy(h => h.average_medicare_payments_2);
                    break;
                case "medic":
                    Hosp = Hosp.OrderByDescending(h => h.average_medicare_payments_2);
                    break;
                default:
                    Hosp = Hosp.OrderBy(h => h.provider_state);
                    break;
            }
            if(!String.IsNullOrEmpty(searchProvState))
            {
                HospsProvNames = HospsProvNames.Where(Hosp => Hosp.provider_state.Contains(searchProvState));
                return View(HospsProvNames.ToList());
            }
            if(!String.IsNullOrEmpty(searchProvCity))
            {
                HospsProvCities = HospsProvCities.Where(Hosp => Hosp.provider_city.Contains(searchProvCity));
                return View(HospsProvCities.ToList());
            }
            return View(Hosp.ToList());
        }

        public IActionResult About()
        {
            return View();
        }

        List<Provider> providers = new List<Provider>();
        List<Location> locations = new List<Location>();

        public IActionResult Chart()
        {
            //IQueryable<LocationProvider> Hosp = locations
            //                         .GroupBy(h => h.provider_state)
            //                         .Select(cl => new Location
            //                         {
            //                             provider_state = cl.Key,
            //                             total_discharges = cl.Sum(c => c.total_discharges),
            //                             average_medicare_payments = cl.Average(c => c.average_medicare_payments),
            //                             average_medicare_payments_2 = cl.Average(c => c.average_medicare_payments_2),
            //                             average_covered_charges = cl.Average(c => c.average_covered_charges)
            //                         })
            //                         .OrderBy(h => h.provider_state);

            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                     .GroupBy(h => h.provider_state)
                                     .Select(cl => new Hospital
                                     {
                                         provider_state = cl.Key,
                                         total_discharges = cl.Sum(c => c.total_discharges),
                                         average_medicare_payments = cl.Average(c => c.average_medicare_payments),
                                         average_medicare_payments_2 = cl.Average(c => c.average_medicare_payments_2),
                                         average_covered_charges = cl.Average(c => c.average_covered_charges)
                                     })
                                     .OrderBy(h => h.provider_state);

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
            ViewBag.Title = "Total Discharges by State";
            ViewBag.Desc = "DRGs are a classification system that groups similar clinical conditions (diagnoses) and the procedures furnished by the hospital during the stay. What are the most common DRGs according to this dataset?";
            ViewBag.Data = String.Join(",", TotalDischarges.Select(d => d));
            ViewBag.Labels = String.Join(",", State.Select(d => "\"" + d + "\""));
            ViewBag.Label = "Total Discharges";

            return View("Chart", Hosp);
        }


        public IActionResult DischargesByState()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .GroupBy(h => h.provider_state)
                                                 .Select(cl => new Hospital
                                                 {
                                                     provider_state = cl.Key,
                                                     total_discharges = cl.Sum(c => c.total_discharges),
                                                     average_medicare_payments = cl.Average(c => c.average_medicare_payments),
                                                     average_medicare_payments_2 = cl.Average(c => c.average_medicare_payments_2),
                                                     average_covered_charges = cl.Average(c => c.average_covered_charges)
                                                 })
                                                 .OrderBy(h => h.provider_state);

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
            ViewBag.Title = "Total Discharges by State";
            ViewBag.Desc = "DRGs are a classification system that groups similar clinical conditions (diagnoses) and the procedures furnished by the hospital during the stay. What are the most common DRGs according to this dataset?";
            ViewBag.Data = String.Join(",", TotalDischarges.Select(d => d));
            ViewBag.Labels = String.Join(",", State.Select(d => "\"" + d + "\""));
            ViewBag.Label = "Total Discharges";

            return View("Chart", Hosp);
        }

        public IActionResult AveragePaymentsByState()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .GroupBy(h => h.provider_state)
                                                 .Select(cl => new Hospital
                                                 {
                                                     provider_state = cl.Key,
                                                     total_discharges = cl.Sum(c => c.total_discharges),
                                                     average_medicare_payments = cl.Average(c => c.average_medicare_payments),
                                                     average_medicare_payments_2 = cl.Average(c => c.average_medicare_payments_2),
                                                     average_covered_charges = cl.Average(c => c.average_covered_charges)
                                                 })
                                                 .OrderBy(h => h.provider_state);

            List<string> State = new List<string>();
            foreach (var item in Hosp)
            {
                State.Add(item.provider_state);
            }
            List<float> TotalPayments = new List<float>();
            foreach (var item in Hosp)
            {
                TotalPayments.Add(item.average_medicare_payments);
            }

            ViewBag.Title = "Average Total Payments by State";
            ViewBag.Desc = "How do states differ in their average charges for a DRG? Here is the information with the most expensive states coming first:";
            ViewBag.Data = String.Join(",", TotalPayments.Select(d => d));
            ViewBag.Labels = String.Join(",", State.Select(d => "\"" + d + "\""));
            ViewBag.Label = "Average Payments";

            return View("Chart", Hosp);
        }

        public IActionResult PaymentDifferenceByState()
        {
            IQueryable<Hospital> Hosp = dbContext.Hospitals
                                                 .GroupBy(h => h.provider_state)
                                                 .Select(cl => new Hospital
                                                 {
                                                     provider_state = cl.Key,
                                                     total_discharges = cl.Sum(c => c.total_discharges),
                                                     average_medicare_payments = cl.Max(c => c.average_medicare_payments) - cl.Min(c => c.average_medicare_payments),
                                                     average_medicare_payments_2 = cl.Max(c => c.average_medicare_payments_2) - cl.Min(c => c.average_medicare_payments_2),
                                                     average_covered_charges = cl.Max(c => c.average_covered_charges) - cl.Min(c => c.average_covered_charges)
                                                 })
                                                 .OrderBy(h => h.provider_state);

            List<string> State = new List<string>();
            foreach (var item in Hosp)
            {
                State.Add(item.provider_state);
            }
            List<float> PaymentDifference = new List<float>();
            foreach (var item in Hosp)
            {
                PaymentDifference.Add(item.average_medicare_payments);
            }

            ViewBag.Title = "Difference in Average Total Payments by State";
            ViewBag.Desc = "How about the difference between the highest and the lowest charges in the same state? Here is the breakdown of locations with the largest difference, which can help find more affordable options in a region.";
            ViewBag.Data = String.Join(",", PaymentDifference.Select(d => d));
            ViewBag.Labels = String.Join(",", State.Select(d => "\"" + d + "\""));
            ViewBag.Label = "Average Payments";

            return View("Chart", Hosp);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
