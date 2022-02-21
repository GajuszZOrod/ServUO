namespace Server.Items
{
	public abstract class ChivalrySpellScroll : SpellScroll
	{
		public ChivalrySpellScroll(int spellID, int itemID, int amount) : base(spellID, itemID, amount)
		{
			Hue = 1150;
		}

		public ChivalrySpellScroll(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
