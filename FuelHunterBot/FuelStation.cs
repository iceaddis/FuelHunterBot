using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelHunterBot
{
    public class FuelStation
    {
        public string Title { get; set; }
        public string Address { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }

        public static List<FuelStation> GetStations()
        {
            var station1 = new FuelStation
            {
                Title = "Gerji NOC",
                Address = "Around Imperial Hotel-Gerji, Addis Ababa 1004",
                Lat = 9.0025986f,
                Lon = 38.7985707f
            };

            var station2 = new FuelStation
            {
                Title = "Papaya car wash",
                Address = "Addis Ababa, Gergi",
                Lat = 9.0025986f,
                Lon = 38.7985707f
            };

            var station3 = new FuelStation
            {
                Title = "Total garage",
                Address = "Equatorial Guinea St, Addis Ababa",
                Lat = 9.0025986f,
                Lon = 38.7985707f
            };

            var station4 = new FuelStation
            {
                Title = "Total garage",
                Address = "Equatorial Guinea St, Addis Ababa",
                Lat = 9.0025986f,
                Lon = 38.7985707f
            };

            var station5 = new FuelStation
            {
                Title = "OiLibya",
                Address = "Tito St, Addis Ababa",
                Lat = 9.0124892f,
                Lon = 38.7442721f
            };

            var station6 = new FuelStation
            {
                Title = "OiLibya",
                Address = "Goro to summit road, Before medhanialem church",
                Lat = 8.9935916f,
                Lon = 38.834849f
            };

            return new List<FuelStation>() { station1, station2, station3, station4, station5, station6 };
        }
    }
}
