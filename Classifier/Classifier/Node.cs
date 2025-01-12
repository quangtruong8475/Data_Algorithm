using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public class Node
    {

        public string Attribute { get; set; }
        public Dictionary<int, Node> Children { get; set; }
        public string LeafValue { get; set; }

        public Node()
        {
            Children = new Dictionary<int, Node>();
        }

    }
}
