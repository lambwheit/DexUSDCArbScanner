using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Web.UI;
using System.Threading;

namespace USDCArbHunter
{
    internal class Aggregator
    {
        public static double Paraswap(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            //return 0;
        start:

            try
            {

                using (HttpRequest httpRequest = new HttpRequest())
                {
                    //httpRequest.Proxy = HttpProxyClient.Parse(Program.proxy);

                    httpRequest.IgnoreProtocolErrors = true;

                    if (BuyTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        BuyTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    if (SellTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        SellTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    httpRequest.AddHeader("Host", "apiv5.paraswap.io");
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                    httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                    httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                    httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                    httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                    httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                    httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                    httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    string getResponse = httpRequest.Get("https://apiv5.paraswap.io/prices/?" +
                        "srcToken=" + SellTokenAddr +
                        "&destToken=" + BuyTokenAddr +
                        "&amount=" + Math.Round((decimal)(amount * Math.Pow(10, SellTokenDeci)),0) +
                        "&srcDecimals=" + SellTokenDeci +
                        "&destDecimals="+ BuyTokenDeci +
                        "&partner=llamaswap&side=SELL" +
                        "&network="+chainID +
                        "&excludeDEXS=ParaSwapPool,ParaSwapLimitOrders").ToString();
                    double RawTotal = 0;
                    if (getResponse.Contains("Invalid Amount") || getResponse.Contains("No routes found with enough liquidity")||getResponse.Contains("Bad USD price"))
                    {
                        return 0;
                    }
                    if(getResponse.Contains("Price Timeout"))
                    {
                        goto start;
                    }
                    foreach(var swaps in JObject.Parse(getResponse)["priceRoute"]["bestRoute"][0]["swaps"].Last["swapExchanges"])
                    {
                        RawTotal += double.Parse(swaps["destAmount"].ToString());
                    }
                    double finalTokenDecimal = BuyTokenDeci;
                    double FormatedTotal = RawTotal / Math.Pow(10, finalTokenDecimal);
                    double SlippageTotal = FormatedTotal * (1 - (slippage / 100));
                    return Math.Round(SlippageTotal, 4);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Unable to connect"))
                {
                    goto start;
                }
                return 0;
            }
        }
        public static double Firebird(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
        start:
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    httpRequest.IgnoreProtocolErrors = true;
                    httpRequest.Proxy = HttpProxyClient.Parse(Program.proxy);

                    if (BuyTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        BuyTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    if (SellTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        SellTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    httpRequest.AddHeader("Host", "router.firebird.finance");
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A: Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                    httpRequest.AddHeader("content-type", "application/json");
                    httpRequest.AddHeader("api-key", "firebird_defillama");
                    httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                    httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                    httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                    httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                    httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                    httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                    httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    string getResponse = httpRequest.Get("https://router.firebird.finance/aggregator/v2/quote?" +
                        "chainId=" + chainID +
                        "&from=" + SellTokenAddr +
                        "&to=" + BuyTokenAddr +
                        "&amount=" + Math.Round((decimal)(amount * Math.Pow(10, SellTokenDeci))) +
                        "&receiver=0x77801Bee9848c3b3B380528C77f6bF0EAe716094" +
                        "&slippage=" + (decimal)slippage / 100 +
                        "&source=defillama" +
                        "&ref=0x08a3c2A819E3de7ACa384c798269B3Ce1CD0e437").ToString();
                    if(getResponse.Contains("null") || string.IsNullOrEmpty(getResponse))
                    {
                        return 0;
                    }
                    if(getResponse.Contains("access denined"))
                    {
                        goto start;
                    }
                    httpRequest.AddHeader("Host", "router.firebird.finance");
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                    httpRequest.AddHeader("content-type", "application/json");
                    httpRequest.AddHeader("api-key", "firebird_defillama");
                    httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                    httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                    httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                    httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                    httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                    httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                    httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    string postResponse = httpRequest.Post("https://router.firebird.finance/aggregator/v2/encode", getResponse, "application/json").ToString();
                    if (postResponse.Contains("access denined"))
                    {
                        goto start;
                    }
                    JToken MaxReturn = JObject.Parse(postResponse)["maxReturn"];
                    double RawTotal = double.Parse(MaxReturn["totalTo"].ToString());
                    double finalTokenDecimal = BuyTokenDeci;
                    double FormatedTotal = RawTotal / Math.Pow(10, finalTokenDecimal);
                    double SlippageTotal = FormatedTotal * (1 - (slippage / 100));
                    return Math.Round(SlippageTotal, 4);
                }
            }
            catch(Exception e)
            {
                if(e.Message.Contains("Unable to connect"))
                {
                    goto start;
                }
                return 0;
            }
        }
        public static double OneInch(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
        start:

            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    httpRequest.Proxy = HttpProxyClient.Parse(Program.proxy);
                    httpRequest.IgnoreProtocolErrors = true;
                    if (BuyTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        BuyTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    if (SellTokenAddr == "0x0000000000000000000000000000000000000000")
                    {
                        SellTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                    }
                    httpRequest.AddHeader("Host", "api.1inch.io");
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                    httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                    httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                    httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                    httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                    httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                    httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                    httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    string getResponse = httpRequest.Get("https://api.1inch.io/v4.0/" + chainID +
                        "/quote?" +
                        "fromTokenAddress=" + SellTokenAddr +
                        "&toTokenAddress=" + BuyTokenAddr +
                        "&amount=" + Math.Round((decimal)(amount * Math.Pow(10, SellTokenDeci)), 0) +
                        "&slippage=" + slippage).ToString();
                    if (getResponse.Contains("insufficient liquidity"))
                    {
                        return 0;
                    }
                    if (getResponse.Contains("Rate limit exceeded"))
                    {
                        Thread.Sleep(5000);
                        goto start;
                    }
                    if (getResponse.Contains("Internal Server Error") || getResponse.Contains("cannot sync"))
                    {
                        goto start;
                    }
                    double RawTotal = double.Parse(JObject.Parse(getResponse)["toTokenAmount"].ToString());
                    double finalTokenDecimal = BuyTokenDeci;
                    double FormatedTotal = RawTotal / Math.Pow(10, finalTokenDecimal);
                    double SlippageTotal = FormatedTotal * (1 - (slippage / 100));
                    return Math.Round(SlippageTotal, 4);
                }   
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Unable to connect"))
                {
                    goto start;
                }
                return 0;
            }
        }
        public static double OpenOcean(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
        start:
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    httpRequest.Proxy = HttpProxyClient.Parse(Program.proxy);
                
                    httpRequest.IgnoreProtocolErrors = true;
                    if(chainID == 56)
                    {
                        if (BuyTokenAddr == "0x0000000000000000000000000000000000000000")
                        {
                            BuyTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                        }
                        if (SellTokenAddr == "0x0000000000000000000000000000000000000000")
                        {
                            SellTokenAddr = "0xeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
                        }
                    }
                    string gasprice = "";
                    string account = "0x0000000000000000000000000000000000000000";
                    if (chainID == 56)
                    {
                        gasprice = "5000000000";
                    }
                    if (chainID == 137 || chainID == 1 ||chainID == 43114)
                    {
                        account = "0x77801Bee9848c3b3B380528C77f6bF0EAe716094";
                        //if(chainID == 1)
                        //{
                        //    account = "0xa0b86991c6218b36c1d19d4a2e9eb0ce3606eb48";
                        //}
                        httpRequest.AddHeader("Host", "ethapi.openocean.finance");
                        httpRequest.AddHeader("Connection", "keep-alive");
                        httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                        httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                        httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                        httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                        httpRequest.AddHeader("Accept", "*/*");
                        httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                        httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                        httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                        httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                        httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                        httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                        if(chainID == 137)
                        {
                            gasprice = httpRequest.Get("https://ethapi.openocean.finance/v2/" + chainID +
                            "/gas-price").ToString().Substring("fast\":\"", "\"");
                        }
                        if(chainID == 1)
                        {
                            gasprice = JObject.Parse(httpRequest.Get("https://ethapi.openocean.finance/v2/" + chainID +
                            "/gas-price").ToString())["fast"]["maxPriorityFeePerGas"].ToString();
                        }

                        if (chainID == 43114)
                        {
                            gasprice = JObject.Parse(httpRequest.Get("https://ethapi.openocean.finance/v2/" + chainID +
                            "/gas-price").ToString())["fast"].ToString();
                        }
                    }
                    httpRequest.AddHeader("Host", "ethapi.openocean.finance");
                    httpRequest.AddHeader("Connection", "keep-alive");
                    httpRequest.AddHeader("sec-ch-ua", "\"Google Chrome\";v=\"111\", \"Not(A:Brand\";v=\"8\", \"Chromium\";v=\"111\"");
                    httpRequest.AddHeader("sec-ch-ua-mobile", "?0");
                    httpRequest.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                    httpRequest.AddHeader("sec-ch-ua-platform", "\"Windows\"");
                    httpRequest.AddHeader("Accept", "*/*");
                    httpRequest.AddHeader("Origin", "https://swap.defillama.com");
                    httpRequest.AddHeader("Sec-Fetch-Site", "cross-site");
                    httpRequest.AddHeader("Sec-Fetch-Mode", "cors");
                    httpRequest.AddHeader("Sec-Fetch-Dest", "empty");
                    httpRequest.AddHeader("Referer", "https://swap.defillama.com/");
                    httpRequest.AddHeader("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    string getResponse = httpRequest.Get("https://ethapi.openocean.finance/v2/"+chainID +
                        "/swap?inTokenAddress=" + SellTokenAddr +
                        "&outTokenAddress=" + BuyTokenAddr +
                        "&amount=" + Math.Round((decimal)(amount * Math.Pow(10, SellTokenDeci)), 0) +
                        "&gasPrice="+gasprice +
                        "&slippage=" + slippage * 100 +
                        "&account=" + account+
                        "&referrer=0x5521c3dfd563d48ca64e132324024470f3498526").ToString();
                    if (getResponse.Contains("Bad Request") || getResponse.Contains("null") || string.IsNullOrEmpty(getResponse) )
                    {
                        return 0;
                    }
                    double RawTotal = double.Parse(JObject.Parse(getResponse)["outAmount"].ToString());
                    double finalTokenDecimal = BuyTokenDeci;
                    double FormatedTotal = RawTotal / Math.Pow(10, finalTokenDecimal);
                    double SlippageTotal = FormatedTotal * (1 - (slippage / 100));
                    return Math.Round(SlippageTotal, 4);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Unable to connect"))
                {
                    goto start;
                }
                return 0;
            }
        }
        //eth only
        public static double CowSwap(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    return 0;
                }
            }
            catch
            { return 0; }
        }
        //avalanche only
        public static double YieldYak(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            try
            {
                using (HttpRequest httpRequest = new HttpRequest())
                {
                    return 0;
                }
            }
            catch
            { return 0; }
        }
        public static KeyValuePair<string, double> BNBGetBestPrice(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            ConcurrentDictionary<string, double> keyValuePairs = new ConcurrentDictionary<string, double>();
            keyValuePairs.TryAdd("Paraswap", Aggregator.Paraswap(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("Firebird", Aggregator.Firebird(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OneInch", Aggregator.OneInch(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OpenOcean", Aggregator.OpenOcean(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            KeyValuePair<string, double> temp = new KeyValuePair<string, double>();
            foreach(var item in keyValuePairs)
            {
                if(item.Value > temp.Value)
                {
                    temp = item;
                }
            }
            return temp;
        }
        public static KeyValuePair<string, double> PolyGetBestPrice(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            ConcurrentDictionary<string, double> keyValuePairs = new ConcurrentDictionary<string, double>();
            keyValuePairs.TryAdd("Paraswap", Aggregator.Paraswap(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("Firebird", Aggregator.Firebird(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OneInch", Aggregator.OneInch(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OpenOcean", Aggregator.OpenOcean(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            KeyValuePair<string, double> temp = new KeyValuePair<string, double>();
            foreach (var item in keyValuePairs)
            {
                if (item.Value > temp.Value)
                {
                    temp = item;
                }
            }
            return temp;
        }
        public static KeyValuePair<string, double> ETHGetBestPrice(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            ConcurrentDictionary<string, double> keyValuePairs = new ConcurrentDictionary<string, double>();
            keyValuePairs.TryAdd("Paraswap", Aggregator.Paraswap(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("Firebird", Aggregator.Firebird(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OneInch", Aggregator.OneInch(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OpenOcean", Aggregator.OpenOcean(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            KeyValuePair<string, double> temp = new KeyValuePair<string, double>();
            foreach (var item in keyValuePairs)
            {
                if (item.Value > temp.Value)
                {
                    temp = item;
                }
            }
            return temp;
        }
        public static KeyValuePair<string, double> AVAXGetBestPrice(int chainID, string SellTokenAddr, string BuyTokenAddr, double amount, double slippage, double SellTokenDeci, double BuyTokenDeci)
        {
            ConcurrentDictionary<string, double> keyValuePairs = new ConcurrentDictionary<string, double>();
            keyValuePairs.TryAdd("Paraswap", Aggregator.Paraswap(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("Firebird", Aggregator.Firebird(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OneInch", Aggregator.OneInch(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            keyValuePairs.TryAdd("OpenOcean", Aggregator.OpenOcean(chainID, SellTokenAddr, BuyTokenAddr, amount, slippage, SellTokenDeci, BuyTokenDeci));
            KeyValuePair<string, double> temp = new KeyValuePair<string, double>();
            foreach (var item in keyValuePairs)
            {
                if (item.Value > temp.Value)
                {
                    temp = item;
                }
            }
            return temp;
        }
    }
}
