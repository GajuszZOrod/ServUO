#region References

using System;

#endregion

namespace Server.OneTime.Events
{
	public static class OneTimeDayEvent
	{
		public static event EventHandler DayTimerTick;

		public static void SendTick(object o, int time)
		{
			if (time == 1)
			{
				if (DayTimerTick != null)
				{
					DayTimerTick.Invoke(o, EventArgs.Empty);

					OneTimeEventHelper.SendIOneTime(6);
				}
			}
		}
	}
}
