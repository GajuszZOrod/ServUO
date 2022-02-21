namespace Server.Items
{
	public class figurka28 : Item
	{
		[Constructable]
		public figurka28() : this(1)
		{
		}

		[Constructable]
		public figurka28(int amount) : base(0x25D7)
		{
			Weight = 1.0;
			ItemID = 9687;
			Amount = amount;
			Name = "Blue Crystal";
			Hue = 0;
		}

		public figurka28(Serial serial) : base(serial)
		{
		}


		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			if (Hue == 0)
				Hue = 0;
		}
	}
}
