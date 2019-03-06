using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Cards
{
	[Serializable]
	[InlineProperty]
	public struct Card : IEquatable<Card>, IComparable<Card>
	{
		[HorizontalGroup]
		[HideLabel]
		public Suit Suit;

		[HorizontalGroup]
		[HideLabel]
		public Rank Rank;

		public Card(Suit suit, Rank rank)
		{
			Suit = suit;
			Rank = rank;
		}

		public static bool operator ==(Card card1, Card card2)
		{
			return card1.Equals(card2);
		}

		public static bool operator !=(Card card1, Card card2)
		{
			return !card1.Equals(card2);
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

		public int CompareTo(Card other)
		{
			return Rank - other.Rank;
		}

		public override bool Equals(object obj)
		{
			if (obj is Card card)
				return Equals(card);
			return false;
		}

		public override int GetHashCode()
		{
			return (int) Suit ^  (int) Rank;
		}

		public bool Equals(Card other)
		{
			return Suit == other.Suit && Rank == other.Rank;
		}

		public override string ToString() => $"{Rank} of {Suit}s";
	}
}