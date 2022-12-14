using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "zwloki szmaragdowego smoka" )]
	public class EmeraldDragon : Dragon
	{
		[Constructable]
		public EmeraldDragon () : base()
		{
			Name = "szmaragdowy smok";
			BaseSoundID = 362;
            Hue = 1368;
            
            SetStr( 796, 825 );
            SetDex( 86, 105 );
            SetInt( 436, 475 );
            
            SetHits( 390, 410 );
		
			SetDamage( 18, 20);

			SetDamageType( ResistanceType.Physical, 25 );
			SetDamageType( ResistanceType.Poison, 70 );
			
			SetResistance( ResistanceType.Physical, 50, 60 );
			SetResistance( ResistanceType.Fire, 30, 50 );
			SetResistance( ResistanceType.Cold, 25, 35 );
			SetResistance( ResistanceType.Poison, 50, 70 );
			SetResistance( ResistanceType.Energy, 10, 30 );

			SetSkill( SkillName.EvalInt, 90.0, 110.0 );
			SetSkill( SkillName.Magery, 90.0, 120.0 );
			SetSkill( SkillName.MagicResist, 99.1, 110.0 );
			SetSkill( SkillName.Tactics, 90.1, 100.0 );
			SetSkill( SkillName.Wrestling, 75.1, 100.0 );
			SetSkill( SkillName.Meditation, 70.0, 100.0 );
			SetSkill( SkillName.Anatomy, 70.0, 100.0 );
			
			Fame = 15000;
			Karma = -15000;

			Tamable = false;
			ControlSlots = 3;
			MinTameSkill = 105.1;
			
			SetSpecialAbility(SpecialAbility.DragonBreath);
		}
		
		public override void OnCarve(Mobile from, Corpse corpse, Item with)
		{
            if (!IsBonded && !corpse.Carved && !IsChampionSpawn)
			{
			    if( Utility.RandomDouble() < 0.05 )
				    corpse.DropItem( new DragonsHeart() );
			    if( Utility.RandomDouble() < 0.15 )
				    corpse.DropItem( new DragonsBlood() );
					
				corpse.DropItem( new Emerald(8) );
            }

			base.OnCarve(from, corpse, with);
		}
		


		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich, 5 );
		}
		
		
		public override int TreasureMapLevel{ get{ return 4; } }
		public override int Meat{ get{ return 19; } }
		public override int Hides{ get{ return 10; } }
		public override HideType HideType{ get{ return HideType.Barbed; } }
		public override int Scales{ get{ return 7; } }
		public override Poison HitPoison{ get{ return Poison.Deadly; } }
		public override ScaleType ScaleType{ get{ return ScaleType.Green; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Emerald; } }

		public EmeraldDragon( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}
