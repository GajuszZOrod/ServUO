﻿#region References

using System;
using Server;

#endregion

namespace Nelderim.Gains
{
	class GainsInfo : NExtensionInfo
	{
		public DateTime LastPowerHour { get; set; }

		public double StrGain { get; set; }

		public double DexGain { get; set; }

		public double IntGain { get; set; }

		public override void Serialize(GenericWriter writer)
		{
			writer.Write( (int)0 ); //version
			writer.Write(LastPowerHour);
			writer.Write(StrGain);
			writer.Write(DexGain);
			writer.Write(IntGain);
		}

		public override void Deserialize(GenericReader reader)
		{
			int version = 0;
			if (Fix)
				version = reader.ReadInt(); //version
			LastPowerHour = reader.ReadDateTime();
			StrGain = reader.ReadDouble();
			DexGain = reader.ReadDouble();
			IntGain = reader.ReadDouble();
		}
	}
}
