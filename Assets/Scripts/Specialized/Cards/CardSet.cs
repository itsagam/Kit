using System;
using System.Collections.Generic;
using System.Linq;

namespace Cards
{
	public class CardSet : List<Card>
	{
		public CardSet()
		{
		}

		public CardSet(IEnumerable<Card> cards)
		{
			Add(cards);
		}

		public new virtual CardSet Add(Card card)
		{
			base.Add(card);
			return this;
		}

		public CardSet Add(IEnumerable<Card> cards)
		{
			foreach (Card card in cards)
				Add(card);
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

		public new CardSet Sort()
		{
			base.Sort((card1, card2) => card1.CompareTo(card2.Suit) * 100 + card1.CompareTo(card2.Rank));
			return this;
		}

		public new CardSet Sort(Comparison<Card> comparison)
		{
			base.Sort(comparison);
			return this;
		}

		public CardSet SortByRank()
		{
			return Sort((c1, c2) => c1.CompareTo(c2.Rank));
		}

		public CardSet SortByRank(Comparison<Rank> comparison)
		{
			base.Sort((c1, c2) => comparison(c1.Rank, c2.Rank));
			return this;
		}

		public CardSet SortBySuit()
		{
			return Sort((c1, c2) => c1.CompareTo(c2.Suit));
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

		public IEnumerable<Card> GetAbove(Card card, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => c >= card) :
					   this.Where(c => c > card);
		}

		public IEnumerable<Card> GetAbove(Rank rank, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => c >= rank) :
					   this.Where(c => c > rank);
		}

		public IEnumerable<Card> GetAbove(Card card, Comparison<Card> comparison, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => comparison(c, card) >= 0) :
					   this.Where(c => comparison(c, card) > 0);
		}

		public IEnumerable<Card> GetAbove(Rank rank, Comparison<Rank> comparison, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => comparison(c.Rank, rank) >= 0) :
					   this.Where(c => comparison(c.Rank, rank) > 0);
		}

		public IEnumerable<Card> GetBelow(Card card, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => c <= card) :
					   this.Where(c => c < card);
		}

		public IEnumerable<Card> GetBelow(Rank rank, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => c <= rank) :
					   this.Where(c => c < rank);
		}

		public IEnumerable<Card> GetBelow(Card card, Comparison<Card> comparison, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => comparison(c, card) <= 0) :
					   this.Where(c => comparison(c, card) < 0);
		}

		public IEnumerable<Card> GetBelow(Rank rank, Comparison<Rank> comparison, bool inclusive = true)
		{
			return inclusive ?
					   this.Where(c => comparison(c.Rank, rank) <= 0) :
					   this.Where(c => comparison(c.Rank, rank) < 0);
		}

		public Card GetHighest()
		{
			return this.Max();
		}

		public Card GetHightest(Suit suit)
		{
			return GetBySuit(suit).Max();
		}

		public Card GetHighest(Comparison<Card> comparison)
		{
			return this.Aggregate((c1, c2) => comparison(c1, c2) > 0 ? c1 : c2);
		}

		public Card GetHighest(Comparison<Rank> comparison)
		{
			return this.Aggregate((c1, c2) => comparison(c1.Rank, c2.Rank) > 0 ? c1 : c2);
		}

		public Card GetLowest()
		{
			return this.Min();
		}

		public Card GetLowest(Suit suit)
		{
			return GetBySuit(suit).Min();
		}

		public Card GetLowest(Comparison<Card> comparison)
		{
			return this.Aggregate((c1, c2) => comparison(c1, c2) < 0 ? c1 : c2);
		}

		public Card GetLowest(Comparison<Rank> comparison)
		{
			return this.Aggregate((c1, c2) => comparison(c1.Rank, c2.Rank) < 0 ? c1 : c2);
		}

		public CardSet Move(List<Card> to)
		{
			to.AddRange(this);
			Clear();
			return this;
		}

		public bool Move(List<Card> to, Card card)
		{
			if (!Remove(card))
				return false;
			to.Add(card);
			return true;
		}

		public bool Move(List<Card> to, IEnumerable<Card> cards)
		{
			to.AddRange(cards);
			return Remove(cards);
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

		public new virtual bool Remove(Card card)
		{
			return base.Remove(card);
		}

		public bool Remove(IEnumerable<Card> cards)
		{
			bool success = true;
			foreach (Card card in cards)
				if (!Remove(card))
					success = false;
			return success;
		}

		public bool IsEmpty => Count > 0;
		public Card Top => this[0];
		public Card Bottom => this[Count - 1];
		public Card First => this[0];
		public Card Last => this[Count - 1];
	}
}