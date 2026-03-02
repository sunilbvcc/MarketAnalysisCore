using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows.Forms;
using System.Linq;

namespace MarketAnalysisCore
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static List<double> priceHistory = new();
        static string symbol = "RELIANCE.NS";
        string[] nse50Symbols = { "RELIANCE.NS", "TCS.NS", "HDFCBANK.NS", "INFY.NS" };


        static bool EnableNotifications = true; // 🔔 Set YES/NO here

        static async Task Main(string[] args)
        {
            Console.WriteLine("NSE 5-Min Market Monitor Started...");

            while (true)
            {
                if (IsMarketOpen())
                {
                    await MonitorMarket();
                }
                else
                {
                    Console.WriteLine("Market Closed...");
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        static bool IsMarketOpen()
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            DateTime istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            if (istTime.DayOfWeek == DayOfWeek.Saturday ||
                istTime.DayOfWeek == DayOfWeek.Sunday)
                return false;

            TimeSpan marketOpen = new TimeSpan(9, 15, 0);
            TimeSpan marketClose = new TimeSpan(15, 30, 0);

            return istTime.TimeOfDay >= marketOpen &&
                   istTime.TimeOfDay <= marketClose;
        }

        // Fetch prices for all top stocks
        static async Task<Dictionary<string, double>> GetAllStockPrices()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            string[] topFinanceSymbols = { "HDFC.NS", "ICICIPRULI.NS", "BAJAJFINSV.NS", "MUTHOOTFIN.NS" };
            string[] topBankSymbols = { "HDFCBANK.NS", "ICICIBANK.NS", "KOTAKBANK.NS", "SBIN.NS" };
            string[] topITSymbols = { "TCS.NS", "INFY.NS", "WIPRO.NS", "HCLTECH.NS" };
            string[] topCurrencySymbols = { "INR=X", "EURINR=X", "JPYINR=X", "GBPINR=X" };

            // Merge arrays using Linq
            string[] allSymbols = topFinanceSymbols
                                 .Concat(topBankSymbols)
                                 .Concat(topITSymbols)
                                 .Concat(topCurrencySymbols)
                                 .ToArray();

            //var prices = new Dictionary<string, double>();

            //foreach (var symbol in allSymbols)
            //{
            //    string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}";
            //    var response = await client.GetStringAsync(url);

            //    using JsonDocument doc = JsonDocument.Parse(response);
            //    double price = doc.RootElement
            //                      .GetProperty("chart")
            //                      .GetProperty("result")[0]
            //                      .GetProperty("meta")
            //                      .GetProperty("regularMarketPrice")
            //                      .GetDouble();

            //    prices[symbol] = price;
            //}

            var tasks = allSymbols.Select(async symbol =>
            {
                try
                {
                    string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}";
                    var response = await client.GetStringAsync(url);
                    using JsonDocument doc = JsonDocument.Parse(response);
                    double price = doc.RootElement
                                      .GetProperty("chart")
                                      .GetProperty("result")[0]
                                      .GetProperty("meta")
                                      .GetProperty("regularMarketPrice")
                                      .GetDouble();
                    return (symbol, price);
                }
                catch
                {
                    return (symbol: symbol, price: 0.0);
                }
            });

            var results = await Task.WhenAll(tasks);
            var stockPrices = results.ToDictionary(r => r.symbol, r => r.price);

            return stockPrices;
        }
        static async Task MonitorMarket()
        {
            try
            {
                //double currentPrice = await GetStockPrice();
                //priceHistory.Add(currentPrice);

                //if (priceHistory.Count > 10)
                //    priceHistory.RemoveAt(0);

                //Console.WriteLine($"[{DateTime.Now}] Current Price: {currentPrice}");

                //if (priceHistory.Count >= 5)
                //{
                //    string recommendation = GetRecommendation();
                //    Console.WriteLine($"Recommendation: {recommendation}");

                //    if (EnableNotifications)
                //        ShowNotification(currentPrice, recommendation);
                //}

                //Console.WriteLine("-----------------------------------");
                var stockPrices = await GetAllStockPrices();

                foreach (var kvp in stockPrices)
                {
                    string symbol = kvp.Key;
                    double price = kvp.Value;

                    Console.WriteLine($"{symbol}: {price}");

                    // Update price history (optional: per stock or overall)
                    priceHistory.Add(price);
                    if (priceHistory.Count > 10)
                        priceHistory.RemoveAt(0);

                    Console.WriteLine($"[{DateTime.Now}] Current Price: {price}");

                    if (priceHistory.Count >= 5)
                    {
                        string recommendation = GetRecommendation_All(priceHistory);
                        Console.WriteLine($"Recommendation for {symbol}: {recommendation}");

                        if (EnableNotifications)
                            ShowNotification_All(symbol, price, recommendation);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        static void ShowNotification_All(string symbol, double price, string recommendation)
        {
            notifyIcon.BalloonTipTitle = "RELIANCE Market Alert";
            notifyIcon.BalloonTipText = $"{symbol} ₹{price} -> {recommendation}";
            notifyIcon.ShowBalloonTip(5000);
            Console.WriteLine($"[NOTIFICATION] {symbol}: {price} -> {recommendation}");
        }
        static async Task<double> GetStockPrice()
        {
            double price = 0;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            string[] topFinanceSymbols = { "HDFC.NS", "ICICIPRULI.NS", "BAJAJFINSV.NS", "MUTHOOTFIN.NS" };
            string[] topBankSymbols = { "HDFCBANK.NS", "ICICIBANK.NS", "KOTAKBANK.NS", "SBIN.NS" };
            string[] topITSymbols = { "TCS.NS", "INFY.NS", "WIPRO.NS", "HCLTECH.NS" };
            string[] topCurrencySymbols = { "INR=X", "EURINR=X", "JPYINR=X", "GBPINR=X" };

            string[] allSymbols = topFinanceSymbols
                     .Concat(topBankSymbols)
                     .Concat(topITSymbols)
                     .Concat(topCurrencySymbols)
                     .ToArray();

            foreach (var symbol in allSymbols)
            {
                string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{symbol}";
                var response = await client.GetStringAsync(url);
                using JsonDocument doc = JsonDocument.Parse(response);
                price = doc.RootElement
                                  .GetProperty("chart")
                                  .GetProperty("result")[0]
                                  .GetProperty("meta")
                                  .GetProperty("regularMarketPrice")
                                  .GetDouble();
                Console.WriteLine($"{symbol}: {price}");
            }

            return price;

        }



        static string GetRecommendation_All(List<double> history)
        {
            double lastPrice = history[history.Count - 1];
            double avgPrice = history.Average();

            if (lastPrice > avgPrice) return "SELL (Downtrend detected)";
            else return "BUY (Uptrend detected)";
        }
        static string GetRecommendation()
        {
            int n = priceHistory.Count;
            double recent = priceHistory[n - 1];
            double previous = priceHistory[n - 5];

            if (recent > previous)
                return "📈 BUY (Uptrend detected)";
            else if (recent < previous)
                return "📉 SELL (Downtrend detected)";
            else
                return "⏸ HOLD (Sideways)";
        }

        static NotifyIcon notifyIcon = new NotifyIcon()
        {
            Icon = SystemIcons.Application,
            Visible = true
        };

        static void ShowNotification(double price, string recommendation)
        {
            notifyIcon.BalloonTipTitle = "RELIANCE Market Alert";
            notifyIcon.BalloonTipText = $"₹{price} - {recommendation}";
            notifyIcon.ShowBalloonTip(5000);
        }

    }
}

