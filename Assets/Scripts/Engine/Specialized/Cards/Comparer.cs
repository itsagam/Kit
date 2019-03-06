using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cards
{
	public class CardComparer
	{
		public static readonly SuitComparer Suit = new SuitComparer();
		public static readonly RankComparer Rank = new RankComparer();

		public class RankComparer : IComparer<Card>
		{
			public int Compare(Card card1, Card card2)
			{
				int value1 = ((int) card1.Rank) * 100 + ((int) card1.Suit);
				int value2 = ((int) card2.Rank) * 100 + ((int) card2.Suit);
				return value1 - value2;
			}
		}

		public class SuitComparer : IComparer<Card>
		{
			public int Compare(Card card1, Card card2)
			{
				int value1 = ((int) card1.Suit) * 100 + ((int) card1.Rank);
				int value2 = ((int) card2.Suit) * 100 + ((int) card2.Rank);
				return value1 - value2;
			}
		}
	}
}