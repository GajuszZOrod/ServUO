using Server.Spells.DeathKnight;

namespace Server.Items
{
	public class OrbOfOrcusSkull : DeathKnightSkull
	{
		[Constructable]
		public OrbOfOrcusSkull() : base( typeof(OrbOfOrcusSpell) )
		{
		}

		public override void AddNameProperties(ObjectPropertyList list)
		{
			base.AddNameProperties(list);
			list.Add( 1070722, "Sir Oslan Knarren");
			list.Add( 1049644, "Kula Smierci");
		}

		public OrbOfOrcusSkull( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
