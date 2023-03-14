using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace USDCArbHunter
{
    internal class ArbCalculation
    {
        //Arb Pathway
        //first request USDT -> RANDOM COIN
        //2nd request RANDOM COIN -> USDC
        //last request USDC -> USDT
        
        public static void CalculateBSCArb(JArray BSCCoins, List<Arbs> arbs)
        {
            List<Thread> threads = new List<Thread>();
            //find usdt and usdc tokens
            JToken USDC = null;
            JToken USDT = null;
            #region Remove USDT USDC From Pool
            foreach (var coin in BSCCoins)
            {
                if (coin["symbol"].ToString() == "USDT")
                {
                    USDT = coin;
                }
                if (coin["symbol"].ToString() == "USDC")
                {
                    USDC = coin;
                }
            }
            BSCCoins.Remove(USDT); BSCCoins.Remove(USDC);
            #endregion
            int chainID = 56;
            string USDTAddr = USDT["address"].ToString();
            string USDCAddr = USDC["address"].ToString();

            double amount = 1000;
            double slippage = 0.5;
            double USDTDecimal = double.Parse(USDT["decimals"].ToString());
            double USDCDecimal = double.Parse(USDC["decimals"].ToString());
            ConcurrentQueue<JToken> CoinQueue = new ConcurrentQueue<JToken>(BSCCoins);
            for(int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while(CoinQueue.Count != 0)
                    {
                        CoinQueue.TryDequeue(out var coin);
                        {
                            string CoinAddr = coin["address"].ToString();
                            double CoinDecimal = double.Parse(coin["decimals"].ToString());
                            KeyValuePair<string, double> USDTRand = Aggregator.BNBGetBestPrice(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                            KeyValuePair<string, double> RandUSDC = Aggregator.BNBGetBestPrice(chainID, CoinAddr, USDCAddr, USDTRand.Value, slippage, CoinDecimal, USDCDecimal);
                            KeyValuePair<string, double> USDCUSDT = Aggregator.BNBGetBestPrice(chainID, USDCAddr, USDTAddr, RandUSDC.Value, slippage, USDCDecimal, USDTDecimal);
                            double profit = (USDCUSDT.Value - amount) / amount * 100;
                            if (profit > 0)
                            {
                                Arbs json = new Arbs();
                                json.chain = chainID;
                                List<string> coins = new List<string>();
                                List<string> addresses = new List<string>();
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                coins.Add(coin["symbol"].ToString());
                                addresses.Add(CoinAddr);
                                coins.Add("USDC");
                                addresses.Add(USDCAddr);
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                json.coins = coins.ToArray();
                                json.addresses = addresses.ToArray();
                                json.profit = Math.Round(profit, 4);
                                arbs.Add(json);
                                Console.WriteLine("[BSC]Pathway:  USDT -> [" + USDTRand.Key + "] -> " + coin["symbol"] + " -> [" + RandUSDC.Key + "] -> USDC -> [" + USDCUSDT.Key + "] -> USDT | " + profit + "% Arb");

                            }
                            Program.bscCounter++;
                        }

                    }
                });
                thread.Start();
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
            /*
            foreach (var coin in BSCCoins)
            {
                //if (coin["symbol"].ToString() == "ASR")
                {
                    string CoinAddr = coin["address"].ToString();
                    double CoinDecimal = double.Parse(coin["decimals"].ToString());
                    //double Paraswap = Aggregator.Paraswap(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal,CoinDecimal);
                    //double Firebird = Aggregator.Firebird(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                    //double Oneinch = Aggregator.OneInch(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                    //double OpenOcean = Aggregator.OpenOcean(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                    KeyValuePair<string, double> USDTRand = Aggregator.BNBGetBestPrice(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                    KeyValuePair<string, double> RandUSDC = Aggregator.BNBGetBestPrice(chainID, CoinAddr, USDCAddr, USDTRand.Value, slippage, CoinDecimal, USDCDecimal);
                    KeyValuePair<string, double> USDCUSDT = Aggregator.BNBGetBestPrice(chainID, USDCAddr, USDTAddr, RandUSDC.Value, slippage, USDCDecimal, USDTDecimal);
                    double profit = (USDCUSDT.Value - amount) / amount * 100;
                    //if (profit > 0)
                    //{
                    Console.WriteLine("Pathway:  USDT -> [" + USDTRand.Key + "] -> " + coin["symbol"] + " -> [" + RandUSDC.Key + "] -> USDC -> [" + USDCUSDT.Key + "] -> USDT | " + profit + "% Arb");
                    //}
                }
                Program.bscCounter++;
            }
            */

        }
        public static void CalculateETHArb(JArray ETHCoins, List<Arbs> arbs)
        {
            List<Thread> threads = new List<Thread>();
            //find usdt and usdc tokens
            JToken USDC = null;
            JToken USDT = null;
            #region Remove USDT USDC From Pool
            foreach (var coin in ETHCoins)
            {
                if (coin["symbol"].ToString() == "USDT")
                {
                    USDT = coin;
                }
                if (coin["symbol"].ToString() == "USDC")
                {
                    USDC = coin;
                }
            }
            ETHCoins.Remove(USDT); ETHCoins.Remove(USDC);
            #endregion
            int chainID = 1;
            string USDTAddr = USDT["address"].ToString();
            string USDCAddr = USDC["address"].ToString();
            double amount = 1000;
            double slippage = 0.5;
            double USDTDecimal = double.Parse(USDT["decimals"].ToString());
            double USDCDecimal = double.Parse(USDC["decimals"].ToString());
            ConcurrentQueue<JToken> CoinQueue = new ConcurrentQueue<JToken>(ETHCoins);
            for (int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while (CoinQueue.Count != 0)
                    {
                        CoinQueue.TryDequeue(out var coin);
                        {
                            string CoinAddr = coin["address"].ToString();
                            double CoinDecimal = double.Parse(coin["decimals"].ToString());
                            KeyValuePair<string, double> USDTRand = Aggregator.ETHGetBestPrice(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                            KeyValuePair<string, double> RandUSDC = Aggregator.ETHGetBestPrice(chainID, CoinAddr, USDCAddr, USDTRand.Value, slippage, CoinDecimal, USDCDecimal);
                            KeyValuePair<string, double> USDCUSDT = Aggregator.ETHGetBestPrice(chainID, USDCAddr, USDTAddr, RandUSDC.Value, slippage, USDCDecimal, USDTDecimal);
                            double profit = (USDCUSDT.Value - amount) / amount * 100;
                            if (profit > 0)
                            {
                                Arbs json = new Arbs();
                                json.chain = chainID;
                                List<string> coins = new List<string>();
                                List<string> addresses = new List<string>();
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                coins.Add(coin["symbol"].ToString());
                                addresses.Add(CoinAddr);
                                coins.Add("USDC");
                                addresses.Add(USDCAddr);
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                json.coins = coins.ToArray();
                                json.addresses = addresses.ToArray();
                                json.profit = Math.Round(profit, 4);
                                arbs.Add(json);
                                Console.WriteLine("[ETH]Pathway:  USDT -> [" + USDTRand.Key + "] -> " + coin["symbol"] + " -> [" + RandUSDC.Key + "] -> USDC -> [" + USDCUSDT.Key + "] -> USDT | " + profit + "% Arb");
                            }
                            Program.ethCounter++;
                        }

                    }
                });
                thread.Start();
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        public static void CalculateAVAXArb(JArray AVAXCoins, List<Arbs> arbs)
        {
            List<Thread> threads = new List<Thread>();
            //find usdt and usdc tokens
            JToken USDC = null;
            JToken USDT = null;
            #region Remove USDT USDC From Pool
            foreach (var coin in AVAXCoins)
            {
                if (coin["symbol"].ToString() == "USDT")
                {
                    USDT = coin;
                }
                if (coin["symbol"].ToString() == "USDC")
                {
                    USDC = coin;
                }
            }
            AVAXCoins.Remove(USDT); AVAXCoins.Remove(USDC);
            #endregion
            int chainID = 43114;
            string USDTAddr = USDT["address"].ToString();
            string USDCAddr = USDC["address"].ToString();
            double amount = 1000;
            double slippage = 0.5;
            double USDTDecimal = double.Parse(USDT["decimals"].ToString());
            double USDCDecimal = double.Parse(USDC["decimals"].ToString());
            ConcurrentQueue<JToken> CoinQueue = new ConcurrentQueue<JToken>(AVAXCoins);
            for (int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while (CoinQueue.Count != 0)
                    {
                        CoinQueue.TryDequeue(out var coin);
                        {
                            string CoinAddr = coin["address"].ToString();
                            double CoinDecimal = double.Parse(coin["decimals"].ToString());
                            KeyValuePair<string, double> USDTRand = Aggregator.AVAXGetBestPrice(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                            KeyValuePair<string, double> RandUSDC = Aggregator.AVAXGetBestPrice(chainID, CoinAddr, USDCAddr, USDTRand.Value, slippage, CoinDecimal, USDCDecimal);
                            KeyValuePair<string, double> USDCUSDT = Aggregator.AVAXGetBestPrice(chainID, USDCAddr, USDTAddr, RandUSDC.Value, slippage, USDCDecimal, USDTDecimal);
                            double profit = (USDCUSDT.Value - amount) / amount * 100;
                            if (profit > 0)
                            {
                                Arbs json = new Arbs();
                                json.chain = chainID;
                                List<string> coins = new List<string>();
                                List<string> addresses = new List<string>();
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                coins.Add(coin["symbol"].ToString());
                                addresses.Add(CoinAddr);
                                coins.Add("USDC");
                                addresses.Add(USDCAddr);
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                json.coins = coins.ToArray();
                                json.addresses = addresses.ToArray();
                                json.profit = Math.Round(profit, 4);
                                arbs.Add(json);
                                Console.WriteLine("[AVAX]Pathway:  USDT -> [" + USDTRand.Key + "] -> " + coin["symbol"] + " -> [" + RandUSDC.Key + "] -> USDC -> [" + USDCUSDT.Key + "] -> USDT | " + profit + "% Arb");    
                            }
                            Program.avaxCounter++;
                        }

                    }
                });
                thread.Start();
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
        public static void ClaculatePolyArb(JArray PolyCoins, List<Arbs> arbs)
        {
            List<Thread> threads = new List<Thread>();
            //find usdt and usdc tokens
            JToken USDC = null;
            JToken USDT = null;
            #region Remove USDT USDC From Pool
            foreach (var coin in PolyCoins)
            {
                if (coin["symbol"].ToString() == "USDT")
                {
                    USDT = coin;
                }
                if (coin["symbol"].ToString() == "USDC")
                {
                    USDC = coin;
                }
            }
            PolyCoins.Remove(USDT); PolyCoins.Remove(USDC);
            #endregion
            int chainID = 137;
            string USDTAddr = USDT["address"].ToString();
            string USDCAddr = USDC["address"].ToString();
            double amount = 1000;
            double slippage = 0.5;
            double USDTDecimal = double.Parse(USDT["decimals"].ToString());
            double USDCDecimal = double.Parse(USDC["decimals"].ToString());
            ConcurrentQueue<JToken> CoinQueue = new ConcurrentQueue<JToken>(PolyCoins);
            for (int i = 0; i < 3; i++)
            {
                Thread thread = new Thread(() =>
                {
                    while (CoinQueue.Count != 0)
                    {
                        CoinQueue.TryDequeue(out var coin);
                        {
                            string CoinAddr = coin["address"].ToString();
                            double CoinDecimal = double.Parse(coin["decimals"].ToString());
                            KeyValuePair<string, double> USDTRand = Aggregator.PolyGetBestPrice(chainID, USDTAddr, CoinAddr, amount, slippage, USDTDecimal, CoinDecimal);
                            KeyValuePair<string, double> RandUSDC = Aggregator.PolyGetBestPrice(chainID, CoinAddr, USDCAddr, USDTRand.Value, slippage, CoinDecimal, USDCDecimal);
                            KeyValuePair<string, double> USDCUSDT = Aggregator.PolyGetBestPrice(chainID, USDCAddr, USDTAddr, RandUSDC.Value, slippage, USDCDecimal, USDTDecimal);
                            double profit = (USDCUSDT.Value - amount) / amount * 100;
                            if (profit > 0)
                            {
                                Arbs json = new Arbs();
                                json.chain = chainID;
                                List<string> coins = new List<string>();
                                List<string> addresses = new List<string>();
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                coins.Add(coin["symbol"].ToString());
                                addresses.Add(CoinAddr);
                                coins.Add("USDC");
                                addresses.Add(USDCAddr);
                                coins.Add("USDT");
                                addresses.Add(USDTAddr);
                                json.coins = coins.ToArray();
                                json.addresses = addresses.ToArray();
                                json.profit = Math.Round(profit, 4);
                                arbs.Add(json);
                                Console.WriteLine("[Poly]Pathway:  USDT -> [" + USDTRand.Key + "] -> " + coin["symbol"] + " -> [" + RandUSDC.Key + "] -> USDC -> [" + USDCUSDT.Key + "] -> USDT | " + profit + "% Arb");
                            }
                            Program.polyCounter++;
                        }

                    }
                });
                thread.Start();
                threads.Add(thread);
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }
        }
    }
}
