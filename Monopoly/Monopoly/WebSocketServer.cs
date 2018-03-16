using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using WebSocket4Net;
using Newtonsoft.Json.Linq;

namespace Monopoly {
    public class WebSocketServer : IServer {
        public static string Url { get; } = "ws://monopoly-ucla.herokuapp.com/ws/locations/";

        public event EventHandler<string> OnServerNotification;

        private WebSocket socket;
        private bool connected = false;

        // ids that are available for request_id 
        private LinkedList<int> available_ids;

        // ids that are still awaiting a response
        private Dictionary<int, SemaphoreSlim> pending_ids;

        // Response buffers for each request. Keyed by Semaphore since it is guaranteed
        // to be known in WebSocketServer.Request(JObject) (request_id may not be resolved
        // until an ID becomes available)
        private Dictionary<SemaphoreSlim, string> responses;

        // requests that are waiting and the corresponding signal of completion
        private Queue<(JObject, SemaphoreSlim)> queued_requests;

        public WebSocketServer() : this(WebSocketServer.Url) { }

        public WebSocketServer(string url) {
            socket = new WebSocket(url);

            // Can only send 8 messages across network at any given time
            available_ids = new LinkedList<int>();
            for (int i = 0; i < 8; i++)
                available_ids.AddLast(i);

            pending_ids = new Dictionary<int, SemaphoreSlim>();
            queued_requests = new Queue<(JObject, SemaphoreSlim)>();
            responses = new Dictionary<SemaphoreSlim, string>();

            // Set up event handlers
            socket.Opened += OnOpen;
            socket.Error += OnError;
            socket.Closed += OnClose;
            socket.MessageReceived += OnMessageReceived;

        }

        ~WebSocketServer() {
            if (connected)
                Close();
        }

        public bool Connect() {
            socket.Open();
            return connected;
        }
        /*
        public Task<bool> ConnectAsync() {
            return socket.OpenAsync();
        }
        */
        // TODO : Design issue. Send some sort of generic message object, not JObject.
        public async void Send(JObject json) {

            int request_id = -1;
            LinkedListNode<int> node;
            SemaphoreSlim complete = new SemaphoreSlim(0, 1);

            lock (available_ids) {
                node = available_ids.First;
                if (node != null) {
                    request_id = node.Value;    // Capture value before removing
                    available_ids.RemoveFirst();
                } else {
                    // Let request be served when ID becomes available
                    queued_requests.Enqueue((json, complete));
                }
            }

            if (node != null) {
                SendWithID(json, request_id, complete);
            }

            // Asynchronously wait for request to complete
            await complete.WaitAsync();

            // Clean up data structures so request_id can be reused
            ServeNextRequest(request_id, complete);


        }

        private void ServeNextRequest(int request_id, SemaphoreSlim complete) {
            pending_ids.Remove(request_id);
            responses.Remove(complete);

            lock (available_ids) {
                if(queued_requests.Count == 0)
                    available_ids.AddLast(request_id);
                else {
                    (var json, var sem) = queued_requests.Dequeue();

                    SendWithID(json, request_id, sem);
                }
            }
        }

        private void SendWithID(JObject json, int request_id, SemaphoreSlim complete) {
            // Set request_id in json
            json["request_id"] = request_id;

            // Add to pending requests
            pending_ids.Add(request_id, complete);

            // Send data
            socket.Send(json.ToString());
        }

        // TODO : Design issue. Send some sort of generic message object, not JObject.
        public async Task<string> Request(JObject json) {
            int request_id = -1;
            LinkedListNode<int> node;
            SemaphoreSlim complete = new SemaphoreSlim(0, 1);

            lock (available_ids) {
                node = available_ids.First;
                if (node != null) {
                    request_id = node.Value;    // Capture value before removing
                    available_ids.RemoveFirst();
                }
                else {
                    // Let it be served once an ID becomes available (FIFO)
                    queued_requests.Enqueue((json, complete));
                }
            }

            if (node != null) {
                SendWithID(json, request_id, complete);
            }

            // Asynchronously wait for request to be served
            await complete.WaitAsync();

            // Get response and clear entry
            string response = responses[complete];

            // Serve next request or make the ID available again
            ServeNextRequest(request_id, complete);

            return response;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            var json = JObject.Parse(e.Message);

            if (json["request_id"] == null) {
                OnServerNotification(this, e.Message);
                return;
            }

            var request_id = (int)json["request_id"];
            var complete = pending_ids[request_id];

            responses[complete] = e.Message;

            // Signal that response is here
            complete.Release();
        }

        private void OnClose(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine($"Socket closed.");
            connected = false;
        }

        private void OnError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e) {
            System.Diagnostics.Debug.WriteLine($"Socket error. {e.Exception.Message}");
            connected = false;
        }

        private void OnOpen(object sender, EventArgs e) {
            System.Diagnostics.Debug.WriteLine("Socket connected.");
            connected = true;
        }

        public void Close() {
            socket.Close();
        }
        /*
        public Task<bool> CloseAsync() {
            return socket.CloseAsync();
        }
        */
    }
}
