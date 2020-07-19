using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ShowFieldType
{
    ReadOnly,
    ShowIf,
    Hide,
    HideAllSwitch,
    Audio,
    GameObject,
}

public enum ConditionOperator
{
    None,
    And,
    InverseAnd,
    Or,
    InverseOr,
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class ShowInspectorAttribute : Attribute
{
    public ShowFieldType ShowFieldType;
    public ConditionOperator OperatorType;
    public string[] ConditionNames;

    public ShowInspectorAttribute(ShowFieldType _showFieldType, ConditionOperator _operatorType = ConditionOperator.None, String[] _conditionNames = null)
    {
        ShowFieldType = _showFieldType;
        OperatorType = _operatorType;
        ConditionNames = _conditionNames;
    }
}

