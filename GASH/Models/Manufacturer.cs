using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH.Models
{
    public class Manufacturer : IComparable<Manufacturer>
    {
        public int id;

        public string? name { get; set; }

        public Manufacturer() 
        { }

        public Manufacturer(string name)
        {
            this.name = name;
        }

        public Manufacturer(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }

        public int CompareTo(Manufacturer? other)
        {
            if (other.id == id)
            {
                return 1;
            }
            return 0;
        }
    }
}
