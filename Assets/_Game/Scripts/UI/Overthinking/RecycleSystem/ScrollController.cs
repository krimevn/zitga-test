using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollController : MonoBehaviour, IScrollControlDataSource
{
    public static ScrollController instance
    {
        get {
            if (!_instance)
            {
                _instance = new GameObject().AddComponent<ScrollController>();
                instance.name = _instance.GetType().ToString();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    private static ScrollController _instance;

    public int stage_unlocked = 4; //test unlocked level

    [SerializeField] RectTransform prefabCell;
    [SerializeField] PoolingLevelCell _poolingCells;
    [SerializeField] int _dataLength;

    [SerializeField] List<LevelInfor> _levelList = new List<LevelInfor>();
    public Coroutine initPoolCoroutine;

    private void Awake()
    {
        _instance = this;
        //InitData();
        _poolingCells.DataSource = this;
    }

    public void InitData() //change this to load data from levelController
    {
        Debug.Log("Init data " + _dataLength); //change this to init if don't have data, then run this for random data.
        SaveData data = LevelController.instance.data;
        if (_levelList != null) _levelList.Clear();

        for (int i = 0; i < _dataLength; i++)
        {
            Debug.Log("Init .. " + i + " -- " + data.stage_unlocked);
            LevelInfor lv = new();
            lv.levelId = i;
            if (i <= data.stage_unlocked)
            {
                lv.isUnlocked = true;
                lv.stars = data.levelDatas[i];
            }
            else
            {
                lv.isUnlocked = false;
            }
            _levelList.Add(lv);
        }



        //if (_levelList != null) _levelList.Clear();
        //System.Random rd = new System.Random();

        //for (int i = 0; i < _dataLength; i++)
        //{
        //    LevelInfor lv = new LevelInfor();
        //    lv.levelId = i + 1; //remember this
        //    lv.stars = rd.Next(1, 4);
        //    if (i + 1 <= stage_unlocked)
        //        lv.isUnlocked = true;
        //    else
        //        lv.isUnlocked = false;

        //    _levelList.Add(lv);
        //}

        //_poolingCells.Init(this,null);
        StartCoroutine(_poolingCells.Init(this, prefabCell, OnInitialized));
        //InitPool();
    }

    public void InitPool()
    {
        if (initPoolCoroutine != null) return;

        initPoolCoroutine = StartCoroutine(_poolingCells.Init(this, prefabCell,
            OnInitialized));
    }

    public void OnInitialized()
    {
        initPoolCoroutine = null;
    }

    public int GetItemCount()
    {
        return _levelList.Count;
    }

    public void SetCell(ICell2 cell, int index, int columnIndex)
    {
        var item = (LevelCell)cell;
        //Debug.Log("SetCell: " + index + " -- " + _levelList.Count);
        if (index >= _levelList.Count)
        {
            Debug.Log("End of list");
            return;
        }
        else
        {
            item.InitCell(_levelList[index], index, columnIndex, null);//temp set null event onclick
        }
    }
}

[System.Serializable]
public struct LevelInfor
{
    public int levelId;
    public bool isUnlocked;
    public int stars;
}
