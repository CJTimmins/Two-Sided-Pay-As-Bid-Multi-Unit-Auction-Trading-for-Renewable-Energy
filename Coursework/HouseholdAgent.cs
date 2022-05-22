using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Collections.Concurrent;
using ActressMas;

namespace Coursework
{
    class HouseholdAgent : Agent
    {
        enum Type { Buyer,Seller,Abstain};
        Type type = Type.Abstain;
        int utilityPrice;
        int need;
        int initNeed;
        int budget = 0;
        int initialRate;
        int finalRate;
        int interval;
        int updateRate;
        int initUpdate;
        int curRate;
        int successfulRenewable = 0;
        Random rnd = new Random();
        bool fail = false;

        public HouseholdAgent()
        {
            updateRate = rnd.Next(3, 8);
            initUpdate = updateRate;
        }

        public override void Setup()
        {
            Send("environment", "start");
        }

        public override void Act(Message message)
        {
            try
            {
                Console.WriteLine($"\t{message.Format()}");
                Log.messages++;
                message.Parse(out string action, out List<string> parameters);

                switch (action)
                {
                    case "inform":
                        HandleInform(parameters);
                        break;
                    //Recieve auction results
                    case "failure":
                        HandleFail(parameters);
                        break;
                    case "success":
                        HandleSuccess(parameters);
                        break;
                    case "end":
                        HandleEnd();
                        break;
                    case "cancel":
                        successfulRenewable--;
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"\t{message.Format()}" + " Unable to process message");
            }
        }

        public override void ActDefault()
        {
            if(type==Type.Buyer||type==Type.Seller)
            {
                if (need<=0)
                {
                    HandleEnd();
                }
            }
            if (fail==true)
            {
                updateRate--;
                if (updateRate <= 0)
                {
                    updateRate = initUpdate;
                    if (type == Type.Buyer)
                    {
                        if (curRate < finalRate)
                        {
                            curRate += interval;
                            Send("auctioneer", $"bid {curRate} {need}");
                            fail = false;
                        }
                    }
                    else if (type==Type.Seller)
                    {
                        if (curRate > finalRate)
                        {
                            curRate -= interval;
                            Send("auctioneer", $"offer {curRate} {need}");
                            fail = false;
                        }
                    }
                }
  

            }
        }

        private void HandleInform(List<string> parameters)
        {
            int demand = Convert.ToInt32(parameters[0]);
            int generation = Convert.ToInt32(parameters[1]);
            Random rnd = new Random();

            if (demand > generation)
            {
                type = Type.Buyer;
                utilityPrice = Convert.ToInt32(parameters[2]);
                need = demand - generation;
                initNeed = need;
                initialRate = Convert.ToInt32(parameters[3]);
                finalRate = utilityPrice;
                interval = 1;
                curRate = initialRate;
            }
            else if (demand < generation)
            {
                type = Type.Seller;
                utilityPrice = Convert.ToInt32(parameters[3]);
                need = generation - demand;
                initNeed = need;
                initialRate =Convert.ToInt32(parameters[2]);
                finalRate = utilityPrice;
                interval = 1;
                curRate = initialRate;
            }
            else if (demand == generation)
            {
                type = Type.Abstain;
                HandleEnd();
            }

            //Message Auctioneer Coordinator
            if (type == Type.Buyer)
                Send("auctioneer", $"bid {curRate} {need}");
            else if (type == Type.Seller)
                Send("auctioneer", $"offer {curRate} {need}");


        }

        private void HandleSuccess(List<string> parameters)
        {
            fail = false;
            if (type ==Type.Buyer)
            {
                budget -= Convert.ToInt32(parameters[0]);
                need -= Convert.ToInt32(parameters[1]);
                successfulRenewable++;
            }
            else if (type==Type.Seller)
            {
                budget += Convert.ToInt32(parameters[1]);
                need-= Convert.ToInt32(parameters[1]);
                successfulRenewable++;
            }
            if (need<=0)
            {
                curRate = Convert.ToInt32(parameters[0]);
                HandleEnd();

            }
            if (need<0)
            {
                throw new Exception("");
            }
            else
            {
                if (type==Type.Buyer)
                {
                    Send("auctioneer", $"bid {parameters[0]} {need}");
                    curRate = Convert.ToInt32(parameters[0]);
                }
                else if (type==Type.Seller)
                {
                    Send("auctioneer", $"offer {parameters[0]} {need}");
                    curRate = Convert.ToInt32(parameters[0]);
                }
            }

        }

        private void HandleFail(List<string> parameters)
        {
            fail = true;
            if (Convert.ToInt32(parameters[0]) == curRate )
            {
                if (type == Type.Buyer)
                {
                    if (curRate < finalRate)
                    {
                        updateRate--;
                        if (updateRate <= 0)
                        {
                            updateRate = initUpdate;
                            curRate += interval;
                            Send("auctioneer", $"bid {curRate} {need}");
                        }
                    }
                }
                else if (type == Type.Seller)
                {
                    if (curRate > finalRate)
                    {
                        updateRate--;
                        if (updateRate <= 0)
                        {
                            updateRate = initUpdate;
                            curRate -= interval;
                            Send("auctioneer", $"offer {curRate} {need}");
                        }
                    }
                }
            }

        }

        private void HandleEnd()
        {
            Stop();
            int utilityEnergy = 0;
            int count = need;
            while (count > 0)
            {

                if (type == Type.Buyer)
                {
                    budget -= utilityPrice;
                    utilityEnergy++;
                    count--;
                }
                else if (type == Type.Seller)
                {
                    budget += utilityPrice;
                    utilityEnergy++;
                    count--;
                }
            }




            if (type == Type.Buyer)
            {
                Send("log", $"log {this.Name} {type} {budget} {initNeed} {successfulRenewable} 0 {utilityEnergy} 0 {utilityPrice} {curRate} {updateRate} {finalRate}");
        }
            else if (type == Type.Seller)
            {
                Send("log", $"log {this.Name} {type} {budget} {-1 * initNeed} 0 {successfulRenewable} 0 {utilityEnergy} {utilityPrice} {curRate} {updateRate} {finalRate}");
            }
            else if (type == Type.Abstain)
            {
                Send("log", $"log {this.Name} {type} {budget} {initNeed} 0 {successfulRenewable} 0 {utilityEnergy} {utilityPrice} {curRate} {updateRate} {finalRate}");
            }
            Console.WriteLine($"Agent {this.Name}");
            Console.WriteLine($"Type:  { type}");

            Console.WriteLine($"Spent/Gained {budget}");
            Console.WriteLine($"Initial Energy Needs {successfulRenewable + utilityEnergy}");
            Console.WriteLine($"Energy gained/sold within community: {successfulRenewable}");
            Console.WriteLine($"Energy bought/sold to utilities: {utilityEnergy} at {utilityPrice}");



        }
    }
}
