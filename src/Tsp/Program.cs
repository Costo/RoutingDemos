using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using GoogleMapsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace Tsp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            if(args.Length == 0)
            {
                args = new[]
                {
                    "montreal",
                    "quebec, qc",
                    "halifax",
                    "calgary",
                    "edmonton",
                    "yellowknife",
                    "moncton, nb",
                    "toronto",
                    "ottawa",
                    "winnipeg",
                    "saskatoon, sk",
                    "regina, sk",
                    "vancouver",
                };
            }

            Console.WriteLine($"INPUT");
            foreach (var arg in args)
            {
                Console.WriteLine($"{Array.IndexOf(args, arg)}: {arg}");
            }
            Console.WriteLine();

            Console.WriteLine($"Using Google Directions solver");

            var apiKey = configuration["ApiKey"];
            var solver = new GoogleDirectionsTsp(apiKey);
            var result = solver.Solve(args);

            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine($"{i}: {result[i]}");
            }
            Console.WriteLine();

            Console.WriteLine($"Using OR Tools solver");

            var maps = new GoogleMaps(new LoggerFactory(), new MemoryCache(Microsoft.Extensions.Options.Options.Create(new MemoryCacheOptions())), apiKey);
            result = new OrToolsTsp(maps).Solve(args);

            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine($"{i}: {result[i]}");
            }

        }
    }
}
