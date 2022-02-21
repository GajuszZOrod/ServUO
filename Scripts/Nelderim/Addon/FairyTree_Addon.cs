////////////////////////////////////////
//                                     //
//   Generated by CEO's YAAAG - Ver 2  //
// (Yet Another Arya Addon Generator)  //
//    Modified by Hammerhand for       //
//      SA & High Seas content         //
//                                     //
////////////////////////////////////////

namespace Server.Items
{
	public class FairyTree_Addon : BaseAddon
	{
		private static readonly int[,] m_AddOnSimpleComponents =
		{
			{ 3341, 1, 0, 3 }, { 3344, -2, 3, 8 }, { 3345, -1, 3, 10 }, //
			{ 3345, 1, -1, 6 }, { 3340, 0, 2, 6 }, { 3146, 0, 3, 82 }, //
			{ 3146, 3, 0, 68 }, { 3375, 3, -1, 48 }, { 3375, 1, 1, 47 }, //
			{ 3375, -2, 4, 37 }, { 3375, -4, 5, 45 }, { 3146, -3, 5, 66 }, //
			{ 3146, -1, 3, 40 }, { 3146, 1, 2, 63 }, { 3146, 2, 1, 61 }, //
			{ 3146, 3, -2, 63 }, { 3146, -1, 4, 60 }, { 3146, -2, 3, 9 }, //
			{ 3146, -4, 2, 0 }, { 3146, 0, 2, 0 }, { 3146, 1, -1, 0 }, //
			{ 3146, 0, -3, 0 }, { 3211, -4, 3, 0 }, { 3210, 0, -2, 2 }, //
			{ 3205, -1, 2, 1 }, { 3207, -3, 3, 0 }, { 3207, 2, 0, 0 }, //
			{ 3148, -2, 4, 0 }, { 3148, 2, -2, 0 }, { 3149, -1, -3, 0 }, //
			{ 3149, 1, 1, 0 }, { 3149, -4, 3, 0 }, { 6815, 1, -4, 0 }, //
			{ 6815, -2, 3, 0 }, { 6817, -4, 3, 0 }, { 6817, 1, -2, 0 }, //
			{ 4806, 3, -2, 10 }, { 4807, 4, -3, 9 }, { 4805, 2, -1, 10 }, //
			{ 4804, 1, 0, 11 }, { 4803, 0, 1, 10 }, { 4802, -1, 2, 9 }, //
			{ 4801, -2, 3, 9 }, { 4800, -3, 4, 9 }, { 4799, -4, 5, 10 }, //
			{ 4797, 3, -2, 0 }, { 4796, 2, -1, 0 }, { 4795, 1, 0, 0 }, //
			{ 4794, 0, 1, 0 }, { 4793, -1, 2, 0 }, { 4792, -2, 3, 0 }, //
			{ 4791, -3, 4, 1 }, { 4790, -4, 5, 0 }
		};


		public override BaseAddonDeed Deed
		{
			get
			{
				return new FairyTree_AddonDeed();
			}
		}

		[Constructable]
		public FairyTree_Addon()
		{
			for (int i = 0; i < m_AddOnSimpleComponents.Length / 4; i++)
				AddComponent(new AddonComponent(m_AddOnSimpleComponents[i, 0]), m_AddOnSimpleComponents[i, 1],
					m_AddOnSimpleComponents[i, 2], m_AddOnSimpleComponents[i, 3]);


			AddComplexComponent(this, 2586, -1, 3, 65, 0, 1, "", 1); // 33
			AddComplexComponent(this, 2586, 3, -2, 53, 0, 1, "", 1); // 34
			AddComplexComponent(this, 2586, 1, 1, 63, 0, 1, "", 1); // 35
			AddComplexComponent(this, 2586, -3, 4, 46, 0, 1, "", 1); // 36
			AddComplexComponent(this, 2586, 2, -4, 18, 0, 1, "", 1); // 37
			AddComplexComponent(this, 2586, 2, 0, 38, 0, 1, "", 1); // 38
			AddComplexComponent(this, 2586, 0, 3, 38, 0, 1, "", 1); // 39
			AddComplexComponent(this, 2586, -4, 4, 26, 0, 1, "", 1); // 40
		}

		public FairyTree_Addon(Serial serial) : base(serial)
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

	public class FairyTree_AddonDeed : BaseAddonDeed
	{
		public override BaseAddon Addon
		{
			get
			{
				return new FairyTree_Addon();
			}
		}

		[Constructable]
		public FairyTree_AddonDeed()
		{
			Name = "FairyTree_";
		}

		public FairyTree_AddonDeed(Serial serial) : base(serial)
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
