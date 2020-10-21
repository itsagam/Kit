using Cysharp.Threading.Tasks;
using Engine;
using Engine.Parsers;
using UnityEngine;

namespace Game
{
	public static class DataManager
	{
		public const ResourceFolder DataFolder = ResourceFolder.StreamingAssets;
		public const string GameDataFile = "Data/GameData.json";
		public const string GameStateFile = "Data/GameState.json";

		public static GameData GameData;
		public static GameState GameState;

		// Makes sure GameData is always loaded in any scene in editor and dev builds
#if UNITY_EDITOR || DEVELOPMENT_BUILD
		static DataManager()
		{
			if (GameData == null)
				GameData = ResourceManager.Load<GameData>(DataFolder, GameDataFile);
		}
#endif

		public static async UniTask LoadData()
		{
			if (GameData == null)
				GameData = await LoadGameData();
			if (GameState == null)
				GameState = await LoadGameState();
		}

		public static UniTask<GameData> LoadGameData()
		{
			return ResourceManager.LoadAsync<GameData>(DataFolder, GameDataFile);
		}

		public static UniTask<GameState> LoadGameState(bool clearGameState = true)
		{
			if (clearGameState || !ResourceManager.Exists(ResourceFolder.PersistentData, GameStateFile))
				return ResourceManager.LoadAsync<GameState>(DataFolder, GameStateFile);
			return ResourceManager.LoadAsync<GameState>(ResourceFolder.PersistentData, GameStateFile);
		}

		public static UniTask SaveGameState()
		{
			return IsGameStateLoaded ?
					   ResourceManager.SaveAsync(ResourceFolder.PersistentData, GameStateFile, GameState) :
					   UniTask.CompletedTask;
		}

		public static bool IsGameDataLoaded => GameData   != null;
		public static bool IsGameStateLoaded => GameState != null;
	}
}