#region References

using System;
using System.Reflection;
using Server.Commands;
using Server.Mobiles;
using Server.Spells;
using Server.Targeting;

#endregion

namespace Server.ACC.CSS.Systems.Rogue
{
	public class RogueCharmSpell : RogueSpell
	{
		private static readonly SpellInfo m_Info = new SpellInfo(
			"Zaklęcie", "Spojrz w moje oczy!",
			//SpellCircle.Fourth,
			218,
			9012
		);

		public override SpellCircle Circle
		{
			get { return SpellCircle.Fourth; }
		}

		public override double CastDelay { get { return 2; } }
		public override double RequiredSkill { get { return 80; } }
		public override int RequiredMana { get { return 30; } }

		private Timer m_Timer;

		public RogueCharmSpell(Mobile caster, Item scroll)
			: base(caster, scroll, m_Info)
		{
			if (this.Scroll != null)
				Scroll.Consume();
		}

		public override void OnCast()
		{
			Caster.Target = new InternalTarget(this);
		}

		public void Target(Mobile ma)
		{
			BaseCreature m = ma as BaseCreature;
			if (ma is CharmedMobile)
			{
				ma.Kill();
				Caster.SendMessage("Uwalniasz tę kreaturę!");
			}
			else if (m != null)
			{
				if (!Caster.CanSee(m))
				{
					Caster.SendLocalizedMessage(500237); // Target can not be seen.
				}
				else if (m.Controlled || m.Summoned)
				{
					Caster.SendMessage("Już ktoś to kontroluje!");
				}
				else if (!m.Alive)
				{
					Caster.SendMessage("Śmierć już ich chyba oczarowała.");
				}
				else if (!m.Tamable || m.Blessed)
				{
					Caster.SendMessage("Nie uda Ci się to!");
				}
				else if (Caster.Followers >= 1)
				{
					Caster.SendMessage("Akurat tego nie możesz kontrolować!");
				}
				else if (CheckHSequence(m))
				{
					SpellHelper.Turn(Caster, m);


					if (!m.Controlled && m != null && !m.Deleted)
					{
						double taming;
						if (m.MinTameSkill <= 1.0)
							taming = 1.0;
						else
							taming = m.MinTameSkill;

						double charmchance = (Caster.Skills[SkillName.Provocation].Value / taming);
						if (charmchance <= 0.5)
						{
							Caster.SendMessage("Akurat tego nie możesz kontrolować.");
						}
						else if (charmchance >= 0.9)
						{
							Caster.SendMessage("Oczarowałeś to stworzenie!");
							Point3D mloc = new Point3D(m.X, m.Y, m.Z);
							CharmedMobile dg = new CharmedMobile(m);
							dg.Owner = m;

							dg.Body = m.Body;
							dg.AI = m.AI;
							dg.Hue = m.Hue;
							dg.Name = m.Name;
							dg.SpeechHue = m.SpeechHue;
							dg.Fame = m.Fame;
							dg.Karma = m.Karma;
							dg.EmoteHue = m.EmoteHue;
							dg.Title = m.Title;
							dg.Criminal = (m.Criminal);
							dg.Str = m.Str;
							dg.Int = m.Int;
							dg.Hits = m.Hits;
							dg.Dex = m.Dex;
							dg.Mana = m.Mana;
							dg.Stam = m.Stam;

							dg.VirtualArmor = (m.VirtualArmor);
							dg.SetSkill(SkillName.Wrestling, m.Skills[SkillName.Wrestling].Value);
							dg.SetSkill(SkillName.Tactics, m.Skills[SkillName.Tactics].Value);
							dg.SetSkill(SkillName.Anatomy, m.Skills[SkillName.Anatomy].Value);

							dg.SetSkill(SkillName.Stealing, m.Skills[SkillName.Stealing].Value);
							dg.SetSkill(SkillName.MagicResist, m.Skills[SkillName.MagicResist].Value);
							dg.SetSkill(SkillName.Meditation, m.Skills[SkillName.Meditation].Value);
							dg.SetSkill(SkillName.EvalInt, m.Skills[SkillName.EvalInt].Value);

							dg.SetSkill(SkillName.Archery, m.Skills[SkillName.Archery].Value);
							dg.SetSkill(SkillName.Macing, m.Skills[SkillName.Macing].Value);
							dg.SetSkill(SkillName.Swords, m.Skills[SkillName.Swords].Value);
							dg.SetSkill(SkillName.Fencing, m.Skills[SkillName.Fencing].Value);
							dg.SetSkill(SkillName.Lumberjacking, m.Skills[SkillName.Lumberjacking].Value);
							dg.SetSkill(SkillName.Alchemy, m.Skills[SkillName.Alchemy].Value);
							dg.SetSkill(SkillName.Parry, m.Skills[SkillName.Parry].Value);
							dg.SetSkill(SkillName.Focus, m.Skills[SkillName.Focus].Value);
							dg.SetSkill(SkillName.Necromancy, m.Skills[SkillName.Necromancy].Value);
							dg.SetSkill(SkillName.Chivalry, m.Skills[SkillName.Chivalry].Value);
							dg.SetSkill(SkillName.ArmsLore, m.Skills[SkillName.ArmsLore].Value);
							dg.SetSkill(SkillName.Poisoning, m.Skills[SkillName.Poisoning].Value);
							dg.SetSkill(SkillName.SpiritSpeak, m.Skills[SkillName.SpiritSpeak].Value);
							dg.SetSkill(SkillName.Stealing, m.Skills[SkillName.Stealing].Value);
							dg.SetSkill(SkillName.Inscribe, m.Skills[SkillName.Inscribe].Value);
							dg.Kills = (m.Kills);


							// Clear Items
							RemoveFromAllLayers(dg);

							// Then copy
							CopyFromLayer(m, dg, Layer.FirstValid);
							CopyFromLayer(m, dg, Layer.TwoHanded);
							CopyFromLayer(m, dg, Layer.Shoes);
							CopyFromLayer(m, dg, Layer.Pants);
							CopyFromLayer(m, dg, Layer.Shirt);
							CopyFromLayer(m, dg, Layer.Helm);
							CopyFromLayer(m, dg, Layer.Gloves);
							CopyFromLayer(m, dg, Layer.Ring);
							CopyFromLayer(m, dg, Layer.Neck);
							CopyFromLayer(m, dg, Layer.Hair);
							CopyFromLayer(m, dg, Layer.Waist);
							CopyFromLayer(m, dg, Layer.InnerTorso);
							CopyFromLayer(m, dg, Layer.Bracelet);
							CopyFromLayer(m, dg, Layer.Face);
							CopyFromLayer(m, dg, Layer.FacialHair);
							CopyFromLayer(m, dg, Layer.MiddleTorso);
							CopyFromLayer(m, dg, Layer.Earrings);
							CopyFromLayer(m, dg, Layer.Arms);
							CopyFromLayer(m, dg, Layer.Cloak);
							CopyFromLayer(m, dg, Layer.OuterTorso);
							CopyFromLayer(m, dg, Layer.OuterLegs);
							CopyFromLayer(m, dg, Layer.LastUserValid);
							CopyFromLayer(m, dg, Layer.Mount);
							dg.ControlSlots = 5;
							dg.Controlled = true;
							dg.ControlMaster = Caster;
							TimeSpan duration =
								TimeSpan.FromSeconds(Caster.Skills[SkillName.Ninjitsu].Value * 1.2); // 120% of Stealing
							m_Timer = new InternalTimer(dg, duration);
							m_Timer.Start();
							dg.Map = m.Map;
							dg.Location = m.Location;
							m.Blessed = true;
							m.Hidden = true;

							m.Location = new Point3D(m.X, m.Y, m.Z - 95);
						}
						else if (charmchance >= Utility.RandomDouble())
						{
							Caster.SendMessage("Oczarowałeś tę kreaturę!");
							CharmedMobile dg = new CharmedMobile(m);
							dg.Owner = m;

							dg.Body = m.Body;

							dg.Hue = m.Hue;
							dg.Name = m.Name;
							dg.SpeechHue = m.SpeechHue;
							dg.Fame = m.Fame;
							dg.Karma = m.Karma;
							dg.EmoteHue = m.EmoteHue;
							dg.Title = m.Title;
							dg.Criminal = (m.Criminal);
							dg.Str = m.Str;
							dg.Int = m.Int;
							dg.Hits = m.Hits;
							dg.Dex = m.Dex;
							dg.Mana = m.Mana;
							dg.Stam = m.Stam;
							dg.AI = m.AI;

							dg.VirtualArmor = (m.VirtualArmor);
							dg.SetSkill(SkillName.Wrestling, m.Skills[SkillName.Wrestling].Value);
							dg.SetSkill(SkillName.Tactics, m.Skills[SkillName.Tactics].Value);
							dg.SetSkill(SkillName.Anatomy, m.Skills[SkillName.Anatomy].Value);

							dg.SetSkill(SkillName.Stealing, m.Skills[SkillName.Stealing].Value);
							dg.SetSkill(SkillName.MagicResist, m.Skills[SkillName.MagicResist].Value);
							dg.SetSkill(SkillName.Meditation, m.Skills[SkillName.Meditation].Value);
							dg.SetSkill(SkillName.EvalInt, m.Skills[SkillName.EvalInt].Value);

							dg.SetSkill(SkillName.Archery, m.Skills[SkillName.Archery].Value);
							dg.SetSkill(SkillName.Macing, m.Skills[SkillName.Macing].Value);
							dg.SetSkill(SkillName.Swords, m.Skills[SkillName.Swords].Value);
							dg.SetSkill(SkillName.Fencing, m.Skills[SkillName.Fencing].Value);
							dg.SetSkill(SkillName.Lumberjacking, m.Skills[SkillName.Lumberjacking].Value);
							dg.SetSkill(SkillName.Alchemy, m.Skills[SkillName.Alchemy].Value);
							dg.SetSkill(SkillName.Parry, m.Skills[SkillName.Parry].Value);
							dg.SetSkill(SkillName.Focus, m.Skills[SkillName.Focus].Value);
							dg.SetSkill(SkillName.Necromancy, m.Skills[SkillName.Necromancy].Value);
							dg.SetSkill(SkillName.Chivalry, m.Skills[SkillName.Chivalry].Value);
							dg.SetSkill(SkillName.ArmsLore, m.Skills[SkillName.ArmsLore].Value);
							dg.SetSkill(SkillName.Poisoning, m.Skills[SkillName.Poisoning].Value);
							dg.SetSkill(SkillName.SpiritSpeak, m.Skills[SkillName.SpiritSpeak].Value);
							dg.SetSkill(SkillName.Stealing, m.Skills[SkillName.Stealing].Value);
							dg.SetSkill(SkillName.Inscribe, m.Skills[SkillName.Inscribe].Value);
							dg.Kills = (m.Kills);


							// Clear Items
							RemoveFromAllLayers(dg);

							// Then copy
							CopyFromLayer(m, dg, Layer.FirstValid);
							CopyFromLayer(m, dg, Layer.TwoHanded);
							CopyFromLayer(m, dg, Layer.Shoes);
							CopyFromLayer(m, dg, Layer.Pants);
							CopyFromLayer(m, dg, Layer.Shirt);
							CopyFromLayer(m, dg, Layer.Helm);
							CopyFromLayer(m, dg, Layer.Gloves);
							CopyFromLayer(m, dg, Layer.Ring);
							CopyFromLayer(m, dg, Layer.Neck);
							CopyFromLayer(m, dg, Layer.Hair);
							CopyFromLayer(m, dg, Layer.Waist);
							CopyFromLayer(m, dg, Layer.InnerTorso);
							CopyFromLayer(m, dg, Layer.Bracelet);
							CopyFromLayer(m, dg, Layer.Face);
							CopyFromLayer(m, dg, Layer.FacialHair);
							CopyFromLayer(m, dg, Layer.MiddleTorso);
							CopyFromLayer(m, dg, Layer.Earrings);
							CopyFromLayer(m, dg, Layer.Arms);
							CopyFromLayer(m, dg, Layer.Cloak);
							CopyFromLayer(m, dg, Layer.OuterTorso);
							CopyFromLayer(m, dg, Layer.OuterLegs);
							CopyFromLayer(m, dg, Layer.LastUserValid);
							CopyFromLayer(m, dg, Layer.Mount);
							dg.ControlSlots = 5;
							dg.Controlled = true;
							dg.ControlMaster = Caster;
							TimeSpan duration =
								TimeSpan.FromSeconds(Caster.Skills[SkillName.Ninjitsu].Value * 1.2); // 120% of Stealing
							m_Timer = new InternalTimer(dg, duration);
							dg.Map = m.Map;
							dg.Location = m.Location;
							m.Blessed = true;
							m.Hidden = true;
							m.Location = new Point3D(m.X, m.Y, m.Z - 95);
						}
						else
							Caster.SendMessage("Oczarowanie nie udało sie.");
					}
				}
				else
					Caster.SendMessage("Akurat tego nie możesz kontrolować.");
			}

			FinishSequence();
		}

		private void CopyFromLayer(Mobile from, Mobile mimic, Layer layer)
		{
			Item newItem;
			Item oldItem = from.FindItemOnLayer(layer);
			if (oldItem != null)
			{
				Type t = oldItem.GetType();
				ConstructorInfo c = t.GetConstructor(Type.EmptyTypes);
				if (c != null)
				{
					try
					{
						object o = c.Invoke(null);

						if (o != null && o is Item)
						{
							newItem = (Item)o;
							Dupe.CopyProperties(newItem, oldItem); //copy.Dupe( item, copy.Amount );
							mimic.AddItem(newItem);
							newItem.LootType = LootType.Newbied;
						}
					}
					catch
					{
						from.SendMessage("oj oj oj... coś poszło nie tak");
					}
				}
			}
		}

		private void DeleteFromLayer(Mobile from, Layer layer)
		{
			if (from.FindItemOnLayer(layer) != null)
				from.RemoveItem(from.FindItemOnLayer(layer));
		}

		private void RemoveFromAllLayers(Mobile from)
		{
			DeleteFromLayer(from, Layer.FirstValid);
			DeleteFromLayer(from, Layer.TwoHanded);
			DeleteFromLayer(from, Layer.Shoes);
			DeleteFromLayer(from, Layer.Pants);
			DeleteFromLayer(from, Layer.Shirt);
			DeleteFromLayer(from, Layer.Helm);
			DeleteFromLayer(from, Layer.Gloves);
			DeleteFromLayer(from, Layer.Ring);
			DeleteFromLayer(from, Layer.Neck);
			DeleteFromLayer(from, Layer.Hair);
			DeleteFromLayer(from, Layer.Waist);
			DeleteFromLayer(from, Layer.InnerTorso);
			DeleteFromLayer(from, Layer.Bracelet);
			DeleteFromLayer(from, Layer.Face);
			DeleteFromLayer(from, Layer.FacialHair);
			DeleteFromLayer(from, Layer.MiddleTorso);
			DeleteFromLayer(from, Layer.Earrings);
			DeleteFromLayer(from, Layer.Arms);
			DeleteFromLayer(from, Layer.Cloak);
			DeleteFromLayer(from, Layer.OuterTorso);
			DeleteFromLayer(from, Layer.OuterLegs);
			DeleteFromLayer(from, Layer.LastUserValid);
			DeleteFromLayer(from, Layer.Mount);
		}

		private class InternalTimer : Timer
		{
			private readonly CharmedMobile m_Item;

			public InternalTimer(CharmedMobile item, TimeSpan duration)
				: base(duration)
			{
				m_Item = item;
			}

			protected override void OnTick()
			{
				if (m_Item != null)
					m_Item.Delete();
			}
		}

		public class InternalTarget : Target
		{
			private readonly RogueCharmSpell m_Owner;

			public InternalTarget(RogueCharmSpell owner)
				: base(12, false, TargetFlags.Harmful)
			{
				m_Owner = owner;
			}

			protected override void OnTarget(Mobile from, object o)
			{
				if (o is Mobile)
					m_Owner.Target((Mobile)o);
			}

			protected override void OnTargetFinish(Mobile from)
			{
				m_Owner.FinishSequence();
			}
		}
	}
}
