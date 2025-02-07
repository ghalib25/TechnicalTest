using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

public class DiscountCalculator : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Users;Integrated Security=True;TrustServerCertificate=True;");
    }

    public static string ReverseHalf(string input)
    {
        int length = input.Length;
        int middle = length / 2;

        if (length % 2 != 0)
        {
            string left = input.Substring(0, middle);
            char middleChar = input[middle];
            string right = input.Substring(middle + 1);

            char[] leftArray = left.ToCharArray();
            Array.Reverse(leftArray);
            char[] rightArray = right.ToCharArray();
            Array.Reverse(rightArray);

            return new string(leftArray) + middleChar + new string(rightArray);
        }
        else
        {
            string left = input.Substring(0, middle);
            string right = input.Substring(middle);

            char[] leftArray = left.ToCharArray();
            Array.Reverse(leftArray);
            char[] rightArray = right.ToCharArray();
            Array.Reverse(rightArray);

            return new string(leftArray) + new string(rightArray);
        }
    }

    public static string CheckAnagram(string[] firstWords, string[] secondWords)
    {
        return string.Join("", firstWords.Zip(secondWords, (a, b) => IsAnagram(a, b) ? "1" : "0"));
    }

    private static bool IsAnagram(string a, string b)
    {
        return a.OrderBy(c => c).SequenceEqual(b.OrderBy(c => c));
    }

    public decimal CalculateDiscount(string customerType, int rewardPoints, decimal totalPurchase)
    {
        decimal discountPercentage = 0;
        int extraDiscount = 0;

        switch (customerType.ToLower())
        {
            case "platinum":
                discountPercentage = 0.50m;
                extraDiscount = rewardPoints switch
                {
                    <= 300 => 35,
                    <= 500 => 50,
                    _ => 68
                };
                break;
            case "gold":
                discountPercentage = 0.25m;
                extraDiscount = rewardPoints switch
                {
                    <= 300 => 25,
                    <= 500 => 34,
                    _ => 52
                };
                break;
            case "silver":
                discountPercentage = 0.10m;
                extraDiscount = rewardPoints switch
                {
                    <= 300 => 12,
                    <= 500 => 27,
                    _ => 39
                };
                break;
        }

        decimal discount = totalPurchase * discountPercentage + extraDiscount;
        decimal totalPay = totalPurchase - discount;

        SaveTransaction(customerType, rewardPoints, totalPurchase, discount, totalPay);

        return totalPay;
    }

    private void SaveTransaction(string customerType, int rewardPoints, decimal totalPurchase, decimal discount, decimal totalPay)
    {
        string transactionId = DateTime.Now.ToString("yyyyMMdd") + "_" + (Transactions.Count() + 1).ToString("D5");
        Transactions.Add(new Transaction { TransactionId = transactionId, CustomerType = customerType, RewardPoints = rewardPoints, TotalPurchase = totalPurchase, Discount = discount, TotalPay = totalPay });
        SaveChanges();
    }
}

public class Transaction
{
    public int Id { get; set; }
    public string TransactionId { get; set; }
    public string CustomerType { get; set; }
    public int RewardPoints { get; set; }
    public decimal TotalPurchase { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPay { get; set; }
}

// Program dengan Menu
class Program
{
    static void Main()
    {
        DiscountCalculator calc = new DiscountCalculator();

        while (true)
        {
            Console.WriteLine("\nMenu:");
            Console.WriteLine("1. Membalik Setengah String");
            Console.WriteLine("2. Cek Anagram");
            Console.WriteLine("3. Menghitung Diskon");
            Console.WriteLine("4. Keluar");
            Console.Write("Pilih menu (1-4): ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Masukkan string: ");
                    string inputString = Console.ReadLine();
                    Console.WriteLine("Hasil Balik: " + DiscountCalculator.ReverseHalf(inputString));
                    break;

                case "2":
                    Console.Write("Masukkan jumlah pasangan kata: ");
                    int n = int.Parse(Console.ReadLine());
                    string[] firstWords = new string[n];
                    string[] secondWords = new string[n];

                    for (int i = 0; i < n; i++)
                    {
                        Console.Write($"Masukkan kata pertama {i + 1}: ");
                        firstWords[i] = Console.ReadLine();
                        Console.Write($"Masukkan kata kedua {i + 1}: ");
                        secondWords[i] = Console.ReadLine();
                    }
                    Console.WriteLine("Hasil Anagram: " + DiscountCalculator.CheckAnagram(firstWords, secondWords));
                    break;

                case "3":
                    Console.WriteLine("\nPilih Tipe Customer:");
                    Console.WriteLine("1. Platinum");
                    Console.WriteLine("2. Gold");
                    Console.WriteLine("3. Silver");
                    Console.Write("Pilih (1-3): ");

                    string customerType = Console.ReadLine() switch
                    {
                        "1" => "platinum",
                        "2" => "gold",
                        "3" => "silver",
                        _ => ""
                    };

                    if (string.IsNullOrEmpty(customerType))
                    {
                        Console.WriteLine("Pilihan tidak valid.");
                        break;
                    }

                    Console.Write("Masukkan Point Reward: ");
                    if (!int.TryParse(Console.ReadLine(), out int rewardPoints))
                    {
                        Console.WriteLine("Input tidak valid untuk point reward.");
                        break;
                    }

                    Console.Write("Masukkan Total Belanja: ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal totalPurchase))
                    {
                        Console.WriteLine("Input tidak valid untuk total belanja.");
                        break;
                    }

                    decimal totalPay = calc.CalculateDiscount(customerType, rewardPoints, totalPurchase);
                    Console.WriteLine($"Total Bayar: {totalPay}");
                    break;

                case "4":
                    Console.WriteLine("Terima kasih!");
                    return;

                default:
                    Console.WriteLine("Pilihan tidak valid. Coba lagi.");
                    break;
            }
        }
    }
}
