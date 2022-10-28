#region References

using System;
using System.Xml;
using Server.Gumps;

#endregion

namespace Server.Regions
{
	public enum RaceRoomType
	{
		None,
		Czlowiek,
		Elf,
		Krasnolud,
		Drow,
		Teleport
	}

	public class RaceRoomRegion : BaseRegion
	{
		private readonly RaceRoomType m_Room;

		public RaceRoomRegion(XmlElement xml, Map map, Region parent) : base(xml, map, parent)
		{
			var roomTypeStr = "";
			ReadString(xml, "room", ref roomTypeStr, true);
			if (!Enum.TryParse(roomTypeStr, out m_Room))
				Console.WriteLine("Invalid RaceRoomRegion type " + roomTypeStr);
		}

		public override bool OnCombatantChange(Mobile from, IDamageable Old, IDamageable New)
		{
			return (from.AccessLevel > AccessLevel.Player);
		}

		public override void OnEnter(Mobile m)
		{
			if (m_Room != RaceRoomType.None && m_Room != RaceRoomType.Teleport)
			{
				m.SendGump(new RaceRoomGump(m, m_Room));
			}
		}

		public override void OnExit(Mobile m)
		{
		}
	}
}
