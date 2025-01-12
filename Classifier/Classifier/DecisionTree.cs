using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classifier
{
    public static class DecisionTree
    {    
        public static Node BuildTree(List<Customer> customers, List<string> attributes)
        {
            if (customers.All(c => c.Classification == customers[0].Classification))
            {
                return new Node { LeafValue = customers[0].Classification };
            }

            if (attributes.Count == 0)
            {
                return new Node { LeafValue = GetMajorityClass(customers) };
            }

            string bestAttribute = SelectBestAttribute(customers, attributes);
            Node node = new Node { Attribute = bestAttribute };

            var attributeValues = GetAttributeValues(bestAttribute);

            foreach (var value in attributeValues)
            {
                var subset = customers.Where(c => GetAttributeValue(c, bestAttribute) == value).ToList();

                if (subset.Count == 0)
                {
                    node.Children[value] = new Node { LeafValue = GetMajorityClass(customers) };
                }
                else
                {
                    var remainingAttributes = new List<string>(attributes);
                    remainingAttributes.Remove(bestAttribute);
                    node.Children[value] = BuildTree(subset, remainingAttributes);
                }
            }

            return node;
        }

        public static string SelectBestAttribute(List<Customer> customers, List<string> attributes)
        {
            // Cần thêm thuật toán chọn thuộc tính tốt nhất
            return attributes[0];
        }

        private static int GetAttributeValue(Customer customer, string attribute)
        {
            switch (attribute)
            {
                case "Gender": return customer.Gender;
                case "Age": return customer.Age;
                case "Income":
                    return customer.Income <= 10 ? 1 :
                           customer.Income <= 20 ? 2 :
                           customer.Income <= 50 ? 3 : 4;
                case "LoanAmount":
                    return customer.LoanAmount <= 10 ? 1 :
                           customer.LoanAmount <= 20 ? 2 :
                           customer.LoanAmount <= 50 ? 3 : 4;
                default: throw new Exception("Unknown attribute");
            }
        }


        public static List<int> GetAttributeValues(string attribute)
        {
            switch (attribute)
            {
                case "Gender": return new List<int> { 0, 1 }; // Nữ, Nam
                case "Age": return new List<int> { 1, 2, 3, 4 }; // Các nhóm tuổi
                case "Income": return new List<int> { 1, 2, 3, 4 }; // Các nhóm thu nhập
                case "LoanAmount": return new List<int> { 1, 2, 3, 4 }; // Các nhóm số tiền vay
                default: throw new Exception("Unknown attribute");
            }
        }




        public static string GetMajorityClass(List<Customer> customers)
        {
            return customers.GroupBy(c => c.Classification)
                            .OrderByDescending(g => g.Count())
                            .First().Key;
        }

        public static string Classify(Customer newCustomer, Node root)
        {
            Node currentNode = root;

            while (currentNode.LeafValue == null)
            {
                int value = GetAttributeValue(newCustomer, currentNode.Attribute);

                if (currentNode.Children.TryGetValue(value, out Node nextNode))
                {
                    currentNode = nextNode;
                }
                else
                {                  
                    return GetMajorityClass(new List<Customer> { newCustomer });
                }
            }

            return currentNode.LeafValue;
        }

    }
}
