namespace Nelderim.Factions
{
	public class East : Faction
	{
		public East(int index) : base(index)
		{
		}
		
		public override string Name => "Frakcja1";

		public override Faction[] Enemies => new[] { West };
	}
}