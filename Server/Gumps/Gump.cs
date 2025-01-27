#region References
using System;
using System.Collections.Generic;
using System.Text;

using Server.Network;
#endregion

namespace Server.Gumps
{
	public class Gump
	{
		private static readonly Dictionary<Type, int> _TypeCodes = new Dictionary<Type, int>(0x100);

		public static int GetTypeID(Type type)
		{
			if (!_TypeCodes.TryGetValue(type, out var id))
			{
				unchecked
				{
					id = 0x1337;

					var name = type.FullName;

					for (var i = 0; i < name.Length; i++)
						id = (id * 397) ^ name[i];
				}

				_TypeCodes[type] = id;
			}

			return id;
		}

		private readonly List<string> m_Strings;

		internal int m_TextEntries, m_Switches;

		private static int m_NextSerial = 1;

		private int m_Serial;
		private int m_X, m_Y;

		private bool m_Dragable = true;
		private bool m_Closable = true;
		private bool m_Resizable = true;
		private bool m_Disposable = true;

		public virtual int GetTypeID()
		{
			return GetTypeID(GetType());
		}

		public Gump(int x, int y)
		{
			do
			{
				m_Serial = m_NextSerial++;
			}
			while (m_Serial == 0); // standard client apparently doesn't send a gump response packet if serial == 0

			m_X = x;
			m_Y = y;

			TypeID = GetTypeID(GetType());

			Entries = new List<GumpEntry>();
			m_Strings = new List<string>();
		}

		public virtual void Invalidate()
		{
			ReleasePackets();
		}

		public int TypeID { get; set; }

		public List<GumpEntry> Entries { get; }

		public int Serial
		{
			get => m_Serial;
			set
			{
				if (m_Serial != value)
				{
					m_Serial = value;
					Invalidate();
				}
			}
		}

		public int X
		{
			get => m_X;
			set
			{
				if (m_X != value)
				{
					m_X = value;
					Invalidate();
				}
			}
		}

		public int Y
		{
			get => m_Y;
			set
			{
				if (m_Y != value)
				{
					m_Y = value;
					Invalidate();
				}
			}
		}

		public bool Disposable
		{
			get => m_Disposable;
			set
			{
				if (m_Disposable != value)
				{
					m_Disposable = value;
					Invalidate();
				}
			}
		}

		public bool Resizable
		{
			get => m_Resizable;
			set
			{
				if (m_Resizable != value)
				{
					m_Resizable = value;
					Invalidate();
				}
			}
		}

		public bool Dragable
		{
			get => m_Dragable;
			set
			{
				if (m_Dragable != value)
				{
					m_Dragable = value;
					Invalidate();
				}
			}
		}

		public bool Closable
		{
			get => m_Closable;
			set
			{
				if (m_Closable != value)
				{
					m_Closable = value;
					Invalidate();
				}
			}
		}

		public void AddPage(int page)
		{
			Add(new GumpPage(page));
		}

		public void AddAlphaRegion(int x, int y, int width, int height)
		{
			Add(new GumpAlphaRegion(x, y, width, height));
		}

		public void AddBackground(int x, int y, int width, int height, int gumpID)
		{
			Add(new GumpBackground(x, y, width, height, gumpID));
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
		{
			Add(new GumpButton(x, y, normalID, pressedID, buttonID, type, param));
		}

		public void AddCheck(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			Add(new GumpCheck(x, y, inactiveID, activeID, initialState, switchID));
		}

		public void AddGroup(int group)
		{
			Add(new GumpGroup(group));
		}

		public void AddTooltip(int number)
		{
			Add(new GumpTooltip(number));
		}

		public void AddTooltip(int number, string args)
		{
			Add(new GumpTooltip(number, args));
		}

		public void AddTooltip(string text)
		{
			Add(new GumpTooltip(1042971, text));
		}

		public void AddHtml(int x, int y, int width, int height, string text, bool background, bool scrollbar)
		{
			Add(new GumpHtml(x, y, width, height, text, background, scrollbar));
		}

		public void AddHtmlIntern(int x, int y, int width, int height, int textid, bool background, bool scrollbar)
		{
			Add(new GumpHtml(x, y, width, height, textid, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, int color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, color, background, scrollbar));
		}

		public void AddHtmlLocalized(int x, int y, int width, int height, int number, string args, int color, bool background, bool scrollbar)
		{
			Add(new GumpHtmlLocalized(x, y, width, height, number, args, color, background, scrollbar));
		}

		public void AddSpriteImage(int x, int y, int gumpID, int width, int height, int sx, int sy)
		{
			Add(new GumpSpriteImage(x, y, gumpID, width, height, sx, sy));
		}

		public void AddImage(int x, int y, int gumpID)
		{
			Add(new GumpImage(x, y, gumpID));
		}

		public void AddImage(int x, int y, int gumpID, int hue)
		{
			Add(new GumpImage(x, y, gumpID, hue));
		}

		public void AddImageTiled(int x, int y, int width, int height, int gumpID)
		{
			Add(new GumpImageTiled(x, y, width, height, gumpID));
		}

		public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height)
		{
			Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height));
		}

		public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param, int itemID, int hue, int width, int height, int localizedTooltip)
		{
			Add(new GumpImageTileButton(x, y, normalID, pressedID, buttonID, type, param, itemID, hue, width, height, localizedTooltip));
		}

		public void AddItem(int x, int y, int itemID)
		{
			Add(new GumpItem(x, y, itemID));
		}

		public void AddItem(int x, int y, int itemID, int hue)
		{
			Add(new GumpItem(x, y, itemID, hue));
		}

		public void AddLabelIntern(int x, int y, int hue, int textid)
		{
			Add(new GumpLabel(x, y, hue, textid));
		}

		public void AddLabel(int x, int y, int hue, string text)
		{
			Add(new GumpLabel(x, y, hue, text));
		}

		public void AddLabelCropped(int x, int y, int width, int height, int hue, string text)
		{
			Add(new GumpLabelCropped(x, y, width, height, hue, text));
		}

		public void AddLabelCroppedIntern(int x, int y, int width, int height, int hue, int textid)
		{
			Add(new GumpLabelCropped(x, y, width, height, hue, textid));
		}

		public void AddRadio(int x, int y, int inactiveID, int activeID, bool initialState, int switchID)
		{
			Add(new GumpRadio(x, y, inactiveID, activeID, initialState, switchID));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText)
		{
			Add(new GumpTextEntry(x, y, width, height, hue, entryID, initialText));
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, int entryID, string initialText, int size)
		{
			Add(new GumpTextEntryLimited(x, y, width, height, hue, entryID, initialText, size));
		}

		public void AddTextEntryIntern(int x, int y, int width, int height, int hue, int entryID, int initialTextID)
		{
			Add(new GumpTextEntry(x, y, width, height, hue, entryID, initialTextID));
		}

		public void AddItemProperty(Item item)
		{
			Add(new GumpItemProperty(item.Serial.Value));
		}

		public void AddItemProperty(int serial)
		{
			Add(new GumpItemProperty(serial));
		}

		public void AddECHandleInput()
		{
			Add(new ECHandleInput());
		}

		public void Add(GumpEntry g)
		{
			if (g.Parent != this)
			{
				g.Parent = this;
			}
			else if (!Entries.Contains(g))
			{
				Invalidate();
				Entries.Add(g);
			}
		}

		public void Remove(GumpEntry g)
		{
			if (g == null || !Entries.Contains(g))
			{
				return;
			}

			Invalidate();
			Entries.Remove(g);
			g.Parent = null;
		}

		public int Intern(string value)
		{
			return Intern(value, true);
		}

		public int Intern(string value, bool enforceUnique)
		{
			if (enforceUnique)
			{
				var indexOf = m_Strings.IndexOf(value);

				if (indexOf >= 0)
					return indexOf;
			}

			m_Strings.Add(value);
			return m_Strings.Count - 1;
		}

		public void SendTo(NetState state)
		{
			state.AddGump(this);
			state.Send(GetPacketFor(state));
		}

		public static byte[] StringToBuffer(string str)
		{
			return Encoding.ASCII.GetBytes(str);
		}

		private static readonly byte[] m_BeginLayout = StringToBuffer("{ ");
		private static readonly byte[] m_EndLayout = StringToBuffer(" }");

		private static readonly byte[] m_NoMove = StringToBuffer("{ nomove }");
		private static readonly byte[] m_NoClose = StringToBuffer("{ noclose }");
		private static readonly byte[] m_NoDispose = StringToBuffer("{ nodispose }");
		private static readonly byte[] m_NoResize = StringToBuffer("{ noresize }");

		public virtual Packet GetPacketFor(NetState ns)
		{
			return OpenPacket;
		}

		private readonly object m_OpenPacketLock = new object();

		private Packet m_OpenPacket;

		public Packet OpenPacket
		{
			get
			{
				if (m_OpenPacket == null)
				{
					lock (m_OpenPacketLock)
					{
						if (m_OpenPacket == null)
						{
							IGumpWriter disp = new DisplayGumpPacked(this);

							if (!m_Dragable)
							{
								disp.AppendLayout(m_NoMove);
							}

							if (!m_Closable)
							{
								disp.AppendLayout(m_NoClose);
							}

							if (!m_Disposable)
							{
								disp.AppendLayout(m_NoDispose);
							}

							if (!m_Resizable)
							{
								disp.AppendLayout(m_NoResize);
							}

							var count = Entries.Count;
							GumpEntry e;

							for (var i = 0; i < count; ++i)
							{
								e = Entries[i];

								disp.AppendLayout(m_BeginLayout);
								e.AppendTo(disp);
								disp.AppendLayout(m_EndLayout);
							}

							disp.WriteStrings(m_Strings);

							disp.Flush();

							m_TextEntries = disp.TextEntries;
							m_Switches = disp.Switches;

							m_OpenPacket = (Packet)disp;
							m_OpenPacket.SetStatic();
						}
					}
				}

				return m_OpenPacket;
			}
		}

		private readonly object m_ClosePacketLock = new object();

		private Packet m_ClosePacket;

		public Packet ClosePacket
		{
			get
			{
				if (m_ClosePacket == null)
				{
					lock (m_ClosePacketLock)
					{
						if (m_ClosePacket == null)
						{
							m_ClosePacket = new CloseGump(TypeID, 0);
							m_ClosePacket.SetStatic();
						}
					}
				}

				return m_ClosePacket;
			}
		}

		public virtual void ReleasePackets()
		{
			Packet.Release(ref m_OpenPacket);
			Packet.Release(ref m_ClosePacket);
		}

		public virtual void OnResponse(NetState sender, RelayInfo info)
		{ }

		public virtual void OnServerClose(NetState owner)
		{ }
	}
}
