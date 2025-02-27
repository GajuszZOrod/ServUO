//
// ** Basic Trap Framework (BTF)
// ** Author: Lichbane
//

#region References

using Server.Mobiles;

#endregion

namespace Server.Items
{
	public class PoisonRegularDartTrap : BaseTinkerTrap
	{
		private static readonly string m_ArmedName = "uzbrojona trująca pułapka z trucizną";
		private static readonly string m_UnarmedName = "nieuzbrojona trująca pułapka ztrucizną";
		private static readonly double m_ExpiresIn = 120.0;
		private static readonly int m_ArmingSkill = 25;
		private static readonly int m_DisarmingSkill = 40;
		private static readonly int m_KarmaLoss = 80;
		private static readonly bool m_AllowedInTown = false;

		[Constructable]
		public PoisonRegularDartTrap()
			: base(m_ArmedName, m_UnarmedName, m_ExpiresIn, m_ArmingSkill, m_DisarmingSkill, m_KarmaLoss,
				m_AllowedInTown)
		{
		}

		public override void TrapEffect(Mobile from)
		{
			from.PlaySound(0x4A); // click sound

			if (@from.Alive)
			{
				double penetration = Utility.RandomMinMax(20, 200);
				if (from.ArmorRating > penetration)
				{
					from.SendMessage("A poison dart bounces off your armor.");
				}
				else
				{
					from.ApplyPoison(from, Poison.Regular);
					from.SendMessage("You feel the sting of a poison dart");
				}
			}

			bool m_TrapsLimit = Trapcrafting.Config.TrapsLimit;
			if ((m_TrapsLimit) && (((PlayerMobile)this.Owner).TrapsActive > 0))
				((PlayerMobile)this.Owner).TrapsActive -= 1;

			this.Delete();
		}

		public PoisonRegularDartTrap(Serial serial) : base(serial)
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
