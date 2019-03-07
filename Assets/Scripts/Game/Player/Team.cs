using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team
{
	public List<TeamPlayer> Players { get; protected set; } = new List<TeamPlayer>();

	public Team()
	{	
	}

	public Team(IEnumerable<TeamPlayer> players)
	{
		foreach (var player in Players)
			AddPlayer(player);
	}

	public void AddPlayer(TeamPlayer player)
	{
		Players.Add(player);
		player.Team = this;
	}

	public void RemovePlayer(TeamPlayer player)
	{
		player.Team = null;
		Players.Remove(player);
	}

	public int CurrentScore
	{
		get
		{
			return Players.Select(p => p.CurrentScore).Sum();
		}
	}

	public int TotalScore
	{
		get
		{
			return Players.Select(p => p.TotalScore).Sum();
		}
	}
}