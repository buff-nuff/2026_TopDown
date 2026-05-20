using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerData
{
    public List<string> collectedItems = new List<string>();
    public int stage = 1;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;
    public PlayerData playerData;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData(PlayerData playerData)
    {
        string filePath = Application.persistentDataPath + "/player_data.json;";
        string json = JsonUtility.ToJson(playerData, true);
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log("게임 데이터 저장됨 : " + json);
    }

    public PlayerData LoadData()
    {
        string filePath = Application.persistentDataPath + "/player_data.json;";
        if (System.IO.File.Exists(filePath))
        {
            string json = System.IO.File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
            Debug.Log("게임 데이터 로드됨 : " + json);
            return playerData;
        }
        else
        {
            Debug.LogWarning("저장된 게임 데이터가 없습니다.");
            return new PlayerData();
        }
    }
    public void GameStart()
    {
        PlayerData playerData = LoadData();
        if (playerData == null)
        {
            playerData = new PlayerData();
            SceneManager.LoadScene("Level_1");
        }
        else
        {
            SceneManager.LoadScene("Level_" + playerData.stage);
        }
    }

    public void PlayerDead()
    {
        PlayerData playerData = LoadData();
        if (playerData != null)
        {
            playerData.stage = 1;

            foreach (string item in playerData.collectedItems.ToList())
            {
                if (UnityEngine.Random.Range(0, 1) == 0)
                {
                    playerData.collectedItems.Remove(item);
                }
            }

            SaveData(playerData);
        }
        SceneManager.LoadScene("GameOver");
    }
} 
