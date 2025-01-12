using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformClustering
{
    public class Cum
    {
        public List<Diem> DiemCum { get; set; }
        public Diem TrungTam { get; set; }
        public Cum() { }
        public Cum(Diem trungTam)
        {
            TrungTam = trungTam;
            DiemCum = new List<Diem>();
        }
    }
}
