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

// When comparing with relational operators like ==, cards are always treated value-type.

using System;
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

		public static bool operator ==(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return ReferenceEquals(card1, card2);
			return card1.Suit == card2.Suit && card1.Rank == card2.Rank;
		}

		public static bool operator !=(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return !ReferenceEquals(card1, card2);
			return card1.Suit != card2.Suit || card1.Rank != card2.Rank;
		}

		public static bool operator >(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return false;
			return card1.Suit == card2.Suit && card1.CompareTo(card2) > 0;
		}

		public static bool operator <(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return false;
			return card1.Suit == card2.Suit && card1.CompareTo(card2) < 0;
		}

		public static bool operator >=(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return ReferenceEquals(card1, card2);
			return card1.Suit == card2.Suit && card1.CompareTo(card2) >= 0;
		}

		public static bool operator <=(Card card1, Card card2)
		{
			if (ReferenceEquals(card1, null) || ReferenceEquals(card2, null))
				return ReferenceEquals(card1, card2);
			return card1.Suit == card2.Suit && card1.CompareTo(card2) <= 0;
		}

		public static bool operator ==(Card card, Suit suit)
		{
			return !ReferenceEquals(card, null) && card.Suit == suit;
		}

		public static bool operator !=(Card card, Suit suit)
		{
			return !ReferenceEquals(card, null) && card.Suit != suit;
		}

		public static bool operator ==(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.Rank == rank;
		}

		public static bool operator !=(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.Rank != rank;
		}

		public static bool operator >(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.CompareTo(rank) > 0;
		}

		public static bool operator <(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.CompareTo(rank) < 0;
		}

		public static bool operator >=(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.CompareTo(rank) >= 0;
		}

		public static bool operator <=(Card card, Rank rank)
		{
			return !ReferenceEquals(card, null) && card.CompareTo(rank) <= 0;
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

#if !CARDS_REFERENCE_TYPE
		public bool Equals(Card other)
		{
			return this == other;
		}

		public override bool Equals(object obj)
		{
			if (obj is Card card)
				return this == card;
			return false;
		}

		public override int GetHashCode()
		{
			return Suit.GetHashCode() * 100 + Rank.GetHashCode();
		}
#endif

		public override string ToString()
		{
			return $"{Rank} of {Suit}s";
		}
	}
}