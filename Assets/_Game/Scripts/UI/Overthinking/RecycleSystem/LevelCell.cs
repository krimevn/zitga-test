using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LevelCell : MonoBehaviour, ICell2
{
    public Text LevelText;
    public Image LockedImg;
    public Image BackgroundImg;

    [SerializeField] LevelInfor _levelInfo;
    [SerializeField] int _cellIndex;
    [SerializeField] int _collumIndex;
    [SerializeField] Button _selfButton;
    [SerializeField] List<Image> starList;

    void Start()
    {
        if (_selfButton == null) _selfButton = this.GetComponent<Button>();
    }

    public void InitCell(LevelInfor info, int cellIndex, int columnIndex, UnityAction onCLick)
    {
        this.gameObject.name = "Cell" + info.levelId;
        _cellIndex = cellIndex;
        _collumIndex = columnIndex;
        _levelInfo = info;

        if (info.levelId == 0)
            LevelText.text = "Tutorial";
        else
            LevelText.text = (info.levelId+1).ToString();

        LockedImg.gameObject.SetActive(!info.isUnlocked);

        _selfButton.onClick.RemoveAllListeners();

        if (info.isUnlocked)
            SetStars(info.stars);
        else
            HideAllStars();

        if(onCLick != null)
            _selfButton.onClick.AddListener(onCLick);
    }

    void SetStars(int starCount)
    {
        if (starCount > 3 || starCount < 1)
        {
            //Debug.Log("Something wrong with star input ? " + starCount);
            HideAllStars();
            return;
        }
        for (int i = 0; i < starList.Count; i++)
        {
            if (starCount > i)
                starList[i].gameObject.SetActive(true);
            else
                starList[i].gameObject.SetActive(false);
        }
    }

    void HideAllStars()
    {
        foreach (var s in starList)
        {
            s.gameObject.SetActive(false);
        }
    }
}
