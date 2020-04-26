using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assignment4.Models
{
    public class LocationProvider
    {
        public class Location
        {
            public string provider_city { get; set; }
            public string provider_state { get; set; }
            public string hospital_referral_region_description { get; set; }
            public List<Provider> Providers { get; set; }
        }

        public class Provider
        {
            public string provider_name { get; set; }
            public string provider_street_address { get; set; }
            public int provider_zip_code { get; set; }
            public int total_discharges { get; set; }
            public string drg_definition { get; set; }
            public float average_covered_charges { get; set; }
            public float average_medicare_payments { get; set; }
            public float average_medicare_payments_2 { get; set; }
            public Location Location { get; set; }
        }
    }
}
