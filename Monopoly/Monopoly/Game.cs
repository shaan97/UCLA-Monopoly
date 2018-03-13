using System;
using System.Collections.Generic;
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
        public Dictionary<string, Player> Opponents { get; protected set; }

        private IServer server;

        // Separate member variable for direct reference to main client
        private Player player;
        
        public Game(string name) {
            server = new WebSocketServer();
            var connection = server.ConnectAsync();

            player = new Player(name);
            Opponents = new Dictionary<string, Player>();

            // Make sure connection is established
            connection.Wait();

            // Must run join request synchronously, so wait for task completion
            var join_request = JoinGame();
            join_request.Wait();

            if(!join_request.Result) {
                System.Diagnostics.Debug.WriteLine("Joining game failed, so throwing exception.");
                throw new Exception("Unable to join game.");
            }

            // Get all information on active opponents in game synchronously
            var info_request = LoadOpponents();
            info_request.Wait();

            if(!info_request.Result) {
                System.Diagnostics.Debug.WriteLine("Unable to get data on opponents, so throwing exception.");
                throw new Exception("Unable to get opponent data.");
            }

            
        }

        private async Task<bool> JoinGame() {
            JObject json = new JObject {
                ["request"] = "JOIN_GAME",
                ["player_name"] = player.Name
            };
            
            var response = JObject.Parse(await server.Request(json));
            var status = (int)response["status"];

            System.Diagnostics.Debug.WriteLine($"JOIN_GAME response status: {status}");
         
            // HTTP Status Code 2xx for Success
            return status / 200 == 1;

        }

        private async Task<bool> LoadOpponents() {
            JObject json = new JObject {
                ["request"] = "PLAYER_INFO"
            };

            var response = JObject.Parse(await server.Request(json));
            var status = (int)response["status"];

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

        public async Task<LocationStats> GetLocationStats((double, double) gps_coordinates) {
            // JSON requesting Location info at specified coordinates
            JObject json = new JObject {
                ["request"] = "LOC_INFO",
                ["location"] = $"{gps_coordinates.Item1},{gps_coordinates.Item2}"
            };

            // Request response from server asynchronously
            var response = JObject.Parse(await server.Request(json));

            // Status using HTTP Status Codes
            int status = (int)response["status"];


            System.Diagnostics.Debug.WriteLine($"LOC_INFO response status: {status}");

            LocationStats location_stats = null;
            if (status / 200 == 1) {
                // HTTP Status Code 2xx for Success
                location_stats = new LocationStats(response);
            }
            

            return location_stats;
        }

        public async Task<bool> Purchase(Location location) {
            JObject json = new JObject {
                ["request"] = "PURCHASE",
                ["purchase_code"] = location.Properties.PurchaseCode,
                ["tier"] = location.Tier
            };

            // Send query and await response asynchronously
            var response = JObject.Parse(await server.Request(json));

            // True iff HTTP Status Code 2xx (Success)
            var success = (int)response["status"] / 200 == 1;
            if (success) {
                player.Purchase(location);
            }

            return success;
        }

        public async Task<bool> Purchase(IPurchasable item) {
            JObject json = new JObject {
                ["request"] = "PURCHASE",
                ["purchase_code"] = item.PurchaseCode
            };

            // Send query and await response asynchronously
            var response = JObject.Parse(await server.Request(json));
            
            // True iff HTTP Status Code 2xx (Success)
            var success = (int)response["status"] / 200 == 1;
            if (success) {
                player.Purchase(item);
            }

            return success;
        }
    }
}
