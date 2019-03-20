using Engine;
using UnityEngine;

namespace Game
{
	public class Test: MonoBehaviour
	{
		private void Awake()
		{
			int[] xpLevels = { 100, 250, 1000, 1500, 5000 };
			print(GetLevelFromXP(xpLevels, 59));
			print(GetPreviousLevelXP(xpLevels, 5));
			print(GetNextLevelXP(xpLevels, 5));
			print(GetXPToNextLevel(xpLevels, 345));
		}

		public static int GetMaxLevel(int[] xpCurve)
		{
			return xpCurve.Length;
		}

		public static int GetLevelFromXP(int[] xpCurve, int currentXP)
		{
			int level = xpCurve.FindIndex(xp => xp > currentXP);
			return level >= 0 ? level : GetMaxLevel(xpCurve);
		}

		public static int GetNextLevelXP(int[] xpCurve, int currentLevel)
		{
			return currentLevel < GetMaxLevel(xpCurve) ? xpCurve[currentLevel] : int.MaxValue;
		}

		public static int GetPreviousLevelXP(int[] xpCurve, int currentLevel)
		{
			return currentLevel > 0 ? xpCurve[currentLevel - 1] : 0;
		}

		public static int GetXPToNextLevel(int[] xpCurve, int currentXP)
		{
			int currentLevel = GetLevelFromXP(xpCurve, currentXP);
			return GetNextLevelXP(xpCurve, currentLevel) - currentXP;
		}

		public static int GetXPToNextLevel(int[] xpCurve, int currentLevel, int currentXP)
		{
			return GetNextLevelXP(xpCurve, currentLevel) - currentXP;
		}
	}
}