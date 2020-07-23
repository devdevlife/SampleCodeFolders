using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerData
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string prefabName { get; set; }

    public PlayerData()
    {

    }

    public PlayerData(object _id, object _name, object _prefabName)
    {
        ID = (string)_id;
        Name = (string)_name;
        prefabName = (string)_prefabName;
    }
}
