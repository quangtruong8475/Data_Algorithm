using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Classifier
{
    public class Customer
    {
        public int Gender { get; set; }
        public int Age { get; set; }
        public int Income { get; set; }
        public int LoanAmount { get; set; }
        public string Classification { get; set; }

        public Customer() { }
        public Customer(int gender, int age, int income, int loanAmount)
        {
            Gender = gender;
            Age = age;
            Income = income;
            LoanAmount = loanAmount;
        }
        public Customer(int gender, int age, int income, int loanAmount, string classification)
        {
            Gender = gender;
            Age = age;
            Income = income;
            LoanAmount = loanAmount;
            Classification = classification;
        }
        public string DisplayGender(int gender)
        {
            if(gender == 0)
            {
                return "Nữ";
            }
            else
            {
                return "Nam";
            }
                            
        }
        public override string ToString()
        {
            return $"\tGender: {DisplayGender(Gender)}\n" +
                $"\tAge: {Age}\n" +
                $"\tIncome: {Income}M VND\n" +
                $"\tLoan Amount: {LoanAmount}M VND\n";
        }

    }

    public static class KNNAlgorithm
    {
        public static List<(Customer Customer, double Distance)> CalculateDistances(Customer newPoint, List<Customer> Customers)
        {
            return Customers.Select(dp => (dp, CalculateEuclideanDistance(newPoint, dp))).ToList();
        }

        public static double CalculateEuclideanDistance(Customer point1, Customer point2)
        {
            return Math.Sqrt(
                Math.Pow(point1.Gender - point2.Gender, 2) +
                Math.Pow(point1.Age - point2.Age, 2) +
                Math.Pow(point1.Income - point2.Income, 2) +
                Math.Pow(point1.LoanAmount - point2.LoanAmount, 2)
            );
        }

        public static List<(Customer Customer, double Distance)> SortDistances(List<(Customer Customer, double Distance)> distances)
        {
            return distances.OrderBy(d => d.Distance).ToList();
        }

        public static List<(Customer Customer, double Distance)> GetNearestNeighbors(List<(Customer Customer, double Distance)> sortedDistances, int k)
        {
            return sortedDistances.Take(k).ToList();
        }

        public static string AnalyzeClassification(List<(Customer Customer, double Distance)> nearestNeighbors)
        {
            var classificationResult = nearestNeighbors
                .GroupBy(n => n.Customer.Classification)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            return classificationResult;
        }
        
    }
}

