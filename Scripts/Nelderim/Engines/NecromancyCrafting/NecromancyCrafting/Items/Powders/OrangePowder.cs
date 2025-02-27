namespace Server.Items
{
	public class OrangePowder : Item
	{
		[Constructable]
		public OrangePowder() : this(1)
		{
		}

		[Constructable]
		public OrangePowder(int amount) : base(0xF8F)
		{
			Name = "Orange Powder";
			Stackable = true;
			Hue = 0x489;
			Amount = amount;
		}

		public OrangePowder(Serial serial)
			: base(serial)
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
