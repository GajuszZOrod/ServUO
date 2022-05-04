using Server.Items;
using System.Collections.Generic;

namespace Server.Mobiles
{
    public class SBFurtrader : SBInfo
    {
        private readonly List<IBuyItemInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public override IShopSellInfo SellInfo => m_SellInfo;
        public override List<IBuyItemInfo> BuyInfo => m_BuyInfo;

        public class InternalBuyInfo : List<IBuyItemInfo>
        {
            public InternalBuyInfo()
            {
                Add( new GenericBuyInfo( typeof( Hides ), 10, 50, 0x1079, 0 ) ); 
            }
        }

        public class InternalSellInfo : GenericSellInfo
        {
            public InternalSellInfo()
            {
                Add( typeof( Hides ), 3 ); 
				Add( typeof( Leather ), 3 ); 
            }
        }
    }
}
