using System.Collections.Generic;
using System.Linq;

public class TeamPlayer : Player
{
	public Team Team;

	public bool IsPartner(TeamPlayer player)
	{
		return player.Team == Team;
	}

	public IEnumerable<TeamPlayer> Partners
	{
		get
		{
			return Team.Players.Where(p => p != this);
		}
	}

	public TeamPlayer Partner
	{
		get
		{
			return Team.Players.Find(p => p != this);
		}
	}
}