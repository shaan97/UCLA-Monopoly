using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Monopoly
{
    public class Location : IPurchasable {
        public Location(string json) : this(JObject.Parse(json)) {}

        public Location(JObject json) {
            // Retrieve values of interest
        }

        public string GetPurchaseCode() {
            throw new NotImplementedException();
        }
    }
}
