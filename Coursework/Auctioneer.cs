using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using ActressMas;

namespace Coursework
{
    class Auctioneer : Agent
    {
        class ValuePair
        {
            public ValuePair(int price, int total)
            {
                this.price = price;
                this.total = total;
                fail = false;
            }
            public int price;
            public int total;
            public bool fail;
        }
        Dictionary<string, ValuePair>bids;
        Dictionary<string, ValuePair> offers;       
        int timer;
        int totalNumOfTransactions = 0;
        const int timerStart = 5;
        bool single;
        public Auctioneer(bool single)
        {
            bids = new Dictionary<string, ValuePair>();
            offers = new Dictionary<string, ValuePair>();
            timer = 5;
            this.single = single;
        }


        public override void Setup()
        {

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
                    case "bid": //Add protection against non int messages
                        if (!bids.ContainsKey(message.Sender))
                        {
                            bids.Add(message.Sender, new ValuePair(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1])));
                            timer = timerStart;
                        }
                        else 
                        {
                            bids[message.Sender] = new ValuePair(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                            timer = timerStart;
                        }
                        if (offers.ContainsKey(message.Sender))
                            Console.WriteLine("Aaaaa");
                    
                        break;
                    case "offer":

                        if (!offers.ContainsKey(message.Sender))
                            offers.Add(message.Sender, new ValuePair(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1])));
                        else
                            offers[message.Sender] = new ValuePair(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                        timer=timerStart;
                        break;
                    case "exit":
                        bids.Remove(message.Sender);
                        offers.Remove(message.Sender);
                        break;

                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"\t{message.Format()}" + " Unable to process message");
            }
        }

        public override void ActDefault()
        {
            timer--;

            if (bids.Count != 0 && offers.Count != 0)
            {

                    RunAuction();


            }

            if (timer <= 0)
                EndAuction();
        }

        private void EndAuction()
        {
            Console.WriteLine("Auction Ended");
            foreach (KeyValuePair<string, ValuePair> bid in bids)
            {
                Send(bid.Key, "end");
            }
            foreach (KeyValuePair<string, ValuePair> offer in offers)
            {
                Send(offer.Key, "end");
            }
            bids.Clear();
            offers.Clear();
            Console.WriteLine($"Total Number of Transactions: {totalNumOfTransactions}");
            Send("log",$"end {totalNumOfTransactions} ");
            Send("environment", "end");
            Stop();

        }

        private void RunAuction()
        {
            //Sorting allows match with greatest quantity
            var sortedBids = bids.OrderByDescending(x => x.Value.price).ThenByDescending(x => x.Value.total).ToDictionary(x => x.Key, x => x.Value);
            var sortedOffer = offers.OrderByDescending(x => x.Value.price).ThenByDescending(x => x.Value.total).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<string, ValuePair> bid in sortedBids)
            {
                foreach (KeyValuePair<string, ValuePair> offer in sortedOffer)
                {
                    if (bid.Value.price == offer.Value.price)
                    {
                        if (bids.ContainsKey(bid.Key) && offers.ContainsKey(offer.Key)) // Check if both participants are still active
                        {

                            
                            int amount;
                            if (single)
                            {
                                amount = 1;
                            }
                            else
                            {
                                if (bid.Value.total <= offer.Value.total)
                                {
                                    amount = bid.Value.total;
                                }
                                else
                                {
                                    amount = offer.Value.total;
                                }
                            }

                            if (Environment.AllAgents().Contains(offer.Key) && Environment.AllAgents().Contains(bid.Key))
                            {
                                Send(bid.Key, $"success {bid.Value.price} {amount} {offer.Key}");
                                Send(offer.Key, $"success {offer.Value.price} {amount} {bid.Key}");
                                //Remove from lists
                                bids.Remove(bid.Key);
                                offers.Remove(offer.Key);
                                totalNumOfTransactions++;
                            }
                        }

                    }
       
                    }
                }



            //Send bids and offers failure messages
            foreach (KeyValuePair<string, ValuePair> bid in bids)
            {
                if (bid.Value.fail == false)
                {
                    Send(bid.Key, $"failure {bid.Value.price}");
                    bid.Value.fail = true;
                    
                }
            }

            foreach (KeyValuePair<string, ValuePair> offer in offers)
            {
                if (offer.Value.fail == false)
                {
                    Send(offer.Key, $"failure {offer.Value.price}");
                    offer.Value.fail = true;
                }
            }

            Console.WriteLine("Auction turn ended");
        }
    }
}
