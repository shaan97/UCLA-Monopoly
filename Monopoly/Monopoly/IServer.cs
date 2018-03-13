using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Monopoly
{
    public interface IServer
    {
        // Connects to the server by opening the connection
        bool Connect();

        // Connects to server asynchronously
        Task<bool> ConnectAsync();

        // Send raw data to the server, with no response expected. 
        void Send(JObject json);

        // Send a request to the server and get a response asynchronously
        Task<string> Request(JObject json);

        // Closes connection to the server synchronously
        void Close();

        // Closes connection to the server asynchronously
        Task<bool> CloseAsync();
    }
}
