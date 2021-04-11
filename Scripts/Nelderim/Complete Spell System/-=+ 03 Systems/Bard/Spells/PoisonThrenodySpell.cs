using System;
using Server.Spells;
using Server.Targeting;

namespace Server.ACC.CSS.Systems.Bard
{
	public class BardPoisonThrenodySpell : BardSpell
	{
		private static SpellInfo m_Info = new SpellInfo(
			"Tren Jadu", "Infectus",
			//SpellCircle.First,
			212, 9041
		);

		public virtual TimeSpan Cooldown { get { return TimeSpan.FromMinutes(0.2); } }

		public override SpellCircle Circle
		{
			get { return SpellCircle.First; }
		}

		public BardPoisonThrenodySpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
			if (this.Scroll != null)
				Scroll.Consume();
		}

		public override double CastDelay { get { return 2; } }
		public override double RequiredSkill { get { return 35.0; } }
		public override int RequiredMana { get { return 14; } }

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public void Target(Mobile m)
		{
			if (!Caster.CanSee(m))
			{
				Caster.SendLocalizedMessage(500237); // Target can not be seen.
			}
			else if (CheckHSequence(m))
			{
				Mobile source = Caster;
				if (m.BeginAction(typeof(BardPoisonThrenodySpell)))
				{
					SpellHelper.Turn(source, m);

					SpellHelper.CheckReflect(this, ref source, ref m);

					m.FixedParticles(0x374A, 10, 30, 5013, 0x238, 2, EffectLayer.Waist);

					int amount = (int)(Caster.Skills[CastSkill].Base * 0.17);
					TimeSpan duration = TimeSpan.FromSeconds(Caster.Skills[SkillName.Musicianship].Base * 0.18);

					m.SendMessage("Twoja odporność na trucizny spada.");
					ResistanceMod mod1 = new ResistanceMod(ResistanceType.Poison, -amount);

					m.AddResistanceMod(mod1);

					ExpireTimer timer1 = new ExpireTimer(m, mod1, duration);
					timer1.Start();
				}

				FinishSequence();
			}
		}

		private class ExpireTimer : Timer
		{
			private Mobile m_Mobile;
			private ResistanceMod m_Mods;

			public ExpireTimer(Mobile m, ResistanceMod mod, TimeSpan delay) : base(delay)
			{
				m_Mobile = m;
				m_Mods = mod;
			}

			public void DoExpire()
			{
				m_Mobile.RemoveResistanceMod(m_Mods);
				m_Mobile.EndAction(typeof(BardPoisonThrenodySpell));
				Stop();
			}

			protected override void OnTick()
			{
				if (m_Mobile != null)
				{
					m_Mobile.SendMessage("Efekt pieśni wygasa.");
					DoExpire();
				}
			}
		}

		private class InternalTarget : Target
		{
			private BardPoisonThrenodySpell m_Owner;

			public InternalTarget(BardPoisonThrenodySpell owner) : base(12, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
					m_Owner.Target((Mobile)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
