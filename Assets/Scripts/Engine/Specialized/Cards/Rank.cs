using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
	public enum Rank
	{
		Ace = 14,
		King = 13,
		Queen = 12,
		Jack = 11,
		Ten = 10,
		Nine = 9,
		Eight = 8,
		Seven = 7,
		Six = 6,
		Five = 5,
		Four = 4,
		Three = 3,
		Two = 2
	}

	public class RankComparer : IComparer<Card>
	{
		public int Compare(Card card1, Card card2)
		{
			int value1 = ((int) card1.Rank) * 100 + ((int) card1.Suit);
			int value2 = ((int) card2.Rank) * 100 + ((int) card2.Suit);
			return value1 - value2;
		}
	}
}