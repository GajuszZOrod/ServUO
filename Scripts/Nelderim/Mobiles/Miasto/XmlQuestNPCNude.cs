#region References

using System;
using Server.Items;
using CPA = Server.CommandPropertyAttribute;

#endregion


namespace Server.Mobiles
{
	public class XmlQuestNPCNude : TalkingBaseCreature
	{
		[Constructable]
		public XmlQuestNPCNude() : this(-1)
		{
		}

		[Constructable]
		public XmlQuestNPCNude(int gender) : base(AIType.AI_Melee, FightMode.None, 10, 1, 0.8, 3.0)
		{
			SetStr(10, 30);
			SetDex(10, 30);
			SetInt(10, 30);

			Fame = 50;
			Karma = 50;

			CanHearGhosts = true;

			SpeechHue = Utility.RandomDyedHue();
			Title = String.Empty;
			Hue = Race.RandomSkinHue();

			switch (gender)
			{
				case -1:
					this.Female = Utility.RandomBool();
					break;
				case 0:
					this.Female = false;
					break;
				case 1:
					this.Female = true;
					break;
			}

			if (this.Female)
			{
				this.Body = 0x191;
				this.Name = NameList.RandomName("female");
				Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2045, 0x204A, 0x2046, 0x2049));
				hair.Hue = Race.RandomHairHue();
				hair.Layer = Layer.Hair;
				hair.Movable = false;
				AddItem(hair);
			}
			else
			{
				this.Body = 0x190;
				this.Name = NameList.RandomName("male");
				Item hair = new Item(Utility.RandomList(0x203B, 0x203C, 0x203D, 0x2044, 0x2045, 0x2047, 0x2048));
				hair.Hue = Race.RandomHairHue();
				hair.Layer = Layer.Hair;
				hair.Movable = false;
				AddItem(hair);
				Item beard =
					new Item(Utility.RandomList(0x0000, 0x203E, 0x203F, 0x2040, 0x2041, 0x2067, 0x2068, 0x2069));
				beard.Hue = hair.Hue;
				beard.Layer = Layer.FacialHair;
				beard.Movable = false;
				AddItem(beard);
			}

			Container pack = new Backpack();

			pack.DropItem(new Gold(0, 50));

			pack.Movable = false;

			AddItem(pack);
		}

		public XmlQuestNPCNude(Serial serial) : base(serial)
		{
		}


		private static int GetRandomHue()
		{
			switch (Utility.Random(6))
			{
				default:
				case 0: return 0;
				case 1: return Utility.RandomBlueHue();
				case 2: return Utility.RandomGreenHue();
				case 3: return Utility.RandomRedHue();
				case 4: return Utility.RandomYellowHue();
				case 5: return Utility.RandomNeutralHue();
			}
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
