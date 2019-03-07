//#define CARDS_REFERENCE_TYPE
// Uncomment the line above if you want to treat cards as a reference-type when comparing in lists,
// dictionaries and using Equals. This is important when you can have duplicates of a card in the
// same set and have to perform an operation on a particular instance. The following code will have
// different results depending on the line above:

// Card card1 = new Card(Suit.Club, Rank.Eight);
// Card card2 = new Card(Suit.Club, Rank.Five);
// Card card3 = new Card(Suit.Club, Rank.Seven);
// Card card4 = new Card(Suit.Club, Rank.Five);
// var cardSet1 = new CardSet { card1, card2, card3, card4 };
// cardSet1.Remove(card4);

// When comparing with ==, cards are always treated value-type.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Cards
{
	[Serializable]
	[InlineProperty]
	public class Card : IComparable<Card>, IComparable<Rank>, IEquatable<Card>
	{
		[HorizontalGroup]
		[HideLabel]
		public Suit Suit = Suit.Heart;

		[HorizontalGroup]
		[HideLabel]
		public Rank Rank = Rank.Two;

		public Card()
		{
		}

		public Card(Suit suit, Rank rank)
		{
			Suit = suit;
			Rank = rank;
		}

		public void Set(Suit suit, Rank rank)
		{
			Suit = suit;
			Rank = rank;
		}

		public void Swap(Card other)
		{
			Suit suit = Suit;
			Rank rank = Rank;
			Suit = other.Suit;
			Rank = other.Rank;
			other.Suit = suit;
			other.Rank = rank;
		}

		public static void Swap(ref Card card1, ref Card card2)
		{
			Card tmp = card1;
			card1 = card2;
			card2 = tmp;
		}

		public static bool operator ==(Card card1, Card card2)
		{
			return card1.Suit == card2.Suit && card1.Rank == card2.Rank;
		}

		public static bool operator !=(Card card1, Card card2)
		{
			return card1.Suit != card2.Suit || card1.Rank != card2.Rank;
		}

		public static bool operator >(Card card1, Card card2)
		{
			return card1.CompareTo(card2) > 0;
		}

		public static bool operator <(Card card1, Card card2)
		{
			return card1.CompareTo(card2) < 0;
		}

		public static bool operator >=(Card card1, Card card2)
		{
			return card1.CompareTo(card2) >= 0;
		}

		public static bool operator <=(Card card1, Card card2)
		{
			return card1.CompareTo(card2) <= 0;
		}

		public static bool operator ==(Card card, Suit suit)
		{
			return card.Suit == suit;
		}

		public static bool operator !=(Card card, Suit suit)
		{
			return card.Suit != suit;
		}

		public static bool operator ==(Card card, Rank rank)
		{
			return card.Rank == rank;
		}

		public static bool operator !=(Card card, Rank rank)
		{
			return card.Rank != rank;
		}

		public static bool operator >(Card card, Rank rank)
		{
			return card.CompareTo(rank) > 0;
		}

		public static bool operator <(Card card, Rank rank)
		{
			return card.CompareTo(rank) < 0;
		}

		public static bool operator >=(Card card, Rank rank)
		{
			return card.CompareTo(rank) >= 0;
		}

		public static bool operator <=(Card card, Rank rank)
		{
			return card.CompareTo(rank) <= 0;
		}

		public int CompareTo(Card other)
		{
			return CompareTo(other.Rank);
		}

		public int CompareTo(Rank rank)
		{
			return Rank - rank;
		}

		public int CompareTo(Suit suit)
		{
			return Suit - suit;
		}

		public int CompareTo(Card other, Comparison<Card> comparison)
		{
			return comparison(this, other);
		}

		public int CompareTo(Rank rank, Comparison<Rank> comparison)
		{
			return comparison(Rank, rank);
		}

		public int CompareTo(Suit suit, Comparison<Suit> comparison)
		{
			return comparison(Suit, suit);
		}

		public bool Equals(Card other)
		{
#if CARDS_REFERENCE_TYPE
			return base.Equals(other);
#else
			return this == other;
#endif
		}

		public override bool Equals(object obj)
		{
#if CARDS_REFERENCE_TYPE
			return base.Equals(obj);
#else
			if (obj is Card card)
				return this == card;
			return false;
#endif
		}

		public override int GetHashCode()
		{
#if CARDS_REFERENCE_TYPE
			return base.GetHashCode();
#else
			return Suit.GetHashCode() * 100 + Rank.GetHashCode();
#endif
		}

		public override string ToString() => $"{Rank} of {Suit}s";
	}
}