#region References

using Server.Items;

#endregion

namespace Server.Mobiles
{
	[CorpseName("zwloki diamentowego smoka")]
	public class DiamondDragon : Dragon
	{
		[Constructable]
		public DiamondDragon()
		{
			Name = "diamentowy smok";
			BaseSoundID = 362;
			Hue = 1154;
			SetStr(850, 940);
			SetDex(90, 120);
			SetInt(550, 600);
			SetHits(450, 625);
			SetMana(415, 450);
			SetStam(120, 150);

			SetDamage(16, 19);

			SetDamageType(ResistanceType.Physical, 25);
			SetDamageType(ResistanceType.Fire, 0);
			SetDamageType(ResistanceType.Cold, 25);
			SetDamageType(ResistanceType.Poison, 25);
			SetDamageType(ResistanceType.Energy, 25);

			SetResistance(ResistanceType.Physical, 60, 75);
			SetResistance(ResistanceType.Fire, 30, 40);
			SetResistance(ResistanceType.Cold, 61, 70);
			SetResistance(ResistanceType.Poison, 50, 60);
			SetResistance(ResistanceType.Energy, 40, 50);

			SetSkill(SkillName.EvalInt, 90.0, 110.0);
			SetSkill(SkillName.Magery, 90.0, 120.0);
			SetSkill(SkillName.MagicResist, 99.1, 110.0);
			SetSkill(SkillName.Tactics, 90.1, 100.0);
			SetSkill(SkillName.Wrestling, 75.1, 100.0);
			SetSkill(SkillName.Meditation, 70.0, 100.0);
			SetSkill(SkillName.Anatomy, 70.0, 100.0);

			Fame = 15000;
			Karma = -15000;

			VirtualArmor = 70;

			Tamable = true;
			ControlSlots = 3;
			MinTameSkill = 95;

			SetWeaponAbility(WeaponAbility.BleedAttack);
			SetWeaponAbility(WeaponAbility.WhirlwindAttack);
			SetSpecialAbility(SpecialAbility.DragonBreath);
		}

		public override void OnCarve(Mobile from, Corpse corpse, Item with)
		{
			if (!IsBonded && !corpse.Carved && !IsChampionSpawn)
			{
				if (Utility.RandomDouble() < 0.05)
					corpse.DropItem(new DragonsHeart());
				if (Utility.RandomDouble() < 0.15)
					corpse.DropItem(new DragonsBlood());

				corpse.DropItem(new Diamond(8));
			}

			base.OnCarve(from, corpse, with);
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.FilthyRich, 5);
		}

		public override bool AutoDispel { get { return true; } }
		public override int TreasureMapLevel { get { return 5; } }
		public override int Meat { get { return 19; } }
		public override int Hides { get { return 10; } }
		public override HideType HideType { get { return HideType.Barbed; } }
		public override int Scales { get { return 7; } }
		public override ScaleType ScaleType { get { return ScaleType.White; } }
		public override FoodType FavoriteFood { get { return FoodType.Diamond; } }

		public DiamondDragon(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();

			if (version < 1)
				SetDamage(16, 19);
		}
	}
}
