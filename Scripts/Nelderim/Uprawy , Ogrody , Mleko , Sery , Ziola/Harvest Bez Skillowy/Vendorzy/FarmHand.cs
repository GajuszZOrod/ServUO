#region References

using System.Collections.Generic;
using Server.Items;

#endregion

namespace Server.Mobiles
{
	public class FarmHand : BaseVendor
	{
		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos { get { return m_SBInfos; } }

		[Constructable]
		public FarmHand() : base(" - Rolnik")
		{
			SetSkill(SkillName.Cooking, 100.0, 120.0);
			SetSkill(SkillName.TasteID, 80.0, 100.0);
		}

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBFarmHand());
		}

		public override VendorShoeType ShoeType
		{
			get { return VendorShoeType.Sandals; }
		}

		public override int GetShoeHue()
		{
			return 0;
		}

		public override void InitOutfit()
		{
			base.InitOutfit();

			AddItem(new WideBrimHat(Utility.RandomNeutralHue()));
		}

		public FarmHand(Serial serial) : base(serial)
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
