using System;
using System.Collections.Generic;
using System.Text;

namespace Monopoly
{
    public class BankAccount
    {
        public long Credits { get; protected set; }

        public BankAccount(long credits = 0) {
            this.Credits = credits;
        }

        public bool Withdraw(long credits) {
            // Must have enough credits
            if (credits > this.Credits)
                return false;

            this.Credits -= credits;
            return true;
        }

        public bool Deposit(long credits) {
            this.Credits += credits;
            return true;
        }

    }
}
