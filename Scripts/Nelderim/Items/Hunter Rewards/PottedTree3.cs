namespace Server.Items
{
	public class PottedTree3 : Item
	{
		[Constructable]
		public PottedTree3() : base(0x2378)
		{
			Weight = 100;
		}

		public PottedTree3(Serial serial) : base(serial)
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
