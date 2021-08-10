using System;
using Server;
using Server.Items;
using System.Collections;
using System.Collections.Generic;
using Server.Misc;
using Server.Spells;
using Server.Spells.Third;
using Server.Spells.Sixth;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "zwloki Dalharuk'a Elghinn'a" )]
	public class DalharukElghinn : BaseCreature
	{
        public override double DifficultyScalar { get { return 1.40; } }
		public override bool BardImmune{ get{ return false; } }
        public override double AttackMasterChance { get { return 0.15; } }
        public override double SwitchTargetChance { get { return 0.15; } }
		public override double DispelDifficulty{ get{ return 135.0; } }
		public override double DispelFocus{ get{ return 45.0; } }
		public override bool AutoDispel{ get{ return false; } }
		public override Poison PoisonImmune { get { return Poison.Greater; } }
		public override int TreasureMapLevel{ get{ return 5; } }
		public override int Meat{ get{ return 19; } }
		public override int Hides{ get{ return 60; } }
		public override HideType HideType{ get{ return HideType.Barbed; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }
        public override Poison HitPoison{ get{ return Utility.RandomBool() ? Poison.Deadly : Poison.Lethal; } } 
        public virtual int StrikingRange{ get{ return 12; } }
	
		
        public override void AddWeaponAbilities()
        {
            WeaponAbilities.Add( WeaponAbility.Bladeweave, 0.4 );
            WeaponAbilities.Add( WeaponAbility.BleedAttack, 0.222 );
            WeaponAbilities.Add( WeaponAbility.WhirlwindAttack, 0.222 );
        }
		
		[Constructable]
		public DalharukElghinn () : base( AIType.AI_Boss, FightMode.Closest, 11, 1, 0.25, 0.5 )
		{
			Name = "Dalharuk Elghinn";

			Body = 312;
			BaseSoundID = 362;
			Hue = 2156;

			SetStr( 1500, 1600 );
			SetDex( 120, 130 );
			SetInt( 600, 800 );

			SetHits( 22000 );

			SetDamage( 25, 35 );

			SetDamageType( ResistanceType.Fire, 20 );
			SetDamageType( ResistanceType.Energy, 80 );

			SetResistance( ResistanceType.Physical, 80 );
			SetResistance( ResistanceType.Fire, 65 );
			SetResistance( ResistanceType.Cold, 70 );
			SetResistance( ResistanceType.Poison, 50 );
			SetResistance( ResistanceType.Energy, 90 );

			SetSkill( SkillName.EvalInt, 160.1, 160.2 );
			SetSkill( SkillName.Magery, 155.1, 160.0 );
			SetSkill( SkillName.MagicResist, 110.1, 120.0 );
			SetSkill( SkillName.Tactics, 110.1, 120.0 );
			SetSkill( SkillName.Wrestling, 70.1, 80.0 );
			SetSkill( SkillName.DetectHidden, 90.1, 120.5 );
            SetSkill( SkillName.Focus, 20.2, 30.0 );

			Fame = 25000;
			Karma = -25000;

			VirtualArmor = 70;
			
            m_Change = DateTime.Now;
			m_Stomp = DateTime.Now;
			
		}
        public override void OnDeath( Container c )
		{
			base.OnDeath( c );

            ArtifactHelper.ArtifactDistribution(this);
		}
		public override void GenerateLoot()
		{
			AddLoot( LootPack.AosSuperBoss );
			// 07.01.2013 :: szczaw :: usuniecie PackGold
			//PackGold(1000, 1500 );
		}
		public override void OnCarve(Mobile from, Corpse corpse, Item with)
		{
            if (!IsBonded && !corpse.Carved && !IsChampionSpawn)
            {
				if( Utility.RandomDouble() < 0.15 )
					corpse.DropItem( new FireRuby() );
            }

			base.OnCarve(from, corpse, with);
		}

        public DalharukElghinn( Serial serial ) : base( serial )
		{
		}

		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			
			int version = reader.ReadInt();
			
			m_Change = DateTime.Now;
			m_Stomp = DateTime.Now;
		}
        public override void OnDamagedBySpell( Mobile attacker )
		{
			base.OnDamagedBySpell( attacker );

			DoCounter( attacker );
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );

			DoCounter( attacker );
		}

		private void DoCounter( Mobile attacker )
		{
			if( this.Map == null )
				return;

			if ( attacker is BaseCreature && ((BaseCreature)attacker).BardProvoked )
				return;

			if ( 0.05 > Utility.RandomDouble() )
			{
				/* Counterattack with Hit Poison Area
				 * 20-25 damage, unresistable
				 * Lethal poison, 100% of the time
				 * Particle effect: Type: "2" From: "0x4061A107" To: "0x0" ItemId: "0x36BD" ItemIdName: "explosion" FromLocation: "(296 615, 17)" ToLocation: "(296 615, 17)" Speed: "1" Duration: "10" FixedDirection: "True" Explode: "False" Hue: "0xA6" RenderMode: "0x0" Effect: "0x1F78" ExplodeEffect: "0x1" ExplodeSound: "0x0" Serial: "0x4061A107" Layer: "255" Unknown: "0x0"
				 * Doesn't work on provoked monsters
				 */

				Mobile target = null;

				if ( attacker is BaseCreature )
				{
					Mobile m = ((BaseCreature)attacker).GetMaster();
					
					if( m != null )
						target = m;
				}

				if ( target == null || !target.InRange( this, 18 ) )
					target = attacker;

				this.Animate( 10, 4, 1, true, false, 0 );

				ArrayList targets = new ArrayList();

				foreach ( Mobile m in target.GetMobilesInRange( 8 ) )
				{
					if ( m == this || !CanBeHarmful( m ) )
						continue;

					if ( m is BaseCreature && (((BaseCreature)m).Controlled || ((BaseCreature)m).Summoned || ((BaseCreature)m).Team != this.Team) )
						targets.Add( m );
					else if ( m.Player && m.Alive )
						targets.Add( m );
				}

				for ( int i = 0; i < targets.Count; ++i )
				{
					Mobile m = (Mobile)targets[i];

					DoHarmful( m );

					AOS.Damage( m, this, Utility.RandomMinMax( 20, 25 ), true, 0, 0, 0, 100, 0 );

					m.FixedParticles( 0x36BD, 1, 10, 0x1F78, 0xA6, 0, (EffectLayer)255 );
					m.ApplyPoison( this, Poison.Lethal );
				}
			}
		}
        public override void OnThink()
		{
			base.OnThink();
			
			if ( Combatant != null )
			{
				if ( m_Change < DateTime.Now && Utility.RandomDouble() < 0.2 )
					ChangeOpponent();					
				
				if ( m_Stomp < DateTime.Now && Utility.RandomDouble() < 0.1 )
					HoofStomp();
			}
				// exit ilsh 1313, 936, 32
		}
		
		public override void Damage( int amount, Mobile from )
		{
			base.Damage( amount, from );
						
			if ( Combatant == null || Hits > HitsMax * 0.05 || Utility.RandomDouble() > 0.1 )
				return;	
							
			new InvisibilitySpell( this, null ).Cast();
			
			Target target = Target;
			
			if ( target != null )
				target.Invoke( this, this );
				
			Timer.DelayCall( TimeSpan.FromSeconds( 2 ), new TimerCallback( Teleport ) );
		}
        private DateTime m_Change;
		private DateTime m_Stomp;
		
		public void Teleport()
		{										
			// 20 tries to teleport
			for ( int tries = 0; tries < 20; tries ++ )
			{
				int x = Utility.RandomMinMax( 5, 7 ); 
				int y = Utility.RandomMinMax( 5, 7 );
				
				if ( Utility.RandomBool() )
					x *= -1;
					
				if ( Utility.RandomBool() )
					y *= -1;
				
				Point3D p = new Point3D( X + x, Y + y, 0 );
				IPoint3D po = new LandTarget( p, Map ) as IPoint3D;
				
				if ( po == null )
					continue;
					
				SpellHelper.GetSurfaceTop( ref po );

				if ( InRange( p, 12 ) && InLOS( p ) && Map.CanSpawnMobile( po.X, po.Y, po.Z ) )
				{					
					
					Point3D from = Location;
					Point3D to = new Point3D( po );
	
					Location = to;
					ProcessDelta();
					
					FixedParticles( 0x376A, 9, 32, 0x13AF, EffectLayer.Waist );
					PlaySound( 0x1FE );
										
					break;					
				}
			}		
			
			RevealingAction();
		}
		
		public void ChangeOpponent()
		{
			Mobile agro, best = null;
			double distance, random = Utility.RandomDouble();
			
			if ( random < 0.75 )
			{			
				// find random target relatively close
				for ( int i = 0; i < Aggressors.Count && best == null; i ++ )
				{
					agro = Validate( Aggressors[ i ].Attacker );
					
					if ( agro == null )
						continue;				
				
					distance = StrikingRange - GetDistanceToSqrt( agro );
					
					if ( distance > 0 && distance < StrikingRange - 2 && InLOS( agro.Location ) )
					{
						distance /= StrikingRange;
						
						if ( random < distance )
							best = agro;
					}
				}		
			}	
			else
			{
				int damage = 0;
				
				// find a player who dealt most damage
				for ( int i = 0; i < DamageEntries.Count; i ++ )
				{
					agro = Validate( DamageEntries[ i ].Damager );
					
					if ( agro == null )
						continue;
					
					distance = GetDistanceToSqrt( agro );
						
					if ( distance < StrikingRange && DamageEntries[ i ].DamageGiven > damage && InLOS( agro.Location ) )
					{
						best = agro;
						damage = DamageEntries[ i ].DamageGiven;
					}
				}
			}
			
			if ( best != null )
			{
				// teleport
				best.Location = BasePeerless.GetSpawnPosition( Location, Map, 1 );
				best.FixedParticles( 0x376A, 9, 32, 0x13AF, EffectLayer.Waist );
				best.PlaySound( 0x1FE );
				
				Timer.DelayCall( TimeSpan.FromSeconds( 1 ), new TimerCallback( delegate()
				{
					// poison
					best.ApplyPoison( this, HitPoison );
					best.FixedParticles( 0x374A, 10, 15, 5021, EffectLayer.Waist );
					best.PlaySound( 0x474 );
				} ) );
				
				m_Change = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 5, 10 ) );
			}
		}
		
		public void HoofStomp()
		{		
			foreach ( Mobile m in GetMobilesInRange( StrikingRange ) )
			{
				Mobile valid = Validate( m );
				
				if ( valid != null && Affect( valid ) )
					valid.SendLocalizedMessage( 1075081 ); // *Dreadhorn�s eyes light up, his mouth almost a grin, as he slams one hoof to the ground!*
			}		
			
			// earthquake
			PlaySound( 0x2F3 );
				
			Timer.DelayCall( TimeSpan.FromSeconds( 30 ), new TimerCallback( delegate{ StopAffecting(); } ) );
						
			m_Stomp = DateTime.Now + TimeSpan.FromSeconds( Utility.RandomMinMax( 40, 50 ) );
		}
		
		public Mobile Validate( Mobile m )
		{			
			Mobile agro;
					
			if ( m is BaseCreature )
				agro = ( (BaseCreature) m ).ControlMaster;
			else
				agro = m;
			
			if ( !CanBeHarmful( agro, false ) || !agro.Player || Combatant == agro )
				return null;	
			
			return agro;
		}
		
		private static Dictionary<Mobile,bool> m_Affected;
		
		public static bool IsUnderInfluence( Mobile mobile )
		{
			if ( m_Affected != null )
			{
				if ( m_Affected.ContainsKey( mobile ) )
					return true;
			}
			
			return false;
		}
		
		public static bool Affect( Mobile mobile )
		{
			if ( m_Affected == null )
				m_Affected = new Dictionary<Mobile,bool>();
				
			if ( !m_Affected.ContainsKey( mobile ) )
			{
				m_Affected.Add( mobile, true );
				return true;
			}
			
			return false;
		}
		
		public static void StopAffecting()
		{
			if ( m_Affected != null )
				m_Affected.Clear();
		}
	}
}
	
		

		



