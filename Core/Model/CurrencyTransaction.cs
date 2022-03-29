using System;
using Core.Enum;
using LiteDB;

namespace Core.Model
{
    public class CurrencyTransaction
    {
        public DateTime LastBought { get; set; }

        public DateTime LastSold { get; set; }

        public int QuantityBought { get; set; }

        public int QuantitySold { get; set; }

        public int TotalProfit { get; set; }

        public int TotalExpenses { get; set; }

        [BsonIgnore]
        public int NetProfit => TotalProfit - TotalExpenses;
    }
}