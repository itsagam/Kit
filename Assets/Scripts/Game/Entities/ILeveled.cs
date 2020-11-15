using Kit;

namespace Game
{
	public interface IXPLeveled : ILeveled
	{
		int XP { get; }
		int[] XPCurve { get; }
	}

	public interface ILeveled
	{
		int Level { get; }
		int MaxLevel { get; }
	}

	public static class LeveledExtensions
	{
		public static bool IsMaxed(this ILeveled leveled)
		{
			return leveled.Level >= leveled.MaxLevel;
		}
	}

	public static class XPLeveledExtensions
	{
		public static int GetMaxLevel(this IXPLeveled leveled)
		{
			return leveled.XPCurve.Length;
		}

		public static int GetLevelFromXP(this IXPLeveled leveled)
		{
			int level = leveled.XPCurve.FindIndex(xp => xp >= leveled.XP);
			return level >= 0 ? level : leveled.GetMaxLevel();
		}

		public static int GetNextLevelXP(this IXPLeveled leveled)
		{
			return leveled.Level < leveled.GetMaxLevel() ? leveled.XPCurve[leveled.Level] : int.MaxValue;
		}

		public static int GetPreviousLevelXP(this IXPLeveled leveled)
		{
			return leveled.Level > 0 ? leveled.XPCurve[leveled.Level - 1] : 0;
		}

		public static int GetXPToNextLevel(this IXPLeveled leveled)
		{
			return leveled.GetNextLevelXP() - leveled.XP;
		}
	}
}