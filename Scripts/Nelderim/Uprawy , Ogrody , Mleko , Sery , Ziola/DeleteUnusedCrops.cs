#region References

using System;
using System.Collections;
using Server.Commands;
using Server.Items.Crops;

#endregion

namespace Server.Misc
{
	public class DeleteUnusedCrops
	{
		public static void Initialize()
		{
			CommandSystem.Register("DeleteUnusedCrops", AccessLevel.Administrator, DeleteUnusedCrops_OnCommand);
		}

		[Usage("DeleteUnusedCrops")]
		[Description("Deletes unused or not needed items.")]
		public static void DeleteUnusedCrops_OnCommand(CommandEventArgs e)
		{
			int i_Count = 0;
			ArrayList toDelete = new ArrayList();

			try
			{
				foreach (Item item in World.Items.Values)
				{
					if (item is BaseCrop)
					{
						if (item.Map == Map.Felucca || item.Map == Map.Trammel || item.Map == Map.Ilshenar ||
						    item.Map == Map.Malas || item.Map == Map.Tokuno)
						{
							string sSowerProp = Properties.GetValue(e.Mobile, item, "Sower");
							if (sSowerProp == "Sower = (-null-)" || sSowerProp == "Sower = null" ||
							    sSowerProp == "null")
							{
								i_Count++;
								CommandLogging.WriteLine(e.Mobile, "{0} {1} delete {2} [{3}]: {4} ({5} '{6}'))",
									e.Mobile.AccessLevel, CommandLogging.Format(e.Mobile), item.Location, item.Map,
									item.GetType().Name, item.Name, sSowerProp);
								e.Mobile.SendMessage("{0}", sSowerProp);
								toDelete.Add(item);
							}
						}
					}
				}

				for (int i = 0; i < toDelete.Count; ++i)
				{
					if (toDelete[i] is Item) ((Item)toDelete[i]).Delete();
				}

				e.Mobile.SendMessage(i_Count + " Item's deleted.");
			}
			catch (Exception err)
			{
				e.Mobile.SendMessage("Exception: " + err.Message);
			}
		}
	}
}
