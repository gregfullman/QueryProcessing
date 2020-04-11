using System;
using QueryProcessing;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var queryProcessor = new QueryProcessor();
            queryProcessor.Test();
            var i = 0;
        }
    }
}
