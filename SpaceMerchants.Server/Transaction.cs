using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceMerchants.Server
{
    public class Transaction
    {
        public Transaction(Wallet sellerWallet, Wallet buyerWallet, string item, int price)
        {
            SellerWallet = sellerWallet;
            BuyerWallet = buyerWallet;
            Item = item;
            Price = price;
        }

        public Wallet SellerWallet { get; }

        public Wallet BuyerWallet { get; }

        public string Item { get; }

        public int Price { get; }
    }
}
