using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Monopoly {
    public class Location {

        // Current Tier Purchased
        public long Tier { get; protected set; }

        // The current price and tax value for this tier
        public long Price { get; protected set; }
        public long Tax { get; protected set; }

        /* LocationStats that manages generic properties of specific location.
         * Location is a specific instantiation of one of the tiers listed by
         * the LocationStats.
         */
        public LocationStats Properties { get; protected set; }

        public Location(LocationStats info, long tier) {
            this.Properties = info;
            Upgrade(tier);
        }

        public void Upgrade(long tier) {
            this.Tier = tier;
            this.Price = Properties.Prices[tier];
            this.Tax = Properties.Taxes[tier];
        }
        


    }
}
