#region References
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;

using Server.ContextMenus;
using Server.Diagnostics;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

using CV = Server.ClientVersion;
#endregion

namespace Server.Network
{
	public enum MessageType
	{
		Regular = 0x00,
		System = 0x01,
		Emote = 0x02,
		Label = 0x06,
		Focus = 0x07,
		Whisper = 0x08,
		Yell = 0x09,
		Spell = 0x0A,

		Guild = 0x0D,
		Alliance = 0x0E,
		Command = 0x0F,

		Encoded = 0xC0
	}

	public static class PacketHandlers
	{
		private static readonly PacketHandler[] m_ExtendedHandlersLow;

		private static readonly Dictionary<int, PacketHandler> m_ExtendedHandlersHigh;

		private static readonly EncodedPacketHandler[] m_EncodedHandlersLow;

		private static readonly Dictionary<int, EncodedPacketHandler> m_EncodedHandlersHigh;

		public static PacketHandler[] Handlers { get; private set; }

		static PacketHandlers()
		{
			Handlers = new PacketHandler[0x100];

			m_ExtendedHandlersLow = new PacketHandler[0x100];
			m_ExtendedHandlersHigh = new Dictionary<int, PacketHandler>();

			m_EncodedHandlersLow = new EncodedPacketHandler[0x100];
			m_EncodedHandlersHigh = new Dictionary<int, EncodedPacketHandler>();

			Register(0x00, 104, false, CreateCharacter);
			Register(0x01, 5, false, Disconnect);
			Register(0x02, 7, true, MovementReq);
			Register(0x03, 0, true, AsciiSpeech);
			Register(0x04, 2, true, GodModeRequest);
			Register(0x05, 5, true, AttackReq);
			Register(0x06, 5, true, UseReq);
			Register(0x07, 7, true, LiftReq);
			Register(0x08, 15, true, DropReq);
			Register(0x09, 5, true, LookReq);
			Register(0x0A, 11, true, Edit);
			Register(0x12, 0, true, TextCommand);
			Register(0x13, 10, true, EquipReq);
			Register(0x14, 6, true, ChangeZ);
			Register(0x22, 3, true, Resynchronize);
			Register(0x2C, 2, true, DeathStatusResponse);
			Register(0x34, 10, true, EntityQuery);
			Register(0x3A, 0, true, ChangeSkillLock);
			Register(0x3B, 0, true, VendorBuyReply);
			Register(0x47, 11, true, NewTerrain);
			Register(0x48, 73, true, NewAnimData);
			Register(0x58, 106, true, NewRegion);
			Register(0x5D, 73, false, PlayCharacter);
			Register(0x61, 9, true, DeleteStatic);
			Register(0x6C, 19, true, TargetResponse);
			Register(0x6F, 0, true, SecureTrade);
			Register(0x72, 5, true, SetWarMode);
			Register(0x73, 2, false, PingReq);
			Register(0x75, 35, true, RenameRequest);
			Register(0x79, 9, true, ResourceQuery);
			Register(0x7E, 2, true, GodviewQuery);
			Register(0x7D, 13, true, MenuResponse);
			Register(0x80, 62, false, AccountLogin);
			Register(0x83, 39, false, DeleteCharacter);
			Register(0x8D, 0, false, CreateCharacter);
			Register(0x91, 65, false, GameLogin);
			Register(0x95, 9, true, HuePickerResponse);
			Register(0x96, 0, true, GameCentralMoniter);
			Register(0x98, 0, true, MobileNameRequest);
			Register(0x9A, 0, true, AsciiPromptResponse);
			Register(0x9B, 258, true, HelpRequest);
			Register(0x9D, 51, true, GMSingle);
			Register(0x9F, 0, true, VendorSellReply);
			Register(0xA0, 3, false, PlayServer);
			Register(0xA4, 149, false, SystemInfo);
			Register(0xA7, 4, true, RequestScrollWindow);
			Register(0xAD, 0, true, UnicodeSpeech);
			Register(0xB1, 0, true, DisplayGumpResponse);
			//Register(0xB5, 64, true, ChatRequest);
			Register(0xB6, 9, true, ObjectHelpRequest);
			Register(0xB8, 0, true, ProfileReq);
			Register(0xBB, 9, false, AccountID);
			Register(0xBD, 0, true, ClientVersion);
			Register(0xBE, 0, true, AssistVersion);
			Register(0xBF, 0, true, ExtendedCommand);
			Register(0xC2, 0, true, UnicodePromptResponse);
			Register(0xC8, 2, true, SetUpdateRange);
			Register(0xC9, 6, true, TripTime);
			Register(0xCA, 6, true, UTripTime);
			Register(0xCF, 0, false, AccountLogin);
			Register(0xD0, 0, true, ConfigurationFile);
			Register(0xD1, 2, true, LogoutReq);
			Register(0xD6, 0, true, BatchQueryProperties);
			Register(0xD7, 0, true, EncodedCommand);
			Register(0xE1, 0, false, ClientType);
			Register(0xEC, 0, false, EquipMacro);
			Register(0xED, 0, false, UnequipMacro);
			Register(0xEF, 21, false, LoginServerSeed);
			//Register(0xF0, 0, false, NewMovementReq);
			Register(0xF4, 0, false, CrashReport);
			Register(0xF8, 106, false, CreateCharacter);
			//Register(0xFA, 1, true, Unhandled); // Currently Handled in UltimaStore.cs
			Register(0xFB, 2, false, PublicHouseContent);

			RegisterExtended(0x05, false, ScreenSize);
			RegisterExtended(0x06, true, PartyMessage);
			RegisterExtended(0x07, true, QuestArrow);
			RegisterExtended(0x09, true, DisarmRequest);
			RegisterExtended(0x0A, true, StunRequest);
			RegisterExtended(0x0B, false, Language);
			RegisterExtended(0x0C, true, CloseStatus);
			RegisterExtended(0x0E, true, Animate);
			RegisterExtended(0x0F, false, Empty); // What's this?
			RegisterExtended(0x10, true, QueryProperties);
			RegisterExtended(0x13, true, ContextMenuRequest);
			RegisterExtended(0x15, true, ContextMenuResponse);
			RegisterExtended(0x1A, true, StatLockChange);
			RegisterExtended(0x1C, true, CastSpell);
			RegisterExtended(0x24, false, UnhandledExtended);
			RegisterExtended(0x2C, true, BandageTarget);
			RegisterExtended(0x2D, true, TargetedSpell);
			RegisterExtended(0x2E, true, TargetedSkillUse);
			RegisterExtended(0x30, true, TargetByResourceMacro);
			RegisterExtended(0x32, true, ToggleFlying);

			RegisterEncoded(0x19, true, SetAbility);
			RegisterEncoded(0x28, true, GuildGumpRequest);
			RegisterEncoded(0x32, true, QuestGumpRequest);
		}

		public static void Register(int packetID, int length, bool ingame, OnPacketReceive onReceive)
		{
			Handlers[packetID] = new PacketHandler(packetID, length, ingame, onReceive);
		}

		public static PacketHandler GetHandler(int packetID)
		{
			return Handlers[packetID];
		}

		public static void RegisterExtended(int packetID, bool ingame, OnPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_ExtendedHandlersLow[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
			}
			else
			{
				m_ExtendedHandlersHigh[packetID] = new PacketHandler(packetID, 0, ingame, onReceive);
			}
		}

		public static PacketHandler GetExtendedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				return m_ExtendedHandlersLow[packetID];
			}

			m_ExtendedHandlersHigh.TryGetValue(packetID, out var handler);

			return handler;
		}

		public static void RemoveExtendedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_ExtendedHandlersLow[packetID] = null;
			}
			else
			{
				m_ExtendedHandlersHigh.Remove(packetID);
			}
		}

		public static void RegisterEncoded(int packetID, bool ingame, OnEncodedPacketReceive onReceive)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_EncodedHandlersLow[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
			}
			else
			{
				m_EncodedHandlersHigh[packetID] = new EncodedPacketHandler(packetID, ingame, onReceive);
			}
		}

		public static EncodedPacketHandler GetEncodedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				return m_EncodedHandlersLow[packetID];
			}

			m_EncodedHandlersHigh.TryGetValue(packetID, out var handler);

			return handler;
		}

		public static void RemoveEncodedHandler(int packetID)
		{
			if (packetID >= 0 && packetID < 0x100)
			{
				m_EncodedHandlersLow[packetID] = null;
			}
			else
			{
				m_EncodedHandlersHigh.Remove(packetID);
			}
		}

		public static void RegisterThrottler(int packetID, ThrottlePacketCallback t)
		{
			var ph = GetHandler(packetID);

			if (ph != null)
			{
				ph.ThrottleCallback = t;
			}
		}

		private static void UnhandledExtended(NetState state, PacketReader pvSrc)
		{
			//pvSrc.Trace(state);
		}

		private static void UnhandledEncoded(NetState state, IEntity e, EncodedReader pvSrc)
		{
			//pvSrc.Trace(state);
		}

		public static void Empty(NetState state, PacketReader pvSrc)
		{ }

		public static void SetAbility(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeSetAbility(new SetAbilityEventArgs(state.Mobile, reader.ReadInt32()));
		}

		public static void GuildGumpRequest(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeGuildGumpRequest(new GuildGumpRequestArgs(state.Mobile));
		}

		public static void QuestGumpRequest(NetState state, IEntity e, EncodedReader reader)
		{
			EventSink.InvokeQuestGumpRequest(new QuestGumpRequestArgs(state.Mobile));
		}

		public static void EncodedCommand(NetState state, PacketReader pvSrc)
		{
			var e = pvSrc.ReadEntity();
			int packetID = pvSrc.ReadUInt16();

			var ph = GetEncodedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Console.WriteLine("Client: {0}: Sent ingame packet (0xD7x{1:X2}) before having been attached to a mobile", state, packetID);

					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, e, new EncodedReader(pvSrc));
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		public static void RenameRequest(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;
			var targ = pvSrc.ReadMobile();

			if (targ != null)
			{
				EventSink.InvokeRenameRequest(new RenameRequestEventArgs(from, targ, pvSrc.ReadStringSafe()));
			}
		}

		public static void ChatRequest(NetState state, PacketReader pvSrc)
		{
			//EventSink.InvokeChatRequest(new ChatRequestEventArgs(state.Mobile));
		}

		public static void SecureTrade(NetState state, PacketReader pvSrc)
		{
			switch (pvSrc.ReadByte())
			{
				case 1: // Cancel
					{
						var serial = pvSrc.ReadSerial();

						var cont = World.FindItem(serial) as SecureTradeContainer;

						if (cont != null)
						{
							var trade = cont.Trade;

							if (trade != null)
							{
								if (trade.From.Mobile == state.Mobile || trade.To.Mobile == state.Mobile)
								{
									trade.Cancel();
								}
							}
						}
					}
					break;
				case 2: // Check
					{
						var serial = pvSrc.ReadSerial();

						var cont = World.FindItem(serial) as SecureTradeContainer;

						if (cont != null)
						{
							var trade = cont.Trade;

							var value = pvSrc.ReadInt32() != 0;

							if (trade != null)
							{
								if (trade.From.Mobile == state.Mobile)
								{
									trade.From.Accepted = value;
									trade.Update();
								}
								else if (trade.To.Mobile == state.Mobile)
								{
									trade.To.Accepted = value;
									trade.Update();
								}
							}
						}
					}
					break;
				case 3: // Update Gold
					{
						var serial = pvSrc.ReadSerial();

						var cont = World.FindItem(serial) as SecureTradeContainer;

						if (cont != null)
						{
							var gold = pvSrc.ReadInt32();
							var plat = pvSrc.ReadInt32();

							var trade = cont.Trade;

							if (trade != null)
							{
								if (trade.From.Mobile == state.Mobile)
								{
									trade.From.Gold = gold;
									trade.From.Plat = plat;
									trade.UpdateFromCurrency();
								}
								else if (trade.To.Mobile == state.Mobile)
								{
									trade.To.Gold = gold;
									trade.To.Plat = plat;
									trade.UpdateToCurrency();
								}
							}
						}
					}
					break;
			}
		}

		public static void VendorBuyReply(NetState state, PacketReader pvSrc)
		{
			pvSrc.Seek(1, SeekOrigin.Begin);

			int msgSize = pvSrc.ReadUInt16();
			var vendor = pvSrc.ReadMobile();
			var flag = pvSrc.ReadByte();

			if (vendor == null)
			{
				return;
			}

			if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
			{
				state.Send(new EndVendorBuy(vendor));
				return;
			}

			if (flag == 0x02)
			{
				msgSize -= 1 + 2 + 4 + 1;

				if ((msgSize / 7) > 100)
				{
					return;
				}

				var buyList = new List<BuyItemResponse>(msgSize / 7);

				for (; msgSize > 0; msgSize -= 7)
				{
					var layer = pvSrc.ReadByte();
					var serial = pvSrc.ReadSerial();
					int amount = pvSrc.ReadInt16();

					buyList.Add(new BuyItemResponse(serial, amount));
				}

				if (buyList.Count > 0)
				{
					var v = vendor as IVendor;

					if (v != null && v.OnBuyItems(state.Mobile, buyList))
					{
						state.Send(new EndVendorBuy(vendor));
					}
				}
			}
			else
			{
				state.Send(new EndVendorBuy(vendor));
			}
		}

		public static void VendorSellReply(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			var vendor = World.FindMobile(serial);

			if (vendor == null)
			{
				return;
			}

			if (vendor.Deleted || !Utility.RangeCheck(vendor.Location, state.Mobile.Location, 10))
			{
				state.Send(new EndVendorSell(vendor));
				return;
			}

			int count = pvSrc.ReadUInt16();

			if (count < 100 && pvSrc.Size == (1 + 2 + 4 + 2 + (count * 6)))
			{
				var sellList = new List<SellItemResponse>(count);

				for (var i = 0; i < count; i++)
				{
					var item = pvSrc.ReadItem();
					int Amount = pvSrc.ReadInt16();

					if (item != null && Amount > 0)
					{
						sellList.Add(new SellItemResponse(item, Amount));
					}
				}

				if (sellList.Count > 0)
				{
					var v = vendor as IVendor;

					if (v != null && v.OnSellItems(state.Mobile, sellList))
					{
						state.Send(new EndVendorSell(vendor));
					}
				}
			}
		}

		public static void DeleteCharacter(NetState state, PacketReader pvSrc)
		{
			pvSrc.Seek(30, SeekOrigin.Current);

			var index = pvSrc.ReadInt32();

			EventSink.InvokeDeleteRequest(new DeleteRequestEventArgs(state, index));
		}

		public static void ResourceQuery(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{ }
		}

		public static void GameCentralMoniter(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int type = pvSrc.ReadByte();
				var num1 = pvSrc.ReadInt32();

				Console.WriteLine("God Client: {0}: Game central moniter", state);
				Console.WriteLine(" - Type: {0}", type);
				Console.WriteLine(" - Number: {0}", num1);

				pvSrc.Trace(state);
			}
		}

		public static void GodviewQuery(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				Console.WriteLine("God Client: {0}: Godview query 0x{1:X}", state, pvSrc.ReadByte());
			}
		}

		public static void GMSingle(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				pvSrc.Trace(state);
			}
		}

		public static void DeathStatusResponse(NetState state, PacketReader pvSrc)
		{
			// Ignored
		}

		public static void ObjectHelpRequest(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			var serial = pvSrc.ReadSerial();
			int unk = pvSrc.ReadByte();
			var lang = pvSrc.ReadString(3);

			if (serial.IsItem)
			{
				var item = World.FindItem(serial);

				if (item != null && from.Map == item.Map && from.InUpdateRange(item) && from.CanSee(item))
				{
					item.OnHelpRequest(from);
				}
			}
			else if (serial.IsMobile)
			{
				var m = World.FindMobile(serial);

				if (m != null && from.Map == m.Map && from.InUpdateRange(m) && from.CanSee(m))
				{
					m.OnHelpRequest(m);
				}
			}
		}

		public static void MobileNameRequest(NetState state, PacketReader pvSrc)
		{
			var m = pvSrc.ReadMobile();

			if (m != null && state.Mobile.InUpdateRange(m) && state.Mobile.CanSee(m))
			{
				state.Send(new MobileName(m));
			}
		}

		public static void RequestScrollWindow(NetState state, PacketReader pvSrc)
		{
			int lastTip = pvSrc.ReadInt16();
			int type = pvSrc.ReadByte();
		}

		public static void AttackReq(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			var m = pvSrc.ReadMobile();

			if (m != null)
			{
				from.Attack(m);
			}
		}

		public static void HuePickerResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			int value = pvSrc.ReadInt16();
			var hue = pvSrc.ReadInt16() & 0x3FFF;

			hue = Utility.ClipDyedHue(hue);

			foreach (var huePicker in state.HuePickers)
			{
				if (huePicker.Serial == serial)
				{
					state.RemoveHuePicker(huePicker);

					hue = Math.Max(0, hue);

					if (state.Mobile == null || state.Mobile.AccessLevel < AccessLevel.GameMaster)
					{
						huePicker.Clip(ref hue);
					}

					huePicker.OnResponse(hue);

					break;
				}
			}
		}

		public static void TripTime(NetState state, PacketReader pvSrc)
		{
			var ping = pvSrc.ReadByte();
			//var data = pvSrc.ReadInt32();

			TripTimeResponse.Send(state, ping, false);
		}

		public static void UTripTime(NetState state, PacketReader pvSrc)
		{
			var ping = pvSrc.ReadByte();
			//var data = pvSrc.ReadInt32();

			TripTimeResponse.Send(state, ping, true);
		}

		public static void ChangeZ(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int z = pvSrc.ReadSByte();

				Console.WriteLine("God Client: {0}: Change Z ({1}, {2}, {3})", state, x, y, z);
			}
		}

		public static void SystemInfo(NetState state, PacketReader pvSrc)
		{
			int v1 = pvSrc.ReadByte();
			int v2 = pvSrc.ReadUInt16();
			int v3 = pvSrc.ReadByte();
			var s1 = pvSrc.ReadString(32);
			var s2 = pvSrc.ReadString(32);
			var s3 = pvSrc.ReadString(32);
			var s4 = pvSrc.ReadString(32);
			int v4 = pvSrc.ReadUInt16();
			int v5 = pvSrc.ReadUInt16();
			var v6 = pvSrc.ReadInt32();
			var v7 = pvSrc.ReadInt32();
			var v8 = pvSrc.ReadInt32();
		}

		public static void Edit(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int type = pvSrc.ReadByte(); // 10 = static, 7 = npc, 4 = dynamic
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int id = pvSrc.ReadInt16();
				int z = pvSrc.ReadSByte();
				int hue = pvSrc.ReadUInt16();

				Console.WriteLine("God Client: {0}: Edit {6} ({1}, {2}, {3}) 0x{4:X} (0x{5:X})", state, x, y, z, id, hue, type);
			}
		}

		public static void DeleteStatic(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int z = pvSrc.ReadInt16();
				int id = pvSrc.ReadUInt16();

				Console.WriteLine("God Client: {0}: Delete Static ({1}, {2}, {3}) 0x{4:X}", state, x, y, z, id);
			}
		}

		public static void NewAnimData(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				Console.WriteLine("God Client: {0}: New tile animation", state);

				pvSrc.Trace(state);
			}
		}

		public static void NewTerrain(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int id = pvSrc.ReadUInt16();
				int width = pvSrc.ReadInt16();
				int height = pvSrc.ReadInt16();

				Console.WriteLine("God Client: {0}: New Terrain ({1}, {2})+({3}, {4}) 0x{5:X4}", state, x, y, width, height, id);
			}
		}

		public static void NewRegion(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				var name = pvSrc.ReadString(40);
				var unk = pvSrc.ReadInt32();
				int x = pvSrc.ReadInt16();
				int y = pvSrc.ReadInt16();
				int width = pvSrc.ReadInt16();
				int height = pvSrc.ReadInt16();
				int zStart = pvSrc.ReadInt16();
				int zEnd = pvSrc.ReadInt16();
				var desc = pvSrc.ReadString(40);
				int soundFX = pvSrc.ReadInt16();
				int music = pvSrc.ReadInt16();
				int nightFX = pvSrc.ReadInt16();
				int dungeon = pvSrc.ReadByte();
				int light = pvSrc.ReadInt16();

				Console.WriteLine("God Client: {0}: New Region '{1}' ('{2}')", state, name, desc);
			}
		}

		public static void AccountID(NetState state, PacketReader pvSrc)
		{ }

		public static bool VerifyGC(NetState state)
		{
			if (state.Mobile == null || state.Mobile.IsPlayer())
			{
				if (state.Running)
				{
					Console.WriteLine("Warning: {0}: Player using godclient, disconnecting", state);
				}

				state.Dispose();
				return false;
			}

			return true;
		}

		public static void TextCommand(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			var command = pvSrc.ReadString();

			var m = state.Mobile;

			switch (type)
			{
				case 0x00: // Go
					{
						if (VerifyGC(state))
						{
							try
							{
								var split = command.Split(' ');

								var x = Utility.ToInt32(split[0]);
								var y = Utility.ToInt32(split[1]);

								int z;

								if (split.Length >= 3)
								{
									z = Utility.ToInt32(split[2]);
								}
								else if (m.Map != null)
								{
									z = m.Map.GetAverageZ(x, y);
								}
								else
								{
									z = 0;
								}

								m.Location = new Point3D(x, y, z);
							}
							catch (Exception e)
							{
								ExceptionLogging.LogException(e);
							}
						}

						break;
					}
				case 0xC7: // Animate
					{
						EventSink.InvokeAnimateRequest(new AnimateRequestEventArgs(m, command));

						break;
					}
				case 0x24: // Use skill
					{
						if (!Int32.TryParse(command.Split(' ')[0], out var skillIndex))
						{
							break;
						}

						Skills.UseSkill(m, skillIndex);

						break;
					}
				case 0x43: // Open spellbook
					{
						if (!Int32.TryParse(command, out var booktype))
						{
							booktype = 1;
						}

						EventSink.InvokeOpenSpellbookRequest(new OpenSpellbookRequestEventArgs(m, booktype));

						break;
					}
				case 0x27: // Cast spell from book
					{
						var split = command.Split(' ');

						if (split.Length > 0)
						{
							var spellID = Utility.ToInt32(split[0]) - 1;
							var serial = split.Length > 1 ? Utility.ToSerial(split[1]) : Serial.MinusOne;

							EventSink.InvokeCastSpellRequest(new CastSpellRequestEventArgs(m, spellID, World.FindEntity(serial) as ISpellbook));
						}

						break;
					}
				case 0x58: // Open door
					{
						EventSink.InvokeOpenDoorMacroUsed(new OpenDoorMacroEventArgs(m));

						break;
					}
				case 0x56: // Cast spell from macro
					{
						var spellID = Utility.ToInt32(command) - 1;

						EventSink.InvokeCastSpellRequest(new CastSpellRequestEventArgs(m, spellID, null));

						break;
					}
				case 0xF4: // Invoke virtues from macro
					{
						var virtueID = Utility.ToInt32(command) - 1;

						EventSink.InvokeVirtueMacroRequest(new VirtueMacroRequestEventArgs(m, virtueID));

						break;
					}
				case 0x2F: // Old scroll double click
					{
						/*
						 * This command is still sent for items 0xEF3 - 0xEF9
						 *
						 * Command is one of three, depending on the item ID of the scroll:
						 * - [scroll serial]
						 * - [scroll serial] [target serial]
						 * - [scroll serial] [x] [y] [z]
						 */
						break;
					}
				default:
					{
						Console.WriteLine("Client: {0}: Unknown text-command type 0x{1:X2}: {2}", state, type, command);
						break;
					}
			}
		}

		public static void GodModeRequest(NetState state, PacketReader pvSrc)
		{
			if (VerifyGC(state))
			{
				state.Send(new GodModeReply(pvSrc.ReadBoolean()));
			}
		}

		public static void AsciiPromptResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadInt32();
			var prompt = pvSrc.ReadInt32();
			var type = pvSrc.ReadInt32();
			var text = pvSrc.ReadStringSafe();

			if (text == null || text.Length > 128)
			{
				return;
			}

			var from = state.Mobile;
			var p = from.Prompt;

			if (p != null && p.Serial == serial && p.Serial == prompt)
			{
				from.Prompt = null;

				if (type == 0)
				{
					p.OnCancel(from);
				}
				else
				{
					p.OnResponse(from, text);
				}
			}
		}

		public static void UnicodePromptResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadInt32();
			var prompt = pvSrc.ReadInt32();
			var type = pvSrc.ReadInt32();
			var lang = pvSrc.ReadString(4);
			var text = pvSrc.ReadUnicodeStringLESafe();

			if (text == null || text.Length > 128)
			{
				return;
			}

			var from = state.Mobile;
			var p = from.Prompt;

			if (p != null && p.Serial == serial && p.Serial == prompt)
			{
				from.Prompt = null;

				if (type == 0)
				{
					p.OnCancel(from);
				}
				else
				{
					p.OnResponse(from, text);
				}
			}
		}

		public static void MenuResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadInt32();
			int menuID = pvSrc.ReadInt16(); // unused in our implementation
			int index = pvSrc.ReadInt16();
			int itemID = pvSrc.ReadInt16();
			int hue = pvSrc.ReadInt16();

			index -= 1; // convert from 1-based to 0-based

			foreach (var menu in state.Menus)
			{
				if (menu.Serial == serial)
				{
					state.RemoveMenu(menu);

					if (index >= 0 && index < menu.EntryLength)
					{
						menu.OnResponse(state, index);
					}
					else
					{
						menu.OnCancel(state);
					}

					break;
				}
			}
		}

		public static void ProfileReq(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			var serial = pvSrc.ReadSerial();

			var beholder = state.Mobile;
			var beheld = World.FindMobile(serial);

			if (beheld == null)
			{
				return;
			}

			switch (type)
			{
				case 0x00: // display request
					{
						EventSink.InvokeProfileRequest(new ProfileRequestEventArgs(beholder, beheld));

						break;
					}
				case 0x01: // edit request
					{
						pvSrc.ReadInt16(); // Skip

						int length = pvSrc.ReadUInt16();

						if (length > 511)
						{
							return;
						}

						var text = pvSrc.ReadUnicodeString(length);

						EventSink.InvokeChangeProfileRequest(new ChangeProfileRequestEventArgs(beholder, beheld, text));

						break;
					}
			}
		}

		public static void Disconnect(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadSerial(); // -1
		}

		public static void LiftReq(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			int amount = pvSrc.ReadUInt16();
			var item = World.FindItem(serial);

			state.Mobile.Lift(item, amount);
		}

		public static void EquipReq(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;
			var item = from.Holding;

			var valid = item != null && item.HeldBy == from && item.Map == Map.Internal;

			from.Holding = null;

			if (!valid)
			{
				return;
			}

			pvSrc.Seek(5, SeekOrigin.Current);

			var to = pvSrc.ReadMobile() ?? from;

			if (!to.AllowEquipFrom(from) || !to.EquipItem(item))
			{
				item.Bounce(from);
			}

			item.ClearBounce();
		}

		public static void DropReq(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadSByte();

			var gridloc = Byte.MaxValue;

			if (state.ContainerGridLines)
			{
				gridloc = pvSrc.ReadByte();
			}

			var dropped = World.FindItem(serial);

			if (dropped != null)
			{
				dropped.GridLocation = gridloc;
			}

			var dest = pvSrc.ReadSerial();

			if (!state.ContainerGridLines)
			{
				pvSrc.Slice(); // push remaining data back to the buffer
			}

			var loc = new Point3D(x, y, z);
			var from = state.Mobile;

			if (dest.IsMobile)
			{
				from.Drop(World.FindMobile(dest), loc);
			}
			else if (dest.IsItem)
			{
				var item = World.FindItem(dest);

				if (item is BaseMulti multi && multi.AllowsRelativeDrop)
				{
					loc.m_X += item.X;
					loc.m_Y += item.Y;
					from.Drop(loc);
				}
				else
				{
					from.Drop(item, loc);
				}
			}
			else
			{
				from.Drop(loc);
			}
		}

		public static void ConfigurationFile(NetState state, PacketReader pvSrc)
		{ }

		public static void LogoutReq(NetState state, PacketReader pvSrc)
		{
			LogoutAck.Send(state);
		}

		public static void ChangeSkillLock(NetState state, PacketReader pvSrc)
		{
			var s = state.Mobile.Skills[pvSrc.ReadInt16()];

			if (s != null)
			{
				s.SetLockNoRelay((SkillLock)pvSrc.ReadByte());
			}
		}

		public static void HelpRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeHelpRequest(new HelpRequestEventArgs(state.Mobile));
		}

		public static void TargetResponse(NetState state, PacketReader pvSrc)
		{
			int type = pvSrc.ReadByte();
			var targetID = pvSrc.ReadInt32();
			int flags = pvSrc.ReadByte();
			var serial = pvSrc.ReadSerial();
			int x = pvSrc.ReadInt16();
			int y = pvSrc.ReadInt16();
			int z = pvSrc.ReadInt16();
			int graphic = pvSrc.ReadUInt16();

			if (targetID == unchecked((int)0xDEADBEEF))
			{
				return;
			}

			var from = state.Mobile;
			var t = from.Target;

			if (t != null)
			{
				var prof = TargetProfile.Acquire(t.GetType());

				if (prof != null)
				{
					prof.Start();
				}

				try
				{
					from.ClearTarget();

					if (x == -1 && y == -1 && !serial.IsValid)
					{
						t.Cancel(from, TargetCancelType.Canceled);
						return;
					}

					if (t.TargetID != targetID)
					{
						t.Cancel(from, TargetCancelType.Invalid);
						return;
					}

					object toTarget;

					if (type == 1)
					{
						if (graphic == 0)
						{
							toTarget = new LandTarget(new Point3D(x, y, z), from.Map);
						}
						else
						{
							var map = from.Map;

							if (map == null || map == Map.Internal)
							{
								t.Cancel(from, TargetCancelType.Invalid);
								return;
							}

							var tiles = map.Tiles.GetStaticTiles(x, y, !t.DisallowMultis);

							var valid = false;
							var hue = 0;

							if (state.HighSeas)
							{
								var id = TileData.ItemTable[graphic & TileData.MaxItemValue];

								if (id.Surface && !id.Flags.HasFlag(TileFlag.Roof))
								{
									z -= id.Height;
								}
							}

							for (var i = 0; !valid && i < tiles.Length; ++i)
							{
								if (tiles[i].Z == z && tiles[i].ID == graphic)
								{
									valid = true;
									hue = tiles[i].Hue;
								}
							}

							if (!valid)
							{
								t.Cancel(from, TargetCancelType.Invalid);
								return;
							}

							toTarget = new StaticTarget(new Point3D(x, y, z), graphic, hue);
						}
					}
					else if (serial.IsMobile)
					{
						toTarget = World.FindMobile(serial);
					}
					else if (serial.IsItem)
					{
						toTarget = World.FindItem(serial);
					}
					else
					{
						t.Cancel(from, TargetCancelType.Invalid);
						return;
					}

					t.Invoke(from, toTarget);
				}
				finally
				{
					if (prof != null)
					{
						prof.Finish();
					}
				}
			}
		}

		public static void DisplayGumpResponse(NetState state, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			var typeID = pvSrc.ReadInt32();
			var buttonID = pvSrc.ReadInt32();

			var index = state.Gumps.Count;

			Gump gump;

			while (--index >= 0)
			{
				if (index >= state.Gumps.Count)
				{
					continue;
				}

				gump = state.Gumps[index];

				if (gump == null)
				{
					state.Gumps.RemoveAt(index);
					continue;
				}

				if (gump.Serial != serial || gump.TypeID != typeID)
				{
					continue;
				}

				var buttonExists = buttonID == 0; // 0 is always 'close'

				if (!buttonExists)
				{
					foreach (var e in gump.Entries)
					{
						if (e is GumpButton && ((GumpButton)e).ButtonID == buttonID)
						{
							buttonExists = true;
							break;
						}

						if (e is GumpImageTileButton && ((GumpImageTileButton)e).ButtonID == buttonID)
						{
							buttonExists = true;
							break;
						}
					}
				}

				if (!buttonExists)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Invalid gump button response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var switchCount = pvSrc.ReadInt32();

				if (switchCount < 0 || switchCount > gump.m_Switches)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Invalid gump switch response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var switches = new int[switchCount];

				for (var j = 0; j < switches.Length; ++j)
				{
					switches[j] = pvSrc.ReadInt32();
				}

				var textCount = pvSrc.ReadInt32();

				if (textCount < 0 || textCount > gump.m_TextEntries)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Invalid gump text response for '{gump.GetType()}'");

					state.RemoveGump(gump);
					gump.OnServerClose(state);

					return;
				}

				var textEntries = new TextRelay[textCount];

				for (var j = 0; j < textEntries.Length; ++j)
				{
					int entryID = pvSrc.ReadUInt16();
					int textLength = pvSrc.ReadUInt16();

					if (textLength > 239)
					{
						Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Invalid gump text response for '{gump.GetType()}' entry '{entryID}'");

						state.RemoveGump(gump);
						gump.OnServerClose(state);

						return;
					}

					var text = pvSrc.ReadUnicodeStringSafe(textLength);

					textEntries[j] = new TextRelay(entryID, text);
				}

				state.RemoveGump(gump);

				var prof = GumpProfile.Acquire(gump.GetType());

				if (prof != null)
				{
					prof.Start();
				}

				gump.OnResponse(state, new RelayInfo(buttonID, switches, textEntries));

				if (prof != null)
				{
					prof.Finish();
				}

				return;
			}

			if (typeID == 461)
			{
				// Virtue gump
				var switchCount = pvSrc.ReadInt32();

				if (buttonID == 1 && switchCount > 0)
				{
					var beheld = pvSrc.ReadMobile();

					if (beheld != null)
					{
						EventSink.InvokeVirtueGumpRequest(new VirtueGumpRequestEventArgs(state.Mobile, beheld));
					}
				}
				else
				{
					var beheld = World.FindMobile(serial);

					if (beheld != null)
					{
						EventSink.InvokeVirtueItemRequest(new VirtueItemRequestEventArgs(state.Mobile, beheld, buttonID));
					}
				}
			}
		}

		public static void SetWarMode(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile.IsStaff() || Core.TickCount - state.Mobile.NextActionTime >= 0)
			{
				state.Mobile.DelayChangeWarmode(pvSrc.ReadBoolean());
			}
			else
			{
				state.Mobile.SendActionMessage();
			}
		}

		public static void Resynchronize(NetState state, PacketReader pvSrc)
		{
			state.Mobile?.SendMapUpdates(false, false);
		}

		private static readonly int[] m_EmptyInts = new int[0];

		public static void AsciiSpeech(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			var type = (MessageType)pvSrc.ReadByte();
			int hue = pvSrc.ReadInt16();

			pvSrc.ReadInt16(); // font

			var text = pvSrc.ReadStringSafe().Trim();

			if (text.Length <= 0 || text.Length > 128)
			{
				return;
			}

			if (!Enum.IsDefined(typeof(MessageType), type))
			{
				type = MessageType.Regular;
			}

			from.DoSpeech(text, m_EmptyInts, type, Utility.ClipDyedHue(hue));
		}

		private static readonly KeywordList m_KeywordList = new KeywordList();

		public static void UnicodeSpeech(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			var type = (MessageType)pvSrc.ReadByte();
			int hue = pvSrc.ReadInt16();

			pvSrc.ReadInt16(); // font

			var lang = pvSrc.ReadString(4);

			string text;

			var isEncoded = (type & MessageType.Encoded) != 0;

			int[] keywords;

			if (isEncoded)
			{
				int value = pvSrc.ReadInt16();

				var count = (value & 0xFFF0) >> 4;
				var hold = value & 0xF;

				if (count < 0 || count > 50)
				{
					return;
				}

				var keyList = m_KeywordList;

				for (var i = 0; i < count; ++i)
				{
					int speechID;

					if ((i & 1) == 0)
					{
						hold <<= 8;
						hold |= pvSrc.ReadByte();
						speechID = hold;
						hold = 0;
					}
					else
					{
						value = pvSrc.ReadInt16();

						speechID = (value & 0xFFF0) >> 4;
						hold = value & 0xF;
					}

					if (!keyList.Contains(speechID))
					{
						keyList.Add(speechID);
					}
				}

				text = pvSrc.ReadUTF8StringSafe();

				keywords = keyList.ToArray();
			}
			else
			{
				text = pvSrc.ReadUnicodeStringSafe();

				keywords = m_EmptyInts;
			}

			text = text.Trim();

			if (text.Length <= 0 || text.Length > 128)
			{
				return;
			}

			type &= ~MessageType.Encoded;

			if (!Enum.IsDefined(typeof(MessageType), type))
			{
				type = MessageType.Regular;
			}

			from.Language = lang;

			from.DoSpeech(text, keywords, type, Utility.ClipDyedHue(hue));
		}

		public static void UseReq(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
			{
				var value = pvSrc.ReadSerial();

				if ((value & ~0x7FFFFFFF) != 0)
				{
					from.OnPaperdollRequest();
				}
				else
				{
					if (value.IsMobile)
					{
						var m = World.FindMobile(value);

						if (m != null && !m.Deleted)
						{
							from.Use(m);
						}
					}
					else if (value.IsItem)
					{
						var item = World.FindItem(value);

						if (item != null && !item.Deleted)
						{
							from.Use(item);
						}
					}
				}

				from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
			}
			else
			{
				from.SendActionMessage();
			}
		}

		public static Func<Mobile, Mobile, bool> MobileClickOverride;
		public static Func<Mobile, Item, bool> ItemClickOverride;

		private static void HandleSingleClick(Mobile m, IEntity target)
		{
			if (m == null || target == null || target.Deleted || !m.CanSee(target))
			{
				return;
			}

			if (target is Item ti)
			{
				if (m.InUpdateRange(ti))
				{
					if (ItemClickOverride == null || !ItemClickOverride(m, ti))
					{
						if (ObjectPropertyList.Enabled && m.ViewOPL)
						{
							ti.OnAosSingleClick(m);
						}
						else if (m.Region.OnSingleClick(m, ti))
						{
							if (ti.Parent is Item tip)
							{
								tip.OnSingleClickContained(m, ti);
							}

							ti.OnSingleClick(m);
						}
					}
				}
			}
			else if (target is Mobile tm)
			{
				if (m.InUpdateRange(tm))
				{
					if (MobileClickOverride == null || !MobileClickOverride(m, tm))
					{
						if (ObjectPropertyList.Enabled && m.ViewOPL)
						{
							tm.OnAosSingleClick(m);
						}
						else if (m.Region.OnSingleClick(m, tm))
						{
							tm.OnSingleClick(m);
						}
					}
				}
			}
		}

		public static void LookReq(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile != null && state.Mobile.ViewOPL)
			{
				HandleSingleClick(state.Mobile, pvSrc.ReadEntity());
			}
		}

		public static void PingReq(NetState state, PacketReader pvSrc)
		{
			PingAck.Send(state, pvSrc.ReadByte());
		}

		public static void SetUpdateRange(NetState state, PacketReader pvSrc)
		{
			//            min   max  default
			/* 640x480    5     18   15
             * 800x600    5     18   18
             * 1024x768   5     24   24
             * 1152x864   5     24   24 
             * 1280x720   5     24   24
             */

			int range = pvSrc.ReadByte();

			// Don't let range drop below the minimum standard.
			range = Math.Max(Core.GlobalUpdateRange, range);

			var old = state.UpdateRange;

			if (old == range)
			{
				return;
			}

			state.UpdateRange = range;

			ChangeUpdateRange.Send(state);

			if (state.Mobile != null)
			{
				state.Mobile.OnUpdateRangeChanged(old, state.UpdateRange);
			}
		}

		private const int BadFood = unchecked((int)0xBAADF00D);
		private const int BadUOTD = unchecked((int)0xFFCEFFCE);

		public static void MovementReq(NetState state, PacketReader pvSrc)
		{
			var dir = (Direction)pvSrc.ReadByte();
			int seq = pvSrc.ReadByte();
			var key = pvSrc.ReadInt32();

			var m = state.Mobile;

			if ((state.Sequence == 0 && seq != 0) || !m.Move(dir))
			{
				state.Send(new MovementRej(seq, m));

				state.Sequence = 0;

				m.ClearFastwalkStack();
			}
			else
			{
				++seq;

				if (seq == 256)
				{
					seq = 1;
				}

				state.Sequence = seq;
			}
		}

		public static void NewMovementReq(NetState state, PacketReader pvSrc)
		{
			/*byte count = pvSrc.ReadByte();

            while (--count >= 0)
            {
                long date1 = pvSrc.ReadUInt32();
                long date2 = pvSrc.ReadUInt32();

                byte seq = pvSrc.ReadByte();
                Direction dir = (Direction)pvSrc.ReadByte();

                int type = pvSrc.ReadUInt16();
                int x = pvSrc.ReadInt16();
                int y = pvSrc.ReadInt16();
                int z = pvSrc.ReadInt16();
            }*/
		}

		public static int[] m_ValidAnimations =
		{
			6, 21, 32, 33, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119,
			120, 121, 123, 124, 125, 126, 127, 128
		};

		public static int[] ValidAnimations { get => m_ValidAnimations; set => m_ValidAnimations = value; }

		public static void Animate(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			var action = pvSrc.ReadInt32();

			var ok = false;

			for (var i = 0; !ok && i < m_ValidAnimations.Length; ++i)
			{
				ok = action == m_ValidAnimations[i];
			}

			if (from != null && ok && from.Alive && from.Body.IsHuman && !from.Mounted)
			{
				from.Animate(action, 7, 1, true, false, 0);
			}
		}

		public static void QuestArrow(NetState state, PacketReader pvSrc)
		{
			var rightClick = pvSrc.ReadBoolean();

			var from = state.Mobile;

			if (from != null && from.QuestArrow != null)
			{
				from.QuestArrow.OnClick(rightClick);
			}
		}

		public static void ExtendedCommand(NetState state, PacketReader pvSrc)
		{
			int packetID = pvSrc.ReadUInt16();

			var ph = GetExtendedHandler(packetID);

			if (ph != null)
			{
				if (ph.Ingame && state.Mobile == null)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Packet (0xBF.0x{packetID:X2}) Requires State Mobile");

					state.Dispose();
				}
				else if (ph.Ingame && state.Mobile.Deleted)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Packet (0xBF.0x{packetID:X2}) Invalid State Mobile");

					state.Dispose();
				}
				else
				{
					ph.OnReceive(state, pvSrc);
				}
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		public static void CastSpell(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (from == null)
			{
				return;
			}

			ISpellbook spellbook = null;

			if (pvSrc.ReadInt16() == 1)
			{
				spellbook = pvSrc.ReadEntity() as ISpellbook;
			}

			var spellID = pvSrc.ReadInt16() - 1;

			EventSink.InvokeCastSpellRequest(new CastSpellRequestEventArgs(from, spellID, spellbook));
		}

		public static void BandageTarget(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			if (from == null)
			{
				return;
			}

			if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
			{
				var bandage = pvSrc.ReadItem();

				if (bandage == null)
				{
					return;
				}

				var target = pvSrc.ReadMobile();

				if (target == null)
				{
					return;
				}

				EventSink.InvokeBandageTargetRequest(new BandageTargetRequestEventArgs(from, bandage, target));

				from.NextActionTime = Core.TickCount + Mobile.ActionDelay;
			}
			else
			{
				from.SendActionMessage();
			}
		}

		public static void ToggleFlying(NetState state, PacketReader pvSrc)
		{
			state.Mobile.Flying = !state.Mobile.Flying;
		}

		public static void BatchQueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || state.Mobile == null || !state.Mobile.ViewOPL)
			{
				return;
			}

			var from = state.Mobile;
			var length = pvSrc.Size - 3;

			if (length < 0 || (length % 4) != 0)
			{
				return;
			}

			var count = length / 4;

			for (var i = 0; i < count; ++i)
			{
				var s = pvSrc.ReadSerial();

				if (s.IsMobile)
				{
					var m = World.FindMobile(s);

					if (m != null && from.CanSee(m) && from.InUpdateRange(m))
					{
						m.SendPropertiesTo(from);
					}
				}
				else if (s.IsItem)
				{
					var item = World.FindItem(s);

					if (item != null && !item.Deleted && from.CanSee(item) && from.InUpdateRange(item))
					{
						item.SendPropertiesTo(from);
					}
				}
			}
		}

		public static void QueryProperties(NetState state, PacketReader pvSrc)
		{
			if (state == null || state.Mobile == null || !state.Mobile.ViewOPL)
			{
				return;
			}

			var from = state.Mobile;

			var s = pvSrc.ReadSerial();

			if (s.IsMobile)
			{
				var m = World.FindMobile(s);

				if (m != null && from.CanSee(m) && from.InUpdateRange(m))
				{
					m.SendPropertiesTo(from);
				}
			}
			else if (s.IsItem)
			{
				var item = World.FindItem(s);

				if (item != null && !item.Deleted && from.CanSee(item) && from.InUpdateRange(item))
				{
					item.SendPropertiesTo(from);
				}
			}
		}

		public static void PartyMessage(NetState state, PacketReader pvSrc)
		{
			if (state.Mobile == null)
			{
				return;
			}

			switch (pvSrc.ReadByte())
			{
				case 0x01:
				PartyMessage_AddMember(state, pvSrc);
				break;
				case 0x02:
				PartyMessage_RemoveMember(state, pvSrc);
				break;
				case 0x03:
				PartyMessage_PrivateMessage(state, pvSrc);
				break;
				case 0x04:
				PartyMessage_PublicMessage(state, pvSrc);
				break;
				case 0x06:
				PartyMessage_SetCanLoot(state, pvSrc);
				break;
				case 0x08:
				PartyMessage_Accept(state, pvSrc);
				break;
				case 0x09:
				PartyMessage_Decline(state, pvSrc);
				break;
				default:
				pvSrc.Trace(state);
				break;
			}
		}

		public static void PartyMessage_AddMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnAdd(state.Mobile);
			}
		}

		public static void PartyMessage_RemoveMember(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnRemove(state.Mobile, pvSrc.ReadMobile());
			}
		}

		public static void PartyMessage_PrivateMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnPrivateMessage(state.Mobile, pvSrc.ReadMobile(), pvSrc.ReadUnicodeStringSafe());
			}
		}

		public static void PartyMessage_PublicMessage(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnPublicMessage(state.Mobile, pvSrc.ReadUnicodeStringSafe());
			}
		}

		public static void PartyMessage_SetCanLoot(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnSetCanLoot(state.Mobile, pvSrc.ReadBoolean());
			}
		}

		public static void PartyMessage_Accept(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnAccept(state.Mobile, pvSrc.ReadMobile());
			}
		}

		public static void PartyMessage_Decline(NetState state, PacketReader pvSrc)
		{
			if (PartyCommands.Handler != null)
			{
				PartyCommands.Handler.OnDecline(state.Mobile, pvSrc.ReadMobile());
			}
		}

		public static void StunRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeStunRequest(new StunRequestEventArgs(state.Mobile));
		}

		public static void DisarmRequest(NetState state, PacketReader pvSrc)
		{
			EventSink.InvokeDisarmRequest(new DisarmRequestEventArgs(state.Mobile));
		}

		public static void StatLockChange(NetState state, PacketReader pvSrc)
		{
			int stat = pvSrc.ReadByte();
			int lockValue = pvSrc.ReadByte();

			if (lockValue > 2)
			{
				lockValue = 0;
			}

			var m = state.Mobile;

			if (m != null)
			{
				switch (stat)
				{
					case 0:
					m.StrLock = (StatLockType)lockValue;
					break;
					case 1:
					m.DexLock = (StatLockType)lockValue;
					break;
					case 2:
					m.IntLock = (StatLockType)lockValue;
					break;
				}
			}
		}

		public static void ScreenSize(NetState state, PacketReader pvSrc)
		{
			var width = pvSrc.ReadInt32();
			var unk = pvSrc.ReadInt32();
		}

		public static void ContextMenuResponse(NetState state, PacketReader pvSrc)
		{
			var user = state.Mobile;

			if (user == null)
			{
				return;
			}

			using (var menu = user.ContextMenu)
			{
				user.ContextMenu = null;

				if (menu != null && user == menu.From)
				{
					var entity = pvSrc.ReadEntity();

					if (entity != null && entity == menu.Target && user.CanSee(entity))
					{
						Point3D p;

						if (entity is Mobile)
						{
							p = entity.Location;
						}
						else if (entity is Item)
						{
							p = ((Item)entity).GetWorldLocation();
						}
						else
						{
							return;
						}

						int index = pvSrc.ReadUInt16();

						if (state.IsEnhancedClient && index > 0x64)
						{
							index = menu.GetIndexEC(index);
						}

						if (index >= 0 && index < menu.Entries.Length)
						{
							using (var e = menu.Entries[index])
							{
								var range = e.Range;

								if (range == -1)
								{
									if (user.NetState != null && user.NetState.UpdateRange > 0)
									{
										range = user.NetState.UpdateRange;
									}
									else
									{
										range = Core.GlobalUpdateRange;
									}
								}

								if (user.InRange(p, range))
								{
									if (e.Enabled)
									{
										e.OnClick();
									}
									else
									{
										e.OnClickDisabled();
									}
								}
							}
						}
					}
				}
			}
		}

		public static void ContextMenuRequest(NetState state, PacketReader pvSrc)
		{
			var target = pvSrc.ReadEntity();

			if (target != null && ObjectPropertyList.Enabled && !state.Mobile.ViewOPL)
			{
				HandleSingleClick(state.Mobile, target);
			}

			ContextMenu.Display(state.Mobile, target);
		}

		public static void CloseStatus(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadSerial();
		}

		public static void Language(NetState state, PacketReader pvSrc)
		{
			var lang = pvSrc.ReadString(4);

			if (state.Mobile != null)
			{
				state.Mobile.Language = lang;
			}
		}

		public static void AssistVersion(NetState state, PacketReader pvSrc)
		{
			var unk = pvSrc.ReadInt32();
			var av = pvSrc.ReadString();
		}

		public static void ClientVersion(NetState state, PacketReader pvSrc)
		{
			var version = state.Version = new CV(pvSrc.ReadString());

			EventSink.InvokeClientVersionReceived(new ClientVersionReceivedArgs(state, version));
		}

		public static void ClientType(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadUInt16(); // 0x1
			pvSrc.ReadUInt32(); // 0x2 for KR, 0x3 for EC

			EventSink.InvokeClientTypeReceived(new ClientTypeReceivedArgs(state));
		}

		public static void MobileQuery(NetState state, PacketReader pvSrc)
		{
			EntityQuery(state, pvSrc);
		}

		public static void ItemQuery(NetState state, PacketReader pvSrc)
		{
			EntityQuery(state, pvSrc);
		}

		public static void EntityQuery(NetState state, PacketReader pvSrc)
		{
			var from = state.Mobile;

			pvSrc.ReadInt32(); // 0xEDEDEDED

			int type = pvSrc.ReadByte();

			var serial = pvSrc.ReadSerial();

			if (serial.IsMobile)
			{
				var m = World.FindMobile(serial);

				if (m != null)
				{
					switch (type)
					{
						case 0x00: // Unknown, sent by godclient
							{
								if (VerifyGC(state))
								{
									Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, m.Name);
								}

								break;
							}
						case 0x04: // Stats
							{
								m.OnStatsQuery(from);
								break;
							}
						case 0x05:
							{
								m.OnSkillsQuery(from);
								break;
							}
						default:
							{
								pvSrc.Trace(state);
								break;
							}
					}
				}
			}
			else if (serial.IsItem)
			{
				var item = World.FindItem(serial) as IDamageable;

				if (item != null)
				{
					switch (type)
					{
						case 0x00:
							{
								if (VerifyGC(state))
								{
									Console.WriteLine("God Client: {0}: Query 0x{1:X2} on {2} '{3}'", state, type, serial, item.Name);
								}

								break;
							}
						case 0x04: // Stats
							{
								item.OnStatsQuery(from);
								break;
							}
						case 0x05:
							{
								break;
							}
						default:
							{
								pvSrc.Trace(state);
								break;
							}
					}
				}
			}
		}

		public delegate void PlayCharCallback(NetState state, bool val);

		public static PlayCharCallback ThirdPartyAuthCallback = null, ThirdPartyHackedCallback = null;

		private static readonly byte[] m_ThirdPartyAuthKey =
		{
			0x9, 0x11, 0x83, (byte)'+', 0x4, 0x17, 0x83, 0x5, 0x24, 0x85, 0x7, 0x17, 0x87, 0x6, 0x19, 0x88
		};

		private class LoginTimer : Timer
		{
			private int m_Ticks;

			private NetState m_State;

			public LoginTimer(NetState state)
				: base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
			{
				m_State = state;
			}

			protected override void OnTick()
			{
				if (m_State == null || !m_State.Running)
				{
					Stop();

					m_State = null;
				}
				else if (m_State.Version != null)
				{
					Stop();

					m_State.BlockAllPackets = false;

					DoLogin(m_State);

					m_State = null;
				}
				else if (++m_Ticks % 10 == 0)
				{
					Stop();

					AccountLogin_ReplyRej(m_State, ALRReason.BadComm);

					m_State = null;
				}
			}
		}

		public static void PlayCharacter(NetState state, PacketReader pvSrc)
		{
			pvSrc.ReadInt32(); // 0xEDEDEDED

			var name = pvSrc.ReadString(30);

			pvSrc.ReadInt16(); // ?

			var flags = pvSrc.ReadInt32();

			if (FeatureProtection.DisabledFeatures != 0 && ThirdPartyAuthCallback != null)
			{
				var authOK = false;

				var razorFeatures = (((ulong)pvSrc.ReadUInt32()) << 32) | pvSrc.ReadUInt32();

				if (razorFeatures == (ulong)FeatureProtection.DisabledFeatures)
				{
					var match = true;

					for (var i = 0; match && i < m_ThirdPartyAuthKey.Length; i++)
					{
						match = pvSrc.ReadByte() == m_ThirdPartyAuthKey[i];
					}

					if (match)
					{
						authOK = true;
					}
				}
				else
				{
					pvSrc.Seek(16, SeekOrigin.Current);
				}

				ThirdPartyAuthCallback(state, authOK);
			}
			else
			{
				pvSrc.Seek(24, SeekOrigin.Current);
			}

			if (ThirdPartyHackedCallback != null)
			{
				pvSrc.Seek(-2, SeekOrigin.Current);

				if (pvSrc.ReadUInt16() == 0xDEAD)
				{
					ThirdPartyHackedCallback(state, true);
				}
			}

			if (!state.Running)
			{
				return;
			}

			var charSlot = pvSrc.ReadInt32();
			var clientIP = pvSrc.ReadInt32();

			var a = state.Account;

			if (a == null || charSlot < 0 || charSlot >= a.Length)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid Character Selection.");

				state.Dispose();
			}
			else
			{
				var m = a[charSlot];

				// Check if anyone is using this account
				for (var i = 0; i < a.Length; ++i)
				{
					var check = a[i];

					if (check != null && check.Map != Map.Internal && check != m)
					{
						Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Account In Use");

						PopupMessage.Send(state, PMMessage.CharInWorld);

						return;
					}
				}

				if (m == null)
				{
					Utility.WriteLine(ConsoleColor.Red, $"Login:{state}:InvalidCharacterSelection.");

					state.Dispose();
				}
				else
				{
					if (m.NetState != null)
					{
						m.NetState.Dispose();
					}

					NetState.ProcessDisposedQueue();

					state.Flags = (ClientFlags)flags;
					state.Mobile = m;

					m.NetState = state;

					if (state.Version == CV.Zero)
					{
						ClientVersionReq.Send(state);

						state.BlockAllPackets = true;

						new LoginTimer(state).Start();
					}
					else
					{
						DoLogin(state);
					}
				}
			}
		}

		public static void DoLogin(NetState state)
		{
			var m = state.Mobile;

			state.BlockAllPackets = false;

			state.Send(new LoginConfirm(m));

			m.SendMapUpdates(false, true);

			state.Send(LoginComplete.Instance);

			MobileStatus.Send(state, m);

			Network.SetWarMode.Send(state);

			state.Send(new CurrentTime());

			EventSink.InvokeLogin(new LoginEventArgs(m));

			Utility.WriteLine(ConsoleColor.Green, "Client: {0}: Entered World ({1})", state, m);
		}

		public static void CreateCharacter(NetState state, PacketReader pvSrc)
		{
			var isEC = pvSrc.ID == 0x8D;
			var is70160 = !isEC && pvSrc.ID == 0xF8;

			pvSrc.ReadUInt32(); // preamble 0xEDEDEDED
			pvSrc.ReadUInt32(); // preamble 0xFFFFFFFF (EC: character index)

			if (!isEC)
			{
				pvSrc.ReadByte(); // preamble terminator 0x0
			}

			var name = pvSrc.ReadString(30);

			if (isEC)
			{
				pvSrc.Skip(30); // password?
			}
			else
			{
				pvSrc.ReadUInt16(); // unknown

				state.Flags = (ClientFlags)pvSrc.ReadInt32();

				pvSrc.ReadUInt32(); // unknown
				pvSrc.ReadUInt32(); // login count
			}

			var prof = (Profession)pvSrc.ReadByte();

			if (!Enum.IsDefined(typeof(Profession), prof))
			{
				prof = Profession.Advanced;
			}

			bool female;
			var raceID = 0;

			if (isEC)
			{
				state.Flags = (ClientFlags)pvSrc.ReadByte();

				female = pvSrc.ReadByte() != 0;
				raceID = pvSrc.ReadByte();
			}
			else
			{
				pvSrc.Skip(15); // unknown

				var genderRace = pvSrc.ReadByte();

				female = genderRace % 2 != 0;

				if (genderRace >= 4)
				{
					raceID = (genderRace / 2) - 1;
				}
			}

			var stats = new[]
			{
				new StatNameValue(StatType.Str, pvSrc.ReadByte()),
				new StatNameValue(StatType.Dex, pvSrc.ReadByte()),
				new StatNameValue(StatType.Int, pvSrc.ReadByte())
			};

			int hairVal, beardVal, faceVal = 0;
			int hairHue, beardHue, faceHue = 0;
			int shirtHue, pantsHue, skinHue = 0;

			if (isEC)
			{
				skinHue = pvSrc.ReadUInt16();

				pvSrc.Skip(8); // unknown
			}

			var skills = new SkillNameValue[isEC || is70160 ? 4 : 3];

			for (var i = 0; i < skills.Length; i++)
			{
				skills[i] = new SkillNameValue((SkillName)pvSrc.ReadByte(), pvSrc.ReadByte());
			}

			var cityIndex = 0;

			if (isEC)
			{
				pvSrc.Skip(26); // unknown

				hairHue = pvSrc.ReadUInt16();
				hairVal = pvSrc.ReadUInt16();

				pvSrc.Skip(6); // unknown

				shirtHue = pvSrc.ReadUInt16();
				pantsHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				faceHue = pvSrc.ReadUInt16();
				faceVal = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				beardHue = pvSrc.ReadUInt16();
				beardVal = pvSrc.ReadUInt16();
			}
			else
			{
				skinHue = pvSrc.ReadUInt16();

				hairVal = pvSrc.ReadUInt16();
				hairHue = pvSrc.ReadUInt16();

				beardVal = pvSrc.ReadUInt16();
				beardHue = pvSrc.ReadUInt16();

				pvSrc.Skip(1); // unknown

				cityIndex = pvSrc.ReadByte();

				pvSrc.ReadInt32(); // character slot
				pvSrc.ReadInt32(); // ipv4 address

				shirtHue = pvSrc.ReadInt16();
				pantsHue = pvSrc.ReadInt16();
			}

			var info = state.CityInfo;

			if (info == null || cityIndex < 0 || cityIndex >= info.Length)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid city");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			var acc = state.Account;

			if (acc == null)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid account");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();

				return;
			}

			// Check if anyone is using this account
			for (var i = 0; i < acc.Length; ++i)
			{
				var check = acc[i];

				if (check != null && check.Map != Map.Internal)
				{
					if (acc.AccessLevel > AccessLevel.Player)
					{
						EventSink.InvokeLogout(new LogoutEventArgs(check));

						check.LogoutMap = check.Map;
						check.LogoutLocation = check.Location;

						check.Internalize();
						continue;
					}

					Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Account in use");

					PopupMessage.Send(state, PMMessage.CharInWorld);

					return;
				}
			}

			var race = Race.Races[raceID] ?? Race.DefaultRace;

			skinHue = race.ClipSkinHue(skinHue);
			hairHue = race.ClipHairHue(hairHue);
			beardHue = race.ClipHairHue(beardHue);

			Utility.Clamp(ref shirtHue, 0, 1001);
			Utility.Clamp(ref pantsHue, 0, 1001);

			if (state.Version == CV.Zero)
			{
				ClientVersionReq.Send(state);

				state.BlockAllPackets = true;
			}

			var args = new CharacterCreationEventArgs(state, name, info[cityIndex], race, prof, stats, skills, female, skinHue, hairVal, hairHue, beardVal, beardHue, faceVal, faceHue, shirtHue, pantsHue);

			EventSink.InvokeCharacterCreation(args);

			var m = args.Mobile;

			if (m == null)
			{
				state.BlockAllPackets = false;
				state.Dispose();
				return;
			}

			state.Mobile = m;

			m.NetState = state;

			EventSink.InvokeCharacterCreated(new CharacterCreatedEventArgs(state, m));

			if (state.Version == CV.Zero)
			{
				new LoginTimer(state).Start();
			}
			else
			{
				DoLogin(state);
			}
		}

		public static void PublicHouseContent(NetState state, PacketReader pvSrc)
		{
			int value = pvSrc.ReadByte();
			
			state.Mobile.PublicHouseContent = Convert.ToBoolean(value);
		}

		public static bool ClientVerification { get; set; } = true;

		public struct AuthIDPersistence
		{
			public DateTime Age;
			public CV Version;

			public AuthIDPersistence(CV v)
			{
				Age = DateTime.UtcNow;
				Version = v;
			}
		}

		private const int m_AuthIDWindowSize = 128;

		public static ConcurrentDictionary<uint, AuthIDPersistence> AuthIDWindow { get; } = new ConcurrentDictionary<uint, AuthIDPersistence>();

		public static uint GenerateAuthID(CV version)
		{
			if (AuthIDWindow.Count >= m_AuthIDWindowSize)
			{
				var oldestID = 0U;

				var oldest = DateTime.MaxValue;

				foreach (var kvp in AuthIDWindow)
				{
					if (kvp.Value.Age < oldest)
					{
						oldestID = kvp.Key;
						oldest = kvp.Value.Age;
					}
				}

				AuthIDWindow.TryRemove(oldestID, out _);
			}

			uint authID;

			do
			{
				authID = (uint)Utility.RandomMinMax(1, UInt32.MaxValue - 1);

				if (Utility.RandomBool())
				{
					authID |= 1U << 31;
				}
			}
			while (AuthIDWindow.ContainsKey(authID));

			AuthIDWindow[authID] = new AuthIDPersistence(version);

			return authID;
		}

		public static bool GetAuth(NetState state, out TimeSpan age, out CV version)
		{
			age = TimeSpan.Zero;
			version = CV.Zero;

			if (AuthIDWindow.TryGetValue(state.AuthID, out var ap))
			{
				age = DateTime.UtcNow - ap.Age;
				version = ap.Version;
			}

			return false;
		}

		public static void GameLogin(NetState state, PacketReader pvSrc)
		{
			if (state.SentFirstPacket)
			{
				state.Dispose();
				return;
			}

			state.SentFirstPacket = true;

			var authID = pvSrc.ReadUInt32();

			if (AuthIDWindow.TryRemove(authID, out var ap))
			{
				state.Version = ap.Version;
			}
			else if (ClientVerification)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid Client");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();
				return;
			}

			if (state.AuthID != 0 && authID != state.AuthID)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid Client");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();
				return;
			}

			if (state.AuthID == 0 && authID != state.Seed)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid Client");

				PopupMessage.Send(state, PMMessage.LoginSyncError);

				state.Dispose();
				return;
			}

			var username = pvSrc.ReadString(30);
			var password = pvSrc.ReadString(30);

			var e = new GameLoginEventArgs(state, username, password);

			EventSink.InvokeGameLogin(e);

			if (e.Accepted)
			{
				state.CityInfo = e.CityInfo;
				state.CompressionEnabled = true;

				SupportedFeatures.Send(state);
				CharacterList.Send(state);
			}
			else
			{
				state.Dispose();
			}
		}

		public static void PlayServer(NetState state, PacketReader pvSrc)
		{
			int index = pvSrc.ReadInt16();

			var info = state.ServerInfo;
			var a = state.Account;

			if (info == null || a == null || index < 0 || index >= info.Length)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Client: {state}: Invalid Server ({index})");

				state.Dispose();
			}
			else
			{
				state.AuthID = GenerateAuthID(state.Version);

				state.SentFirstPacket = false;

				state.Send(new PlayServerAck(info[index], state.AuthID));
			}
		}

		public static void LoginServerSeed(NetState state, PacketReader pvSrc)
		{
			state.Seed = pvSrc.ReadUInt32();

			state.Seeded = true;

			if (state.Seed == 0)
			{
				Utility.WriteLine(ConsoleColor.Red, $"Login: {state}: Invalid Client");

				state.Dispose();
				return;
			}

			var clientMaj = pvSrc.ReadInt32();
			var clientMin = pvSrc.ReadInt32();
			var clientRev = pvSrc.ReadInt32();
			var clientPat = pvSrc.ReadInt32();

			state.Version = new CV(clientMaj, clientMin, clientRev, clientPat);
		}

		public static void CrashReport(NetState state, PacketReader pvSrc)
		{
			var version = new CV(pvSrc.ReadByte(), pvSrc.ReadByte(), pvSrc.ReadByte(), pvSrc.ReadByte());
			var location = new Point3D(pvSrc.ReadUInt16(), pvSrc.ReadUInt16(), pvSrc.ReadSByte());
			var map = Map.AllMaps[pvSrc.ReadByte()];

			var account = state.Account;
			pvSrc.ReadString(32); // Account Name

			var mobile = state.Mobile;
			pvSrc.ReadString(32); // Character Name

			if (!IPAddress.TryParse(pvSrc.ReadString(15), out var ip))
				ip = IPAddress.None;

			Utility.Intern(ref ip);

			var addr = pvSrc.ReadInt32();
			var exception = pvSrc.ReadInt32();

			var process = pvSrc.ReadString(100);
			var report = pvSrc.ReadString(100);

			pvSrc.ReadByte(); // 0x00

			var offset = pvSrc.ReadInt32();

			var trace = new int[pvSrc.ReadByte()];

			for (var i = 0; i < trace.Length; i++)
				trace[i] = pvSrc.ReadInt32();

			EventSink.InvokeClientCrashed(new ClientCrashedEventArgs(version, ip, account, mobile, location, map, exception, addr, trace, offset, process, report));
		}

		public static void AccountLogin(NetState state, PacketReader pvSrc)
		{
			if (state.SentFirstPacket)
			{
				state.Dispose();
				return;
			}

			state.SentFirstPacket = true;

			var username = pvSrc.ReadString(30);
			var password = pvSrc.ReadString(30);

			var e = new AccountLoginEventArgs(state, username, password);

			EventSink.InvokeAccountLogin(e);

			if (e.Accepted)
			{
				AccountLogin_ReplyAck(state);
			}
			else
			{
				AccountLogin_ReplyRej(state, e.RejectReason);
			}
		}

		public static void AccountLogin_ReplyAck(NetState state)
		{
			var e = new ServerListEventArgs(state, state.Account);

			EventSink.InvokeServerList(e);

			if (e.Rejected)
			{
				state.Account = null;

				AccountLoginRej.Send(state, ALRReason.BadComm);

				state.Dispose();
			}
			else
			{
				var info = e.Servers.ToArray();

				state.ServerInfo = info;

				state.Send(new AccountLoginAck(info));
			}
		}

		public static void AccountLogin_ReplyRej(NetState state, ALRReason reason)
		{
			AccountLoginRej.Send(state, reason);

			state.Dispose();
		}

		public static void EquipMacro(NetState ns, PacketReader pvSrc)
		{
			int count = pvSrc.ReadByte();

			var serials = new Serial[count];

			for (var i = 0; i < count; ++i)
			{
				serials[i] = pvSrc.ReadSerial();
			}

			EventSink.InvokeEquipMacro(new EquipMacroEventArgs(ns.Mobile, serials));
		}

		public static void UnequipMacro(NetState ns, PacketReader pvSrc)
		{
			int count = pvSrc.ReadByte();

			var layers = new Layer[count];

			for (var i = 0; i < count; ++i)
			{
				layers[i] = (Layer)pvSrc.ReadInt16();
			}

			EventSink.InvokeUnequipMacro(new UnequipMacroEventArgs(ns.Mobile, layers));
		}

		public static void TargetedSpell(NetState ns, PacketReader pvSrc)
		{
			var spellId = (short)(pvSrc.ReadInt16() - 1); // zero based;
			var target = pvSrc.ReadSerial();

			EventSink.InvokeTargetedSpell(new TargetedSpellEventArgs(ns.Mobile, World.FindEntity(target), spellId));
		}

		public static void TargetedSkillUse(NetState ns, PacketReader pvSrc)
		{
			var skillId = pvSrc.ReadInt16();
			var target = pvSrc.ReadSerial();

			EventSink.InvokeTargetedSkill(new TargetedSkillEventArgs(ns.Mobile, World.FindEntity(target), skillId));
		}

		public static void TargetByResourceMacro(NetState ns, PacketReader pvSrc)
		{
			var serial = pvSrc.ReadSerial();
			int resourcetype = pvSrc.ReadInt16();

			if (serial.IsItem)
			{
				EventSink.InvokeTargetByResourceMacro(new TargetByResourceMacroEventArgs(ns.Mobile, World.FindItem(serial), resourcetype));
			}
		}
	}
}
