using System.Collections.Generic;
using Server.Items;
using System;

namespace Server.Mobiles
{
    public class SBAnimalTrainer : SBInfo
    {
        private readonly List<IBuyItemInfo> m_BuyInfo = new InternalBuyInfo();
        private readonly IShopSellInfo m_SellInfo = new InternalSellInfo();

        public override IShopSellInfo SellInfo => m_SellInfo;
        public override List<IBuyItemInfo> BuyInfo => m_BuyInfo;

        public class InternalBuyInfo : List<IBuyItemInfo>
        {
            public InternalBuyInfo()
			{
				Add( new AnimalBuyInfo( 1, typeof( Cat ), 100, 50, 201, 0 ) );
				Add( new AnimalBuyInfo( 1, typeof( Dog ), 100, 50, 217, 0 ) );
                Add(new AnimalBuyInfo( 1, typeof(PackLlama), 600, 50, 292, 0));
                Add(new AnimalBuyInfo( 1, typeof(RidableLlama), 500, 50, 292, 0));
				Add( new GenericBuyInfo( typeof( BallOfSummoning ), 10000, 20, 3630, 0 ) );
				Add( new GenericBuyInfo( typeof( PowderOfTranslocation ), 3000, 20, 9912, 0 ) );
				Add( new AnimalBuyInfo( 1, typeof( Bandage ), 10, 500, 0xE21, 0 ) );
                
				Add( new AnimalBuyInfo( 1, typeof( Horse ), 550, 50, 204, 0 ) );
				Add( new AnimalBuyInfo( 1, typeof( PackHorse ), 631, 50, 291, 0 ) );
              
				Add( new AnimalBuyInfo( 1, typeof( Rabbit ), 80, 50, 205, 0 ) );
                
                Add( new GenericBuyInfo( typeof( Carrot ), 5, 50, 0xC78, 0 ) );
				Add( new GenericBuyInfo( typeof( Apple ), 5, 50, 0x9D0, 0 ) );
				Add( new GenericBuyInfo( typeof( SheafOfHay ), 5, 5, 0xF36, 0 ) );
				
				if( !Core.AOS )
				{
					Add( new AnimalBuyInfo( 1, typeof( Eagle ), 402, 50, 5, 0 ) );
					Add( new AnimalBuyInfo( 1, typeof( BrownBear ), 855, 50, 167, 0 ) );
					Add( new AnimalBuyInfo( 1, typeof( GrizzlyBear ), 1767, 50, 212, 0 ) );
					Add( new AnimalBuyInfo( 1, typeof( Panther ), 1271, 50, 214, 0 ) );
					Add( new AnimalBuyInfo( 1, typeof( TimberWolf ), 768, 60, 225, 0 ) );
					Add( new AnimalBuyInfo( 1, typeof( Rat ), 107, 50, 238, 0 ) );
				}
					
			}
        }

        public class InternalSellInfo : GenericSellInfo
        {
        }
    }
}
