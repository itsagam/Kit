using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cards
{
	public class CardSet : List<Card>
	{
		public CardSet()
		{
		}

		public CardSet(IEnumerable<Card> cards)
		{
			AddRange(cards);
		}

		public new CardSet Add(Card card)
		{
			base.Add(card);
			return this;
		}

		public CardSet Add(IEnumerable<Card> cards)
		{
			AddRange(cards);
			return this;
		}

		public CardSet Fill(bool clear = true)
		{
			if (clear)
				Clear();

			foreach (Suit suit in Enum.GetValues(typeof(Suit)))
				foreach (Rank rank in Enum.GetValues(typeof(Rank)))
					Add(new Card(suit, rank));

			return this;
		}

		// Modern Fisher–Yates shuffle
		public CardSet Shuffle()
		{
			for (int i = Count; i > 1; i--)
			{
				int j = UnityEngine.Random.Range(0, i);
				Swap(j, i - 1);
			}
			return this;
		}

		public CardSet Swap(int index1, int index2)
		{
			Card tmp = this[index1];
			this[index1] = this[index2];
			this[index2] = tmp;
			return this;
		}

		public new CardSet Sort(Comparison<Card> comparison)
		{
			base.Sort(comparison);
			return this;
		}

		public CardSet SortByRank()
		{
			return Sort((card1, card2) => card1.CompareTo(card2.Rank));
		}

		public CardSet SortByRank(Comparison<Rank> comparison)
		{
			base.Sort((card1, card2) => comparison(card1.Rank, card2.Rank));
			return this;
		}

		public CardSet SortBySuit()
		{
			return Sort((card1, card2) => card1.CompareTo(card2.Suit));
		}

		public CardSet SortBySuit(Comparison<Suit> comparison)
		{
			base.Sort((card1, card2) => comparison(card1.Suit, card2.Suit));
			return this;
		}

		public Dictionary<Suit, IEnumerable<Card>> GetBySuit()
		{
			return this.GroupBy(c => c.Suit).ToDictionary(g => g.Key, g => g.AsEnumerable());
		}

		public IEnumerable<Card> GetBySuit(Suit suit)
		{
			return this.Where(c => c.Suit == suit);
		}

		public Dictionary<Rank, IEnumerable<Card>> GetByRank()
		{
			return this.GroupBy(c => c.Rank).ToDictionary(g => g.Key, g => g.AsEnumerable());
		}

		public IEnumerable<Card> GetByRank(Rank rank)
		{
			return this.Where(c => c == rank);
		}

		public IEnumerable<Card> GetAboveRank(Rank rank, bool inclusive = true)
		{
			if (inclusive)
				return this.Where(c => c >= rank);
			else
				return this.Where(c => c > rank);
		}

		public IEnumerable<Card> GetAboveRank(Rank rank, Comparison<Rank> comparison, bool inclusive = true)
		{
			if (inclusive)
				return this.Where(c => c.CompareTo(rank, comparison) >= 0);
			else
				return this.Where(c => c.CompareTo(rank, comparison) > 0);
		}

		public IEnumerable<Card> GetBelowRank(Rank rank, bool inclusive = true)
		{
			if (inclusive)
				return this.Where(c => c <= rank);
			else
				return this.Where(c => c < rank);
		}

		public IEnumerable<Card> GetBelowRank(Rank rank, Comparison<Rank> comparison, bool inclusive = true)
		{
			if (inclusive)
				return this.Where(c => c.CompareTo(rank, comparison) <= 0);
			else
				return this.Where(c => c.CompareTo(rank, comparison) < 0);
		}

		public CardSet Move(List<Card> to)
		{
			to.AddRange(this);
			Clear();
			return this;
		}

		public bool Move(List<Card> to, Card card)
		{
			if (Remove(card))
			{
				to.Add(card);
				return true;
			}
			return false;
		}

		public bool Move(List<Card> to, IEnumerable<Card> cards)
		{
			if (Remove(cards))
			{
				to.AddRange(cards);
				return true;
			}
			return false;
		}
		
		public Card Deal()
		{
			Card top = Top;
			Remove(top);
			return top;
		}

		public CardSet Deal(int numberOfCards)
		{
			CardSet dealt = new CardSet();
			for (int i = 0; i < numberOfCards; i++)
				dealt.Add(Deal());
			return dealt;
		}

		public Card Deal(List<Card> to)
		{
			Card top = Top;
			Move(to, top);
			return top;
		}

		public CardSet Deal(List<Card> to, int numberOfCards)
		{
			CardSet dealt = new CardSet();
			for (int i = 0; i < numberOfCards; i++)
				dealt.Add(Deal(to));
			return dealt;
		}

		public CardSet Deal(IEnumerable<List<Card>> sets)
		{
			CardSet dealt = new CardSet();
			foreach (var to in sets)
				dealt.Add(Deal(to));
			return dealt;
		}

		public CardSet Deal(IEnumerable<List<Card>> sets, int numberOfCards)
		{
			CardSet dealt = new CardSet();
			foreach (var to in sets)
				for (int i=0; i<numberOfCards; i++)
					dealt.Add(Deal(to));
			return dealt;
		}

		public bool Remove(IEnumerable<Card> cards)
		{
			bool success = true;
			foreach (Card card in cards)
				if (!Remove(card))
					success = false;
			return success;
		}

		public bool IsEmpty
		{
			get
			{
				return Count > 0;
			}
		}

		public Card Top
		{
			get
			{
				return this[0];
			}
		}

		public Card Bottom
		{
			get
			{
				return this[Count - 1];
			}
		}


		public Card First
		{
			get
			{
				return this[0];
			}
		}


		public Card Last
		{
			get
			{
				return this[Count - 1];
			}
		}
	}
}