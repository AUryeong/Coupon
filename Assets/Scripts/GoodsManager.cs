using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoodsManager : SingletonBehavior<GoodsManager>
{
    [SerializeField] private List<TextMeshProUGUI> countTexts = new(4);

    public void SetGoodsText(int index, int amount)
    {
        countTexts[index].text = amount.ToString("N0");
    }
}