using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InfiniteScrollBottomToTop : MonoBehaviour
{
    public RectTransform contentPanel;
    public ScrollRect scrollRect;
    public GameObject itemPrefab;
    public int itemCount = 100;
    public float itemHeight = 200f;
    public float padding = 10f;

    private List<GameObject> spawnedItems = new List<GameObject>();
    [SerializeField] Dictionary<GameObject, int> itemIndexMap;// = new Dictionary<GameObject, int>();
    private float contentHeight;
    private int firstVisibleItemIndex;
    private int lastVisibleItemIndex;
    private float viewPortHeight;
    private float lastScrollPosition;

    bool isUpdatingScroll = false;

    void Start()
    {
        itemIndexMap = new();
        InitializeScroll();
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    void InitializeScroll()
    {
        viewPortHeight = scrollRect.viewport.rect.height;
        contentHeight = itemCount * (itemHeight + padding) - padding;
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, contentHeight);

        // Calculate the initial scroll position to place the bottom item at the bottom of the viewport.
        //float initialYPosition = contentHeight - viewPortHeight;
        //contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, initialYPosition);
        contentPanel.anchoredPosition = new Vector2(contentPanel.anchoredPosition.x, 0);

        SpawnInitialItems();

        //lastScrollPosition = initialYPosition;
        lastScrollPosition = 0;
        UpdateVisibleItems();
    }

    void SpawnInitialItems()
    {
        //int startingIndex = Mathf.Max(0, itemCount - Mathf.CeilToInt(viewPortHeight / (itemHeight + padding)) - 2);

        //for (int i = startingIndex; i < itemCount; i++)
        //{
        //    SpawnItem(i);
        //}

        int initialVisibleCount = Mathf.CeilToInt(viewPortHeight / (itemHeight + padding)) + 2;
        initialVisibleCount = Mathf.Min(initialVisibleCount, itemCount);

        for (int i = 0; i < initialVisibleCount; i++)
        {
            SpawnItem(i);
        }
    }

    void SpawnItem(int index)
    {
        GameObject newItem = Instantiate(itemPrefab, contentPanel);
        RectTransform itemRect = newItem.GetComponent<RectTransform>();

        itemRect.anchorMin = new Vector2(0, 0);
        itemRect.anchorMax = new Vector2(1, 0);
        itemRect.pivot = new Vector2(0.5f, 0);

        //float yPosition = -index * (itemHeight + padding);
        float yPosition = index * (itemHeight + padding);
        itemRect.anchoredPosition = new Vector2(0, yPosition);
        SetItemData(newItem, index);
        spawnedItems.Add(newItem);
        itemIndexMap[newItem] = index;
    }

    void SetItemData(GameObject item, int index)
    {
        item.name = "Item" + index;
        Text text = item.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.text = "Item " + index;
        }
    }

    void OnScroll(Vector2 scrollPosition)
    {
        if (isUpdatingScroll) return;
        if (!isUpdatingScroll && lastScrollPosition != scrollRect.content.anchoredPosition.y)
        {
            isUpdatingScroll = true;
            UpdateVisibleItems();
            //lastScrollPosition = scrollRect.content.anchoredPosition.y;
            isUpdatingScroll = false;
        }
    }

    void UpdateVisibleItems()
    {
        //while (lastScrollPosition != scrollRect.content.anchoredPosition.y)
        //{ 
            
        //}
        float scrollPosition = scrollRect.content.anchoredPosition.y;
        firstVisibleItemIndex = Mathf.FloorToInt(-scrollPosition / (itemHeight + padding));
        lastVisibleItemIndex = Mathf.FloorToInt((-scrollPosition + viewPortHeight) / (itemHeight + padding));

        //lastVisibleItemIndex = Mathf.FloorToInt(scrollPosition / (itemHeight + padding));
        //firstVisibleItemIndex = Mathf.FloorToInt((scrollPosition + viewPortHeight) / (itemHeight + padding));

        firstVisibleItemIndex = Mathf.Clamp(firstVisibleItemIndex, 0, itemCount - 1);
        lastVisibleItemIndex = Mathf.Clamp(lastVisibleItemIndex, 0, itemCount - 1);
        //RecycleItems();
        RecycleItems(firstVisibleItemIndex, lastVisibleItemIndex);
        lastScrollPosition = scrollRect.content.anchoredPosition.y;

    }
    //void RecycleItems()
    //{
    //    for (int i = 0; i < spawnedItems.Count; i++)
    //    {
    //        RectTransform itemRect = spawnedItems[i].GetComponent<RectTransform>();
    //        int itemIndex = Mathf.FloorToInt(-itemRect.anchoredPosition.y / (itemHeight + padding));
    //        if (itemIndex < firstVisibleItemIndex - 1 || itemIndex > lastVisibleItemIndex + 1)
    //        {
    //            RecycleItem(spawnedItems[i], GetNextIndex(itemIndex));
    //        }
    //    }
    //}

    //int GetNextIndex(int currentIndex)
    //{
    //    if (currentIndex < firstVisibleItemIndex)
    //    {
    //        return lastVisibleItemIndex;
    //    }
    //    else
    //    {
    //        return firstVisibleItemIndex;
    //    }
    //}

    void RecycleItems(int firstVisible, int lastVisible)
    {
        List<GameObject> itemsToRecycle = new List<GameObject>();
        foreach (var item in spawnedItems)
        {
            if (itemIndexMap.ContainsKey(item))
            {
                int itemIndex = itemIndexMap[item];

                if (itemIndex < firstVisible - 1 || itemIndex > lastVisible + 1)
                {
                    itemsToRecycle.Add(item);
                }
            }
        }

        foreach (var item in itemsToRecycle)
        {
            int oldIndex = itemIndexMap[item];
            int newIndex = GetNextIndex(oldIndex, firstVisible, lastVisible);
            RecycleItem(item, newIndex);
        }
    }

    int GetNextIndex(int currentIndex, int firstVisible, int lastVisible)
    {
        if (currentIndex < firstVisible)
        {
            return lastVisible;
        }
        else
        {
            return firstVisible;
        }
    }

    void RecycleItem(GameObject item, int newIndex)
    {
        RectTransform itemRect = item.GetComponent<RectTransform>();

        //itemRect.anchoredPosition = new Vector2(0, -newIndex * (itemHeight + padding));
        itemRect.anchoredPosition = new Vector2(0, newIndex * (itemHeight + padding));

        SetItemData(item, newIndex);
        itemIndexMap[item] = newIndex;
    }
}