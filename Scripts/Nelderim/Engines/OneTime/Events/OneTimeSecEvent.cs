#region References

using System;

#endregion

namespace Server.OneTime.Events
{
	public static class OneTimeSecEvent
	{
		public static event EventHandler SecTimerTick;

		public static void SendTick(object o, int time)
		{
			if (time == 1)
			{
				if (SecTimerTick != null)
				{
					SecTimerTick.Invoke(o, EventArgs.Empty);

					OneTimeEventHelper.SendIOneTime(3);
				}
			}
		}
	}
}
