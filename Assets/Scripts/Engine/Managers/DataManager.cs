using UniRx;
using UniRx.Async;
using Game;

public class DataManager
{
	public const ResourceFolder DataFolder = ResourceFolder.StreamingAssets;
	public const string GameDataFile = "Data/GameData.json";
	public const string GameStateFile = "Data/GameState.json";

	public static GameData GameData;
	public static GameState GameState;

	static DataManager()
	{
		Observable.OnceApplicationQuit().Subscribe(u => SaveGameState());
	}

	public static async UniTask LoadData()
	{
		GameData = await LoadGameData();
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

	public static bool IsGameDataLoaded => GameData != null;
	public static bool IsGameStateLoaded => GameState != null;
}