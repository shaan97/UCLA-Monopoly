using System;
using System.Collections.Generic;
using System.Text;

namespace Monopoly
{
    public interface IPurchasable
    {
        long Price { get; }
        long PurchaseCode { get; }
    }
}
