﻿namespace Server.Items
{
	public class PrzekletaMaskaSmierci : BoneHelm
	{
		public override int InitMinHits { get { return 255; } }
		public override int InitMaxHits { get { return 255; } }

		public override int BaseColdResistance { get { return 15; } }
		public override int BaseEnergyResistance { get { return 12; } }
		public override int BasePhysicalResistance { get { return 11; } }
		public override int BasePoisonResistance { get { return 12; } }
		public override int BaseFireResistance { get { return 30; } }

		[Constructable]
		public PrzekletaMaskaSmierci()
		{
			Hue = 1180;
			Name = "Przeklęta Maska Smierci";
			ArmorAttributes.MageArmor = 1;
			Attributes.DefendChance = 15;
			Attributes.LowerManaCost = 8;
			Attributes.NightSight = 1;
			Attributes.SpellDamage = 10;
			LootType = LootType.Cursed;
		}

		public PrzekletaMaskaSmierci(Serial serial)
			: base(serial)
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
		}
	}
}
