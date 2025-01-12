using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformClustering
{
    public class Diem
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Diem(double x, double y)
        {
            X = Math.Round(x, 1);
            Y = Math.Round(y, 1);
        }
        public override string ToString()
        {
            return $"({X:F1}, {Y:F1})"; 
        }
    }

}
