#region References

using System.Collections;

#endregion

namespace Server.Mobiles
{
	[CorpseName("zwloki starszego goblina sapera")]
	public class FieryGoblinSapper : BaseCreature
	{
		[Constructable]
		public FieryGoblinSapper() : base(AIType.AI_Melee, FightMode.Closest, 12, 1, 0.2, 0.4)
		{
			Name = "starszy goblin saper";
			Body = 17;
			BaseSoundID = 0x45A;
			Hue = 1161;

			SetStr(96, 120);
			SetDex(81, 105);
			SetInt(36, 60);

			SetHits(38, 42);

			SetDamage(5, 7);

			SetDamageType(ResistanceType.Physical, 100);

			SetResistance(ResistanceType.Physical, 25, 30);
			SetResistance(ResistanceType.Fire, 20, 30);
			SetResistance(ResistanceType.Cold, 10, 20);
			SetResistance(ResistanceType.Poison, 10, 20);
			SetResistance(ResistanceType.Energy, 20, 30);

			SetSkill(SkillName.MagicResist, 50.1, 75.0);
			SetSkill(SkillName.Tactics, 55.1, 80.0);
			SetSkill(SkillName.Wrestling, 1000, 1000);

			Fame = 1500;
			Karma = -1500;

			VirtualArmor = 28;
		}

		public override void GenerateLoot()
		{
			AddLoot(LootPack.Meager);
		}

		public void Explode()
		{
			ArrayList list = new ArrayList();

			foreach (Mobile m in this.GetMobilesInRange(8))
			{
				if (!CanBeHarmful(m))
					continue;

				if (m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned))
					list.Add(m);
				else if (m.Player && m.AccessLevel == AccessLevel.Player)
					list.Add(m);
			}

			foreach (Mobile m in list)
			{
				DoHarmful(m);

				m.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
				m.PlaySound(0x207);

				m.SendMessage("Otrzymujesz obrazenia od eksplodujacego goblina!");

				int toDamage = Utility.RandomMinMax(20, 60);
				m.Damage(toDamage, this);
			}
		}

		public override void OnGaveMeleeAttack(Mobile defender)
		{
			base.OnGaveMeleeAttack(defender);
			this.Kill();
			this.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
			;
			this.PlaySound(0x207);
			Explode();
		}

		public override bool OnBeforeDeath()
		{
			Explode();
			this.FixedParticles(0x3709, 10, 30, 5052, EffectLayer.LeftFoot);
			this.PlaySound(0x207);
			return base.OnBeforeDeath();
		}

		public override double AttackMasterChance { get { return 0.15; } }
		public override bool CanRummageCorpses { get { return true; } }
		public override int TreasureMapLevel { get { return 1; } }
		public override int Meat { get { return 1; } }


		public FieryGoblinSapper(Serial serial) : base(serial)
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
