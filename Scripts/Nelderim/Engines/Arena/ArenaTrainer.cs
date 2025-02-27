#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Server.ContextMenus;
using Server.Engines.Tournament;
using Server.Gumps;
using Server.Items;
using Server.Network;
using Server.Prompts;
using Server.Regions;
using Server.Targeting;

#endregion

namespace Server.Mobiles
{
	public class ArenaTrainer : BaseVendor
	{
		public override bool CanTeach { get { return false; } }

		#region Subclasses

		private class OpponentTarget : Target
		{
			private readonly ArenaTrainer m_Owner;
			private readonly FightType m_FightType;
			private readonly Mobile m_Atacker;

			public OpponentTarget(ArenaTrainer owner, Mobile atacker, FightType ft) : base(4, false, TargetFlags.None)
			{
				m_Owner = owner;
				m_FightType = ft;
				m_Atacker = atacker;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				try
				{
					if (targeted is PlayerMobile && !(targeted as PlayerMobile).Young)
					{
						PlayerMobile op = targeted as PlayerMobile;

						if (!op.Deleted && op.Alive && op != m_Atacker)
						{
							string args = String.Format("{0}\t{1}", m_Atacker.Name,
								(m_FightType == FightType.ShortDuel) ? "krotki" : "dlugi");

							op.SendLocalizedMessage(00505178,
								args); // ~1_NAME~ wyzywa Cie na ~2_TYPE~ pojedynek. Wskaz wyzywajacego, jesli akceptujesz wyzwanie. 
							op.Target = new AcceptTarget(m_Owner, m_Atacker, op, m_FightType);
						}
						else
							from.SendLocalizedMessage(00505179); // "Nie mozesz tego wyzwac na pojedynek!"
					}
					else
						from.SendLocalizedMessage(00505179); // "Nie mozesz tego wyzwac na pojedynek!"
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.ToString());
					from.SendLocalizedMessage(00505180); // Wystapil nieznany blad areny!
				}
			}
		}

		private class AcceptTarget : Target
		{
			private readonly ArenaTrainer m_Owner;
			private readonly FightType m_FightType;
			private readonly Mobile m_Atacker;
			private readonly Mobile m_Opponent;

			public AcceptTarget(ArenaTrainer owner, Mobile atacker, Mobile opponent, FightType ft) : base(4, false,
				TargetFlags.None)
			{
				m_Owner = owner;
				m_FightType = ft;
				m_Atacker = atacker;
				m_Opponent = opponent;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				try
				{
					if (targeted is Mobile && (targeted as Mobile) == m_Atacker)
					{
						if (m_Atacker.Deleted || !m_Atacker.Alive)
						{
							from.SendLocalizedMessage(00505185); // "Pojedynek nie doszedl do skutku!"
							return;
						}

						if (m_Owner.Arena.Busy(true))
						{
							m_Owner.Say(00505186); // Trzeba bylo sie szybko decydowac! Arena jest juz zajeta.
							return;
						}

						BankBox abox = m_Atacker.BankBox;
						BankBox obox = m_Opponent.BankBox;
						int price = (m_FightType == FightType.ShortDuel)
							? m_Owner.PriceDuelShort
							: m_Owner.PriceDuelLong;

						if (abox != null && obox != null
						                 && !((GetBalance(m_Atacker) < price)
						                      || (GetBalance(m_Opponent) < price)))
						{
							Withdraw(m_Atacker, price);
							Withdraw(m_Opponent, price);
							m_Atacker.SendLocalizedMessage(00505184);
							m_Opponent.SendLocalizedMessage(00505184); // Stawaj do walki!
							m_Owner.Arena.AddFighter(m_Atacker, m_Opponent, m_FightType);
						}
						else
							m_Owner.Say(00505183); // "Nie macie zlota, nie zawracajcie glowy!"
					}
					else
					{
						from.SendLocalizedMessage(00505181); // Nie akceptujesz wyzwania na pojedynek!
						m_Atacker.SendLocalizedMessage(00505182); // Pojedynek nie zostal przyjety!
					}
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.ToString());
					from.SendLocalizedMessage(00505180); // Wystapil nieznany blad areny!
				}
			}

			protected override void OnTargetCancel(Mobile from, TargetCancelType cancelType)
			{
				try
				{
					from.SendLocalizedMessage(00505181); // Nie akceptujesz wyzwania na pojedynek!
					m_Atacker.SendLocalizedMessage(00505182); // Pojedynek nie zostal przyjety!
				}
				catch (Exception exc)
				{
					Console.WriteLine(exc.ToString());
					from.SendLocalizedMessage(00505180); // Wystapil nieznany blad areny!
				}
			}
		}

		private class ArenaRulesGump : Gump
		{
			public ArenaRulesGump(ArenaTrainer owner) : base(140, 80)
			{
				string title = "Zasady areny " + owner.ArenaName;
				string rules = "Witaj przybyszu na arenie " + owner.ArenaName + "\n\n";

				#region Rules

				if (!owner.NoTrainings)
				{
					if (!owner.NoDuels)
						rules += "Jesli wlasnie nie toczy sie pojedynek, czy turniej ";
					else
						rules += "Jesli wlasnie nie toczy sie turniej ";
					rules += "mozesz tu w spokoju odbyc trening z innymi chetnymi. Ceny, to jedyne ";
					rules += owner.PriceTrainingShort + " sztuk zlota za trwajacy dziesiec klepsydr krotki trenig i ";
					rules += owner.PriceTrainingLong + " sztuk zlota za trwajacy dobe dlugi trenig.\n\n";
				}

				if (!owner.NoDuels)
				{
					rules += "Jesli arena jest wolna ";
					rules += "mozesz tu stoczyc pojedynek. Kazdy z dwoch jego uczestnikow zaplaci ";
					rules += owner.PriceDuelShort + " sztuk zlota za trwajacy klepsydre krotki pojedynek i ";
					rules += owner.PriceDuelLong + " sztuk zlota za trwajacy trzy klepsydry dlugi pojedynek.\n\n";
				}

				rules += owner.GetArenaRules();

				#endregion

				AddPage(0);
				AddImage(0, 0, 1228);
				AddImage(340, 255, 9005);

				AddHtml(71, 6, 248, 18, "<div align=\"center\" color=\"2100\">" + title + "</div>", false, false);

				AddHtml(28, 33, 350, 218, "<BASEFONT COLOR=\"black\">" + rules + "</BASEFONT>", false, true);
			}
		}

		private class TournamentStatusGump : Gump
		{
			public TournamentStatusGump(ArenaTrainer owner) : base(140, 80)
			{
				string title = owner.TrnName;
				string info = "Jakis blad?";

				switch (owner.TrnState)
				{
					case TournamentState.None:
					case TournamentState.Configuration:
					case TournamentState.Initialization:
					case TournamentState.Start:
						return;
					case TournamentState.Recrutation:
						info = owner.GetTournamentStatus();
						break;
					case TournamentState.InProgress:
					case TournamentState.Finished:
						info = owner.Tournament.GetStatus();
						break;
				}

				AddPage(0);
				AddImage(0, 0, 1228);
				AddImage(340, 255, 9005);

				AddHtml(71, 6, 248, 18, "<div align=\"center\" color=\"2100\">" + title + "</div>", false, false);

				AddHtml(28, 33, 350, 218, "<BASEFONT COLOR=\"black\">" + info + "</BASEFONT>", false, true);
			}
		}

		private class TournamentStartTimer : Timer
		{
			private readonly ArenaTrainer m_Owner;

			public TournamentStartTimer(ArenaTrainer owner)
				: base(owner.TournamentStart - DateTime.Now - TimeSpan.FromSeconds((int)TournamentIntervals.Day))
			{
				Priority = TimerPriority.FiftyMS;
				m_Owner = owner;
			}

			protected override void OnTick()
			{
				m_Owner.TrnState = TournamentState.Start;
			}
		}

		private class MastersGump : Gump
		{
			public MastersGump() : base(140, 80)
			{
				string title = "Lista Mistrzow";

				string list = TournamentStatistics.PrintCompetitors(20);

				AddPage(0);
				AddImage(0, 0, 1228);
				AddImage(340, 255, 9005);

				AddHtml(71, 6, 248, 18, "<div align=\"center\" color=\"2100\">" + title + "</div>", false, false);

				AddHtml(28, 33, 350, 218, "<BASEFONT COLOR=\"black\">" + list + "</BASEFONT>", false, true);
			}
		}

		private class HistoryGump : Gump
		{
			public HistoryGump() : base(140, 80)
			{
				string title = "Historia turniejow";

				string list = TournamentStatistics.PrintRecords(20);

				AddPage(0);
				AddImage(0, 0, 1228);
				AddImage(340, 255, 9005);

				AddHtml(71, 6, 248, 18, "<div align=\"center\" color=\"2100\">" + title + "</div>", false, false);

				AddHtml(28, 33, 350, 218, "<BASEFONT COLOR=\"black\">" + list + "</BASEFONT>", false, true);
			}
		}

		public class FoundTournamentPrompt : Prompt
		{
			private readonly int m_Step;
			private Mobile m_From;
			private readonly ArenaTrainer m_Owner;

			public FoundTournamentPrompt(Mobile from, ArenaTrainer owner)
			{
				m_From = from;
				m_Owner = owner;
				m_Step = 0;

				owner.TrnAutoStart = true;
				owner.TrnClass = TournamentClass.Private;
				owner.TrnName = "Turniej prywatny - fundator " + from.Name;
				owner.TrnRewardName = null;

				owner.Say("Zatem chcesz ufundowac turniej? Podaj jego date... ");
				from.SendMessage(167, "Podaj date w formacie: yyyy-mm-dd hh:mm");
				from.SendMessage(38, "Turniej nie moze odbywac sie dalej, niz za piec dni czasu rzeczywistego");
				from.SendMessage(38, "Turniej nie moze odbywac sie blizej, niz za jeden dzien czasu rzeczywistego");
			}

			public FoundTournamentPrompt(Mobile from, ArenaTrainer owner, int step)
			{
				m_From = from;
				m_Owner = owner;
				m_Step = step;

				switch (m_Step)
				{
					#region Liczba uczestnikow

					case 1:
					{
						owner.Say("Powiedz ilu uczestnikow ma wystartowac w turnieju?");
						from.SendMessage(167, "Podaj liczbe uczestnikow (cyframi) bedaca liczba ze zbioru {8,16,32}");
						from.SendMessage(38, "Koszty turnieju to 16 tys. (8), 28 tys. (16), 44 tys. (32)");
						break;
					}

					#endregion

					#region Nagroda

					case 2:
					{
						owner.Say("Powiedz jak wysoka nagrode chcesz ufundowac zwyciezcy?");
						from.SendMessage(167, "Podaj cyfre bedaca liczba wieksza od 10000");
						break;
					}

					#endregion

					#region Wstepne

					case 3:
					{
						owner.Say("Powiedz jak wysokie ma byc wstepne na turniej?");
						from.SendMessage(167, "Podaj cyfre bedaca liczba wieksza od 0");
						break;
					}

					#endregion
				}
			}

			public override void OnResponse(Mobile from, string text)
			{
				switch (m_Step)
				{
					#region Data

					case 0:
					{
						try
						{
							m_Owner.TournamentStart = DateTime.Parse(text);
						}
						catch
						{
							from.SendMessage(38, "Niepoprawny format daty");
							m_Owner.TournamentStart = DateTime.Now + TimeSpan.FromDays(2);

							m_Owner.Say("Nie rozumiem daty. Ustalam termin turnieju na "
							            + NelderimDateTime.TransformToString(m_Owner.TournamentStart,
								            NelderimDateTimeFormat.LongIs));

							from.SendMessage(38, "Start ustalony na {0}", m_Owner.TournamentStart);
						}

						if ((m_Owner.TournamentStart > DateTime.Now + TimeSpan.FromDays(5)) ||
						    (m_Owner.TournamentStart < DateTime.Now + TimeSpan.FromDays(1)))
						{
							m_Owner.Say("Nie mozna urzadzic turnieju w tym czasie!");
							from.SendMessage(38, "Data za bliska, lub zbyt daleka!");
						}
						else
							from.Prompt = new FoundTournamentPrompt(from, m_Owner, 1);

						break;
					}

					#endregion

					#region Liczba uczestnikow

					case 1:
					{
						try
						{
							m_Owner.TrnCompMinCount = Int32.Parse(text);
						}
						catch
						{
							from.SendMessage(38, "Niepoprawny format cyfry");
							break;
						}

						from.SendMessage(167, "Liczba uczestnikow ustalona na {0}", m_Owner.TrnCompMinCount);
						from.Prompt = new FoundTournamentPrompt(from, m_Owner, 2);

						break;
					}

					#endregion

					#region Nagroda

					case 2:
					{
						try
						{
							m_Owner.TrnReward1st = Int32.Parse(text);
						}
						catch
						{
							from.SendMessage(38, "Niepoprawny format cyfry");
							break;
						}

						from.SendMessage(167, "Nagroda ustalona na {0}", m_Owner.TrnReward1st);
						m_Owner.Say("Turniej bedzie Ci kosztowal {0} sztuk zlota", m_Owner.PrivateTournamentCost());
						from.SendMessage(38, "Jesli nie chcesz kontynuowac anuluj kolejny prompt (ESC)!");

						from.Prompt = new FoundTournamentPrompt(from, m_Owner, 3);
						break;
					}

					#endregion

					#region Wstepne

					case 3:
					{
						try
						{
							m_Owner.TrnFee = Int32.Parse(text);
						}
						catch
						{
							from.SendMessage(38, "Niepoprawny format cyfry");
							break;
						}

						from.SendMessage(167, "Wstepne ustalone na {0}", m_Owner.TrnFee);

						int cost = m_Owner.PrivateTournamentCost();
						BankBox box = from.BankBox;

						if (box == null || !Withdraw(from, cost))
							m_Owner.Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
						else if (box != null)
						{
							m_Owner.TrnFounder = from;
							m_Owner.Say("Turniej zaklepany! Rozglos nowiny!");
							m_Owner.TrnState = TournamentState.Initialization;
						}

						break;
					}

					#endregion
				}
			}

			public override void OnCancel(Mobile from)
			{
				from.SendLocalizedMessage(502980); // Message entry cancelled.
				m_Owner.Say(00505121); // "Nie chesz, nie musisz. Tylkio nie zawracaj mi wiecej glowy!"
			}
		}

		#region ContextMenu

		public class MastersEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public MastersEntry(Mobile m, Mobile from) : base(50001, 6) // Mistrzowie
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(00505094);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_From.SendGump(new MastersGump());
			}
		}

		public class HistoryEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public HistoryEntry(Mobile m, Mobile from) : base(50002, 6) // Historia
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(00505095);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_From.SendGump(new HistoryGump());
			}
		}

		public class RulesEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public RulesEntry(Mobile m, Mobile from) : base(50003, 6) // Zasady Areny
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(00505096);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_From.SendGump(new ArenaRulesGump(m_Mobile as ArenaTrainer));
			}
		}

		public class TrainingEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;
			private readonly bool m_Long;

			public TrainingEntry(Mobile m, Mobile from, bool longTraining) :
				base(50006 + ((longTraining) ? 1 : 0), 6) // Krotki & Dlugi trening 
			{
				m_Mobile = m;
				m_From = from;
				m_Long = longTraining;
			}

			public override void OnClick()
			{
				m_From.Say(00505097 + (!m_Long ? 0 : 1));

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, (m_Long) ? "dlugi trening" : "krotki trening",
						MessageType.Regular, -1, null));
			}
		}

		public class DuelEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;
			private readonly bool m_Long;

			public DuelEntry(Mobile m, Mobile from, bool longDuel) :
				base(50004 + ((longDuel) ? 1 : 0), 6) // Krotki & Dlugi pojedynek
			{
				m_Mobile = m;
				m_From = from;
				m_Long = longDuel;
			}

			public override void OnClick()
			{
				m_From.Say(00505099 + (!m_Long ? 0 : 1));

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, (m_Long) ? "dlugi pojedynek" : "krotki pojedynek",
						MessageType.Regular, -1, null));
			}
		}

		public class TournamentFoundEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public TournamentFoundEntry(Mobile m, Mobile from) : base(50010, 6) // Ufunduj turniej 
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(505283);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, "turniej ufund", MessageType.Regular, -1, null));
			}
		}

		public class TournamentInfoEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public TournamentInfoEntry(Mobile m, Mobile from) : base(50014, 6) // Informacje o turnieju 
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(505284);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, "turniej informacje", MessageType.Regular, -1, null));
			}
		}

		public class TournamentJoinEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public TournamentJoinEntry(Mobile m, Mobile from) : base(50011, 6) // Zglos udzial w turnieju 
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(505285);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, "turniej zglos", MessageType.Regular, -1, null));
			}
		}

		public class TournamentQuitEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public TournamentQuitEntry(Mobile m, Mobile from) : base(50013, 6) // Wycofaj sie z turnieju 
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(505286);

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, "turniej wycof", MessageType.Regular, -1, null));
			}
		}

		public class TournamentConfirmEntry : ContextMenuEntry
		{
			private readonly Mobile m_Mobile;
			private readonly Mobile m_From;

			public TournamentConfirmEntry(Mobile m, Mobile from) : base(50012, 6) // Potwierdz udzial w turnieju 
			{
				m_Mobile = m;
				m_From = from;
			}

			public override void OnClick()
			{
				m_From.Say(505296); // Czolem! Chce potwierdzic udzial w turnieju.

				if (m_Mobile is ArenaTrainer && ((BaseVendor)m_Mobile).CheckVendorAccess(m_From))
					m_Mobile.OnSpeech(new SpeechEventArgs(m_From, "turniej potwierdz", MessageType.Regular, -1, null));
			}
		}

		#endregion

		#endregion

		#region Fields

		private readonly List<SBInfo> m_SBInfos = new List<SBInfo>();
		private bool m_Active;
		private Point3D m_FirstCorner;
		private Point3D m_SecondCorner;
		private Point3D m_ExtortPoint;
		private Point3D m_EndOfFightPoint;
		private bool m_NoMagery;
		private bool m_NoNecro;
		private bool m_NoDruidism;
		private bool m_NoChivalry;
		private bool m_NoSummons;
		private bool m_NoFamiliars;
		private bool m_NoControls;
		private bool m_NoAlchemy;
		private bool m_NoHealing;
		private bool m_NoEnter;
		private bool m_NoControlledSummons;
		private bool m_NoMounts;
		private bool m_NoHiding;
		private int m_Price;
		private TournamentState m_TournamentState;
		private int m_TournamentFee;
		private int m_TournamentReward;
		private int m_Tournament2ndReward;
		private int m_Tournament3rdReward;
		private DateTime m_TournamentStart;
		private bool m_TournamentAutoStart;
		private TournamentStartTimer m_TournamentStartTimer;
		private string m_TournamentName;
		private int m_TournamentMinNumberOfCompetitors;
		private string m_TournamentRewardName;
		private TournamentClass m_TournamentClass;
		private Serial m_TournamentFounderSerial;

		#endregion

		#region Properties

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Mobile TrnFounder
		{
			get
			{
				return World.FindMobile(m_TournamentFounderSerial);
			}
			set
			{
				if (value == null)
					m_TournamentFounderSerial = new Serial();
				else
					m_TournamentFounderSerial = value.Serial;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public TournamentClass TrnClass
		{
			get { return m_TournamentClass; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
				{
					m_TournamentClass = value;

					if (m_TournamentClass == TournamentClass.Private || m_TournamentClass == TournamentClass.Masters)
					{
						TrnCompMinCount = 8;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public string TrnRewardName
		{
			get { return m_TournamentRewardName; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_TournamentRewardName = value;
			}
		}

		public ArrayList Competitors { get; private set; }

		public Tournament Tournament { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TournamentPushFight TrnPushFight
		{
			get { return TournamentPushFight.None; }
			set
			{
				if (TrnStateProgress == TournamentProgress.Fight)
					Tournament.PushFight(value);
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int TrnCompMinCount
		{
			get { return m_TournamentMinNumberOfCompetitors; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
				{
					m_TournamentMinNumberOfCompetitors = (value <= 4) ? 4 :
						(value <= 8) ? 8 :
						(value <= 16) ? 16 :
						(value <= 32) ? 32 :
						(value <= 64) ? 64 : 128;

					if ((m_TournamentClass == TournamentClass.Masters || m_TournamentClass == TournamentClass.Private)
					    && m_TournamentMinNumberOfCompetitors < 8)
						m_TournamentMinNumberOfCompetitors = 8;
					else if (m_TournamentClass == TournamentClass.Private && m_TournamentMinNumberOfCompetitors > 32)
						m_TournamentMinNumberOfCompetitors = 32;
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public string TrnName
		{
			get { return m_TournamentName; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_TournamentName = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public bool TrnAutoStart
		{
			get { return m_TournamentAutoStart; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_TournamentAutoStart = value;
			}
		}

		public DateTime TournamentStart
		{
			get { return m_TournamentStart; }
			set { m_TournamentStart = value; }
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public string TrnDate
		{
			get
			{
				return m_TournamentStart.ToString();
			}
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
				{
					try
					{
						m_TournamentStart = DateTime.Parse(value);
					}
					catch
					{
						m_TournamentStart = DateTime.Now + TimeSpan.FromDays(1);

						Say("Nie rozumiem daty. Ustalam termin turnieju na "
						    + NelderimDateTime.TransformToString(m_TournamentStart, NelderimDateTimeFormat.LongIs));
					}
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int TrnReward1st
		{
			get { return m_TournamentReward; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_TournamentReward = (value < 0) ? 0 : value;

				if (m_TournamentClass == TournamentClass.Private)
					m_TournamentReward = (m_TournamentReward < 10000) ? 10000 : m_TournamentReward;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int TrnReward2nd
		{
			get { return m_Tournament2ndReward; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_Tournament2ndReward = (value < 0) ? 0 : value;

				if (m_TournamentClass == TournamentClass.Private)
					m_Tournament2ndReward = 0;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int TrnReward3rd
		{
			get { return m_Tournament3rdReward; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_Tournament3rdReward = (value < 0) ? 0 : value;

				if (m_TournamentClass == TournamentClass.Private)
					m_Tournament3rdReward = 0;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int TrnFee
		{
			get { return m_TournamentFee; }
			set
			{
				TrnState = TournamentState.Configuration;

				if (m_TournamentState == TournamentState.Configuration)
					m_TournamentFee = (value < 0) ? 0 : value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public TournamentState TrnState
		{
			get { return m_TournamentState; }
			set
			{
				Active = true;

				if (!Active)
				{
					Say(00505170); // Heh! Tu nie ma areny! Tak tylko stoje!
					return;
				}

				TournamentState newValue = value;

				#region Tryby niedostepne

				if (newValue == TournamentState.Recrutation || newValue == TournamentState.Finished
				                                            || newValue == TournamentState.InProgress)
				{
					Say(00505171); // O nie! Tego nie mozesz mi kazac!
					return;
				}

				#endregion

				#region Turniej trwa

				if (m_TournamentState == TournamentState.InProgress)
				{
					Say(00505172); // Turniej wlasnie trwa!
					return;
				}

				#endregion

				#region Trwa rekrutacja a polecenie nie jest poleceniem startu

				if (m_TournamentState == TournamentState.Recrutation && newValue != TournamentState.Start)
				{
					Say(00505173); // Trwa rekrutacja! Nie moge tego zrobic.
					return;
				}

				#endregion

				#region Jesli jest Finished, a polecenie dozwolone, to czyscimy statystyki starego turnieju

				if (m_TournamentState == TournamentState.Finished)
				{
					Tournament = null;
				}

				#endregion

				#region Inicjalizacja = jesli OK -> Rekrutacja

				if (newValue == TournamentState.Initialization && true)
//				    && ( ( !m_TournamentAutoStart && m_TournamentStart >= DateTime.Now + TimeSpan.FromSeconds( ( int ) TournamentIntervals.Day ) )
//				        || ( m_TournamentAutoStart && m_TournamentStart >= DateTime.Now + TimeSpan.FromSeconds( ( int ) TournamentIntervals.Month ) ) ) )
				{
					m_TournamentState = TournamentState.Recrutation;

					if (m_TournamentAutoStart)
					{
						m_TournamentStartTimer = new TournamentStartTimer(this);
						m_TournamentStartTimer.Start();
					}

					return;
				}

				if (newValue == TournamentState.Initialization)
				{
					Say(00505174); // Za malo czasu na rekrutacje! Podaj inne daty!
					m_TournamentState = TournamentState.Configuration;
					return;
				}

				#endregion

				#region Start = jesli Rekrutacja i OK -> InProgress

				if (newValue == TournamentState.Start && m_TournamentState == TournamentState.Recrutation)
//				    && ( m_TournamentStart < DateTime.Now + TimeSpan.FromSeconds( ( int ) TournamentIntervals.Day ) ) )
				{
					m_TournamentState = TournamentState.InProgress;
					Tournament = new Tournament(this);
					return;
				}

				if (newValue == TournamentState.Start && m_TournamentState == TournamentState.Recrutation)
				{
					Say((m_TournamentAutoStart)
						? 00505175
						: 00505176); // "Sam zaczne! Nie ruszaj!" : "Jest za wczesnie. Nie rozpoczne turnieju."
					return;
				}

				if (newValue == TournamentState.Start)
				{
					Say(00505177); // Moze najpierw zbierzemy kilku chetnych do turnieju?
					return;
				}

				#endregion

				m_TournamentState = newValue;
			}
		}

		[CommandProperty(AccessLevel.Seer)]
		public TournamentProgress TrnStateProgress
		{
			get
			{
				return (Tournament != null) ? Tournament.Progress : TournamentProgress.None;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int TrnCompCount { get { return Competitors.Count; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public bool Active
		{
			get { return m_Active; }
			set
			{
				if (m_Active != value)
				{
					m_Active = value;

					if (m_Active)
						m_Active = ActivateArena();
					else
						DeactivateArena();
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoMagery
		{
			get { return m_NoMagery; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoMagery = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoNecro
		{
			get { return m_NoNecro; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoNecro = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoDruidism
		{
			get { return m_NoDruidism; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoDruidism = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoChivalry
		{
			get { return m_NoChivalry; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoChivalry = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoAlchemy
		{
			get { return m_NoAlchemy; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoAlchemy = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoHealing
		{
			get { return m_NoHealing; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoHealing = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoHiding
		{
			get { return m_NoHiding; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoHiding = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoControls
		{
			get { return m_NoControls; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoControls = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoFamiliars
		{
			get { return m_NoFamiliars; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoFamiliars = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoSummons
		{
			get { return m_NoSummons; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoSummons = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoControlledSummons
		{
			get { return m_NoControlledSummons; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoControlledSummons = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoMounts
		{
			get { return m_NoMounts; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoMounts = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoEnter
		{
			get { return m_NoEnter; }
			set
			{
				if (m_TournamentState != TournamentState.Recrutation && m_TournamentState != TournamentState.InProgress)
					m_NoEnter = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoTrainings { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool NoDuels { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D Corner1st
		{
			get { return m_FirstCorner; }
			set
			{
				Active = false;

				m_FirstCorner = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D Corner2nd
		{
			get { return m_SecondCorner; }
			set
			{
				Active = false;

				m_SecondCorner = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D CornerBlue { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D CornerRed { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D ExtortPoint
		{
			get { return m_ExtortPoint; }
			set
			{
				m_ExtortPoint = value;
				if (m_EndOfFightPoint == Point3D.Zero)
					m_EndOfFightPoint = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public Point3D EndOfFightPoint
		{
			get { return m_EndOfFightPoint; }
			set
			{
				m_EndOfFightPoint = value;
				if (m_ExtortPoint == Point3D.Zero)
					m_ExtortPoint = value;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public string ArenaName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Seer)]
		public int PriceBase
		{
			get { return m_Price; }
			set
			{
				m_Price = value;

				if (m_Price < 0)
					m_Price = 20;
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public int PriceDuelShort { get { return m_Price * 10; } }

		[CommandProperty(AccessLevel.Counselor)]
		public int PriceDuelLong { get { return m_Price * 25; } }

		[CommandProperty(AccessLevel.Counselor)]
		public int PriceTrainingShort { get { return m_Price * 10; } }

		[CommandProperty(AccessLevel.Counselor)]
		public int PriceTrainingLong { get { return m_Price * 25; } }

		public NArenaRegion Arena { get; private set; }

		protected override List<SBInfo> SBInfos
		{
			get { return m_SBInfos; }
		}

		public override NpcGuild NpcGuild
		{
			get { return NpcGuild.WarriorsGuild; }
		}

		#endregion

		#region Constructors & Serialization

		[Constructable]
		public ArenaTrainer() : base("- zarzadca areny")
		{
			SetSkill(SkillName.Anatomy, 80.0, 120.0);
			SetSkill(SkillName.Fencing, 90.0, 120.0);
			SetSkill(SkillName.Wrestling, 60.0, 100.0);
			SetSkill(SkillName.Parry, 80.0, 100.0);
			SetSkill(SkillName.Bushido, 75.0, 110.0);
			m_Active = false;
			Arena = null;
			m_NoMagery = false;
			m_NoDruidism = false;
			m_NoChivalry = false;
			m_NoNecro = false;
			m_NoSummons = true;
			m_NoFamiliars = true;
			m_NoControls = true;
			m_NoAlchemy = false;
			m_NoHealing = false;
			m_NoEnter = true;
			NoTrainings = false;
			NoDuels = false;
			ArenaName = "arena";
			m_Price = 20;
			Competitors = new ArrayList();
			m_TournamentState = TournamentState.None;
			m_TournamentFee = 5000;
			m_TournamentReward = 0;
			m_Tournament2ndReward = 0;
			m_Tournament3rdReward = 0;
			m_TournamentStart = DateTime.Now;
			m_TournamentAutoStart = false;
			Tournament = null;
			m_TournamentStartTimer = null;
			m_TournamentName = "Turniej mistrzowski";
			m_TournamentMinNumberOfCompetitors = 8;
			m_NoControlledSummons = true;
			m_NoMounts = false;
			m_NoHiding = false;
			m_TournamentRewardName = null;
			m_TournamentClass = TournamentClass.Normal;
			m_TournamentFounderSerial = new Serial();
		}

		public ArenaTrainer(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(12); // version

			#region version 11

			writer.Write(m_Tournament2ndReward);
			writer.Write(m_Tournament3rdReward);

			#endregion

			#region version 10

			// writer.Write( ( bool ) m_TournamentPassToIRCBot );

			#endregion

			#region version 9

			writer.Write(m_TournamentFounderSerial.Value);

			#endregion

			#region version 8

			writer.Write((int)m_TournamentClass);

			if (m_TournamentRewardName != null)
			{
				writer.Write(true);
				writer.Write(m_TournamentRewardName);
			}
			else
				writer.Write(false);

			#endregion

			#region version 7

			writer.Write(m_NoControlledSummons);
			writer.Write(m_NoMounts);
			writer.Write(m_NoHiding);

			#endregion

			#region version 6

			writer.Write(m_TournamentName);
			writer.Write(m_TournamentMinNumberOfCompetitors);

			#endregion

			#region version 5

			writer.Write((int)m_TournamentState);

			#endregion

			#region version 4

			writer.Write(Competitors.Count);

			for (int i = 0; i < Competitors.Count; i++)
				(Competitors[i] as TournamentCompetitor).Serialize(writer);

			writer.Write(m_TournamentFee);
			writer.Write(m_TournamentReward);
			writer.Write(m_TournamentStart);
			writer.Write(m_TournamentAutoStart);

			if (Tournament != null)
			{
				writer.Write(true);
				Tournament.Serialize(writer);
			}
			else
				writer.Write(false);

			#endregion

			#region version 3

			writer.Write(NoTrainings);
			writer.Write(NoDuels);

			#endregion

			#region version 2

			writer.Write(m_NoDruidism);
			writer.Write(m_NoChivalry);
			writer.Write(m_NoNecro);
			writer.Write(m_NoSummons);
			writer.Write(m_NoFamiliars);
			writer.Write(m_NoControls);

			#endregion

			#region version 1

			writer.Write(CornerBlue);
			writer.Write(CornerRed);

			#endregion

			#region version 0

			writer.Write(m_Active);
			writer.Write(m_FirstCorner);
			writer.Write(m_SecondCorner);
			writer.Write(m_ExtortPoint);
			writer.Write(m_EndOfFightPoint);
			writer.Write(ArenaName);
			writer.Write(m_NoMagery);
			writer.Write(m_NoAlchemy);
			writer.Write(m_NoHealing);
			writer.Write(m_NoEnter);
			writer.Write(m_Price);

			#endregion
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 12:

				#region Case 11

				case 11:
				{
					m_Tournament2ndReward = reader.ReadInt();
					m_Tournament3rdReward = reader.ReadInt();

					goto case 10;
				}

				#endregion

				#region Case 10

				case 10:
				{
					if (version == 11)
					{
						/*m_TournamentPassToIRCBot = */
						reader.ReadBool();
					}

					goto case 9;
				}

				#endregion

				#region Case 9

				case 9:
				{
					m_TournamentFounderSerial = new Serial();
					m_TournamentFounderSerial = reader.ReadSerial();

					goto case 8;
				}

				#endregion

				#region Case 8

				case 8:
				{
					m_TournamentClass = (TournamentClass)reader.ReadInt();

					bool test = reader.ReadBool();

					if (test)
						m_TournamentRewardName = reader.ReadString();
					else
						m_TournamentRewardName = null;

					goto case 7;
				}

				#endregion

				#region Case 7

				case 7:
				{
					m_NoControlledSummons = reader.ReadBool();
					m_NoMounts = reader.ReadBool();
					m_NoHiding = reader.ReadBool();
					goto case 6;
				}

				#endregion

				#region Case 6

				case 6:
				{
					m_TournamentName = reader.ReadString();
					m_TournamentMinNumberOfCompetitors = reader.ReadInt();
					goto case 5;
				}

				#endregion

				#region Case 5

				case 5:
				{
					m_TournamentState = (TournamentState)reader.ReadInt();
					goto case 4;
				}

				#endregion

				#region Case 4

				case 4:
				{
					Competitors = new ArrayList();

					int cnt = reader.ReadInt();

					for (int i = 0; i < cnt; i++)
					{
						TournamentCompetitor tc = new TournamentCompetitor();
						tc.Deserialize(reader);
						Competitors.Add(tc);
					}

					m_TournamentFee = reader.ReadInt();
					m_TournamentReward = reader.ReadInt();
					m_TournamentStart = reader.ReadDateTime();
					m_TournamentAutoStart = reader.ReadBool();

					if (reader.ReadBool())
					{
						Tournament = new Tournament(this, reader);
					}
					else
						Tournament = null;

					goto case 3;
				}

				#endregion

				#region Case 3

				case 3:
				{
					NoTrainings = reader.ReadBool();
					NoDuels = reader.ReadBool();
					goto case 2;
				}

				#endregion

				#region Case 2

				case 2:
				{
					m_NoDruidism = reader.ReadBool();
					m_NoChivalry = reader.ReadBool();
					m_NoNecro = reader.ReadBool();
					m_NoSummons = reader.ReadBool();
					m_NoFamiliars = reader.ReadBool();
					m_NoControls = reader.ReadBool();
					goto case 1;
				}

				#endregion

				#region Case 1

				case 1:
				{
					CornerBlue = reader.ReadPoint3D();
					CornerRed = reader.ReadPoint3D();
					goto case 0;
				}

				#endregion

				#region Case 0

				case 0:
				{
					if (version < 11)
					{
						m_Tournament2ndReward = 0;
						m_Tournament3rdReward = 0;
					}

					//if ( version < 10 )
					//{
					//	m_TournamentPassToIRCBot = false;
					//}

					if (version < 9)
					{
						m_TournamentFounderSerial = new Serial();
					}

					if (version < 8)
					{
						m_TournamentRewardName = null;
						m_TournamentClass = TournamentClass.Normal;
					}

					if (version < 7)
					{
						m_NoControlledSummons = true;
						m_NoMounts = false;
						m_NoHiding = false;
					}

					if (version < 6)
					{
						m_TournamentName = "Turniej mistrzowski";
						m_TournamentMinNumberOfCompetitors = 8;
					}

					if (version < 4)
					{
						Competitors = new ArrayList();
						m_TournamentFee = 5000;
						m_TournamentReward = 0;
						m_TournamentStart = DateTime.Now;
						m_TournamentAutoStart = false;
						Tournament = null;
					}

					if (version < 3)
					{
						NoTrainings = false;
						NoDuels = false;
					}

					if (version < 2)
					{
						m_NoDruidism = false;
						m_NoChivalry = false;
						m_NoNecro = false;
						m_NoSummons = true;
						m_NoFamiliars = true;
						m_NoControls = true;
					}

					m_Active = reader.ReadBool();
					m_FirstCorner = reader.ReadPoint3D();
					m_SecondCorner = reader.ReadPoint3D();
					m_ExtortPoint = reader.ReadPoint3D();
					m_EndOfFightPoint = reader.ReadPoint3D();
					ArenaName = reader.ReadString();
					m_NoMagery = reader.ReadBool();
					m_NoAlchemy = reader.ReadBool();
					m_NoHealing = reader.ReadBool();
					m_NoEnter = reader.ReadBool();
					m_Price = reader.ReadInt();

					if ((version) < 1)
					{
						CornerBlue = Point3D.Zero;
						CornerRed = Point3D.Zero;
					}

					break;
				}

				#endregion
			}

			#region Other

			if (m_Active)
			{
				Arena = new NArenaRegion("arena", this.Map, null, this);
				Arena.Owner = this;
				ActivateArena();
			}

			if (m_TournamentState == TournamentState.Recrutation && m_TournamentAutoStart)
			{
				m_TournamentStartTimer = new TournamentStartTimer(this);
				m_TournamentStartTimer.Start();
			}
			else
				m_TournamentStartTimer = null;

			#endregion
		}

		public override void Delete()
		{
			if (m_Active)
				DeactivateArena();
			base.Delete();
		}

		#endregion

		public override void InitSBInfo()
		{
			m_SBInfos.Add(new SBArenaTrainer());
		}

		#region ContextMenu

		public override void AddCustomContextEntries(Mobile from, List<ContextMenuEntry> list)
		{
			try
			{
				if (from.Alive)
				{
					list.Add(new RulesEntry(this, from));

					if (m_TournamentState == TournamentState.Finished ||
					    (int)TrnState < (int)TournamentState.Initialization)
						list.Add(new TournamentFoundEntry(this, from));

					if ((int)m_TournamentState > (int)TournamentState.Configuration)
					{
						list.Add(new TournamentInfoEntry(this, from));

						if (m_TournamentState == TournamentState.Recrutation)
						{
							if (!IsCompetitor(from))
								list.Add(new TournamentJoinEntry(this, from));
							else
								list.Add(new TournamentQuitEntry(this, from));
						}

						if (m_TournamentState == TournamentState.InProgress &&
						    (int)TrnStateProgress < (int)TournamentProgress.Fight)
						{
							if (IsCompetitor(from) && !IsConfirmed(from))
								list.Add(new TournamentConfirmEntry(this, from));
							else if (!IsCompetitor(from))
								list.Add(new TournamentJoinEntry(this, from));
						}
					}

					if (!NoTrainings && TrnState != TournamentState.InProgress && !Arena.Busy(false))
					{
						list.Add(new TrainingEntry(this, from, false));
						list.Add(new TrainingEntry(this, from, true));
					}

					if (!NoDuels && TrnState != TournamentState.InProgress && !Arena.Busy(true))
					{
						list.Add(new DuelEntry(this, from, false));
						list.Add(new DuelEntry(this, from, true));
					}

					list.Add(new HistoryEntry(this, from));
					list.Add(new MastersEntry(this, from));
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}

			base.AddCustomContextEntries(from, list);
		}

		#endregion

		public override bool HandlesOnSpeech(Mobile from)
		{
			return true;
		}

		public override void OnSpeech(SpeechEventArgs e)
		{
			try
			{
				if (m_Active && !e.Handled && e.Mobile.InRange(this, 3))
				{
					int[] keywords = e.Keywords;
					string speech = e.Speech.ToLower();
					Mobile from = e.Mobile;

					#region Rules

					if (Regex.IsMatch(e.Speech, "zasad", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;
							if (e.Speech.ToLower() == "zasad" ||
							    Regex.IsMatch(e.Speech, "^zasad.$", RegexOptions.IgnoreCase)) OnLazySpeech();
							else
							{
								try
								{
									from.SendGump(new ArenaRulesGump(this));
								}
								catch (Exception exc)
								{
									Console.WriteLine(exc.ToString());
									Say(00505187); // Tego nie zrobie! Nie da sie.
								}
							}
						}
					}

					#endregion

					#region Rankings

					if (Regex.IsMatch(e.Speech, "list", RegexOptions.IgnoreCase)
					    && Regex.IsMatch(e.Speech, "mistrzow", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;

							try
							{
								from.SendGump(new MastersGump());
							}
							catch (Exception exc)
							{
								Console.WriteLine(exc.ToString());
								Say(00505187); // Tego nie zrobie! Nie da sie.
							}
						}
					}

					#endregion

					#region Historie

					if (Regex.IsMatch(e.Speech, "histori", RegexOptions.IgnoreCase)
					    && Regex.IsMatch(e.Speech, "turniej", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;

							try
							{
								from.SendGump(new HistoryGump());
							}
							catch (Exception exc)
							{
								Console.WriteLine(exc.ToString());
								Say(00505187); // Tego nie zrobie! Nie da sie.
							}
						}
					}

					#endregion

					#region Tournament

					else if (Regex.IsMatch(e.Speech, "turniej", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;

							if (Regex.IsMatch(e.Speech, "ufund", RegexOptions.IgnoreCase))
							{
								TrnState = TournamentState.Configuration;

								if (TrnState == TournamentState.Configuration)
									from.Prompt = new FoundTournamentPrompt(from, this);
								else
									Say(00505188); // Nie mozesz ufundowac turnieju w tej chwili!
							}
							else if ((int)m_TournamentState < (int)TournamentState.Recrutation)
							{
								Say(00505189); // W najblizszym czasie nie planujemy turnieju.
							}
							else if (Regex.IsMatch(e.Speech, "informacj", RegexOptions.IgnoreCase))
							{
								from.SendGump(new TournamentStatusGump(this));
							}
							else if (Regex.IsMatch(e.Speech, "zglos", RegexOptions.IgnoreCase) &&
							         (m_TournamentState == TournamentState.Recrutation ||
							          (m_TournamentState == TournamentState.InProgress &&
							           (int)TrnStateProgress < (int)TournamentProgress.Fight)))
							{
								BankBox box = e.Mobile.BankBox;

								if (from is PlayerMobile && (from as PlayerMobile).Young)
									this.Say(00505190); // "Jestes za mlody, by wystartowac w turnieju!"
								else if (IsCompetitor(from))
									this.Say(00505191); // "Jestes juz zgloszony jako uczestnik turnieju!"
								else if (box == null || !Withdraw(from, m_TournamentFee))
									this.Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
								else if (box != null)
								{
									if (m_TournamentFee > 0)
										Say(00505192,
											m_TournamentFee
												.ToString()); // Zgloszenie do turnieju kosztuje Cie ~1_GOLD~ centarow. Do zobaczenia w dniu zawodow!
									else
										Say(00505193); // "Zgloszenie przyjete. Do zobaczenia w dniu zawodow!"

									Competitors.Add(new TournamentCompetitor(from));
								}
							}
							else if (m_TournamentState == TournamentState.InProgress &&
							         (!Regex.IsMatch(e.Speech, "potwierdz", RegexOptions.IgnoreCase)))
							{
								Say(00505172); // Turniej wlasnie trwa!
							}
							else if (Regex.IsMatch(e.Speech, "wycof", RegexOptions.IgnoreCase) &&
							         m_TournamentState == TournamentState.Recrutation)
							{
								if (RemoveCompetitor(from))
								{
									if (m_TournamentFee > 0)
									{
										Say(00505194); // Heh! Masz polowe wpisowego!
										Emote(00505195); // *spoglada z pogarda*
									}
									else
										Say(00505196); // "Nie ma sprawy! Wypisuje!"

									Banker.Deposit(from, m_TournamentFee / 2);
								}
								else
								{
									Emote(00505197); // *spoglada na liste uczestnikow*
									Say(00505198); // Heh! Nie ma Cie tu!
								}
							}
							else if (Regex.IsMatch(e.Speech, "potwierdz", RegexOptions.IgnoreCase))
							{
								ConfirmCompetitor(from);
							}
							else if (m_TournamentState == TournamentState.Recrutation)
							{
								Say(00505199); // Wlasnie trwa nabor. Chcesz wiecej informacji?
							}
							else
								this.Yell(00505200); // CO!?
						}
					}

					#endregion

					#region Duel

					else if (Regex.IsMatch(e.Speech, "pojedynek", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;

							if (TrnState == TournamentState.InProgress)
							{
								Say(505200); //Niestety na arenie odbywa sie turniej. Przyjdz pozniej.
							}
							else if (Arena.Busy(true))
							{
								Say(505201); // Niestety arena jest zajeta. Przyjdz pozniej.
							}
							else if (NoDuels)
							{
								Say(505202); // "Niestety nie pozwalamy na pojedynki."
							}
							else if (from is PlayerMobile && (from as PlayerMobile).Young)
								Say(505203); // "Jestes za mlody, bym wpuscil Cie na arene!"
							else if (Regex.IsMatch(e.Speech, "krotki", RegexOptions.IgnoreCase))
							{
								Say(505204); // Wskaz z kim chcesz walczyc.
								from.Target = new OpponentTarget(this, from, FightType.ShortDuel);
							}
							else if (Regex.IsMatch(e.Speech, "dlugi", RegexOptions.IgnoreCase))
							{
								Say(505204);
								from.Target = new OpponentTarget(this, from, FightType.LongDuel);
							}
							else
								Say(505205); // Jak dlugi?
						}
					}

					#endregion

					#region Training

					else if (Regex.IsMatch(e.Speech, "trening", RegexOptions.IgnoreCase))
					{
						if (CheckVendorAccess(from))
						{
							e.Handled = true;

							if (TrnState == TournamentState.InProgress)
							{
								Say(505200); //Niestety na arenie odbywa sie turniej. Przyjdz pozniej.
							}
							else if (Arena.Busy(false))
							{
								Say(505206); // "Niestety na arenie toczy sie pojedynek. Przyjdz pozniej."
							}
							else if (NoTrainings)
							{
								Say(505207); // Nie pozwalamy tu na treningni.
							}
							else if (Regex.IsMatch(e.Speech, "krotki", RegexOptions.IgnoreCase))
							{
								BankBox box = e.Mobile.BankBox;

								if (box == null || !Withdraw(from, PriceTrainingShort))
									this.Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
								else if (box != null)
								{
									string args = String.Format("{0}\t{1}", "Krotki", PriceTrainingShort.ToString());

									this.Say(505208,
										args); // ~1_TYPE~ trening kosztuje Cie ~2_GOLD~ centarow. Mozesz wkroczyc na arene.
									Arena.AddFighter(e.Mobile, FightType.ShortTraining);
								}
							}
							else if (Regex.IsMatch(e.Speech, "dlugi", RegexOptions.IgnoreCase))
							{
								BankBox box = e.Mobile.BankBox;

								if (box == null || !Withdraw(from, PriceTrainingLong))
									this.Say(500384); // Ah, art thou trying to fool me? Thou hast not so much gold!
								else if (box != null)
								{
									string args = String.Format("{0}\t{1}", "Dlugi", PriceTrainingLong.ToString());

									this.Say(505208,
										args); // ~1_TYPE~ trening kosztuje Cie ~2_GOLD~ centarow. Mozesz wkroczyc na arene.
									Arena.AddFighter(e.Mobile, FightType.LongTraining);
								}
							}
							else
								this.Say(505205); // jak dlugi
						}
					}

					#endregion
				}
				else if (!m_Active && !e.Handled && e.Mobile.InRange(this, 3))
				{
					if (Regex.IsMatch(e.Speech, "zasady", RegexOptions.IgnoreCase)
					    || Regex.IsMatch(e.Speech, "pojedynek", RegexOptions.IgnoreCase)
					    || Regex.IsMatch(e.Speech, "trening", RegexOptions.IgnoreCase)
					    || Regex.IsMatch(e.Speech, "turniej", RegexOptions.IgnoreCase))
					{
						e.Handled = true;
						Say(505209); // Przykro mi, ale tak tylko sobie stoje.
					}
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Say(00505167); // To zabolalo, az mi sie we lbie pokrecilo!
				e.Mobile.SendLocalizedMessage(00505180); // Wystapil nieznany blad areny!
			}

			base.OnSpeech(e);
		}

		public bool IsCompetitor(Mobile mob)
		{
			try
			{
				for (int i = 0; i < Competitors.Count; i++)
				{
					Mobile m = (Competitors[i] as TournamentCompetitor).Competitor;

					if (m != null && m == mob)
						return true;
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}

			return false;
		}

		private bool RemoveCompetitor(Mobile m)
		{
			try
			{
				for (int i = 0; i < Competitors.Count; i++)
				{
					if ((Competitors[i] as TournamentCompetitor).Competitor == m)
					{
						Competitors.RemoveAt(i);
						return true;
					}
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}

			return false;
		}

		private void ConfirmCompetitor(Mobile m)
		{
			try
			{
				if (m_TournamentState != TournamentState.InProgress)
					Say(505287); // Co chcesz potwierdzac?! Turniej sie jeszcze nie rozpoczal!
				else if ((int)TrnStateProgress > (int)TournamentProgress.Roostering)
					Say(505288); // Troche za pozno!
				else
				{
					for (int i = 0; i < Competitors.Count; i++)
					{
						Mobile mob = (Competitors[i] as TournamentCompetitor).Competitor;

						if (mob != null && m == mob)
						{
							if ((Competitors[i] as TournamentCompetitor).Confirmed)
							{
								Emote(505289); // *smieje sie*
								Say(505290); // Nie boj sie. Pamietam, ze jestes!
								m.SendLocalizedMessage(505291, "", 38); // Juz potwierdziles udzial w turnieju!
								return;
							}

							(Competitors[i] as TournamentCompetitor).Confirmed = true;
							Emote(505292); // *usmiecha sie*
							Say(505293); // Zaznaczylem, ze juz jestes. Badz w pogotowiu!
							m.SendLocalizedMessage(505294, "", 167); // Potwierdziles udzial w turnieju!
							return;
						}
					}

					Say(505295); //Troche za pozno na regulowanie spraw uczestnictwa!
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}
		}

		private bool IsConfirmed(Mobile m)
		{
			try
			{
				for (int i = 0; i < Competitors.Count; i++)
				{
					Mobile mob = (Competitors[i] as TournamentCompetitor).Competitor;

					if (mob != null && m == mob)
						return (Competitors[i] as TournamentCompetitor).Confirmed;
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}

			return false;
		}

		private void RefundCompetitors()
		{
			try
			{
				for (int i = 0; Competitors != null && i < Competitors.Count; i++)
					Banker.Deposit((Competitors[i] as TournamentCompetitor).Competitor, this.m_TournamentFee);

				Competitors.Clear();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}

			try
			{
				if (m_TournamentClass == TournamentClass.Private && TrnFounder != null)
					Banker.Deposit(TrnFounder, PrivateTournamentCost());
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}
		}

		public string GetTournamentStatus()
		{
			string status = "Kolejny turniej zostal ogloszony!\nArena ";

			try
			{
				#region 1st part ( Header + Rewards )

				status += ArenaName + " zaprasza wszystkich chetnych ";
				status += NelderimDateTime.TransformToString(TournamentStart, NelderimDateTimeFormat.LongWhen);
				status += " do wziecia udzialu w pasjonujacych zawodach";

				if (TrnRewardName != null)
					status += " o trofeum " + TrnRewardName;

				if (TrnReward1st > 0 && TrnRewardName == null)
					status += " o nagrode w wysokosci " + this.TrnReward1st + " sztuk zlota";
				else if (TrnReward1st > 0)
					status += " oraz " + this.TrnReward1st + " sztuk zlota";

				status += ".\n";

				if (TrnFee > 0)
				{
					if (TrnReward1st > 0)
						status += "Ponadto zwyciezca ";
					else
						status += "Zwyciezca ";

					status +=
						" otrzyma polowe, jego pokonany finalowy przeciwnik czwarta czesc, a pokonani w polfinalach ";
					status += " po osmej czesci zebranego wpisowego, ktore wynosi jedyne " + TrnFee + ".\n";
				}

				if (this.TrnReward2nd > 0)
				{
					status += " Drugi zawodnik turnieju otrzyma nagrode w wysokosci " + TrnReward2nd + " centarow.\n";
				}

				if (this.TrnReward3rd > 0)
				{
					status += " Trzeci zawodnicy otrzymaja po " + TrnReward3rd + " centarow.\n";
				}

				status += "\nRekrutacja odbedzie sie na zasadach ";

				switch (m_TournamentClass)
				{
					case TournamentClass.Normal:
						status += "turnieju klasycznego.\n";
						break;
					case TournamentClass.Masters:
						status += "turnieju mistrzow.\n";
						break;
					case TournamentClass.Open:
						status += "turnieju otwartego.\n";
						break;
					case TournamentClass.Private:
						status += "turnieju prywatnego.\n";
						break;
				}

				status += "\n";

				#endregion

				#region Rules

				status += "Zasady turnieju sa jak nastepuje.\n";

				if (TrnFee > 0)
				{
					status += "Wycofanie sie z turnieju na pozniej niz dobe owocuje utrata wpisowego.";
					status += " Wycofanie wczesniejsze pozwoli odzyskac jedynie polowe zaplaconej sumy.\n";
				}

				status += this.GetArenaRules();

				#endregion

				#region 3rd part ( Competitors )

				status += "\nDo tej chwili ";

				if (Competitors.Count < 1)
					status += "niestety jeszcze sie nikt nie zglosil.";
				else if (Competitors.Count == 1)
				{
					Mobile mob = (Competitors[0] as TournamentCompetitor).Competitor;
					status += "niestety zglosil" + ((mob.Female) ? "a " : " ") + "sie tylko " + mob.Name + ".";
				}
				else
				{
					status += "swoj udzial zglosilo " + Competitors.Count + " zawodnikow (w kolejnosci zgloszen): \n\n";

					for (int i = 0; i < Competitors.Count; i++)
						status += (Competitors[i] as TournamentCompetitor).Competitor.Name + "\n";
				}

				status += "\nMinimalna liczba zawodnikow wynosi " + this.TrnCompMinCount;

				#endregion
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				status += "\n\nKrytyczny blad! Zglos go Ekipie!";
			}

			return status;
		}

		public string GetArenaRules()
		{
			string status = "";

			try
			{
				#region Rules

				if (NoEnter)
					status += "Na arenie moga przebywac tylko walczacy.\n";

				if (NoHiding)
					status += "Proba ukrycia podczas walki skonczy sie przegrana.\n";

				if (NoMounts)
					status += "Zakazane jest dosiadanie wierzchowcow.\n";

				if (NoAlchemy)
					status += "Na arenie nie mozna stosowac mikstur alchemicznych.\n";

				if (NoHealing)
					status += "Na arenie nie mozna leczyc bandazami.\n";

				if (NoChivalry || NoMagery || NoNecro || NoDruidism)
				{
					if (NoChivalry && NoMagery && NoNecro && NoDruidism)
						status += "Na arenie zakazane jest stosowanie jakiejkowiek magii. ";
					else
					{
						status += "Na arenie zakazane sa zaklecia transportujace na duze odleglosci. ";

						if (NoMagery)
							status += "Zakazane sa takze pozostale zaklecia magii podstawowej. ";

						if (NoNecro)
							status += "Zakaza jest nekromancja. ";

						if (NoChivalry)
							status += "Zakaze sa czary paladynskie. ";

						// TODO: odkomentowac po wprowadzeniu druidyzmu
						/*
						if ( NoDruidism )
							rules += "Zakaza jest nekromancja. ";
						*/
					}

					status += "\n\n";
				}
				else
					status += "Na arenie mozna czarowac z wyjatkiem zaklec przenoszacych na duze odleglosci.\n";

				if (NoControls || NoFamiliars || NoSummons || NoControlledSummons)
				{
					if (NoControls && NoFamiliars && NoSummons && NoControlledSummons)
						status += "Zakazane jest wprowadzanie jakichkolwiek istot. ";
					else
					{
						if (NoControls)
							status += "Zakazane jest wprowadzanie istot oswojonych. ";

						if (NoFamiliars)
							status += "Zakazane jest wprowadzanie istot nekromanckich. ";

						if (this.NoControlledSummons)
							status += "Zakazane jest wprowadzanie przywolancow. ";

						if (NoSummons)
							status += "Zakazane jest wypuszczanie agresywnych istot niekontrolowalnych. ";
					}

					status += "\n";
				}
				else
					status += "Na arene mozna wprowadzac wszelkie istoty kontrolowane.\n";

				#endregion
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				status += "\n\nKrytyczny blad! Zglos go Ekipie!";
			}

			return status;
		}

		public void PassTournamentFightWinner(Mobile m)
		{
			if (this.Tournament != null)
				Tournament.WonTheFight(m);
		}

		public void FinishTournament()
		{
			m_TournamentState = TournamentState.Finished;
		}

		public int PrivateTournamentCost()
		{
			return TrnReward1st + 16000 +
			       ((TrnCompMinCount < 16) ? 0 : (TrnCompMinCount > 16) ? 28000 : 12000);
		}

		#region Activation & Deactivation

		public void RestartArena()
		{
			try
			{
				if (DeactivateArena())
					ActivateArena();
				else
					Active = false;
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Active = false;
			}
		}

		private bool ActivateArena()
		{
			try
			{
				if (m_FirstCorner == Point3D.Zero || m_SecondCorner == Point3D.Zero
				                                  || m_EndOfFightPoint == Point3D.Zero || m_ExtortPoint == Point3D.Zero)
				{
					Say(00505164); // Nie mam wskazanych wszystkich waznych koordynat!
					return false;
				}

				if (m_FirstCorner == m_SecondCorner)
				{
					Say(00505165); //Arena nie moze miec rozmiaru jednej tarczy!
					return false;
				}

				if (CornerRed == Point3D.Zero)
				{
					CornerRed = (m_FirstCorner.X <= m_SecondCorner.X) ? m_FirstCorner : m_SecondCorner;
				}

				if (CornerBlue == Point3D.Zero)
				{
					CornerBlue = (m_FirstCorner.X <= m_SecondCorner.X) ? m_SecondCorner : m_FirstCorner;
				}

				#region Walidacja punktow

				int Xmin = Math.Min(m_FirstCorner.X, m_SecondCorner.X);
				int Ymin = Math.Min(m_FirstCorner.Y, m_SecondCorner.Y);
				int Zmin = Math.Min(m_FirstCorner.Z, m_SecondCorner.Z);
				int Xmax = Math.Max(m_FirstCorner.X, m_SecondCorner.X);
				int Ymax = Math.Max(m_FirstCorner.Y, m_SecondCorner.Y);
				int Zmax = Math.Max(m_FirstCorner.Z, m_SecondCorner.Z) + 1;

				m_FirstCorner = new Point3D(Xmin, Ymin, Zmin);
				m_SecondCorner = new Point3D(Xmax, Ymax, Zmax);

				#endregion

				Rectangle3D rect = new Rectangle3D(m_FirstCorner, m_SecondCorner);

				Arena = new NArenaRegion("arena", this.Map, new[] { rect }, this);

				if (Arena.Contains(m_EndOfFightPoint) || Arena.Contains(m_ExtortPoint)
				                                      || !Arena.Contains(CornerBlue) || !Arena.Contains(CornerRed))
				{
					Say(00505166); // "Nalezy poprawnie wskazac punkty areny!"
					Arena = null;
					return false;
				}

				Arena.Register();

				#region Report

				/*
					Console.WriteLine( "*** Raport Areny ***" );
					Console.WriteLine( "ArenaName = {0}", ArenaName );
					Console.WriteLine( "Corner1st = ({0},{1},{2})", Corner1st.X, Corner1st.Y, Corner1st.Z );
					Console.WriteLine( "Corner2nd = ({0},{1},{2})", Corner2nd.X, Corner2nd.Y, Corner2nd.Z );
					Console.WriteLine( "CornerBlue = ({0},{1},{2})", CornerBlue.X, CornerBlue.Y, CornerBlue.Z );
					Console.WriteLine( "CornerRed = ({0},{1},{2})", CornerRed.X, CornerRed.Y, CornerRed.Z );
					Console.WriteLine( "EndOfFightPoint = ({0},{1},{2})", EndOfFightPoint.X, EndOfFightPoint.Y, EndOfFightPoint.Z );
					Console.WriteLine( "ExtortPoint = ({0},{1},{2})", ExtortPoint.X, ExtortPoint.Y, ExtortPoint.Z );
					Console.WriteLine( "NoAlchemy = {0}", NoAlchemy );
					Console.WriteLine( "NoChivalry = {0}", NoChivalry );
					Console.WriteLine( "NoControlledSummons = {0}", NoControlledSummons );
					Console.WriteLine( "NoControls = {0}", NoControls );
					Console.WriteLine( "NoDruidism = {0}", NoDruidism);
					Console.WriteLine( "NoDuels = {0}", NoDuels);
					Console.WriteLine( "NoEnter = {0}", NoEnter );
					Console.WriteLine( "NoFamiliars = {0}", NoFamiliars );
					Console.WriteLine( "NoHealing = {0}", NoHealing );
					Console.WriteLine( "NoHiding = {0}", NoHiding );
					Console.WriteLine( "NoMagery = {0}", NoMagery);
					Console.WriteLine( "NoMounts = {0}", NoMounts );
					Console.WriteLine( "NoNecro = {0}", NoNecro );
					Console.WriteLine( "NoSummons = {0}", NoSummons );
					Console.WriteLine( "NoTrainings = {0}", NoTrainings );
					Console.WriteLine( "PriceBase = {0}", PriceBase);
					*/

				#endregion
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Say(00505167); // "To zabolalo, az mi sie we lbie pokrecilo!"
				return false;
			}

			Say(00505168); // Do roboty!

			return true;
		}

		private bool DeactivateArena()
		{
			try
			{
				if (Tournament != null)
					Tournament.Stop(TournamentEndReason.ArenaDeactivation);
				else if (m_TournamentState == TournamentState.Recrutation)
				{
					RefundCompetitors();
					m_TournamentState = TournamentState.None;
				}

				Arena.CloseBussiness();
				Arena.Unregister();
				Arena = null;
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Say(00505167);
				return false;
			}

			Say(00505169); // "Nie bede pracowal! A co? Poleze!"

			return true;
		}

		#endregion

		#region Static Bank Functions

		public static int GetBalance(Mobile from)
		{
			Backpack b = from.Backpack as Backpack;

			if (b != null)
				return b.GetAmount(typeof(Gold)) + Banker.GetBalance(from);

			return Banker.GetBalance(from);
		}

		public static bool Withdraw(Mobile from, int amount)
		{
			int balance = GetBalance(from);
			int oldamount = amount;

			if (balance < amount)
				return false;

			Backpack b = from.Backpack as Backpack;

			if (b != null)
			{
				int backpackgold = from.Backpack.GetAmount(typeof(Gold));

				if (backpackgold < amount && backpackgold > 0)
				{
					if (from.Backpack.ConsumeTotal(typeof(Gold), backpackgold))
						amount -= backpackgold;
				}
				else if (from.Backpack.ConsumeTotal(typeof(Gold), amount))
					return true;
			}

			if (!Banker.Withdraw(from, amount))
			{
				Banker.Deposit(from, oldamount - amount);
				return false;
			}

			return true;
		}

		#endregion
	}
}
