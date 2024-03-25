using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Firebase.Database;
using TMPro;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class CouponManager : SingletonBehavior<CouponManager>
{
    [SerializeField] private Button applyButton;
    [SerializeField] private TMP_InputField couponInput;

    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private TextMeshProUGUI hashText;

    protected override void OnCreated()
    {
        base.OnCreated();
        applyButton.onClick.RemoveAllListeners();
        applyButton.onClick.AddListener(CheckCoupon);
    }

    public void CheckCoupon()
    {
        CheckCouponTask().Forget();
    }

    private async UniTask CheckCouponTask()
    {
        popupText.text = string.Empty;

        string text = couponInput.text.Trim();
        string hash = GetHash(text).ToString();

        hashText.text = $"Fnv-1a Value : {hash}";

        var reference = FirebaseDatabase.DefaultInstance.RootReference.Child("Coupon");
        reference = reference.Child(hash);

        var dataSnapshot = await reference.GetValueAsync();
        string json = dataSnapshot.GetRawJsonValue();
        if (string.IsNullOrEmpty(json))
        {
            popupText.text = "잘못된 쿠폰 번호입니다. 쿠폰 번흐롤 확인해주시기 바랍니다.";
            return;
        }

        if (DataManager.Instance.UserData.getCouponList.Contains(text))
        {
            popupText.text = "이미 사용된 쿠폰입니다.";
            return;
        }

        DataManager.Instance.UserData.getCouponList.Add(text);

        var goodsDataList = JsonConvert.DeserializeObject<List<int>>(json);
        for (int i = 0; i < goodsDataList.Count; i++)
        {
            DataManager.Instance.UserData.goodsDataList[i] += goodsDataList[i];
        }


        for (int i = 0; i < DataManager.Instance.UserData.goodsDataList.Count; i++)
        {
            GoodsManager.Instance.SetGoodsText(i, DataManager.Instance.UserData.goodsDataList[i]);
        }

        await DataManager.Instance.UpdateServerData();
        popupText.text = "쿠폰이 적용되었습니다";
    }

    private static uint GetHash(string origin)
    {
        const uint primeNumber = 16777619u;
        const uint offset = 2166136261u;
        const string salt = "Alice";

        uint result = offset;
        var stringBytes = Encoding.ASCII.GetBytes(origin + salt);
        foreach (var value in stringBytes)
        {
            unchecked
            {
                result ^= value;
                result *= primeNumber;
            }
        }

        return result;
    }
}