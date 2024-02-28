using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH.Models
{
    public class Storage
    {
        public int id;

        public Bitmap bitmap { get; set; }

        public string name { get; set; }

        public string tag { get; set; }

        public Storage() 
        { }
        public Storage(int id, Bitmap bitmap, string name, string tag)
        {
            this.id = id;
            this.name = name;
            this.tag = tag;
            this.bitmap = bitmap;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
