using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MonsterData
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string prefabName { get; set; }

    public MonsterData()
    {

    }

    public MonsterData(object _id, object _name, object _prefabName)
    {
        ID = (string)_id;
        Name = (string)_name;
        prefabName = (string)_prefabName;
    }
}
