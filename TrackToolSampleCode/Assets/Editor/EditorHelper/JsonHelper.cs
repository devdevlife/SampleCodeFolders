using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using UnityEditor;

/// <summary>
/// NewtonSoft json 을 사용하였음.
/// </summary>
public static class JsonHelper
{
    private static Newtonsoft.Json.JsonSerializerSettings newtonSoftJsonSetting = new JsonSerializerSettings()
    {
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
        //NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, // 널 값 무시할건지 말건지.
    };

    public static void SaveJson<T>(string _directoryPath, string _jsonFileName, object _serializeData)
    {
        var json = JsonConvert.SerializeObject(_serializeData, typeof(T), newtonSoftJsonSetting); 

        if (Directory.Exists(_directoryPath) == false)
        {
            Directory.CreateDirectory(_directoryPath);
        }

        var jsonFullPath = string.Format("{0}/{1}", _directoryPath, _jsonFileName);
        System.IO.File.WriteAllText(jsonFullPath, json, Encoding.UTF8);

        Debug.Log(string.Format("Save Json / FullPathName : {0}", jsonFullPath));
    }

    public static T LoadJson<T>(string _directoryPath, string _jsonFileNameWhithExtention) where T : class
    {
        if(Directory.Exists(_directoryPath) == false)
        {
            Debug.LogWarning(string.Format("Load Json Not Found Directory : {0}", _directoryPath));
            return null;
        }

        var findFile = (from file in Directory.EnumerateFiles(_directoryPath)
                        where file.Contains(Path.GetFileNameWithoutExtension(_jsonFileNameWhithExtention))
                        select file).FirstOrDefault();

        if (string.IsNullOrEmpty(findFile))
        {
            Debug.LogWarning(string.Format("Load Json Not Found File : {0}", _jsonFileNameWhithExtention));
            return null;
        }

        var fullPath = string.Format("{0}/{1}", _directoryPath, _jsonFileNameWhithExtention);

        var json = System.IO.File.ReadAllText(fullPath, Encoding.UTF8);

        var deserializeObj = JsonConvert.DeserializeObject(json, typeof(T), newtonSoftJsonSetting); 
        if (deserializeObj == null)
        {
            Debug.LogWarning(string.Format("Load Json Not Matching Of Type : {0}", typeof(T).Name.ToString()));
            return null;
        }

        Debug.Log(string.Format("Load Json / FullPathName : {0}", fullPath));

        return (T)deserializeObj;
    }

}
