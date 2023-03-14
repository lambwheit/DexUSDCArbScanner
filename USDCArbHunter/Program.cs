using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace USDCArbHunter
{
    internal class Program
    {
        public static int bscCounter = 0;
        public static int polyCounter = 0;
        public static int ethCounter = 0;
        public static int avaxCounter = 0;
        //public static string proxy = "192.168.1.213:51080";
        //public static string proxy = "127.0.0.1:8888";
        public static string proxy = "127.0.0.1:1090";
        static void TitleUpdate()
        {
            while (true)
            {
                Console.Title = "Arb Hunter - Scanning BSC [" + bscCounter + "/" + BSCCoins.Count() + "] - Poly ["+polyCounter+"/"+PolyCoins.Count()+ "] - ETH ["+ethCounter+"/"+ETHCoins.Count()+ "] - AVAX ["+avaxCounter+"/"+ETHCoins.Count()+"]";
            }
        }
        static void SerializeJson(List<Arbs> arbs)
        {
            int count = 0;
            while (true)
            {
                string output = JsonConvert.SerializeObject(arbs);
                if(arbs.Count != count)
                {
                    File.WriteAllText("ArbLog.json", output);
                    count = arbs.Count;
                }
                Thread.Sleep(1000);
            }
        }
        static JArray BSCCoins = new JArray();
        static JArray ETHCoins = new JArray();
        static JArray AVAXCoins = new JArray();
        static JArray PolyCoins = new JArray();
        static void Main(string[] args)
        {
            Console.Title = "Arb Hunter";
            //get list of coins

            using (HttpRequest httpRequest = new HttpRequest())
            {
                string response = httpRequest.Get("https://swap.defillama.com/?chain=bsc").ToString();
                //File.WriteAllText("Source.html", response);
                string coins = response.Substring("<script id=\"__NEXT_DATA__\" type=\"application/json\">", "</script>");
                JObject coinobject = JObject.Parse(coins);
                JObject chains = JObject.Parse(JToken.FromObject(coinobject)["props"]["pageProps"]["tokenlist"].ToString());
                foreach (var chain in chains)
                {
                    //remove the chain if it's not the ones we want
                    if (chain.Key == "56")
                    {
                        foreach (var coin in chain.Value)
                        {
                            if (coin["isGeckoToken"] == null)
                            {
                                BSCCoins.Add(coin);
                            }
                        }
                    }
                    if (chain.Key == "1")
                    {
                        foreach (var coin in chain.Value)
                        {
                            if (coin["isGeckoToken"] == null)
                            {
                                ETHCoins.Add(coin);
                            }
                        }
                    }
                    if (chain.Key == "43114")
                    {
                        foreach (var coin in chain.Value)
                        {
                            if (coin["isGeckoToken"] == null)
                            {
                                AVAXCoins.Add(coin);
                            }
                        }
                    }
                    if (chain.Key == "137")
                    {
                        foreach (var coin in chain.Value)
                        {
                            if (coin["isGeckoToken"] == null)
                            {
                                PolyCoins.Add(coin);
                            }
                        }
                    }
                }
                /*
                //COINS
                File.WriteAllText("AVAX.json", AVAXCoins.ToString());
                File.WriteAllText("BSC.json", BSCCoins.ToString());
                File.WriteAllText("ETH.json", ETHCoins.ToString());
                File.WriteAllText("Poly.json", PolyCoins.ToString());
                */
                new Thread(TitleUpdate).Start();
                List<Thread>ArbCalc = new List<Thread>();
                List<Arbs> arbs = new List<Arbs>();
                ArbCalc.Add(new Thread(()=> ArbCalculation.CalculateBSCArb(BSCCoins, arbs)));
                ArbCalc.Add(new Thread(() => ArbCalculation.ClaculatePolyArb(PolyCoins, arbs)));
                ArbCalc.Add(new Thread(() => ArbCalculation.CalculateETHArb(ETHCoins, arbs)));
                ArbCalc.Add(new Thread(() => ArbCalculation.CalculateAVAXArb(AVAXCoins, arbs)));
                new Thread(() => SerializeJson(arbs)).Start();
                foreach (var thread in ArbCalc)
                {
                    thread.Start();
                }
                foreach(var thread in ArbCalc)
                {
                    thread.Join();
                }                
                //ArbCalculation.CalculateETHArb(ETHCoins);
                //ArbCalculation.CalculateAVAXArb(AVAXCoins);
                //ArbCalculation.CalculateBSCArb(BSCCoins);
                //ArbCalculation.ClaculatePolyArb(PolyCoins);
            }
            Console.ReadLine();
        }
    }
}
