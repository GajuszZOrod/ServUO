using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Spells;
using Server.Spells.Seventh;
using Server.Gumps;

namespace Server.ACC.CSS.Systems.Rogue
{
    public class RogueSlyFoxSpell : RogueSpell
    {
        private static SpellInfo m_Info = new SpellInfo(
                                                        "Sly Fox", " *Twoje cialo zaczyna zmieniac ksztalt* ",
            //SpellCircle.Fourth,
                                                        212,
                                                        9041,
                                                        CReagent.PetrafiedWood,
                                                        Reagent.NoxCrystal,
                                                        Reagent.Nightshade
                                                        );

         public override SpellCircle Circle
        {
            get { return SpellCircle.Eighth; }
        }
		public override double RequiredSkill{ get{ return 80.0; } }

		public override int RequiredMana{ get{ return 50; } }

		private static Hashtable m_Table = new Hashtable();

		public RogueSlyFoxSpell( Mobile caster, Item scroll ) : base( caster, scroll, m_Info )
		{
			                    if (this.Scroll != null)
                        Scroll.Consume();
		}

		public static bool HasEffect( Mobile m )
		{
			return ( m_Table[m] != null );
		}

		public static void RemoveEffect( Mobile m )
		{
			object[] mods = (object[])m_Table[m];

			if ( mods != null )
			{
				m.RemoveStatMod( ((StatMod)mods[0]).Name );
				m.RemoveStatMod( ((StatMod)mods[1]).Name );
				m.RemoveStatMod( ((StatMod)mods[2]).Name );
				//m.RemoveSkillMod( (SkillMod)mods[3] );
				//m.RemoveSkillMod( (SkillMod)mods[4] );
				m.RemoveSkillMod( (SkillMod)mods[3] );
			}

			m_Table.Remove( m );

			m.EndAction( typeof( RogueSlyFoxSpell ) );

			m.BodyMod = 0;
			m.HueMod = 0;
		}

		public override bool CheckCast()
		{
            if (!TransformationSpellHelper.CheckCast(Caster, this))
                return false;

            return base.CheckCast();
		}

		public override void OnCast()
		{
            TransformationSpellHelper.OnCast(Caster, this);

            if ( CheckSequence() )
			{
                RemoveEffect(Caster);

				object[] mods = new object[]
				{
					new StatMod( StatType.Str, "[Rogue] Str Offset", 10, TimeSpan.Zero ),
					new StatMod( StatType.Dex, "[Rogue] Dex Offset", 10, TimeSpan.Zero ),
					new StatMod( StatType.Int, "[Rogue] Int Offset", 10, TimeSpan.Zero ),
					//new DefaultSkillMod( SkillName.Macing, true, 20 ),
					//new DefaultSkillMod( SkillName.Healing, true, 20 ),
					new DefaultSkillMod( SkillName.Tracking, true, 20 )
				};

				m_Table[Caster] = mods;

				Caster.AddStatMod( (StatMod)mods[0] );
				Caster.AddStatMod( (StatMod)mods[1] );
				Caster.AddStatMod( (StatMod)mods[2] );
				//Caster.AddSkillMod( (SkillMod)mods[3] );
				//Caster.AddSkillMod( (SkillMod)mods[4] );
				Caster.AddSkillMod( (SkillMod)mods[3] );

				double span = 10.0 /* ClericDivineFocusSpell.GetScalar( Caster )*/;
				new InternalTimer( Caster, TimeSpan.FromMinutes( (int)span ) ).Start();

				IMount mount = Caster.Mount;

				if ( mount != null )
					mount.Rider = null;

				Caster.BodyMod = 65;
				Caster.HueMod = 2702;
				Caster.BeginAction( typeof( RogueSlyFoxSpell ) );
				Caster.PlaySound( 0x165 );
				Caster.FixedParticles( 0x3728, 1, 13, 0x480, 92, 3, EffectLayer.Head );
			}

            FinishSequence();
		}

		private class InternalTimer : Timer
		{
			private Mobile m_Owner;
			private DateTime m_Expire;

			public InternalTimer( Mobile owner, TimeSpan duration ) : base( TimeSpan.Zero, TimeSpan.FromSeconds( 0.1 ) )
			{
				m_Owner = owner;
				m_Expire = DateTime.Now + duration;
			}

			protected override void OnTick()
			{
				if ( DateTime.Now >= m_Expire )
				{
					RogueSlyFoxSpell.RemoveEffect( m_Owner );
					Stop();
				}
			}
		}
	}
}
