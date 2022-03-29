using System;
using System.Collections.Generic;
using Core.Enum;
using LiteDB;

namespace Core.Model
{
    public class GoodsOrService
    {

        public GoodsOrService()
        {
            CurrencyTransactions = new Dictionary<CurrencyType, CurrencyTransaction>();
        }
        
        [BsonId]
        public string Name { get; set; } = null!;
        
        public DateTime Created { get; set; }

        public DateTime Updated { get; set; }

        public IDictionary<CurrencyType, CurrencyTransaction> CurrencyTransactions { get; set; }
    }
}