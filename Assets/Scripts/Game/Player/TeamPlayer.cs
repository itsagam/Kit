using System.Collections.Generic;
using System.Linq;

namespace Game
{
	public class TeamPlayer : Player
	{
		public Team Team;

		public bool IsPartner(TeamPlayer player)
		{
			return player.Team == Team;
		}

		public IEnumerable<TeamPlayer> Partners => Team.Players.Where(p => p != this);
		public TeamPlayer Partner => Team.Players.Find(p => p != this);
	}
}