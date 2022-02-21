#region References

using System;
using System.Collections;

#endregion

namespace Server.Items.Crops
{
	public class CabbageSeed : BaseCrop
	{
		// return true to allow planting on Dirt Item (ItemID 0x32C9)
		// See CropHelper.cs for other overriddable types
		public override bool CanGrowGarden { get { return true; } }

		[Constructable]
		public CabbageSeed() : this(1)
		{
		}

		[Constructable]
		public CabbageSeed(int amount) : base(0xF27)
		{
			Stackable = true;
			Weight = .5;
			Hue = 0x5E2;

			Movable = true;

			Amount = amount;
			Name = "nasiona kapusty";
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.Mounted && !CropHelper.CanWorkMounted)
			{
				from.SendMessage("Nie mozesz uprawiac roslin na wierzchowcu!.");
				return;
			}

			Point3D m_pnt = from.Location;
			Map m_map = from.Map;

			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042010); //You must have the object in your backpack to use it. 
				return;
			}

			if (!CropHelper.CheckCanGrow(this, m_map, m_pnt.X, m_pnt.Y))
			{
				@from.SendMessage("Ta sadzonka nie urosnie tutaj.");
				return;
			}

			//check for BaseCrop on this tile
			ArrayList cropshere = CropHelper.CheckCrop(m_pnt, m_map, 0);
			if (cropshere.Count > 0)
			{
				from.SendMessage("Tutaj juz cos rosnie.");
				return;
			}

			//check for over planting prohibt if 6 maybe 5 neighboring crops
			ArrayList cropsnear = CropHelper.CheckCrop(m_pnt, m_map, 1);
			if ((cropsnear.Count > 5) || ((cropsnear.Count == 5) && Utility.RandomBool()))
			{
				from.SendMessage("W tym miejscu jest zbyt wiele roslin.");
				return;
			}

			if (this.BumpZ) ++m_pnt.Z;

			if (!from.Mounted)
				from.Animate(32, 5, 1, true, false, 0); // Bow

			from.SendMessage("Zasadziles rosline.");
			this.Consume();
			Item item = new CabbageSeedling(from);
			item.Location = m_pnt;
			item.Map = m_map;
		}

		public CabbageSeed(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
		}
	}


	public class CabbageSeedling : BaseCrop
	{
		private static Mobile m_sower;
		public Timer thisTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Sower { get { return m_sower; } set { m_sower = value; } }

		[Constructable]
		public CabbageSeedling(Mobile sower) : base(0xCB5)
		{
			Movable = false;
			Name = "sadzonka kapusty";
			m_sower = sower;

			init(this);
		}

		public static void init(CabbageSeedling plant)
		{
			plant.thisTimer = new CropHelper.GrowTimer(plant, typeof(CabbageCrop), plant.Sower);
			plant.thisTimer.Start();
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (from.Mounted && !CropHelper.CanWorkMounted)
			{
				from.SendMessage("Nie mozesz tego zebrac bedac na wierzchowcu.");
				return;
			}

			if ((Utility.RandomDouble() <= .25) && !(m_sower.AccessLevel > AccessLevel.Counselor))
			{
				//25% Chance
				from.SendMessage("Wyrwales rosline z korzeniami.");
				thisTimer.Stop();
				this.Delete();
			}
			else from.SendMessage("Ta roslina jest za mloda.");
		}

		public CabbageSeedling(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0);
			writer.Write(m_sower);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			m_sower = reader.ReadMobile();

			init(this);
		}
	}

	public class CabbageCrop : BaseCrop
	{
		private const int max = 1;
		private DateTime lastpicked;

		public Timer regrowTimer;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastSowerVisit { get; private set; }

		[CommandProperty(AccessLevel.GameMaster)] // debuging
		public bool Growing { get { return regrowTimer.Running; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile Sower { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Yield { get; set; }

		public int Capacity { get { return max; } }
		public int FullGraphic { get; set; }

		public int PickGraphic { get; set; }

		public DateTime LastPick { get { return lastpicked; } set { lastpicked = value; } }

		[Constructable]
		public CabbageCrop(Mobile sower) : base(0xC61)
		{
			Movable = false;
			Name = "kapusta";

			Sower = sower;
			LastSowerVisit = DateTime.Now;

			init(this, false);
		}

		public static void init(CabbageCrop plant, bool full)
		{
			plant.PickGraphic = (0xC61);
			plant.FullGraphic = (0xC7C);

			plant.LastPick = DateTime.Now;
			plant.regrowTimer = new CropTimer(plant);

			if (full)
			{
				plant.Yield = plant.Capacity;
				plant.ItemID = plant.FullGraphic;
			}
			else
			{
				plant.Yield = 0;
				plant.ItemID = plant.PickGraphic;
				plant.regrowTimer.Start();
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (Sower == null || Sower.Deleted)
				Sower = from;

			if (from != Sower)
			{
				from.SendMessage("To nie jest twoja roslina !");
				from.Criminal = true;
			}

			if (from.Mounted && !CropHelper.CanWorkMounted)
			{
				from.SendMessage("Nie mozesz tego zebrac bedac na wierzchowcu.");
				return;
			}

			if (DateTime.Now > lastpicked.AddSeconds(3)) // 3 seconds between picking
			{
				lastpicked = DateTime.Now;

				if (from.InRange(this.GetWorldLocation(), 1))
				{
					if (Yield < 1)
					{
						from.SendMessage("Nie ma tu nic do zebrania.");

						if (PlayerCanDestroy && !(Sower.AccessLevel > AccessLevel.Counselor))
						{
							from.SendMessage("Wyrwales rosline z korzeniami.");
							this.Delete();
						}
					}
					else //check sower
					{
						from.Direction = from.GetDirectionTo(this);

						from.Animate(from.Mounted ? 29 : 32, 5, 1, true, false, 0);

						if (from == Sower)
						{
							LastSowerVisit = DateTime.Now;
						}

						int pick = 1;

						if (pick == 1)
						{
							if (Utility.RandomDouble() < .25)
							{
								from.SendMessage("Zebrales troche nasion.");
								from.AddToBackpack(new CabbageSeed());
							}

							from.SendMessage("Wyrwales rosline z korzeniami.");
							this.Delete();
						}

						Yield -= pick;
						from.SendMessage("Zebrales {0} kapusty{1}!", pick, (pick == 1 ? "" : ""));

						this.ItemID = PickGraphic;

						Cabbage crop = new Cabbage(pick);
						from.AddToBackpack(crop);

						if (!regrowTimer.Running)
						{
							regrowTimer.Start();
						}
					}
				}
				else
				{
					from.SendMessage("Jestes za daleko.");
				}
			}
		}

		private class CropTimer : Timer
		{
			private readonly CabbageCrop i_plant;

			public CropTimer(CabbageCrop plant) : base(TimeSpan.FromSeconds(450), TimeSpan.FromSeconds(15))
			{
				Priority = TimerPriority.OneSecond;
				i_plant = plant;
			}

			protected override void OnTick()
			{
				if (Utility.RandomBool())
				{
					if ((i_plant != null) && (!i_plant.Deleted))
					{
						int current = i_plant.Yield;

						if (++current >= i_plant.Capacity)
						{
							current = i_plant.Capacity;
							i_plant.ItemID = i_plant.FullGraphic;
							Stop();
						}
						else if (current <= 0)
							current = 1;

						i_plant.Yield = current;
					}
					else Stop();
				}
			}
		}

		public CabbageCrop(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1);
			writer.Write(LastSowerVisit);
			writer.Write(Sower);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			int version = reader.ReadInt();
			switch (version)
			{
				case 1:
				{
					LastSowerVisit = reader.ReadDateTime();
					goto case 0;
				}
				case 0:
				{
					Sower = reader.ReadMobile();
					break;
				}
			}

			if (version == 0)
				LastSowerVisit = DateTime.Now;

			init(this, true);
		}
	}
}
