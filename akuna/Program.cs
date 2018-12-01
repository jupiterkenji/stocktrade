using System;

namespace akuna
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Akuna Trading Test:");
            Console.WriteLine("---------------------------------------------------:");

            var solution = new Solution2();
        
            var input = string.Empty;
            do
            {
                Console.WriteLine("Enter Input:");
                input = Console.ReadLine();
                
                if (string.IsNullOrEmpty(input) || input == "QUIT")
                {
                    break;
                }
                
                var output = solution.Process(input);
                if (output != string.Empty)
                {
                    Console.WriteLine(output);
                }               
                Console.WriteLine("----------------------------------------");
            }
            while (true);       
        }
    }
}
