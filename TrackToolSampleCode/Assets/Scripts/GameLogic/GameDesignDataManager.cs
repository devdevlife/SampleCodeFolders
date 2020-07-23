using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using System.IO;
using System.Linq;
using System.CodeDom;
using UnityEditor;

public class GameDesignDataManager : Singleton<GameDesignDataManager>
{
    public struct LoadInfo
    {
        public System.Type Type { get; set; }
        public string TypeName { get; set; }
        public string CSVName { get; set; }
        public bool IsString { get; set; }
        public bool IsGroup { get; set; }

        public LoadInfo(System.Type _type, string _csvName, bool _isString, bool _isGroup)
        {
            Type = _type;
            TypeName = _type.Name.ToString();
            CSVName = _csvName;
            IsString = _isString;
            IsGroup = _isGroup;
        }
    }

    protected Dictionary<string, Dictionary<string, object>> m_dictionary = new Dictionary<string, Dictionary<string, object>>();

    public List<TYPE> FindAll<TYPE>() where TYPE : new()
    {
        var typeName = typeof(TYPE).Name.ToString();
        var findData = FindData(typeName);
        if(findData == null)
        {
            //LoadData<TYPE>();
        }

        return findData.ToList().ConvertAll<TYPE>(item => (TYPE)item.Value);
    }

    private Dictionary<string, object> FindData(string typeName)
    {
        if (m_dictionary.ContainsKey(typeName) == false)
            return null;

        return m_dictionary[typeName];
    }

    private void LoadData<TYPE>(LoadInfo _loadInfo) where TYPE : new()
    {
        MakeDictionary(_loadInfo);
        LoadFromInfo<TYPE>(_loadInfo);
    }

    /// <summary>
    /// 테이블 데이터는 정보들을 바이너리로 만든 후 바이너리로 읽어오게 해야됨. 
    /// 테스트 코드이기에 csv 파일 리플렉션 사용해서 바로 읽어오자...
    /// </summary>
    /// <typeparam name="TYPE"></typeparam>
    /// <param name="_loadInfo"></param>
    private void LoadFromInfo<TYPE>(LoadInfo _loadInfo) where TYPE : new()
    {
        var assetPath = Application.dataPath;
        var readPath = string.Format("{0}/Bundle/_Resource/GameDesignData/{1}.csv", assetPath, _loadInfo.CSVName);
        var loadCSVlines = File.ReadAllLines(readPath).Skip(1);

        foreach(var line in loadCSVlines)
        {
            var values = line.Split(',');
            TYPE newData = new TYPE();
            var props = typeof(TYPE).GetProperties();
            string ID = string.Empty;

            for(int icnt = 0; icnt < values.Length; icnt++)
            {
                if(props[icnt].Name.Contains("ID"))
                {
                    ID = values[icnt].ToString();
                }

                props[icnt].SetValue(newData, values[icnt]);
            }

            Add(_loadInfo.TypeName, ID, newData);
        }
    }

    private void Add(string _typeName, string _key, object _value)
    {
        if (m_dictionary[_typeName].ContainsKey(_key) == false)
        {
            m_dictionary[_typeName].Add(_key, _value);
        }
    }

    private void MakeDictionary(LoadInfo loadInfo)
    {
        var typeName = loadInfo.Type.Name.ToString();
        if (m_dictionary.ContainsKey(typeName) == false)
        {
            m_dictionary[typeName] = new Dictionary<string, object>();
        }
    }

    public void Initialize()
    {
        LoadData<PlayerData>(new LoadInfo(typeof(PlayerData), "Player", false, false));
        LoadData<MonsterData>(new LoadInfo(typeof(MonsterData), "Monster", false, false));
    }
}
