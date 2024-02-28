using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH.Models
{
    public class Unit : IComparable<Unit>
    {
        public int id;

        public string? name { get; set; }

        public Unit()
        { }

        public Unit(string name)
        {
            this.name = name;
        }

        public Unit(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public override string ToString()
        {
            return name;
        }

        public int CompareTo(Unit? other)
        {
            if (other.id == id)
            {
                return 1;
            }
            return 0;
        }
    }
}
