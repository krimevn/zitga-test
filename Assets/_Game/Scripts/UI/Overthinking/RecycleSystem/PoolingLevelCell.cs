using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PoolingLevelCell : ScrollRect
{
    //recycle system
    //data source
    public IScrollControlDataSource DataSource;

    [SerializeField] RectTransform Viewport, Content;
    [SerializeField] RectTransform PrefabCell;
    readonly float MinPoolCoverage = 1.5f;
    readonly int MinPoolSize = 10;
    readonly float RecyclingThreshold = 0.2f;

    //---------------------------
    [SerializeField] int _columns = 4;
    //public int column;

    List<RectTransform> _cellPool;
    List<ICell2> _cachedCells;
    Bounds _recyclableViewBounds;

    readonly Vector3[] _corners = new Vector3[4];
    bool _recycling;

    float _cellWidth, _cellHeight; //for calculate size of view, row, column

    int _currentItemCount;
    int topMostCellIndex, bottomMostCellIndex;
    int _topMostCellColumn, _bottomMostCellColumn;

    Vector2 _prevAnchoredPos;

    public IEnumerator Init(IScrollControlDataSource dataSource, RectTransform _prefabCell, UnityAction onInitialized)
    {
        Debug.Log("Init pooling ...");
        DataSource = dataSource;
        PrefabCell = _prefabCell;

        //set top anchor;
        Content.anchoredPosition = Vector3.zero; //set mid
        yield return null; //wait end frame
        SetRecycleBounds();

        CreateCellPool();
        _currentItemCount = _cellPool.Count;
        topMostCellIndex = 0;
        bottomMostCellIndex = _cellPool.Count - 1;

        int rowCount = (int)Mathf.Ceil((float)_cellPool.Count / (float)_columns);
        float contentYSize = rowCount * _cellHeight;
        Content.sizeDelta = new Vector2(Content.sizeDelta.x, contentYSize);
        //set top anchor

        _prevAnchoredPos = content.anchoredPosition;

        onValueChanged.RemoveAllListeners();
        onValueChanged.AddListener((v) => OnValueChangeListener(v));

        onInitialized?.Invoke();
    }

    void SetRecycleBounds()
    {
        Viewport.GetWorldCorners(_corners);
        float threshHold = RecyclingThreshold * (_corners[2].y - _corners[0].y);
        _recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshHold);
        _recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshHold);
    }

    void CreateCellPool()
    {
        Debug.Log("Create cell pool");
        if (_cellPool != null) //clean pool
        {
            _cellPool.ForEach((RectTransform item) => Destroy(item.gameObject));
            _cellPool.Clear();
            _cachedCells.Clear();
        }
        else
        {
            _cellPool = new List<RectTransform>();
            _cachedCells = new List<ICell2>();
        }

        //set cell

        float width = content.rect.width;
        float height = content.rect.height;

        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(0, 1);
        content.pivot = new Vector2(0, 0); //bottom left scroll
        content.sizeDelta = new Vector2(width, height);

        _topMostCellColumn = _bottomMostCellColumn = 0;

        float currentPoolCoverage = 0;
        int poolsize = 0;
        float posX = 0;
        float posY = 0;

        //set cell size
        _cellWidth = Content.rect.width / _columns;
        _cellHeight = PrefabCell.sizeDelta.y / PrefabCell.sizeDelta.x * _cellWidth;

        float requireCoverage = MinPoolCoverage * Viewport.rect.height;
        int minPoolSize = Math.Min(MinPoolSize, DataSource.GetItemCount()); //item count

        while ((poolsize < minPoolSize || currentPoolCoverage < requireCoverage) && poolsize < DataSource.GetItemCount())
        {
            RectTransform item = Instantiate(PrefabCell.gameObject).GetComponent<RectTransform>();
            item.name = "Cell";
            item.sizeDelta = new Vector2(_cellWidth, _cellHeight);
            _cellPool.Add(item);
            item.SetParent(Content, false);

            posX = _bottomMostCellColumn * _cellWidth;
            int cachedColumn = _bottomMostCellColumn;
            item.anchoredPosition = new Vector2(posX, posY);
            if (++_bottomMostCellColumn >= _columns)
            {
                _bottomMostCellColumn = 0;
                posY += _cellHeight;
                currentPoolCoverage += item.rect.height;
            }

            _cachedCells.Add(item.gameObject.GetComponent<ICell2>());
            DataSource.SetCell(_cachedCells[_cachedCells.Count - 1], poolsize, cachedColumn);
            poolsize++;
        }

        _bottomMostCellColumn = (_bottomMostCellColumn - 1 + _columns) % _columns;
    }

    #region Recycle Prefab zone

    public void OnValueChangeListener(Vector2 normalizedPos)
    {
        //Debug.Log("Check normalized pos: " + normalizedPos + " -- " + Content.anchoredPosition + " -- " + _prevAnchoredPos);
        Vector2 dir = Content.anchoredPosition - _prevAnchoredPos;
        //Debug.Log("Check Direction: " + dir);
        m_ContentStartPosition += GetNewRecyclePos(dir);
        _prevAnchoredPos = Content.anchoredPosition;
    }

    public Vector2 GetNewRecyclePos(Vector2 direction) //not corrected mean, temp
    {
        if (_recycling || _cellPool == null || _cellPool.Count == 0) return Vector2.zero;

        //recycle bound...
        SetRecycleBounds();

        //if (direction.y > 0 && _cellPool[topMostCellIndex].MaxY() > _recyclableViewBounds.min.y)
        if (direction.y > 0 && _cellPool[bottomMostCellIndex].MinY() > _recyclableViewBounds.max.y)
        {
            Debug.Log("Scroll up");
            //return RecycleTopToBottom();
            return RecycleBottomToTop();
        }
        //else if (direction.y < 0 && _cellPool[bottomMostCellIndex].MinY() < _recyclableViewBounds.max.y)
        else if (direction.y < 0 && _cellPool[topMostCellIndex].MaxY() < _recyclableViewBounds.min.y)
        {
            //return RecycleBottomToTop();
            return RecycleTopToBottom();
        }
        return Vector2.zero;
    }

    //Vector2 RecycleTopToBottom()
    Vector2 RecycleBottomToTop() //reverted name due reverted list, visual opposited with list order //change later
    {
        Debug.Log("<color=blue>RecycleBottomToTop</color>");
        _recycling = true;
        int n = 0;
        float posX = 0;
        float posY = _cellPool[topMostCellIndex].anchoredPosition.y;

        int additionalRows = 0;
        Debug.Log("Rec 0: "+ topMostCellIndex + " : " + bottomMostCellIndex);

        while (_cellPool[bottomMostCellIndex].MinY() > _recyclableViewBounds.max.y && _currentItemCount < DataSource.GetItemCount())
        {
            Debug.Log("RecycleTopToBottom run " + _topMostCellColumn);
            if (--_topMostCellColumn < 0)
            {
                Debug.Log("Rec 1: " + _topMostCellColumn);
                n++;
                _topMostCellColumn = _columns - 1;
                //_topMostCellColumn = 0;
                Debug.Log("Rec 2: " + topMostCellIndex + " : " + _cellPool[topMostCellIndex].anchoredPosition);
                posY = _cellPool[topMostCellIndex].anchoredPosition.y - _cellHeight;
                additionalRows++;
            }

            //if (_bottomMostCellColumn < 0)
            //{
            //    //n++;
            //    _bottomMostCellColumn = _columns - 1;
            //    //posY = _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellHeight;
            //    additionalRows--;
            //}

            posX = _topMostCellColumn * _cellWidth; 
            _cellPool[bottomMostCellIndex].anchoredPosition = new Vector2(posX, posY);


            DataSource.SetCell(_cachedCells[bottomMostCellIndex], _currentItemCount, _bottomMostCellColumn);
            Debug.Log("Rec 3: " + topMostCellIndex + " : " + bottomMostCellIndex);
            //bottomMostCellIndex = topMostCellIndex;
            topMostCellIndex = bottomMostCellIndex;
            //topMostCellIndex = (topMostCellIndex + 1) % _cellPool.Count;
            bottomMostCellIndex = (bottomMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;

            _currentItemCount++;
        }

        Content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
        if (additionalRows > 0)
            n -= additionalRows;

        foreach (RectTransform c in _cellPool)
        {
            c.anchoredPosition -= _cellPool[topMostCellIndex].sizeDelta.y * n * Vector2.up;
        }

        Content.anchoredPosition -= _cellPool[topMostCellIndex].sizeDelta.y * n * Vector2.up;
        _recycling = false;
        Vector2 newPos = new Vector2(0, _cellPool[topMostCellIndex].sizeDelta.y * n);
        return -newPos;
    }

    //Vector2 RecycleBottomToTop()
    Vector2 RecycleTopToBottom()
    {
        //Debug.Log("<color=green>RecycleTopToBottom</color>");
        _recycling = true;

        int n = 0;
        float posX = 0;
        float posY = _cellPool[bottomMostCellIndex].anchoredPosition.y;
        int additionalRows = 0;

        while (_cellPool[topMostCellIndex].MaxY() < _recyclableViewBounds.min.y && _currentItemCount < DataSource.GetItemCount()) //corrected //dont change
        {
            if (++_bottomMostCellColumn >= _columns)
            {
                n++;
                _bottomMostCellColumn = 0;
                posY = _cellPool[bottomMostCellIndex].anchoredPosition.y + _cellHeight;
                additionalRows++;
            }

            //move bottom to top
            posX = _bottomMostCellColumn * _cellWidth;
            _cellPool[topMostCellIndex].anchoredPosition = new Vector2(posX, posY);

            //if (--_topMostCellColumn < 0)
            //if (++_topMostCellColumn >= _columns)
            //{
            //    Debug.Log("Rec 3: " + _topMostCellColumn + " -- " + _columns);
            //    //_topMostCellColumn = _columns - 1;
            //    _topMostCellColumn = 0;
            //    additionalRows--;
            //}
            
            DataSource.SetCell(_cachedCells[topMostCellIndex], _currentItemCount, _bottomMostCellColumn);

            bottomMostCellIndex = topMostCellIndex;
            topMostCellIndex = (topMostCellIndex + 1) % _cellPool.Count;
            _currentItemCount++;

        }

        Content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
        if (additionalRows > 0)
            n -= additionalRows;

        foreach (RectTransform c in _cellPool)
        {
            c.anchoredPosition += _cellPool[topMostCellIndex].sizeDelta.y * n * Vector2.down;
        }

        Content.anchoredPosition -= _cellPool[topMostCellIndex].sizeDelta.y * n * Vector2.down;
        _recycling = false;
        Vector2 newPos = new Vector2(0, _cellPool[topMostCellIndex].sizeDelta.y * n);
        return -newPos;
    }

    #endregion




    #region TESTING
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(_recyclableViewBounds.min - new Vector3(2000, 0), _recyclableViewBounds.min + new Vector3(2000, 0));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_recyclableViewBounds.max - new Vector3(2000, 0), _recyclableViewBounds.max + new Vector3(2000, 0));
    }
    #endregion
}
