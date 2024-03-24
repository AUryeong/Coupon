using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class UserData
{
    public List<string> getCouponList = new();
    public List<int> goodsDataList;
}

public class DataManager : SingletonBehavior<DataManager>
{
    private string GuestKey
    {
        get
        {
            if (string.IsNullOrEmpty(guestKey))
                guestKey = PlayerPrefs.GetString("GuestKey", string.Empty);

            guestKeyText.text = $"Guest Auth : {guestKey}";
            return guestKey;
        }
        set
        {
            guestKey = value;
            PlayerPrefs.SetString("GuestKey", value);
            guestKeyText.text = $"Guest Auth : {value}";
        }
    }

    private string guestKey;

    public UserData UserData { get; private set; }

    [SerializeField] private TextMeshProUGUI guestKeyText;

    protected override void OnCreated()
    {
        base.OnCreated();
        Init().Forget();
    }

    private async UniTask Init()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        Debug.Assert(status == DependencyStatus.Available);

        var reference = FirebaseDatabase.DefaultInstance.RootReference.Child("UserData");

        string userGuestKey = GuestKey;
        if (string.IsNullOrEmpty(userGuestKey))
        {
            userGuestKey = reference.Push().Key;
            GuestKey = userGuestKey;
        }

        reference = reference.Child(userGuestKey);

        var task = reference.GetValueAsync();
        await task;

        if (!task.IsCompletedSuccessfully) return;

        string json = task.Result.GetRawJsonValue();

        if (string.IsNullOrEmpty(json))
        {
            UserData = new UserData()
            {
                getCouponList = new List<string>(),
                goodsDataList = new List<int>(4) { 0, 0, 0, 0 }
            };
            await reference.SetRawJsonValueAsync(JsonUtility.ToJson(UserData));
        }
        else
        {
            UserData = JsonUtility.FromJson<UserData>(json);
        }

        for (int i = 0; i < UserData.goodsDataList.Count; i++)
        {
            GoodsManager.Instance.SetGoodsText(i, UserData.goodsDataList[i]);
        }
    }

    public async UniTask UpdateServerData()
    {
        var reference = FirebaseDatabase.DefaultInstance.RootReference.Child("UserData");
        reference = reference.Child(GuestKey); 
        await reference.SetRawJsonValueAsync(JsonUtility.ToJson(UserData));
    }
}