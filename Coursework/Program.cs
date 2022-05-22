using System;
using ActressMas;

namespace Coursework
{

    class Program
    {

        static void Main(string[] args)
        {

            var env = new EnvironmentMas(0, 0, true, null, false);

            EnvironmentAgent environmentAgent = new EnvironmentAgent();
            env.Add(environmentAgent, "environment");

            for (int j  = 0; j < 10; j++)
            {
                HouseholdAgent householdAgent = new HouseholdAgent();
                env.Add(householdAgent, $"house{j:D2}");
            }

            Auctioneer auctioneer = new Auctioneer(false);
            env.Add(auctioneer, "auctioneer");
            LoggingAgent loggingAgent = new LoggingAgent();
            env.Add(loggingAgent, "log");
            env.Start();



            Console.ReadLine();
        }
    }
}
