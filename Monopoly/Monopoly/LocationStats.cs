﻿using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json.Linq;

namespace Monopoly
{
    public class LocationStats
    {
        public string Name { get; protected set; }

        // The i^th price corresponds to the i^th tier
        public long[] Prices { get; protected set; }

        // The i^th tax corresponds to the taxation needed for the i^th tier
        public long[] Taxes { get; protected set; }

        // (NorthEast, SouthWest) GPS Coordinates to define the region
        public ((double, double), (double, double)) Corners { get; protected set; }

        // Purchase Code to represent this item
        public long PurchaseCode { get; protected set; }

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

            var northeast = ((string)json["northeast"]).Split(',');
            var southwest = ((string)json["southwest"]).Split(',');
            Corners = ((Convert.ToDouble(northeast[0]), Convert.ToDouble(northeast[1])),
                       (Convert.ToDouble(southwest[0]), Convert.ToDouble(southwest[1])));

            var prices = (JArray)json["prices"];
            var taxes = (JArray)json["taxes"];
            var size = Math.Min(prices.Count, taxes.Count);

            var owned_until = (string)json["owned_until"];
            OwnedUntil = Convert.ToInt64(owned_until);

            var purchase_code = (string)json["purchase_code"];
            PurchaseCode = Convert.ToInt64(purchase_code);
            Prices = new long[size];
            Taxes = new long[size];


            // Makes sure Prices.Count == Taxes.Count
            for (int i = 0; i < size; i++) {
                Prices[i] = (long)prices[i];
                Taxes[i] = (long)taxes[i];
            }
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
