#region References

using System;
using System.Collections;
using Server.Gumps;
using Server.Mobiles;

#endregion

namespace Server.Engines.Tournament
{
	public enum EOFReason
	{
		DefaultReason = 0,
		Exit = 1,
		ForceRestart = 2,
		EndOfTime = 3,
		Defeat = 4,
		Victory = 5
	}

	public enum FightType
	{
		None = 0,
		ShortDuel = 2,
		LongDuel = 6,
		Tournament = 12,
		ShortTraining = 20,
		LongTraining = 60
	}

	public enum TournamentState
	{
		None = 0,
		Configuration = 1,
		Initialization = 2,
		Recrutation = 3,
		Start = 4,
		InProgress = 5,
		Finished = 6
	}

	public enum TournamentProgress
	{
		None = 0,
		Start = 1,
		FirstCall = 2,
		SecondCall = 3,
		ThirdCall = 4,
		Roostering = 5,
		Fight = 6,
		End = 7,
		Finished = 8
	}

	public enum TournamentEndReason
	{
		Unfinished,
		Finished,
		Exception,
		NoFighters,
		ArenaDeactivation
	}

	public enum TournamentPushFight
	{
		None = 0,
		Restart = 1,
		BlueWon = 2,
		RedWon = 3
	}

	public enum TournamentIntervals
	{
		VeryShort = 15, //15,
		Short = 30, //30,
		HalfTimeUnit = 60, //60,
		TimeUnit = 120, //120,
		DoubleTimeUnit = 240, //240
		Day = 3600, // 3600,
		Month = 86400 // 86400
	}

	public enum TournamentClass
	{
		Open = 0,
		Normal = 1,
		Masters = 2,
		Private = 3
	}

	public class TournamentCompetitor
	{
		private Serial m_Serial;

		public Mobile Competitor
		{
			get
			{
				return World.FindMobile(m_Serial);
			}
			set
			{
				if (value == null)
					m_Serial = new Serial();
				else
					m_Serial = value.Serial;
			}
		}

		public bool Confirmed { get; set; }

		public TournamentCompetitor(Mobile m)
		{
			if (m == null)
				m_Serial = new Serial();
			else
				m_Serial = m.Serial;

			Confirmed = false;
		}

		public TournamentCompetitor()
		{
			m_Serial = new Serial();
			Confirmed = false;
		}

		public TournamentCompetitor(GenericReader reader)
		{
			int version = reader.ReadInt();

			if (version == 1)
				Confirmed = reader.ReadBool();
			else
				Confirmed = false;

			m_Serial = reader.ReadSerial();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1); // version
			writer.Write(Confirmed);
			writer.Write(m_Serial.Value);
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			if (version == 1)
				Confirmed = reader.ReadBool();
			else
				Confirmed = false;

			m_Serial = reader.ReadSerial();
		}
	}

	public class TournamentFight : IComparer
	{
		private bool m_Finished;

		public int Round { get; set; }

		public int Fight { get; set; }

		public int Number { get; set; }

		public bool Finished
		{
			get { return m_Finished; }
			set
			{
				m_Finished = value;

				if (m_Finished)
					Date = DateTime.Now;
			}
		}

		public bool BlueIsWinner { get; set; }

		public Mobile Blue { get; set; }

		public Mobile Red { get; set; }

		public DateTime Date { get; set; }


		public TournamentFight(Mobile blue, Mobile red, int round, int fight, int nr)
		{
			Blue = blue;
			Red = red;
			Round = round;
			Fight = fight;
			Number = nr;
			BlueIsWinner = true;
			m_Finished = false;
			Date = DateTime.Now;
		}

		public TournamentFight(Mobile blue, int round, int fight, int nr)
		{
			Blue = blue;
			Red = null;
			Round = round;
			Fight = fight;
			Number = nr;
			BlueIsWinner = true;
			m_Finished = false;
			Date = DateTime.Now;
		}

		public TournamentFight(int round, int fight, int nr)
		{
			Blue = null;
			Red = null;
			Round = round;
			Fight = fight;
			Number = nr;
			BlueIsWinner = true;
			m_Finished = false;
			Date = DateTime.Now;
		}

		public TournamentFight()
		{
			Blue = null;
			Red = null;
			Round = 0;
			Fight = 0;
			Number = 0;
			BlueIsWinner = true;
			m_Finished = false;
			Date = DateTime.Now;
		}

		public TournamentFight(GenericReader reader)
		{
			int version = reader.ReadInt();

			if (version == 1)
				Date = reader.ReadDateTime();
			else
				Date = DateTime.Now;

			m_Finished = reader.ReadBool();
			BlueIsWinner = reader.ReadBool();
			Round = reader.ReadInt();
			Fight = reader.ReadInt();
			Number = reader.ReadInt();

			bool test = reader.ReadBool();

			if (test)
			{
				Serial serial = reader.ReadSerial();
				Blue = World.FindMobile(serial);
			}
			else
				Blue = null;

			test = reader.ReadBool();

			if (test)
			{
				Serial serial = reader.ReadSerial();
				Red = World.FindMobile(serial);
			}
			else
				Red = null;
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1); // version
			writer.Write(Date);
			writer.Write(m_Finished);
			writer.Write(BlueIsWinner);
			writer.Write(Round);
			writer.Write(Fight);
			writer.Write(Number);

			if (Blue != null)
			{
				writer.Write(true);
				writer.Write(Blue.Serial.Value);
			}
			else
				writer.Write(false);

			if (Red != null)
			{
				writer.Write(true);
				writer.Write(Red.Serial.Value);
			}
			else
				writer.Write(false);
		}

		public override string ToString()
		{
			string nr = "#" + Fight;
			string blue = (Blue == null) ? "brak" : Blue.Name;
			string red = (Red == null) ? "brak" : Red.Name;


			if (m_Finished && BlueIsWinner)
				return nr + "   " + blue + " *V* vs " + red + "\n";
			if (m_Finished && !BlueIsWinner)
				return nr + "   " + blue + " vs *V* " + red + "\n";
			return nr + "   " + blue + " vs " + red + " (nierozstrzygniety)\n";
		}

		public bool AddFighter(Mobile m)
		{
			if (Blue != null && Red != null)
				return false;

			if (Blue == null)
				Blue = m;
			else
				Red = m;

			BlueIsWinner = true;
			m_Finished = false;

			return true;
		}

		public int Compare(object left, object right)
		{
			TournamentFight l = left as TournamentFight;
			TournamentFight r = right as TournamentFight;

			if ((l == null) && (r == null))
				return 0;

			if (l == null)
				return -1;

			if (r == null)
				return 1;

			if (l.Round > r.Round)
				return 1;
			if (l.Round < r.Round)
				return -1;

			if (l.Fight > r.Fight)
				return 1;
			if (l.Fight < r.Fight)
				return -1;

			if (l.Number > r.Number)
				return 1;
			if (l.Number < r.Number)
				return -1;

			return 0;
		}
	}

	public class Tournament
	{
		#region Subclasses

		private class TournamentTimer : Timer
		{
			private readonly Tournament m_Owner;

			public TournamentTimer(Tournament owner, int span) : this(owner, TimeSpan.FromSeconds(span))
			{
			}

			public TournamentTimer(Tournament owner, TimeSpan span) : base(span)
			{
				Priority = TimerPriority.TwoFiftyMS;
				m_Owner = owner;
			}

			protected override void OnTick()
			{
				try
				{
					m_Owner.DoAction();
				}
				catch (Exception exc)
				{
					Console.WriteLine("Turniej: [timer] nie moglem wykonac akcji!");
					Console.WriteLine(exc.ToString());
				}
			}
		}

		private class TournamentCountdownTimer : Timer
		{
			private readonly Mobile m_Owner;
			private int m_Count;

			public TournamentCountdownTimer(Mobile owner, int delay) : base(
				TimeSpan.FromSeconds((delay > 0) ? delay : 2), TimeSpan.FromSeconds(2), 11)
			{
				Priority = TimerPriority.FiftyMS;
				m_Owner = owner;
				m_Count = 10;
			}

			protected override void OnTick()
			{
				try
				{
					m_Owner.Say(505220 - m_Count);
					m_Count--;
				}
				catch (Exception exc)
				{
					Console.WriteLine("Turniej: [count] nie moglem wykonac akcji!");
					Console.WriteLine(exc.ToString());
				}
			}
		}

		private class TournamentStatusGump : Gump
		{
			public TournamentStatusGump(Tournament owner) : base(140, 80)
			{
				string title = "Status turnieju";
				string info = "Informacje o statusie nie mogly byc uzyskane!";

				try
				{
					info = owner.GetStatus();
				}
				catch (Exception exc)
				{
					Console.WriteLine(info);
					Console.WriteLine(exc.ToString());
				}

				AddPage(0);
				AddImage(0, 0, 1228);
				AddImage(340, 255, 9005);

				AddHtml(71, 6, 248, 18, "<div align=\"center\" color=\"2100\">" + title + "</div>", false, false);

				AddHtml(28, 33, 350, 218, "<BASEFONT COLOR=\"black\">" + info + "</BASEFONT>", false, true);
			}
		}

		#endregion

		#region Fields

		private TimeSpan m_TimeToGo;
		private readonly ArrayList m_Competitors;
		private readonly ArrayList m_Fights;
		private int m_Reward;
		private int m_CurrentFightNr;
		private int m_NextFightNr;
		private TournamentFight m_CurrentFight;
		private TournamentFight m_NextFight;
		private readonly int[][] m_MooreTable;

		#endregion

		#region Properites

		public TournamentEndReason EndReason { get; private set; }
		public int HalfQuarterReward { get { return (m_Reward - (m_Reward % 8)) / 8; } }
		public TournamentProgress Progress { get; private set; }
		public ArenaTrainer Owner { get; }

		public DateTime EndTime
		{
			get
			{
				if (Progress != TournamentProgress.Finished || EndReason != TournamentEndReason.Finished)
					return DateTime.Now;

				return (m_Fights[m_Fights.Count - 1] as TournamentFight).Date;
			}
		}

		public int Fights { get { return m_Fights.Count; } }

		#endregion

		#region Constructors & Serialization

		public Tournament(ArenaTrainer owner)
		{
			Owner = owner;
			Progress = TournamentProgress.Start;
			EndReason = TournamentEndReason.Unfinished;
			m_Competitors = new ArrayList();
			m_Fights = new ArrayList();
			m_Reward = 0;
			m_CurrentFightNr = -1;
			m_NextFightNr = -1;
			m_CurrentFight = null;
			m_NextFight = null;
			m_TimeToGo = TimeSpan.Zero;

			#region Tablica Moore'a

			m_MooreTable = new int[5][]
			{
				new int [64]
				{
					1, 64, 33, 32, 17, 48, 49, 16, 9, 56, 41, 24, 25, 40, 57, 8, 5, 60, 37, 28, 21, 44, 12, 53,
					52, 13, 45, 20, 29, 36, 61, 4, 3, 62, 35, 30, 19, 46, 51, 14, 11, 54, 43, 22, 27, 38, 59, 6,
					7, 58, 39, 26, 23, 42, 55, 10, 15, 50, 47, 18, 31, 34, 63, 2
				},
				new int [32]
				{
					1, 32, 17, 16, 9, 24, 25, 8, 5, 28, 21, 12, 13, 20, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 7,
					26, 23, 10, 15, 18, 31, 2
				},
				new int [16] { 1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2 },
				new int [8] { 1, 8, 5, 4, 3, 6, 7, 2 }, new int [4] { 1, 4, 3, 2 }
			};

			#endregion

			new TournamentTimer(this, TimeSpan.FromSeconds((int)TournamentIntervals.VeryShort)).Start();
		}

		public Tournament(ArenaTrainer owner, GenericReader reader)
		{
			Owner = owner;

			#region Tablica Moore'a

			m_MooreTable = new int[5][]
			{
				new int [64]
				{
					1, 64, 33, 32, 17, 48, 49, 16, 9, 56, 41, 24, 25, 40, 57, 8, 5, 60, 37, 28, 21, 44, 12, 53,
					52, 13, 45, 20, 29, 36, 61, 4, 3, 62, 35, 30, 19, 46, 51, 14, 11, 54, 43, 22, 27, 38, 59, 6,
					7, 58, 39, 26, 23, 42, 55, 10, 15, 50, 47, 18, 31, 34, 63, 2
				},
				new int [32]
				{
					1, 32, 17, 16, 9, 24, 25, 8, 5, 28, 21, 12, 13, 20, 29, 4, 3, 30, 19, 14, 11, 22, 27, 6, 7,
					26, 23, 10, 15, 18, 31, 2
				},
				new int [16] { 1, 16, 9, 8, 5, 12, 13, 4, 3, 14, 11, 6, 7, 10, 15, 2 },
				new int [8] { 1, 8, 5, 4, 3, 6, 7, 2 }, new int [4] { 1, 4, 3, 2 }
			};

			#endregion


			int version = reader.ReadInt();

			switch (version)
			{
				#region Case 1

				case 1:
				{
					m_CurrentFightNr = reader.ReadInt();
					m_NextFightNr = reader.ReadInt();
					m_Reward = reader.ReadInt();
					m_TimeToGo = reader.ReadTimeSpan();
					m_Competitors = new ArrayList();
					m_Fights = new ArrayList();

					int cnt = reader.ReadInt();

					if (cnt > 0)
					{
						for (int i = 0; i < cnt; i++)
							m_Competitors.Add(new TournamentCompetitor(reader));
					}

					cnt = reader.ReadInt();

					if (cnt > 0)
					{
						for (int i = 0; i < cnt; i++)
							m_Fights.Add(new TournamentFight(reader));
					}

					goto case 0;
				}

				#endregion

				#region Case 0

				case 0:
				{
					Progress = (TournamentProgress)reader.ReadInt();
					EndReason = (TournamentEndReason)reader.ReadInt();
					break;
					// goto case X;
				}

				#endregion
			}

			#region Przywracamy turniej

			if (version > 0 && m_CurrentFightNr > -1)
			{
				m_CurrentFight = m_Fights[m_CurrentFightNr] as TournamentFight;

				if (m_NextFightNr > -1)
					m_NextFight = m_Fights[m_NextFightNr] as TournamentFight;
			}

			if (version < 1)
				m_TimeToGo = TimeSpan.FromMinutes(2);

			#endregion

			new TournamentTimer(this, TimeSpan.FromSeconds((int)TournamentIntervals.DoubleTimeUnit)).Start();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(1); // version

			#region version 1

			writer.Write(m_CurrentFightNr);
			writer.Write(m_NextFightNr);
			writer.Write(m_Reward);
			writer.Write(m_TimeToGo);

			writer.Write(m_Competitors.Count);

			for (int i = 0; i < m_Competitors.Count; i++)
				(m_Competitors[i] as TournamentCompetitor).Serialize(writer);

			writer.Write(m_Fights.Count);

			for (int i = 0; i < m_Fights.Count; i++)
				(m_Fights[i] as TournamentFight).Serialize(writer);

			#endregion

			#region version 0

			writer.Write((int)Progress);
			writer.Write((int)EndReason);

			#endregion
		}

		#endregion

		#region Progress

		public void DoAction()
		{
			switch (Progress)
			{
				case TournamentProgress.Start:
				{
					if (!Initialize())
						Stop();

					break;
				}
				case TournamentProgress.FirstCall:
				case TournamentProgress.SecondCall:
				case TournamentProgress.ThirdCall:
				{
					if (!Call())
						Stop();

					break;
				}
				case TournamentProgress.Roostering:
				{
					if (!Rooster())
						Stop();

					break;
				}
				case TournamentProgress.Fight:
				{
					if (!Fight())
						Stop();

					break;
				}
				case TournamentProgress.End:
				{
					if (!Final())
						Stop();

					break;
				}
			}
		}

		private bool Initialize()
		{
			Console.WriteLine("Turniej: [init] inicjalizacja turnieju na arenie {0}.", Owner.ArenaName);

			try
			{
				if (Owner.TrnCompCount < Owner.TrnCompMinCount)
				{
					Owner.Say(505221); // "Nie ma chetnych? Nie ma turnieju!"
					Stop(TournamentEndReason.NoFighters);
					Console.WriteLine(
						"Turniej: [init] odwolany z powodu braku odpowiedniej liczby uczestnikow ({0}<{1}).",
						Owner.TrnCompCount, Owner.TrnCompMinCount);
				}
				else
				{
					Owner.Yell(505222); // UWAGA! UWAGA!
					Owner.Say(505223); // Rozpoczynamy faze wstepna turnieju!

					#region Obliczamy czas do rozpoczecia

					m_TimeToGo = (Owner.TournamentStart > DateTime.Now)
						? Owner.TournamentStart - DateTime.Now
						: TimeSpan.Zero;

					m_TimeToGo = (m_TimeToGo.Minutes > 6) ? m_TimeToGo : TimeSpan.FromMinutes(6);

					#endregion

					Progress = TournamentProgress.FirstCall;
				}

				new TournamentTimer(this, (int)TournamentIntervals.VeryShort).Start();
			}
			catch (Exception exc)
			{
				Owner.Say(505224); // Fatalnie! Siostra rodzi! Musze uciekac! Koniec turnieju!
				Console.WriteLine(exc.ToString());
				return false;
			}

			return true;
		}

		private bool Call()
		{
			try
			{
				string nr = (Progress == TournamentProgress.FirstCall) ? "pierwsze" :
					(Progress == TournamentProgress.SecondCall) ? "drugie" : "trzecie";

				long timeToGo = (m_TimeToGo.Ticks - (m_TimeToGo.Ticks % 3)) / 3;

				new TournamentTimer(this, TimeSpan.FromTicks(timeToGo)).Start();

				timeToGo = (Progress == TournamentProgress.FirstCall) ? timeToGo * 3 :
					(Progress == TournamentProgress.SecondCall) ? timeToGo * 2 : timeToGo;

				Owner.Yell(505222); // UWAGA! UWAGA!
				Owner.Say(505228, nr); // To jest ~1_NUMBER~ wezwanie do uczestnikow turnieju!
				Owner.Say(505229); // "Prosze o potwierdzenie przybycia!"
				Owner.Say(505230); // Ostatnia chwila na zglaszanie uczestnictwa!

				Console.WriteLine("Turniej: [call] {0} wezwanie.", nr);

				#region Rozsylamy ogloszenia do uczestnikow

				try
				{
					for (int i = 0; i < Owner.TrnCompCount; i++)
					{
						Mobile c = (Owner.Competitors[i] as TournamentCompetitor).Competitor;

						c.SendLocalizedMessage(505231, nr,
							167); // To jest ~1_NUMBER~ wezwanie do potwierdzenia uczestnictwa w turnieju!
						c.SendLocalizedMessage(505232,
							String.Format("{0}\t{1}", TimeSpan.FromTicks(timeToGo).Minutes, Owner.ArenaName),
							167); // Turniej rozpocznie sie za ~1_TIME~ na arenie ~2_ARENA~.
					}
				}
				catch (Exception exc)
				{
					Console.WriteLine("Turniej: [call] blad przy publikacji czasu startu!");
					Console.WriteLine(exc.ToString());
				}

				#endregion

				Progress++;
			}
			catch (Exception exc)
			{
				Owner.Say(505224); // Fatalnie! Siostra rodzi! Musze uciekac! Koniec turnieju!
				Console.WriteLine(exc.ToString());
				return false;
			}

			return true;
		}

		private bool Rooster()
		{
			try
			{
				#region Czyscimy liste zgloszonych z nieobecnych ich kase dodajac do nagrody

				Console.WriteLine("Turniej: [rooster] usuwamy niepotwierdzonych zawodnikow:");


				for (int i = Owner.Competitors.Count - 1; i >= 0; i--)
				{
					TournamentCompetitor tc = Owner.Competitors[i] as TournamentCompetitor;

					if (tc == null || tc.Competitor == null)
					{
						Console.WriteLine("Turniej: [rooster] bledny rekord uczestnika turnieju [usuwam]");
						Owner.Competitors.RemoveAt(i);
						m_Reward += Owner.TrnFee;
					}

					if (!tc.Confirmed)
					{
						Owner.Competitors.RemoveAt(i);
						m_Reward += Owner.TrnFee;
						Console.WriteLine("Turniej: [rooster] {0}", tc.Competitor.Name);
					}
				}

				#endregion

				#region Tworzymy liste dopuszczonych do walki

				bool classify = (Owner.TrnClass == TournamentClass.Masters || Owner.TrnClass == TournamentClass.Open);

				int nr = Owner.Competitors.Count;

				Console.WriteLine("Turniej: [rooster] tworzymy liste dopuszczonych do walki ({0}):", nr);

				if (Owner.TrnClass == TournamentClass.Private)
					nr = (nr < Owner.TrnCompMinCount) ? 0 : Owner.TrnCompMinCount;
				else if (Owner.TrnClass == TournamentClass.Open)
					nr = (nr <= 4) ? 0 : (nr < 8) ? 8 : (nr < 16) ? 16 : (nr < 32) ? 32 : (nr < 64) ? 64 : 128;
				else
				{
					nr = (nr < 4) ? 0 :
						(nr < 8) ? 4 :
						(nr < 16) ? 8 :
						(nr < 32) ? 16 :
						(nr < 64) ? 32 :
						(nr < 128) ? 64 : 128;
					nr = (nr < Owner.TrnCompMinCount) ? 0 : nr;
				}

				m_Competitors.Clear();

				if (nr > 0 && classify)
					TournamentStatistics.Clasify(Owner.Competitors);

				for (int i = 0; i < Owner.Competitors.Count && i < nr; i++)
				{
					TournamentCompetitor tc = Owner.Competitors[i] as TournamentCompetitor;

					Console.WriteLine("Turniej: [rooster] #{1} {0}", tc.Competitor.Name, i + 1);
					m_Competitors.Add(new TournamentCompetitor(tc.Competitor));
					tc.Confirmed = false;
					TournamentStatistics.UpdateRank(tc.Competitor, 1, Fights, false, Owner.TrnClass);
				}

				#endregion

				#region Oddajemy kase niedopuszczonym

				Console.WriteLine("Turniej: [rooster] oddajemy wpisowe niedopuszczonym:");

				for (int i = 0; i < Owner.Competitors.Count; i++)
				{
					TournamentCompetitor tc = Owner.Competitors[i] as TournamentCompetitor;

					if (tc.Confirmed)
					{
						Console.WriteLine("Turniej: [rooster] #{1} {0}", tc.Competitor.Name, i + 1);
						Banker.Deposit(tc.Competitor, Owner.TrnFee);
					}
				}

				Owner.Competitors.Clear();

				#endregion

				#region Generujemy liste walki

				Console.WriteLine("Turniej: [rooster] generujemy drzewo walk.");

				int fights = m_Competitors.Count;
				int round = 1;
				int number = 1;
				ArrayList toAdd = (ArrayList)m_Competitors.Clone();

				fights = (fights < 4) ? 0 :
					(fights == 4) ? 2 :
					(fights <= 8) ? 4 :
					(fights <= 16) ? 8 :
					(fights <= 32) ? 16 :
					(fights <= 64) ? 32 : 64;

				m_Fights.Clear();

				do
				{
					for (int i = 0; i < fights; i++)
					{
						m_Fights.Add(new TournamentFight(round, i + 1, number));
						number++;
					}

					if (round == 1)
					{
						#region Losowanie niebieskich

						for (int i = 0; i < fights; i++)
						{
							int index = 0;

							if (!classify)
								index = Utility.Random(toAdd.Count - 1);
							else
							{
								TournamentFight tf = m_Fights[i] as TournamentFight;

								tf.Fight = GetMooreIndex(i, fights);
								tf.Number = tf.Fight;
							}

							Mobile mob = (toAdd[index] as TournamentCompetitor).Competitor;

							Console.WriteLine(
								"Turniej: [rooster] losowanie zawodnika niebieskiego dla walki [{2}+{1}] - {0}",
								mob.Name, i, index);

							(m_Fights[i] as TournamentFight).AddFighter(mob);
							toAdd.RemoveAt(index);
							m_Reward += Owner.TrnFee;
						}

						#endregion

						#region Losowanie czerwonych

						int cnt = toAdd.Count;

						for (int i = 0; i < cnt; i++)
						{
							int index = 0;

							if (!classify)
								index = Utility.Random(toAdd.Count - 1);

							Mobile mob = (toAdd[index] as TournamentCompetitor).Competitor;

							int ibis = m_Fights.Count - (1 + i);

							Console.WriteLine(
								"Turniej: [rooster] losowanie zawodnika czerwonego dla walki [{2}+{1}] - {0}", mob.Name,
								ibis, index);

							(m_Fights[ibis] as TournamentFight).AddFighter(mob);
							toAdd.RemoveAt(index);
							m_Reward += Owner.TrnFee;
						}

						#endregion

						if (classify)
							m_Fights.Sort(new TournamentFight());
					}

					fights /= 2;
					round++;
				} while (fights >= 1);

				#endregion

				#region Znajdujemy pierwsza i kolejna walke + sprawdzamy czy wogole sa jakies

				if (m_Fights.Count >= 1)
				{
					m_CurrentFightNr = 0;
					m_CurrentFight = m_Fights[m_CurrentFightNr] as TournamentFight;

					if (m_Fights.Count > 1)
					{
						m_NextFightNr = 1;
						m_NextFight = m_Fights[m_NextFightNr] as TournamentFight;
					}
				}
				else
				{
					Owner.Say(505221);
					Console.WriteLine("Turniej: [rooster] brak walk!");
					Stop(TournamentEndReason.NoFighters);
					return true;
				}

				#endregion

				Progress++;

				bool isFight = (m_CurrentFight.Red != null && m_CurrentFight.Blue != null);

				Owner.Say(505233); // "Walki zostaly rozpisane!"

				if (isFight)
				{
					// "W pierwszej ~1_NAME~ zmierzy sie z ~2_NAME~"
					Owner.Say(505234,
						String.Format("{0}\t{1}", m_CurrentFight.Blue.Name,
							(m_CurrentFight.Red != null) ? m_CurrentFight.Red.Name : "samym soba"));
					SendInfo(m_CurrentFight);
				}

				Owner.Say(505235); // Dwie klepsydry na przygotowania!

				try
				{
					for (int i = 0; i < m_Competitors.Count; i++)
					{
						Mobile mob = (m_Competitors[i] as TournamentCompetitor).Competitor;

						mob.CloseGump(typeof(TournamentStatusGump));
						mob.SendGump(new TournamentStatusGump(this));
					}
				}
				catch (Exception exc)
				{
					Console.WriteLine("Turniej: [rooster] blad przy rozsylaniu rozkladu walk!");
					Console.WriteLine(exc.ToString());
				}

				new TournamentTimer(this, (int)TournamentIntervals.DoubleTimeUnit).Start();
				if (isFight)
					new TournamentCountdownTimer(Owner, (int)TournamentIntervals.DoubleTimeUnit - 24).Start();
			}
			catch (Exception exc)
			{
				Owner.Say(505224); // Fatalnie! Siostra rodzi! Musze uciekac! Koniec turnieju!
				Console.WriteLine(exc.ToString());
				return false;
			}

			return true;
		}

		private bool Fight()
		{
			try
			{
				#region Jest walka!

				if (m_CurrentFight != null)
				{
					// Runda ~1_NUMBER~, walka ~2_NUMBER~!"
					Owner.Say(505236, String.Format("{0}\t{1}", m_CurrentFight.Round, m_CurrentFight.Fight));
					Console.WriteLine("Turniej: [fight] [{0}-{1}], {2}", m_CurrentFight.Round, m_CurrentFight.Fight,
						m_CurrentFight);

					#region Sprawdzamy czy walczacy sa w poblizu

					if (m_CurrentFight.Blue != null && m_CurrentFight.Blue.Alive &&
					    m_CurrentFight.Blue.NetState != null && Owner.GetDistanceToSqrt(m_CurrentFight.Blue) < 15)
						Owner.Say(505237, m_CurrentFight.Blue.Name); // Niebieski naroznik ~1_NAME~
					else
						m_CurrentFight.Blue = null;

					if (m_CurrentFight.Red != null && m_CurrentFight.Red.Alive && m_CurrentFight.Red.NetState != null &&
					    Owner.GetDistanceToSqrt(m_CurrentFight.Red) < 15)
						Owner.Say(505238, m_CurrentFight.Red.Name); // // Czerwony naroznik ~1_NAME~
					else
						m_CurrentFight.Red = null;

					Owner.Arena.CleanArena(505239); // Prosze opuscic arene!

					#endregion

					#region Jesli sie da konczymy pojedynek

					if (m_CurrentFight.Blue == null && m_CurrentFight.Red == null)
					{
						Owner.Say(505240); // "Walka odwolana z powodu braku walczacych!"
						m_CurrentFight.Finished = true;
					}
					else if (m_CurrentFight.Blue != null && m_CurrentFight.Red == null)
					{
						Owner.Say(505241, m_CurrentFight.Blue.Name); // ~1_NAME~ z braku oponenta wygrywa walke!
						m_CurrentFight.BlueIsWinner = true;
						m_CurrentFight.Finished = true;
					}
					else if (m_CurrentFight.Blue == null && m_CurrentFight.Red != null)
					{
						Owner.Say(505241, m_CurrentFight.Red.Name); // ~1_NAME~ z braku oponenta wygrywa walke!
						m_CurrentFight.BlueIsWinner = false;
						m_CurrentFight.Finished = true;
					}

					if (m_CurrentFight.Finished && m_NextFightNr != -1)
					{
						Mobile winner = (m_CurrentFight.BlueIsWinner) ? m_CurrentFight.Blue : m_CurrentFight.Red;
						bool added = false;
						int rnd = m_CurrentFight.Round + 1;
						int fght = ((m_CurrentFight.Fight - 1) - ((m_CurrentFight.Fight - 1) % 2)) / 2;

						TournamentStatistics.UpdateRank(winner, rnd, Fights, false, Owner.TrnClass);

						for (int i = 0; i < m_Fights.Count && !added; i++)
						{
							TournamentFight tf = m_Fights[i] as TournamentFight;

							if (tf.Round == rnd && tf.Fight == fght + 1)
							{
								tf.AddFighter(winner);
								added = true;
							}
						}

						m_CurrentFight = m_NextFight;
						m_CurrentFightNr = m_NextFightNr;

						if (m_CurrentFightNr + 1 < m_Fights.Count)
						{
							m_NextFightNr++;
							m_NextFight = m_Fights[m_NextFightNr] as TournamentFight;
						}
						else
						{
							m_NextFightNr = -1;
							m_NextFight = null;
						}

						bool firstFight = (m_CurrentFight.Fight == 1 && m_CurrentFight.Round != 1);
						bool isFight = (m_CurrentFight.Red != null && m_CurrentFight.Blue != null);
						int ts = (isFight) ? (int)TournamentIntervals.HalfTimeUnit : (int)TournamentIntervals.VeryShort;

						if (firstFight)
						{
							Owner.Yell(505242, m_CurrentFight.Round.ToString()); // RUNDA ~1_NUMBER~!
							Owner.Say(505235); // Dwie klepsydry na przygotowania!
							ts = (int)TournamentIntervals.DoubleTimeUnit;
						}

						if (isFight)
						{
							if (!firstFight)
							{
								Owner.Say(505243); // Przygotowac sie do kolejnej walki!
								// W niej ~1_NAME~ zmierzy sie z ~2_NAME~
								Owner.Say(505244,
									String.Format("{0}\t{1}", m_CurrentFight.Blue.Name,
										(m_CurrentFight.Red != null) ? m_CurrentFight.Red.Name : "samym soba"));
							}
							else
								// "W pierwszej ~1_NAME~ zmierzy sie z ~2_NAME~"
								Owner.Say(505234,
									String.Format("{0}\t{1}", m_CurrentFight.Blue.Name,
										(m_CurrentFight.Red != null) ? m_CurrentFight.Red.Name : "samym soba"));

							SendInfo(m_CurrentFight);

							new TournamentTimer(this, ts).Start();
							new TournamentCountdownTimer(Owner, ts - 24).Start();
						}
						else
						{
							if (m_CurrentFight.Red == null && m_CurrentFight.Blue == null)
								Owner.Say(505245,
									((firstFight)
										? "Pierwsza"
										: "Kolejna")); // ~1_TXT~ walka nie odbedzie sie z powodu braku zawodnikow! 
							else if (m_CurrentFight.Red == null || m_CurrentFight.Blue == null)
								Owner.Say(505246,
									((firstFight)
										? "Pierwsza"
										: "Kolejna")); // ~1_TXT~ walka nie odbedzie sie z powodu braku zawodnika! 

							new TournamentTimer(this, ts).Start();
						}

						return true;
					}

					if (m_CurrentFight.Finished)
					{
						Owner.Say(505247); // To byla ostatnia walka!

						new TournamentTimer(this, (int)TournamentIntervals.Short).Start();
						Progress++;
						return true;
					}

					#endregion

					#region Odpalamy pojedynek

					Owner.Arena.AddFighter(m_CurrentFight);
					Owner.Yell(505248); // WALKA!

					#endregion
				}
				else
					Progress++;

				#endregion
			}
			catch (Exception exc)
			{
				Owner.Say(505224); // Fatalnie! Siostra rodzi! Musze uciekac! Koniec turnieju!
				Console.WriteLine(exc.ToString());
				return false;
			}

			return true;
		}

		private bool Final()
		{
			try
			{
				EndReason = TournamentEndReason.Finished;

				#region Ogloszenie zwycezcow

				Owner.Say(505249); // Oglaszam wyniki!

				try
				{
					for (int i = 0; i < m_Competitors.Count; i++)
					{
						Mobile mob = (m_Competitors[i] as TournamentCompetitor).Competitor;

						mob.CloseGump(typeof(TournamentStatusGump));
						mob.SendGump(new TournamentStatusGump(this));
					}
				}
				catch (Exception exc)
				{
					Console.WriteLine("Turniej: [final] blad publikacji wynikow turnieju!");
					Console.WriteLine(exc.ToString());
				}

				IPooledEnumerable eable = Owner.GetMobilesInRange(8);

				foreach (Mobile mobile in eable)
				{
					if (mobile == Owner)
						continue;

					if (mobile.NetState == null)
						continue;

					if (mobile.HasGump(typeof(TournamentStatusGump)))
						continue;

					mobile.SendGump(new TournamentStatusGump(this));
				}

				#endregion

				#region Wyplata nagrod

				for (int i = 4; i > 0; i--)
				{
					Mobile winner = GetWinner(i);

					if (winner != null)
					{
						Banker.Deposit(winner, GetReward(i));

						if (i == 1)
						{
							int rnd = (m_Fights[m_Fights.Count - 1] as TournamentFight).Round;

							TournamentStatistics.UpdateRank(winner, rnd, Fights, true, Owner.TrnClass);
						}

						Console.WriteLine("Turniej: [final] #{0} {1} (nagroda {2} gp)", i, winner, GetReward(i));
					}
				}

				#endregion

				Owner.FinishTournament();

				Progress++;

				TournamentStatistics.UpdateRecords(this);
			}
			catch (Exception exc)
			{
				Owner.Say(505224); // Fatalnie! Siostra rodzi! Musze uciekac! Koniec turnieju!
				Console.WriteLine(exc.ToString());
				return false;
			}

			return true;
		}

		public void Stop()
		{
			Stop(TournamentEndReason.Exception);
		}

		public void Stop(TournamentEndReason reason)
		{
			try
			{
				EndReason = reason;
				Progress = TournamentProgress.Finished;
				Owner.FinishTournament();

				Console.WriteLine("Turniej: [stop] reason - {0} [zwrot wpisowego dla {1}+{2} zawodnikow].", reason,
					m_Competitors.Count, Owner.Competitors.Count);

				for (int i = 0; m_Competitors != null && i < m_Competitors.Count; i++)
					Banker.Deposit((m_Competitors[i] as TournamentCompetitor).Competitor, Owner.TrnFee);

				for (int i = 0; Owner.Competitors != null && i < Owner.Competitors.Count; i++)
					Banker.Deposit((Owner.Competitors[i] as TournamentCompetitor).Competitor, Owner.TrnFee);

				Owner.Competitors.Clear();

				if (Owner.TrnClass == TournamentClass.Private)
				{
					int toRefund = Owner.PrivateTournamentCost();

					if (reason == TournamentEndReason.NoFighters)
						toRefund = toRefund * 3 / 4;

					Banker.Deposit(Owner.TrnFounder, toRefund);
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
			}
		}

		#endregion

		#region Methods

		private int GetMooreIndex(int rank, int fights)
		{
			int i = (fights <= 4) ? 4 : (fights <= 8) ? 3 : (fights <= 16) ? 2 : (fights <= 32) ? 1 : 0;

			return m_MooreTable[i][rank];
		}

		public void PushFight(TournamentPushFight how)
		{
			try
			{
				if (how == TournamentPushFight.None)
					return;

				if (how == TournamentPushFight.BlueWon)
				{
					Console.WriteLine("Turniej: [fight] forsowanie zwyciestwa zawodnika niebieskiego");
					// Dezyzja sedziow pojedynek wygrywa ~1_NAME~
					Owner.Say(505250, (m_CurrentFight.Blue != null) ? m_CurrentFight.Blue.Name : "#505252");

					m_CurrentFight.Finished = true;
					m_CurrentFight.BlueIsWinner = true;
					Owner.Arena.EndFight(m_CurrentFight.Red, EOFReason.Defeat);
					Owner.Arena.EndFight(m_CurrentFight.Blue, EOFReason.Victory);
				}
				else if (how == TournamentPushFight.RedWon)
				{
					Console.WriteLine("Turniej: [fight] forsowanie zwyciestwa zawodnika czerwonego");
					// Dezyzja sedziow pojedynek wygrywa ~1_NAME~
					Owner.Say(505250, (m_CurrentFight.Blue != null) ? m_CurrentFight.Red.Name : "#505251");

					m_CurrentFight.Finished = true;
					m_CurrentFight.BlueIsWinner = false;
					Owner.Arena.EndFight(m_CurrentFight.Blue, EOFReason.Defeat);
					Owner.Arena.EndFight(m_CurrentFight.Red, EOFReason.Victory);
				}
				else
				{
					Console.WriteLine("Turniej: [fight] forsowanie restartu walki");
					// Decyzja sedziow pojednynek zostanie powtorzony! Klepsydra na przygotowanie.
					Owner.Say(505253);

					Owner.Arena.EndFight(m_CurrentFight.Blue, EOFReason.ForceRestart);
					Owner.Arena.EndFight(m_CurrentFight.Red, EOFReason.ForceRestart);
					new TournamentTimer(this, (int)TournamentIntervals.TimeUnit).Start();
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Owner.Say(505254); // Nie moge wykonac decyzji sedziow!
			}
		}

		public void WonTheFight(Mobile m)
		{
			try
			{
				if (m_CurrentFight == null)
					return;

				if (m_CurrentFight.Blue != null && m_CurrentFight.Blue == m)
				{
					m_CurrentFight.BlueIsWinner = true;
					m_CurrentFight.Finished = true;
				}
				else if (m_CurrentFight.Red != null && m_CurrentFight.Red == m)
				{
					m_CurrentFight.BlueIsWinner = false;
					m_CurrentFight.Finished = true;
				}
				else
					m_CurrentFight.Finished = true;

				if (m_NextFightNr != -1)
				{
					Mobile winner = (m_CurrentFight.BlueIsWinner) ? m_CurrentFight.Blue : m_CurrentFight.Red;
					bool added = false;
					int rnd = m_CurrentFight.Round + 1;
					int fght = ((m_CurrentFight.Fight - 1) - ((m_CurrentFight.Fight - 1) % 2)) / 2;

					TournamentStatistics.UpdateRank(winner, rnd, Fights, false, Owner.TrnClass);

					for (int i = 0; i < m_Fights.Count && !added; i++)
					{
						TournamentFight tf = m_Fights[i] as TournamentFight;

						if (tf.Round == rnd && tf.Fight == fght + 1)
						{
							tf.AddFighter(winner);
							added = true;
						}
					}

					m_CurrentFight = m_NextFight;
					m_CurrentFightNr = m_NextFightNr;

					if (m_CurrentFightNr + 1 < m_Fights.Count)
					{
						m_NextFightNr++;
						m_NextFight = m_Fights[m_NextFightNr] as TournamentFight;
					}
					else
					{
						m_NextFightNr = -1;
						m_NextFight = null;
					}

					bool firstFight = (m_CurrentFight.Fight == 1 && m_CurrentFight.Round != 1);
					bool isFight = (m_CurrentFight.Red != null && m_CurrentFight.Blue != null);
					int ts = (isFight) ? (int)TournamentIntervals.Short : (int)TournamentIntervals.VeryShort;

					if (firstFight)
					{
						Owner.Yell(505242, m_CurrentFight.Round.ToString()); // RUNDA ~1_NUMBER~!
						Owner.Say(505235); // Dwie klepsydry na przygotowania!
						ts = (int)TournamentIntervals.DoubleTimeUnit;
					}

					if (isFight)
					{
						if (!firstFight)
						{
							Owner.Say(505243); // Przygotowac sie do kolejnej walki!
							// W niej ~1_NAME~ zmierzy sie z ~2_NAME~
							Owner.Say(505244,
								String.Format("{0}\t{1}", m_CurrentFight.Blue.Name,
									(m_CurrentFight.Red != null) ? m_CurrentFight.Red.Name : "samym soba"));
						}
						else
							// "W pierwszej ~1_NAME~ zmierzy sie z ~2_NAME~"
							Owner.Say(505234,
								String.Format("{0}\t{1}", m_CurrentFight.Blue.Name,
									(m_CurrentFight.Red != null) ? m_CurrentFight.Red.Name : "samym soba"));

						SendInfo(m_CurrentFight);

						new TournamentTimer(this, ts).Start();
						new TournamentCountdownTimer(Owner, ts - 24).Start();
					}
					else
					{
						if (m_CurrentFight.Red == null && m_CurrentFight.Blue == null)
							Owner.Say(505245,
								((firstFight)
									? "Pierwsza"
									: "Kolejna")); // ~1_TXT~ walka nie odbedzie sie z powodu braku zawodnikow! 
						else if (m_CurrentFight.Red == null || m_CurrentFight.Blue == null)
							Owner.Say(505246,
								((firstFight)
									? "Pierwsza"
									: "Kolejna")); // ~1_TXT~ walka nie odbedzie sie z powodu braku zawodnika! 

						new TournamentTimer(this, ts).Start();
					}
				}
				else
				{
					Owner.Say(505247); // "To byla ostatnia walka!"

					new TournamentTimer(this, (int)TournamentIntervals.Short).Start();
					Progress++;
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				Stop();
			}
		}

		public string GetStatus()
		{
			string status = "status";

			switch (Progress)
			{
				#region Start

				case TournamentProgress.Start:
					status = Owner.GetTournamentStatus();
					break;

				#endregion

				#region Initialization

				case TournamentProgress.FirstCall:
				case TournamentProgress.SecondCall:
				case TournamentProgress.ThirdCall:
				case TournamentProgress.Roostering:
				{
					status = "Trwa kolejny turniej na arenie " + Owner.ArenaName + "!\n";
					string nr = (Progress == TournamentProgress.SecondCall) ? "pierwszym" :
						(Progress == TournamentProgress.ThirdCall) ? "drugim" : "trzecim";

					status += "Aktualnie jestesmy po " + nr + " wezwaniu.\n";

					#region 1st part Rewards

					if (Owner.TrnReward1st > 0 || Owner.TrnRewardName != null)
						status += "Nagroda bedzie";

					if (Owner.TrnRewardName != null)
						status += " " + Owner.TrnRewardName;

					if (Owner.TrnReward1st > 0 && Owner.TrnRewardName == null)
						status += " " + Owner.TrnReward1st + " sztuk zlota";
					else if (Owner.TrnReward1st > 0)
						status += " oraz " + Owner.TrnReward1st + " sztuk zlota";

					status += ".\n";

					if (Owner.TrnFee > 0)
					{
						if (Owner.TrnReward1st > 0)
							status += "Ponadto zwyciezca";
						else
							status += "Zwyciezca";

						status +=
							" otrzyma polowe, jego pokonany finalowy przeciwnik czwarta czesc, a pokonani w polfinalach ";
						status += " po osmej czesci zebranego wpisowego, ktore wynosi jedyne " + Owner.TrnFee + ".\n";
					}

					if (Owner.TrnReward2nd > 0)
					{
						status += " Drugi zawodnik turnieju otrzyma nagrode w wysokosci " + Owner.TrnReward2nd +
						          " centarow.\n";
					}

					if (Owner.TrnReward3rd > 0)
					{
						status += " Trzeci zawodnicy otrzymaja po " + Owner.TrnReward3rd + " centarow.\n";
					}

					status += "\nLosowanie zawodnikow odbedzie sie na zasadach ";

					switch (Owner.TrnClass)
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

					status += Owner.GetArenaRules();

					#endregion

					#region 3rd part ( Competitors )

					status += "\nDo turnieju swoj udzial zglosili (w kolejnosci zgloszen): \n\n";

					for (int i = 0; i < Owner.Competitors.Count; i++)
					{
						TournamentCompetitor tc = Owner.Competitors[i] as TournamentCompetitor;
						status += tc.Competitor.Name + (tc.Confirmed ? " ( potwierdzony )\n" : "\n");
					}


					status += "\nMinimalna liczba zawodnikow wynosi " + Owner.TrnCompMinCount;

					#endregion

					break;
				}

				#endregion

				#region Fight

				case TournamentProgress.Fight:
				{
					status = "Trwa kolejny ";

					switch (Owner.TrnClass)
					{
						case TournamentClass.Normal:
							status += "turniej ";
							break;
						case TournamentClass.Masters:
							status += "turniej mistrzow ";
							break;
						case TournamentClass.Open:
							status += "turniej otwarty ";
							break;
						case TournamentClass.Private:
							status += "prywatny turniej ";
							break;
					}

					status += "na arenie " + Owner.ArenaName + "!\n";

					status += "Walki zostaly rozpisane, a wojownicy scieraja sie na arenie:\n";

					for (int i = 0; i < m_Fights.Count; i++)
					{
						TournamentFight tf = m_Fights[i] as TournamentFight;

						if (tf.Fight == 1)
							status += "\nRunda #" + tf.Round + "\n\n";

						status += tf + "\n";
					}

					#region Rewards

					if (Owner.TrnReward1st > 0 || Owner.TrnRewardName != null)
						status += "Nagroda jest";

					if (Owner.TrnRewardName != null)
						status += " " + Owner.TrnRewardName;

					if (Owner.TrnReward1st > 0 && Owner.TrnRewardName == null)
						status += " " + Owner.TrnReward1st + " sztuk zlota";
					else if (Owner.TrnReward1st > 0)
						status += " oraz " + Owner.TrnReward1st + " sztuk zlota";
					status += ".\n";

					if (Owner.TrnFee > 0)
					{
						if (Owner.TrnReward1st > 0)
							status += "Ponadto zwyciezca";
						else
							status += "Zwyciezca";

						status +=
							" otrzyma polowe, jego pokonany finalowy przeciwnik czwarta czesc, a pokonani w polfinalach ";
						status += " po osmej czesci zebranego wpisowego. Kwota ta wynosi " + this.m_Reward +
						          " sztuk zlota.\n";
					}

					if (Owner.TrnReward2nd > 0)
					{
						status += " Drugi zawodnik turnieju otrzyma nagrode w wysokosci " + Owner.TrnReward2nd +
						          " centarow.\n";
					}

					if (Owner.TrnReward3rd > 0)
					{
						status += " Trzeci zawodnicy otrzymaja po " + Owner.TrnReward3rd + " centarow.\n";
					}

					status += "\n";

					#endregion

					#region Rules

					status += "Zasady turnieju sa jak nastepuje.\n";

					status += Owner.GetArenaRules();

					#endregion

					break;
				}

				#endregion

				#region Default

				default:
				{
					status = "Wyniki ";

					switch (Owner.TrnClass)
					{
						case TournamentClass.Normal:
							status += "turnieju ";
							break;
						case TournamentClass.Masters:
							status += "turnieju mistrzow ";
							break;
						case TournamentClass.Open:
							status += "turnieju otwartego ";
							break;
						case TournamentClass.Private:
							status += "prywatnego turnieju ";
							break;
					}

					status += "ktory odbyl sie "
					          + NelderimDateTime.TransformToString(Owner.TournamentStart,
						          NelderimDateTimeFormat.LongWhen)
					          + "\n\n";


					if (EndReason == TournamentEndReason.Finished)
					{
						#region Pierwsze miejsca

						status += "Zwyciezcy: \n";

						for (int i = 1; i < 5; i++)
						{
							Mobile winner = GetWinner(i);

							if (winner != null)
							{
								status += "#" + i + " - " + winner.Name + " z nagroda w wysokosci " + GetReward(i) +
								          " sztuk zlota\n";

								if (i == 1 && Owner.TrnRewardName != null)
									status += winner.Name + " przypadlo trofeum - " + Owner.TrnRewardName + "\n";
							}
						}

						#endregion

						status += "\nWalki przebiegaly nastepujaco: \n";

						for (int i = 0; i < m_Fights.Count; i++)
						{
							TournamentFight tf = m_Fights[i] as TournamentFight;

							if (tf.Fight == 1)
								status += "\nRunda #" + tf.Round + "\n\n";

							status += tf + "\n";
						}
					}
					else if (EndReason == TournamentEndReason.NoFighters)
					{
						status += "Turniej nie odbyl sie z powodu zbyt malej liczby uczestnikow.\n";
					}
					else if (EndReason == TournamentEndReason.Unfinished)
					{
						status += "Wyniki turnieju nie sa w tej chwili dostepne.\n";
						;
					}
					else
					{
						status += "Turniej zostal niespodziewanie przerwany i wyniki nie sa dostepne.\n";
					}

					break;
				}

				#endregion
			}

			return status;
		}

		private void SendInfo(TournamentFight tf)
		{
			if (tf == null)
				return;

			if (tf.Blue != null && tf.Blue.NetState != null)
				tf.Blue.SendLocalizedMessage(505255, "",
					167); // Walczysz w kolejnej walce! Badz w pogotowiu! Staw sie u Zarzadcy Areny!

			if (tf.Red != null && tf.Red.NetState != null)
				tf.Red.SendLocalizedMessage(505255, "",
					167); // Walczysz w kolejnej walce! Badz w pogotowiu! Staw sie u Zarzadcy Areny!
		}

		#region Zwyciezcy i ich nagrody

		/*
		 *  1 miejsce - zwyciezca ostatniej walki
		 *  2 miejsce - przegrany w ostatniej walce
		 *  3 miejsce - przegrany w przedostatniej walce
		 *  4 miejsce - przegrany w przed- przedostatniej walce
		 */

		public Mobile GetWinner(int place)
		{
			try
			{
				if (m_Fights == null || place < 1 || place > 4 || (int)Progress < (int)TournamentProgress.End ||
				    EndReason != TournamentEndReason.Finished)
					return null;

				if (m_Fights.Count < 2 && place > 2)
					return null;

				if (place < 3)
				{
					TournamentFight tf = m_Fights[m_Fights.Count - 1] as TournamentFight;

					if (place == 1)
						return (tf.BlueIsWinner) ? tf.Blue : tf.Red;
					return (tf.BlueIsWinner) ? tf.Red : tf.Blue;
				}
				else
				{
					TournamentFight tf = m_Fights[m_Fights.Count - place + 1] as TournamentFight;

					return (tf.BlueIsWinner) ? tf.Red : tf.Blue;
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				return null;
			}
		}

		private int GetReward(int place)
		{
			int fullReward = (2 * Owner.TrnReward3rd) + Owner.TrnReward2nd + Owner.TrnReward1st + m_Reward;
			try
			{
				if (m_Fights == null || place < 1 || place > 4 || (int)Progress < (int)TournamentProgress.End ||
				    EndReason != TournamentEndReason.Finished)
					return 0;

				if (m_Fights.Count < 2 && place > 2)
					return 0;

				if (place < 3)
				{
					TournamentFight tf = m_Fights[m_Fights.Count - 1] as TournamentFight;

					if (place == 1)
						return (!tf.BlueIsWinner)
							? ((tf.Red != null) ? fullReward - (GetReward(2) + GetReward(3) + GetReward(4)) : 0)
							: ((tf.Blue != null) ? fullReward - (GetReward(2) + GetReward(3) + GetReward(4)) : 0);
					return (tf.BlueIsWinner)
						? ((tf.Red != null) ? Owner.TrnReward2nd + HalfQuarterReward * 2 : 0)
						: ((tf.Blue != null) ? Owner.TrnReward2nd + HalfQuarterReward * 2 : 0);
				}
				else
				{
					TournamentFight tf = m_Fights[m_Fights.Count - place + 1] as TournamentFight;

					return (tf.BlueIsWinner)
						? ((tf.Red != null) ? Owner.TrnReward3rd + HalfQuarterReward : 0)
						: ((tf.Blue != null) ? Owner.TrnReward3rd + HalfQuarterReward : 0);
				}
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.ToString());
				return 0;
			}
		}

		#endregion

		#endregion
	}
}
