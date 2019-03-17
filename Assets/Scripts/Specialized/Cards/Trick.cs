using System.Collections.Generic;
using System.Linq;
using Engine;
using Game;

namespace Cards
{
	public class Trick: CardSet
	{
		private Dictionary<Card, Player> players = new Dictionary<Card, Player>();

		public Trick()
		{
		}

		public Trick(Dictionary<Card, Player> cards)
		{
			Add(cards);
		}

		public Trick(Dictionary<Player, Card> cards)
		{
			Add(cards);
		}

		public Trick Add(Player player, Card card)
		{
			base.Add(card);
			players.Add(card, player);
			return this;
		}

		public Trick Add(Dictionary<Card, Player> cards)
		{
			foreach (var kvp in cards)
				Add(kvp.Value, kvp.Key);
			return this;
		}

		public Trick Add(Dictionary<Player, Card> cards)
		{
			foreach (var kvp in cards)
				Add(kvp.Key, kvp.Value);
			return this;
		}

		public Card GetCard(Player player)
		{
			return players.FirstOrDefault(kvp => kvp.Value == player).Key;
		}

		public Player GetPlayer(Card card)
		{
			return players.GetOrDefault(card);
		}

		public bool Remove(Player player)
		{
			Card card = GetCard(player);
			return card != null && Remove(card);
		}

		public override bool Remove(Card card)
		{
			return base.Remove(card) && players.Remove(card);
		}
	}
}