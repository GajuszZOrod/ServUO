#region References

using System;
using System.Collections;
using Server.Mobiles;
using Server.Spells;

#endregion

namespace Server.ACC.CSS.Systems.Avatar
{
	public class AvatarArmysPaeonSpell : AvatarSpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
			"Witalność Armii", "Vitalium Engrevo Maxi",
			//SpellCircle.First,
			212,
			9041
		);

		public override SpellCircle Circle => SpellCircle.First;

		public override double CastDelay => 2;
		public override int RequiredTithing => 50;
		public override double RequiredSkill => 60.0;
		public override int RequiredMana => 15;

		public AvatarArmysPaeonSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
			if (this.Scroll != null)
				Scroll.Consume();
		}

		public override void OnCast()
		{
			if (CheckSequence())
			{
				ArrayList targets = new ArrayList();

				foreach (Mobile m in Caster.GetMobilesInRange(3))
				{
					if (Caster.CanBeBeneficial(m, false, true) && !(m is Golem))
						targets.Add(m);
				}

				for (int i = 0; i < targets.Count; ++i)
				{
					Mobile m = (Mobile)targets[i];

					TimeSpan duration = TimeSpan.FromSeconds(Caster.Skills[CastSkill].Value * 0.6);
					int rounds = (int)(Caster.Skills[SkillName.Anatomy].Value * .16);

					new ExpireTimer(m, 0, rounds, TimeSpan.FromSeconds(2)).Start();

					m.FixedParticles(0x376A, 9, 32, 5030, 0x21, 3, EffectLayer.Waist);
				}
			}

			FinishSequence();
		}

		private class ExpireTimer : Timer
		{
			private readonly Mobile m_Mobile;
			private int m_Round;
			private readonly int m_Totalrounds;

			public ExpireTimer(Mobile m, int round, int totalrounds, TimeSpan delay) : base(delay)
			{
				m_Mobile = m;
				m_Round = round;
				m_Totalrounds = totalrounds;
			}

			protected override void OnTick()
			{
				if (m_Mobile != null)
				{
					m_Mobile.Hits += 2;

					if (m_Round >= m_Totalrounds)
					{
						m_Mobile.SendMessage("Efekt modlitwy wygasa.");
					}
					else
					{
						m_Round += 1;
						new ExpireTimer(m_Mobile, m_Round, m_Totalrounds, TimeSpan.FromSeconds(2)).Start();
					}
				}
			}
		}
	}
}
