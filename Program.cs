using Demon.Functions.Objects;
using Demon.Models;
using System.Net;
using System.Net.Http.Json;

namespace Demon
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7140");

            Core core = new Core(client);
            Authorization authorizePc = new Authorization();

            await authorizePc.Authorize(client);
            core.ComputerID = authorizePc.ReturnId();


            while (true)
            {

                if (await core.CheckStatus())
                {
                    await core.GetDataFromAPI();

                    await core.Saver();

                    await core.PostReports();
                    core.Logs.Clear();
                }
                else
                {
                    Console.WriteLine("This computer is blocked");
                }

                Thread.Sleep(1000 * 60);

            }
        }
    }
}