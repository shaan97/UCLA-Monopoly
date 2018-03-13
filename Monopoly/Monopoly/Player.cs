using System;
using System.Collections.Generic;
using System.Text;

namespace Monopoly
{
    public class Player
    {
        public string Name { get; protected set; }
        public BankAccount Account { get; protected set; }
        public HashSet<Location> Locations { get; protected set; }
        public HashSet<IPurchasable> Purchasables { get; protected set; }

        public Player(string name) 
            : this(name, new BankAccount(), new HashSet<Location>()) { }

        public Player(string name, BankAccount account, IEnumerable<Location> land) {
            this.Name = name;
            this.Account = account;
            this.Locations = new HashSet<Location>(land);
            this.Purchasables = new HashSet<IPurchasable>();
        }

        public bool Purchase(IPurchasable item) {
            // Withdraw from account and purchase if player can purchase
            if (item.Price <= Account.Credits) {
                Account.Withdraw(item.Price);
                Purchasables.Add(item);
                return true;
            }
            return false;
        }

        public bool Purchase(Location loc) {
            // Withdraw from account and add land if player can purchase
            if(loc.Price <= Account.Credits) {
                Account.Withdraw(loc.Price);
                Locations.Add(loc);
                return true;
            }

            return false;
        }
    }
}
