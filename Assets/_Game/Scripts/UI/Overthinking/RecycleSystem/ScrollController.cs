using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollController : MonoBehaviour, IScrollControlDataSource
{
    public int stage_unlocked = 4; //test unlocked level

    [SerializeField] RectTransform prefabCell;
    [SerializeField] PoolingLevelCell _poolingCells;
    [SerializeField] int _dataLength;

    [SerializeField] List<LevelInfor> _levelList = new List<LevelInfor>();

    private void Awake()
    {
        InitData();
        //_poolingCells.DataSource = this;
    }

    void InitData()
    {
        Debug.Log("Init data " + _dataLength); //change this to init if don't have data, then run this for random data.
        if (_levelList != null) _levelList.Clear();
        System.Random rd = new System.Random();

        for (int i = 0; i < _dataLength; i++)
        {
            LevelInfor lv = new LevelInfor();
            lv.levelId = i+1; //remember this
            lv.stars = rd.Next(1,3);
            if (i + 1 <= stage_unlocked)
                lv.isUnlocked = true;
            else
                lv.isUnlocked = false;

            _levelList.Add(lv);
        }

        //_poolingCells.Init(this,null);
        StartCoroutine(_poolingCells.Init(this, prefabCell, null));
    }


    public int GetItemCount()
    {
        return _levelList.Count;
    }

    public void SetCell(ICell2 cell, int index, int columnIndex)
    {
        var item = (LevelCell)cell;
        Debug.Log("SetCell: " + index + " -- " + _levelList.Count);
        if (index >= _levelList.Count)
        {
            Debug.Log("End of list, disable button");
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
