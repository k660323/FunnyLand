using BackEnd;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.StatInfo> StatDict { get; private set; } = new Dictionary<int, Data.StatInfo>();

    public Data.PlayerInfo PlayerInfoData { get; private set; }

    public Data.PlayerInventory PlayerInventoryData { get; set; }
    
    public Dictionary<int, Data.Map> SoloMapDict { get; private set; } = new Dictionary<int, Data.Map>();

    public Dictionary<int, Data.Map> TeamMapDict { get; private set; } = new Dictionary<int, Data.Map>();

    public Dictionary<int, Data.Map> HybridMapDict { get; private set; } = new Dictionary<int, Data.Map>();

    public Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
    
    public void Init()
    {
        StatDict = LoadJson<Data.StatData, int, Data.StatInfo>("StatData").MakeDict();
        SoloMapDict = LoadJson<Data.MapData, int, Data.Map>("SoloMapData").MakeDict();
        TeamMapDict = LoadJson<Data.MapData, int, Data.Map>("TeamMapData").MakeDict();
        HybridMapDict = LoadJson<Data.MapData, int, Data.Map>("HybridMapData").MakeDict();
        ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
    }

    Loader LoadJson<Loader, Key, Value>(string path) where Loader :ILoader<Key,Value>
    {
        TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<Loader>(textAsset.text);
    }

    public bool PlayerInfoInit() 
    {
        PlayerInfoData = new Data.PlayerInfo();
        PlayerInfoData = LoadJsonBackEnd(PlayerInfoData, "PlayerInfo", "playerInfoToJson");
        PlayerInfoData.PlayerIcon = PlayerInfoData.PlayerIcon;
        PlayerInfoData.Style = PlayerInfoData.Style;
        return PlayerInfoData != null;
    }

    public bool PlayerInventoryInit()
    {
        PlayerInventoryData = new Data.PlayerInventory();
        PlayerInventoryData = LoadJsonBackEnd(PlayerInventoryData, "PlayerInventory", "playerInventoryToJson");

        return PlayerInventoryData != null;
    }

    T LoadJsonBackEnd<T>(T original, string tableName, string columnName)
    {
        var bro = Backend.GameData.GetMyData(tableName, new Where());
        if(bro.IsSuccess())
        {
            string dataText;
            if (bro.GetReturnValuetoJSON()["rows"].Count > 0)
            {
                //Debug.Log(bro.GetReturnValuetoJSON()["rows"].Count); // 열 수 
                //Debug.Log(bro.FlattenRows()[0][columnName].ToString()); //[0] PlayerInfo
                dataText = bro.FlattenRows()[0][columnName].ToString();
            }
            else // 없으면 초기화
            {
                dataText = JsonUtility.ToJson(original);

                Param param = new Param();
                param.Add(columnName, dataText);

                Backend.GameData.Insert(tableName, param);
            }

            return JsonUtility.FromJson<T>(dataText);
        }
        else
        {
            return default;
        }
    }

    public bool SaveBackEndData<T>(T dataClass, string tableName, string columnName) where T : class
    {
        string dataText = JsonUtility.ToJson(dataClass);

        Param param = new Param();
        param.Add(columnName, dataText);

        Where where = new Where();
        where.Equal("owner_inDate", Backend.UserInDate);

        var result = Backend.GameData.Update(tableName, where, param);
        return result.IsSuccess();
    }
}
