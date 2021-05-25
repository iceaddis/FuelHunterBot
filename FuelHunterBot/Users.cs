using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuelHunterBot
{
    public class Users
    {
        public long Id { get; set; }
        public float CurrentLat { get; set; }
        public float CurrentLong { get; set; }
        public int OrderOfNearness { get; set; }

    }

}
