#region References

using System;
using Server.Mobiles;
using Server.Spells;

#endregion

namespace Server.ACC.CSS.Systems.Ranger
{
	public class RangerSummonMountSpell : RangerSpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
			"Przyzwanie Wierzcha", "*Gwizdze*",
			//SpellCircle.Fifth,
			266,
			9040,
			CReagent.SpringWater,
			Reagent.BlackPearl,
			Reagent.SulfurousAsh
		);

		public override SpellCircle Circle
		{
			get { return SpellCircle.Fifth; }
		}

		public override double CastDelay { get { return 2.0; } }
		public override int RequiredMana { get { return 15; } }
		public override double RequiredSkill { get { return 30; } }

		public RangerSummonMountSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
		{
			if (this.Scroll != null)
				Scroll.Consume();
		}

		private static readonly Type[] m_Types =
		{
			typeof(ForestOstard), typeof(DesertOstard), typeof(Ridgeback), typeof(ForestOstard), typeof(Horse),
			typeof(DesertOstard), typeof(Horse), typeof(Ridgeback),
			//typeof( SwampDragon ),
			typeof(RidableLlama), typeof(RidableLlama), typeof(Horse)
		};

		public override bool CheckCast()
		{
			if (!base.CheckCast())
				return false;

			if ((Caster.Followers + 1) > Caster.FollowersMax)
			{
				Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
				return false;
			}

			Caster.PlaySound(0x3D);
			return true;
		}

		public override void OnCast()
		{
			if (CheckSequence())
			{
				try
				{
					BaseCreature creature =
						(BaseCreature)Activator.CreateInstance(m_Types[Utility.Random(m_Types.Length)]);

					creature.ControlSlots = 1;

					TimeSpan duration;

					duration = TimeSpan.FromSeconds(Caster.Skills[DamageSkill].Fixed * 0.4);

					SpellHelper.Summon(creature, Caster, 0x215, duration, false, false);
				}
				catch
				{
				}
			}

			FinishSequence();
		}
	}
}
