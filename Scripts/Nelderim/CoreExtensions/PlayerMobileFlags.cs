﻿using System;
using Nelderim;

namespace Server.Mobiles
{
	public partial class PlayerMobile
	{
		[Flags]
		public enum NPlayerFlag
		{
			None = 0x00000000,
			Mysticism = 0x00000001,
			Cleric = 0x00000002,
			DeathKnight = 0x00000003,
			Nature = 0x00000004,
			Ancient = 0x00000005,
			Avatar = 0x00000006,
			Bard = 0x00000007,
			Ranger = 0x00000008,
			Rogue = 0x00000009,
			Undead = 0x0000000A
		}
		
		public bool NGetFlag(NPlayerFlag flag)
		{
			return ((PlayerFlagsExt.Get(this).Flags & flag) != 0);
		}

		public void NSetFlag(NPlayerFlag flag, bool value)
		{
			if (value)
			{
				PlayerFlagsExt.Get(this).Flags |= flag;
			}
			else
			{
				PlayerFlagsExt.Get(this).Flags &= ~flag;
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Mysticism { 
			get => NGetFlag(NPlayerFlag.Mysticism);
			set => NSetFlag(NPlayerFlag.Mysticism, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Cleric { 
			get => NGetFlag(NPlayerFlag.Cleric);
			set => NSetFlag(NPlayerFlag.Cleric, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool DeathKnight { 
			get => NGetFlag(NPlayerFlag.DeathKnight);
			set => NSetFlag(NPlayerFlag.DeathKnight, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Nature { 
			get => NGetFlag(NPlayerFlag.Nature);
			set => NSetFlag(NPlayerFlag.Nature, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Ancient { 
			get => NGetFlag(NPlayerFlag.Ancient);
			set => NSetFlag(NPlayerFlag.Ancient, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Avatar { 
			get => NGetFlag(NPlayerFlag.Avatar);
			set => NSetFlag(NPlayerFlag.Avatar, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Bard { 
			get => NGetFlag(NPlayerFlag.Bard);
			set => NSetFlag(NPlayerFlag.Bard, value);
		}
		
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Ranger { 
			get => NGetFlag(NPlayerFlag.Ranger);
			set => NSetFlag(NPlayerFlag.Ranger, value);
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Rogue { 
			get => NGetFlag(NPlayerFlag.Rogue);
			set => NSetFlag(NPlayerFlag.Rogue, value);
		}
		[CommandProperty(AccessLevel.GameMaster)]
		public bool Undead { 
			get => NGetFlag(NPlayerFlag.Undead);
			set => NSetFlag(NPlayerFlag.Undead, value);
		}
	}
	
	class PlayerFlagsExt : NExtension<PlayerFlagsExtInfo>
	{
		public static string ModuleName = "PlayerFlags";

		public static void Initialize()
		{
			EventSink.WorldSave += Save;
			Load(ModuleName);
		}

		public static void Save(WorldSaveEventArgs args)
		{
			Save(args, ModuleName);
		}
	}

	class PlayerFlagsExtInfo : NExtensionInfo
	{
		public PlayerMobile.NPlayerFlag Flags { get; set; }

		public override void Serialize(GenericWriter writer)
		{
			writer.Write( (int)0 ); //version
			writer.Write((int)Flags);
		}

		public override void Deserialize(GenericReader reader)
		{
			int version = 0;
			if (Fix)
				version = reader.ReadInt(); //version
			Flags = (PlayerMobile.NPlayerFlag)reader.ReadInt();
		}
	}
}
