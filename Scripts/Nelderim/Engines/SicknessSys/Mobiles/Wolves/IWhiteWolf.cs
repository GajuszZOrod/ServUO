#region References

using Server.Mobiles;

#endregion

namespace Server.SicknessSys.Mobiles
{
	[CorpseName("zwloki zarazonegoi bialego wilka")]
	public class IWhiteWolf : InfectedWolf
	{
		[Constructable]
		public IWhiteWolf() : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
		{
			Name = "zarazony bialy wilk";
			Body = Utility.RandomList(34, 37);
			BaseSoundID = 0xE5;

			SetStr(56, 80);
			SetDex(56, 75);
			SetInt(31, 55);

			SetHits(34, 48);
			SetMana(0);

			SetDamage(3, 7);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 15, 20);
			SetResistance(ResistanceType.Fire, 10, 15);
			SetResistance(ResistanceType.Cold, 20, 25);
			SetResistance(ResistanceType.Poison, 10, 15);
			SetResistance(ResistanceType.Energy, 10, 15);

			SetSkill(SkillName.MagicResist, 20.1, 35.0);
			SetSkill(SkillName.Tactics, 45.1, 60.0);
			SetSkill(SkillName.Wrestling, 45.1, 60.0);

			Fame = 450;
			Karma = 0;

			VirtualArmor = 16;

			Tamable = false;
		}

		public IWhiteWolf(Serial serial) : base(serial)
		{
		}

		public override int Meat
		{
			get
			{
				return 1;
			}
		}

		public override int Hides
		{
			get
			{
				return 6;
			}
		}

		public override FoodType FavoriteFood
		{
			get
			{
				return FoodType.Meat;
			}
		}

		public override PackInstinct PackInstinct
		{
			get
			{
				return PackInstinct.Canine;
			}
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
		}
	}
}
