using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class carpet21m : BaseAddon
	{
		public override BaseAddonDeed Deed
		{
			get
			{
				return new carpet21mDeed();
			}
		}

		[ Constructable ]
		public carpet21m()
		{
			AddonComponent ac = null;
			ac = new AddonComponent( 2762 );
			AddComponent( ac, -2, -2, 0 );
			ac = new AddonComponent( 2765 );
			AddComponent( ac, -2, -1, 0 );
			ac = new AddonComponent( 2765 );
			AddComponent( ac, -2, 0, 0 );
			ac = new AddonComponent( 2765 );
			AddComponent( ac, -2, 1, 0 );
			ac = new AddonComponent( 2763 );
			AddComponent( ac, -2, 2, 0 );
			
			ac = new AddonComponent( 2766 );
			AddComponent( ac, -1, -2, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, -1, -1, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, -1, 0, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, -1, 1, 0 );
			ac = new AddonComponent( 2768 );			
			AddComponent( ac, -1, 2, 0 );
			
		  ac = new AddonComponent( 2766 );
			AddComponent( ac, 0, -2, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 0, -1, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 0, 0, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 0, 1, 0 );
			ac = new AddonComponent( 2768 );			
			AddComponent( ac, 0, 2, 0 );
			
      ac = new AddonComponent( 2766 );
			AddComponent( ac, 1, -2, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 1, -1, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 1, 0, 0 );
			ac = new AddonComponent( 2758 );
			AddComponent( ac, 1, 1, 0 );
			ac = new AddonComponent( 2768 );			
			AddComponent( ac, 1, 2, 0 );
			
			ac = new AddonComponent( 2764 );
			AddComponent( ac, 2, -2, 0 );
			ac = new AddonComponent( 2767 );
			AddComponent( ac, 2, -1, 0 );
			ac = new AddonComponent( 2767 );
			AddComponent( ac, 2, 0, 0 );
			ac = new AddonComponent( 2767 );
			AddComponent( ac, 2, 1, 0 );
			ac = new AddonComponent( 2761 );
			AddComponent( ac, 2, 2, 0 );
			
		}

		public carpet21m( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}

	public class carpet21mDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new carpet21m();
			}
		}

		[Constructable]
		public carpet21mDeed()
		{
			Name = "Sredni czerwony dywan z wzorem [E]";
		}

		public carpet21mDeed( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( 0 ); // Version
		}

		public override void	Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
