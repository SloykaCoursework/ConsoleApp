using CourseWorkLibrary;
using CourseWorkLibrary.DataApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class DataApiClient : IDataApi
    {

        private readonly NetTcpClient netTcpClient;
        public DataApiClient(NetTcpClient netTcpClient)
        {
            this.netTcpClient = netTcpClient;
        }

        public async Task<string?> SendCommand(CommandCode commandCode)
        {

            var command = new Command()
            {
                Code = (byte)commandCode
            };

            var result = await netTcpClient.SendAsync(command);

            return result?.Arguments["Data"]?.ToString();

        }

    }
}
