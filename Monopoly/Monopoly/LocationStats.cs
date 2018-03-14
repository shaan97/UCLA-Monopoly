using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Monopoly
{
    public class LocationStats
    {
        public readonly int NUM_TIERS = 5;

        public string Name { get; protected set; }

        // The i^th price corresponds to the i^th tier
        public long[] Prices { get; protected set; }

        // The i^th tax corresponds to the taxation needed for the i^th tier
        public long[] Taxes { get; protected set; }

        // (NorthEast, SouthWest) GPS Coordinates to define the region
        public ((double, double), (double, double)) Corners { get; protected set; }

        // If owned, set to the player's name
        public string Owner { get; protected set; } = null;

        // Unix Timestamp of when property expires if owned
        public long OwnedUntil { get; protected set; } = 0;

        // Tier of property if owned
        public int Tier { get; protected set; } = 0;

        public LocationStats(string json) : this(JObject.Parse(json)) { }

        public LocationStats(JObject json) {
            // Retrieve values of interest
            Name = (string)json["name"];

            var northeast = (Convert.ToDouble((string)json["ne_lat"]), Convert.ToDouble((string)json["ne_long"]));
            var southwest = (Convert.ToDouble((string)json["sw_lat"]), Convert.ToDouble((string)json["sw_long"]));

            Corners = ((Convert.ToDouble(northeast.Item1), Convert.ToDouble(northeast.Item2)),
                       (Convert.ToDouble(southwest.Item1), Convert.ToDouble(southwest.Item2)));

            var price = (int)json["price"];
            var taxes = (int)json["tax"];

            Owner = (string)json["owner"];
            Tier = (int)json["tier"];

            var owned_until = (string)json["owned_until"];
            OwnedUntil = Convert.ToInt64(owned_until);

            Prices = new long[] { 100, 200, 300, 400, 500 };
            Taxes = new long[] { 50, 100, 150, 200, 250 };
        }

        // @returns true if @param gps_coordinates is inside this location
        public bool Contains((double, double) gps_coordinates) {
            return  gps_coordinates.Item1 >= Corners.Item2.Item1 &&
                    gps_coordinates.Item2 >= Corners.Item2.Item2 &&
                    gps_coordinates.Item1 <= Corners.Item1.Item1 &&
                    gps_coordinates.Item2 <= Corners.Item1.Item2;
        }
    }
}
