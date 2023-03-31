using System.Net;

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

            authorizePc.Authorize(client);

            core.ComputerID = authorizePc.ReturnId();

            while (true)
            {
                await core.GetDataFromAPI();

                core.Saver();

                Thread.Sleep(1000 * 600);
            }
        }
    }
}