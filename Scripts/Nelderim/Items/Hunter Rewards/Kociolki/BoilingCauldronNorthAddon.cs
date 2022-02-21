namespace Server.Items
{
	public class BoilingCauldronNorthAddon : BaseAddon
	{
		private static readonly int[,] m_AddOnSimpleComponents =
		{
			{ 2416, 0, 0, 8 }, { 2420, 0, 0, 0 } // 1	3	
		};


		public override BaseAddonDeed Deed
		{
			get
			{
				return new BoilingCauldronNorthAddonDeed();
			}
		}

		[Constructable]
		public BoilingCauldronNorthAddon()
		{
			for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
				AddComponent(new AddonComponent(m_AddOnSimpleComponents[i, 0]), m_AddOnSimpleComponents[i, 1],
					m_AddOnSimpleComponents[i, 2], m_AddOnSimpleComponents[i, 3]);


			AddComplexComponent(this, 4012, 0, 0, 0, 0, 0, "", 1); // 2
		}

		public BoilingCauldronNorthAddon(Serial serial) : base(serial)
		{
		}

		private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset,
			int hue, int lightsource)
		{
			AddComplexComponent(addon, item, xoffset, yoffset, zoffset, hue, lightsource, null, 1);
		}

		private static void AddComplexComponent(BaseAddon addon, int item, int xoffset, int yoffset, int zoffset,
			int hue, int lightsource, string name, int amount)
		{
			AddonComponent ac;
			ac = new AddonComponent(item);
			if (name != null && name.Length > 0)
				ac.Name = name;
			if (hue != 0)
				ac.Hue = hue;
			if (amount > 1)
			{
				ac.Stackable = true;
				ac.Amount = amount;
			}

			if (lightsource != -1)
				ac.Light = (LightType)lightsource;
			addon.AddComponent(ac, xoffset, yoffset, zoffset);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}

	public class BoilingCauldronNorthAddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new BoilingCauldronNorthAddon();
			}
		}

		[Constructable]
		public BoilingCauldronNorthAddonDeed()
		{
			Name = "Kociołek (N)";
		}

		public BoilingCauldronNorthAddonDeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // Version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}
}
