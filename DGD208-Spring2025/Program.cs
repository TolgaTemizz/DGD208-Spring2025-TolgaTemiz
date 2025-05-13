using System;
using System.Threading.Tasks;

namespace DGD208_Spring2025
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Pet Simulator";
            var game = new Game();
            await game.RunAsync();
        }
    }
}
