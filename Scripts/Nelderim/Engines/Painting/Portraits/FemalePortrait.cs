#region References

using System;

#endregion

namespace Server.Items
{
	[FlipableAttribute(0xEE7, 0xEC9)]
	public class FemalePortrait : Item
	{
		[Constructable]
		public FemalePortrait(string artistName, string subject) : base(0xEE7)
		{
			if (artistName == subject)
			{
				Name = String.Format("Autoportret {0}", artistName);
			}
			else
			{
				Name = String.Format("Portret {0} namalowany przez {1}", subject, artistName);

				Weight = 3.0;
				Hue = 0;
			}
		}

		public FemalePortrait(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}
