//
// ** Basic Trap Framework (BTF)
// ** Author: Lichbane
//

#region References

using Server.Mobiles;

#endregion

namespace Server.Items
{
	public class LightningGreaterTrap : BaseTinkerTrap
	{
		private static readonly string m_ArmedName = "uzbrojona powiększa porażająca pułapka";
		private static readonly string m_UnarmedName = "nieuzbrojona powiększa porażająca pułapka";
		private static readonly double m_ExpiresIn = 120.0;
		private static readonly int m_ArmingSkill = 25;
		private static readonly int m_DisarmingSkill = 60;
		private static readonly int m_KarmaLoss = 60;
		private static readonly bool m_AllowedInTown = false;

		[Constructable]
		public LightningGreaterTrap()
			: base(m_ArmedName, m_UnarmedName, m_ExpiresIn, m_ArmingSkill, m_DisarmingSkill, m_KarmaLoss,
				m_AllowedInTown)
		{
		}

		public override void TrapEffect(Mobile from)
		{
			from.PlaySound(0x4A); // click sound

			from.BoltEffect(0);

			int damage = Utility.RandomMinMax(40, 60);
			AOS.Damage(from, from, damage, 50, 0, 0, 0, 100);

			bool m_TrapsLimit = Trapcrafting.Config.TrapsLimit;
			if ((m_TrapsLimit) && (((PlayerMobile)this.Owner).TrapsActive > 0))
				((PlayerMobile)this.Owner).TrapsActive -= 1;

			this.Delete();
		}

		public LightningGreaterTrap(Serial serial) : base(serial)
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
