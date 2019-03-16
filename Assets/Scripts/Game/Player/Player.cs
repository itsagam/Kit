using System;
using UnityEngine;

namespace Game
{
	public class Player
	{
		protected int currentScore;
		protected int totalScore;

		public event Action<int> CurrentScoreChanged;
		public event Action<int> TotalScoreChanged;

		public void IncreaseCurrentScore(int by = 1)
		{
			CurrentScore += by;
		}

		public void DecreaseCurrentScore(int by = 1)
		{
			CurrentScore -= by;
		}

		public void ResetCurrentScore()
		{
			CurrentScore = 0;
		}

		public void IncreaseTotalScore(int by = 1)
		{
			TotalScore += by;
		}

		public void DecreaseTotalScore(int by = 1)
		{
			TotalScore -= by;
		}

		public void ResetTotalScore()
		{
			TotalScore = 0;
		}

		public int CurrentScore
		{
			get => currentScore;
			set
			{
				value = Mathf.Max(0, value);
				currentScore = value;
				CurrentScoreChanged?.Invoke(value);
			}
		}

		public int TotalScore
		{
			get => totalScore;
			set
			{
				value = Mathf.Max(0, value);
				totalScore = value;
				TotalScoreChanged?.Invoke(value);
			}
		}
	}
}