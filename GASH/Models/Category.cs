using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH.Models
{
    public class Category : IComparable<Category>
    {
        public int id;

        public string? name { get; set; }

        public Category() 
        { }

        public Category(int id, string? name)
        {
            this.id = id;
            this.name = name;
        }

        public Category(string name)
        {
            this.name = name;
        }

        public override string ToString() 
        {
            return name;
        }

        public int CompareTo(Category? other)
        {
            if (other.id == id)
            {
                return 1;
            }
            return 0;
        }
    }
}
