using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GASH.Models
{
    public class Good
    {
        public int id;

        public Bitmap? image 
        { 
            get
            {
                if (img == null)
                {
                    img = Imager.ImageFromPath(imgPath);
                }

                return img;
            }
            set
            {
                img = value;
            }
        }

        private Bitmap img;

        public string imgPath;

        public string? name { get; set; }

        public string? description { get; set; }

        public Manufacturer? manufacturer { get; set; }

        public Category? category { get; set; }

        public Unit? unit { get; set; }

        public float price { get; set; }

        public int count { get; set; }

        public Good()
        {
        }

        public Good(string imagePath, string name, string description, Manufacturer manufacturer, Category category, Unit unit, float price, int count)
        {
            imgPath = imagePath;
            this.name = name;
            this.description = description;
            this.price = price;
            this.manufacturer = manufacturer;
            this.category = category;
            this.count = count;
            this.unit = unit;

            image = new Bitmap(imgPath);
        }

        public Good(int id, string imagePath, string name, string description, Manufacturer manufacturer, Category category, Unit unit, float price, int count)
        {
            this.id = id;
            imgPath = imagePath;
            this.name = name;
            this.description = description;
            this.price = price;
            this.manufacturer = manufacturer;
            this.category = category;
            this.count = count;
            this.unit = unit;

            

            image = new Bitmap(imgPath);

            Debug.WriteLine($"PATH = {imgPath} || BITMAP = {image.Size}");
        }

        public override string ToString()
        {
            return name;
        }
    }
}
