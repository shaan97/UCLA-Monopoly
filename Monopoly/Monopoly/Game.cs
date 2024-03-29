﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
#if __ANDROID__
using Android.Gms.Location;
#endif

namespace Monopoly
{
    public class Game
    {

        public event EventHandler<long> OnCreditsChange;

        public Dictionary<string, Player> Opponents { get; protected set; }

        private LocationStats last_location;

        private IMap map;

        private IServer server;
        private readonly string https_uri = "https://monopoly-ucla.herokuapp.com";

        // Separate member variable for direct reference to main client
        public Player Player { get; protected set; }

        // A TEMPORARY HACK, PLEASE DELETE
        public static Game Instance;

        public Game(string name) {
            Instance = this;
            
            server = new WebSocketServer();
            server.OnServerNotification += Server_OnServerNotification;
            var connection = server.Connect();
            
            Player = new Player(name);
            Opponents = new Dictionary<string, Player>();
            
#if __ANDROID__
            map = Monopoly.Droid.AndroidMap.Instance;
#endif
            // If the location changes, change state accordingly
            map.LocationChanged += this.OnLocationChanged;
            
            // Must run join request synchronously, so wait for task completion
            var join_request = JoinGame();
            join_request.Wait();

            if(!join_request.Result) {
                System.Diagnostics.Debug.WriteLine("Joining game failed, so throwing exception.");
                throw new Exception("Unable to join game.");
            }
           
        }

        private void Server_OnServerNotification(object sender, string notif) {
            JObject json = JObject.Parse(notif);

            if (json["operation"] == null || (string)json["operation"] != "purchase_made") {
                System.Diagnostics.Debug.WriteLine($"Received invalid notification:\n{notif}");
                return;
            }

            Opponents[(string)json["owner"]].Locations.Add(new Location(new LocationStats(json), (long)json["tier"]));
        }

        private async Task<bool> JoinGame() {
            // Build HTTP Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(https_uri + "/players/addplayer/");
            request.Method = "POST";

            string post_data = $"player_name={Player.Name}";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(post_data);
            
            // Set ContentType for POST request
            request.ContentType = "application/x-www-form-urlencoded";

            // Set Length to length of post_data
            request.ContentLength = bytes.Length;

            // Send data
            using (var stream = request.GetRequestStream()) {
                stream.Write(bytes, 0, bytes.Length);
            }

            // Get response
            HttpWebResponse response = (HttpWebResponse)(request.GetResponse());

            System.Diagnostics.Debug.WriteLine($"JOIN_GAME response status: {(int)response.StatusCode}");
         
            // HTTP Status Code 2xx for Success
            return (int)response.StatusCode / 200 == 1;

        }

        // This code is not extensible if this app becomes global
        public async Task<List<LocationStats>> GetAllLocations() {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(https_uri + "/locations/alllocations/");
            request.Method = "GET";
            var response = request.GetResponse();

            var stream = response.GetResponseStream();
            string message = new StreamReader(stream).ReadToEnd();
            JArray json = JArray.Parse(message);

            List<LocationStats> locations = new List<LocationStats>();
            foreach (var location in json) {
                locations.Add(new LocationStats((JObject)location));
            }

            return locations;
        }
        /*
        private async Task<bool> LoadOpponents() {
            JObject json = new JObject {
                ["operation"] = "PLAYER_INFO"
            };

            var response = JObject.Parse(await server.Request(json));
            var status = (bool)response["success"];

            System.Diagnostics.Debug.WriteLine($"PLAYER_INFO response status: {status}");

            if (status / 200 == 1) {
                // HTTP Status Code 2xx Success
                var players = (JArray)response["players"];

                // Get data on opponents
                Player p = null;
                LinkedList<Location> land = null;
                foreach (var player in players) {

                    // Get list of Locations and corresponding tiers for given player
                    var locations = (JArray)player["locations"];
                    var tiers = (JArray)player["tiers"];

                    land = new LinkedList<Location>();
                    for(int i = 0; i < locations.Count; i++) {
                        var manager = new LocationStats((JObject)locations[i]);
                        land.AddLast(new Location(manager, (long)tiers[i]));
                    }

                    // Give player correct name, credits, and land
                    var name = (string)player["name"];
                    p = new Player(name, new BankAccount((long)player["credits"]), land);

                    // Add to opponents Dictionary
                    Opponents.Add(name, p);

                }
                return true;
            } else {
                // HTTP Status Code Failure or Unknown
                return false;
            }
        }
        */

        private async void OnLocationChanged(object sender, (double, double) current_location) {
            if (last_location == null || last_location.Contains(current_location)) {
                System.Diagnostics.Debug.WriteLine("Still in same location.");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Getting Location Stats on current location.");

            // Update last_location field
            last_location = await GetLocationStats(current_location);

            System.Diagnostics.Debug.WriteLine($"Now at {last_location.Name}.");

            // If someone owns property, pay tax to them
            if (last_location.Owner != null)
                await TaxPlayer(last_location);
        }

        private async Task<bool> TaxPlayer(LocationStats property) {
            if (property.Owner == null)
                return false;

            // Build HTTP Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(https_uri + "/locations/visited/");
            request.Method = "PUT";

            string put_data = $"player_name={Player.Name}&location_name={property.Name}";
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(put_data);

            // Set Length to length of post_data
            request.ContentLength = bytes.Length;

            // Send data
            Stream stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();

            // Get response
            HttpWebResponse response = (HttpWebResponse)(request.GetResponse());
            bool success = (int)response.StatusCode / 200 == 1;

            // Withdraw tax from player account if successful status code
            if (success) {
                Player.Account.Withdraw(property.Taxes[property.Tier]);
                // Notify subscribers

                var handler = OnCreditsChange;
                if(handler != null)
                    handler(this, Player.Account.Credits);
            }

            return success;
        }

        public async Task<LocationStats> GetLocationStats((double, double) gps_coordinates) {
            // JSON requesting Location info at specified coordinates
            JObject json = new JObject {
                ["operation"] = "LOC_INFO",
                ["parameters"] = new JObject {
                    ["lat"] = gps_coordinates.Item1,
                    ["long"] = gps_coordinates.Item2
                }
            };

            // Request response from server asynchronously
            var response = JObject.Parse(await server.Request(json));

            bool success = (bool)response["success"];

            System.Diagnostics.Debug.WriteLine($"LOC_INFO response status: {success}");

            LocationStats location_stats = null;
            if (success) {
                location_stats = new LocationStats(response);
            }
            

            return location_stats;
        }

        public async Task<bool> Purchase(Location location) {
            JObject json = new JObject {
                ["operation"] = "PURCHASE",
                ["tier"] = location.Tier
            };

            // Send query and await response asynchronously
            var response = JObject.Parse(await server.Request(json));

            // True iff HTTP Status Code 2xx (Success)
            var success = (bool)response["success"];
            if (success) {
                Player.Purchase(location);

                // Notify subscribers of change in credits
                OnCreditsChange(this, Player.Account.Credits);
            }

            return success;
        }

        public async Task<bool> Purchase(IPurchasable item) {
            JObject json = new JObject {
                ["operation"] = "PURCHASE",
                ["purchase_code"] = item.PurchaseCode
            };

            // Send query and await response asynchronously
            var response = JObject.Parse(await server.Request(json));

            // True iff HTTP Status Code 2xx (Success)
            var success = (bool)response["success"];
            if (success) {
                Player.Purchase(item);
                var handler = OnCreditsChange;
                
                // Notify subscribers of change in credits
                if (handler != null)
                    OnCreditsChange(this, Player.Account.Credits);
            }

            return success;
        }
    }
}
