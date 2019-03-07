using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
	public class Trick: BiDictionary<Player, Card>
	{
		public new void Add(Player player, Card card)
		{
			base.Add(player, card);
		}

		public Card GetCard(Player player)
		{
			return Get(player);
		}

		public Player GetPlayer(Card card)
		{
			return Get(card);
		}

		public new void Remove(Player player)
		{
			base.Remove(player);
		}

		public new void Remove(Card card)
		{
			base.Remove(card);
		}
	}
}