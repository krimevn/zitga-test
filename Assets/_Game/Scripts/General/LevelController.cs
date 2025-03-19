using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

public class LevelController : SerializedMonoBehaviour
{
    //simple singleton
    public static LevelController instance {
        get {
            if (!_instance)
            {
                _instance = new GameObject().AddComponent<LevelController>();
                _instance.name = _instance.GetType().ToString();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    private static LevelController _instance;

    public SaveData data;
    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey(StringConstance.SaveDataString))
        {
            //CreateNewData();
            CreateRandomData(); //1st time, random data
        }
        LoadData();
    }

    public void CreateRandomData()
    {
        data = new();
        data.stage_unlocked = 50;
        data.levelDatas = new();
        System.Random rd = new System.Random();

        for (int i = 0; i <= data.stage_unlocked; i++)
        {
            data.levelDatas.Add(i, rd.Next(1, 4));
        }
        SaveData();
    }

    public void ForceCreateNewRandomData()
    {
        PlayerPrefs.DeleteKey(StringConstance.SaveDataString);
        CreateRandomData();
        SaveData();
        LoadData();
    }

    public void CreateNewData()
    {
        data = new();
        data.stage_unlocked = 0;
        data.levelDatas = new();
        data.levelDatas.Add(0, 0);
        SaveData();
    }
    
    public void SaveData()
    {
        string json = JsonConvert.SerializeObject(data);
        PlayerPrefs.SetString(StringConstance.SaveDataString, json);
    }

    public void LoadData()
    {
        string json = PlayerPrefs.GetString(StringConstance.SaveDataString); //json
        data = new();
        data = JsonConvert.DeserializeObject<SaveData>(json);
        ScrollController.instance.InitData();
    }

    public void ResetData()
    {
        PlayerPrefs.DeleteKey(StringConstance.SaveDataString);
        CreateNewData();
        LoadData();
    }
}

[SerializeField]
public class SaveData
{
    public int stage_unlocked; //this for most recent level playing.
    public Dictionary<int, int> levelDatas;// key levelid, value stars
}