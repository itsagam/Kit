using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Async;
using Modding.Parsers;
using Game;

public class DataManager
{
	public const ResourceFolder DataFolder = ResourceFolder.StreamingAssets;
	public const string GameDataFile = "Data/GameData.json";
	public const string GameStateFile = "Data/GameState.json";

	public static GameData GameData;
	public static GameState GameState;

	public static bool ClearGameState = false;
	
	static DataManager()
	{
		Observable.OnceApplicationQuit().Subscribe(u => SaveGameState());
	}

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

	public static UniTask<GameState> LoadGameState()
	{
		if (ClearGameState || !ResourceManager.Exists(ResourceFolder.PersistentData, GameStateFile))
			return ResourceManager.LoadAsync<GameState>(DataFolder, GameStateFile);
		else
			return ResourceManager.LoadAsync<GameState>(ResourceFolder.PersistentData, GameStateFile);
	}

	public static UniTask SaveGameState()
	{
		if (IsGameStateLoaded)
			return ResourceManager.SaveAsync(ResourceFolder.PersistentData, GameStateFile, GameState);
		return UniTask.CompletedTask;
	}

	public static bool IsGameDataLoaded
	{
		get
		{
			return GameData != null;
		}
	}

	public static bool IsGameStateLoaded
	{
		get
		{
			return GameState != null;
		}
	}
}