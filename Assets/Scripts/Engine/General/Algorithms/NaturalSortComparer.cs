using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Engine.Algorithms
{
	public class NaturalSortComparer : IComparer<string>, IDisposable
	{
		public readonly bool IsAscending;

		public NaturalSortComparer(bool inAscendingOrder = true)
		{
			IsAscending = inAscendingOrder;
		}

		public int Compare(string x, string y)
		{
			if (x == y)
				return 0;

			if (!table.TryGetValue(x, out string[] x1))
			{
				x1 = Regex.Split(x.Replace(" ", ""), "([0-9]+)");
				table.Add(x, x1);
			}

			if (!table.TryGetValue(y, out string[] y1))
			{
				y1 = Regex.Split(y.Replace(" ", ""), "([0-9]+)");
				table.Add(y, y1);
			}

			int returnVal;

			for (int i = 0; i < x1.Length && i < y1.Length; i++)
			{
				if (x1[i] != y1[i])
				{
					returnVal = PartCompare(x1[i], y1[i]);
					return IsAscending ? returnVal : -returnVal;
				}
			}

			if (y1.Length > x1.Length)
				returnVal = 1;
			else if (x1.Length > y1.Length)
				returnVal = -1;
			else
				returnVal = 0;

			return IsAscending ? returnVal : -returnVal;
		}

		private static int PartCompare(string left, string right)
		{
			if (!int.TryParse(left, out int x))
				return left.CompareTo(right);

			if (!int.TryParse(right, out int y))
				return left.CompareTo(right);

			return x.CompareTo(y);
		}

		private Dictionary<string, string[]> table = new Dictionary<string, string[]>();

		public void Dispose()
		{
			table.Clear();
			table = null;
		}
	}
}