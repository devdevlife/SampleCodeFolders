using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.Reflection;
using System.CodeDom;
using System;
using System.Linq;
using System.Resources;

namespace ReflectionHelper
{
    public class FieldInfoData
    {
        public object InstanceObject { get; set; }
        public FieldInfo FiedlInfoData { get; set; }

        public FieldInfoData(object _object, FieldInfo _fieldInfo)
        {
            InstanceObject = _object;
            FiedlInfoData = _fieldInfo;
        }

        public object GetValue()
        {
            if (InstanceObject == null ||
               FiedlInfoData == null)
                return null;

            return FiedlInfoData.GetValue(InstanceObject);
        }

        public void SetValue(object _changeValue, object _instanceObject = null)
        {
            if (InstanceObject == null ||
               FiedlInfoData == null)
                return;

            FiedlInfoData.SetValue(_instanceObject == null ? InstanceObject : _instanceObject, _changeValue);
        }

        public void DrawInspector()
        {
            if (DrawerHelper.IsMyCustomAttribue(this))
            {
                DrawerHelper.DrawCustomAttributeFieldInfo(this);
            }
            else
            {
                DrawerHelper.DrawFieldInfo(this);
            }
        }
    }

    public class DrawerHelper
    {
        private static UnityEngine.Object CustomDrawerObjectTemp;
        private static readonly string pathAssetSoundEffet = "Sound/Effect/";
        private static readonly string pathAssetSoundUI = "Sound/UI/";
        private static readonly string pathAssetEffect = "Effects/KTK_Effect_Samples/Prefab/";

        public static object GetCreateInstanceAssembly(string typeName, object[] args = null)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            var assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
                return null;

            return assembly.CreateInstance(typeName, false, BindingFlags.Public, null, args, null, null);
        }

        public static FieldInfoData GetFieldInfo(object targetFieldInfoData, string fieldName)
        {
            return GetAllFieldInfo(targetFieldInfoData, f => f.Name.Equals(fieldName, StringComparison.InvariantCulture)).FirstOrDefault();
        }

        public static IEnumerable<FieldInfoData> GetAllFieldInfo(object target, Func<FieldInfo, bool> predicate = null)
        {
            List<Type> typeList = new List<Type>()
        {
            target.GetType(),
        };

            while (typeList.Last().BaseType != null)
            {
                typeList.Add(typeList.Last().BaseType);
            }

            for (int icnt = typeList.Count - 1; icnt >= 0; icnt--)
            {
                IEnumerable<FieldInfo> fieldInfos = null;

                if (predicate == null)
                {
                    fieldInfos = typeList[icnt]
                        .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
                }
                else
                {
                    fieldInfos = typeList[icnt]
                        .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(predicate);
                }

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return new FieldInfoData(target, fieldInfo);
                }
            }
        }

        public static bool IsMyCustomAttribue(FieldInfoData _fieldInfoData)
        {
            if (_fieldInfoData == null ||
                _fieldInfoData.FiedlInfoData == null)
                return false;

            if (_fieldInfoData.FiedlInfoData.Attributes == (FieldAttributes.Private | FieldAttributes.InitOnly))
                return true;

            if (_fieldInfoData.FiedlInfoData.GetCustomAttribute<ShowInspectorAttribute>() != null)
                return true;

            return false;
        }

        public static bool IsSystemAttribute(FieldInfoData _fieldInfoData)
        {
            if (_fieldInfoData == null ||
               _fieldInfoData.FiedlInfoData == null)
                return false;

            if (_fieldInfoData.FiedlInfoData.GetCustomAttributes(true).Length > 0)
                return true;

            return false;
        }

        public static FieldInfoData FindDisableAllField(ref List<FieldInfoData> _myCustomFieldDataList)
        {
            if (_myCustomFieldDataList == null)
                return null;

            return _myCustomFieldDataList.Find(e => e.FiedlInfoData.GetCustomAttribute<ShowInspectorAttribute>().ShowFieldType == ShowFieldType.HideAllSwitch); ;
        }

        public static void DrawCustomAttributeFieldInfo(FieldInfoData _fieldInfoData)
        {
            if (_fieldInfoData == null ||
                _fieldInfoData.FiedlInfoData == null)
                return;

            var showInspectorAttribute = _fieldInfoData.FiedlInfoData.GetCustomAttribute<ShowInspectorAttribute>();
            var isPrivateInitOnly = _fieldInfoData.FiedlInfoData.Attributes == (FieldAttributes.Private | FieldAttributes.InitOnly);

            showInspectorAttribute = isPrivateInitOnly == true ? new ShowInspectorAttribute(ShowFieldType.ReadOnly) : showInspectorAttribute;
            if (showInspectorAttribute != null)
            {
                switch (showInspectorAttribute.ShowFieldType)
                {
                    case ShowFieldType.Hide:
                        {

                        }
                        break;

                    case ShowFieldType.HideAllSwitch:
                        {
                            DrawFieldInfo(_fieldInfoData);
                        }
                        break;

                    case ShowFieldType.ReadOnly:
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            DrawFieldInfo(_fieldInfoData);
                            EditorGUI.EndDisabledGroup();
                        }
                        break;

                    case ShowFieldType.ShowIf:
                        {
                            var conditionNames = showInspectorAttribute.ConditionNames;
                            if (conditionNames.Length <= 0)
                                return;

                            List<bool> conditionCheckList = new List<bool>();

                            foreach (var condition in conditionNames)
                            {
                                var targetConditionData = GetFieldInfo(_fieldInfoData.InstanceObject, condition);
                                if (targetConditionData != null)
                                {
                                    conditionCheckList.Add((bool)targetConditionData.GetValue());
                                }
                            }

                            var trueConditionCount = conditionCheckList.FindAll(e => e == true).Count;
                            switch (showInspectorAttribute.OperatorType)
                            {
                                case ConditionOperator.None:
                                    {
                                        DrawFieldInfo(_fieldInfoData);
                                    }
                                    break;

                                case ConditionOperator.And:
                                case ConditionOperator.InverseAnd:
                                    {
                                        var isShow = trueConditionCount > 0;
                                        isShow = showInspectorAttribute.OperatorType == ConditionOperator.InverseAnd ? !isShow : isShow;
                                        if (isShow)
                                        {
                                            DrawFieldInfo(_fieldInfoData);
                                        }
                                    }
                                    break;

                                case ConditionOperator.Or:
                                case ConditionOperator.InverseOr:
                                    {
                                        var isShow = trueConditionCount == conditionCheckList.Count;
                                        isShow = showInspectorAttribute.OperatorType == ConditionOperator.InverseOr ? !isShow : isShow;

                                        if (isShow)
                                        {
                                            DrawFieldInfo(_fieldInfoData);
                                        }
                                    }
                                    break;
                            }
                        }
                        break;

                    case ShowFieldType.Audio:
                        {
                            var audioClipName = (string)_fieldInfoData.GetValue();
                            var targetAudioClip = LoadInspectorResourceObject<AudioClip>(ResourceType.Sound_Effect, audioClipName);

                            var changeObject = EditorGUILayout.ObjectField(_fieldInfoData.FiedlInfoData.Name, targetAudioClip, typeof(AudioClip), true);
                            if (changeObject != null &&
                               changeObject.name.Equals(audioClipName) == false)
                            {
                                _fieldInfoData.SetValue(changeObject.name);
                            }
                        }
                        break;

                    case ShowFieldType.GameObject:
                        {
                            var audioClipName = (string)_fieldInfoData.GetValue();
                            var targetAudioClip = LoadInspectorResourceObject<GameObject>(ResourceType.Effect, audioClipName);

                            var changeObject = EditorGUILayout.ObjectField(_fieldInfoData.FiedlInfoData.Name, targetAudioClip, typeof(GameObject), true);
                            if (changeObject != null &&
                               changeObject.name.Equals(audioClipName) == false)
                            {
                                _fieldInfoData.SetValue(changeObject.name);
                            }
                        }
                        break;
                }
            }

            // 추가 커스텀 애트리뷰트가 있다면 추가.
        }

        public enum ResourceType
        {
            Sound_Effect,
            Sound_UI,
            Prefab_Player,
            Prefab_Monster,
            Effect,
            Effect_UI,
        }

        public static T LoadResourceObject<T>(ResourceType resourceType, string resourceName) where T : UnityEngine.Object
        {
            var path = string.Empty;
            switch (resourceType)
            {
                case ResourceType.Sound_Effect: { path = pathAssetSoundEffet; } break;
                case ResourceType.Sound_UI: { path = pathAssetSoundUI; }    break;
                case ResourceType.Effect: { path = pathAssetEffect; }   break;
            }

            return Resources.Load<T>(path + resourceName);
        }

        /// <summary>
        /// 헬더 내에서만 사용.
        /// </summary>
        private static T LoadInspectorResourceObject<T>(ResourceType resourceType, string resourceName) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                if (CustomDrawerObjectTemp != null)
                    CustomDrawerObjectTemp = null;
            }
            else
            {
                if (CustomDrawerObjectTemp == null ||
                  CustomDrawerObjectTemp.name.Contains(resourceName) == false)
                {
                    CustomDrawerObjectTemp = LoadResourceObject<T>(resourceType, resourceName);
                }
            }

            return (T)CustomDrawerObjectTemp;
        }

        public static void DrawFieldInfo(FieldInfoData _fieldInfoData)
        {
            if (_fieldInfoData == null ||
                _fieldInfoData.FiedlInfoData == null)
                return;

            var targetObject = _fieldInfoData.InstanceObject;
            var fieldInfo = _fieldInfoData.FiedlInfoData;

            object beforValue = fieldInfo.GetValue(targetObject);
            object afterBalue = beforValue;

            if (fieldInfo.FieldType == typeof(int))
            {
                afterBalue = EditorGUILayout.IntField(fieldInfo.Name, (int)fieldInfo.GetValue(targetObject));
            }
            else if (fieldInfo.FieldType == typeof(float))
            {
                afterBalue = EditorGUILayout.FloatField(fieldInfo.Name, (float)beforValue);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                afterBalue = EditorGUILayout.EnumPopup(fieldInfo.Name, (Enum)fieldInfo.GetValue(targetObject));
            }
            else if (fieldInfo.FieldType == typeof(bool))
            {
                afterBalue = EditorGUILayout.Toggle(fieldInfo.Name, (Boolean)fieldInfo.GetValue(targetObject));
            }
            else if (fieldInfo.FieldType == typeof(string))
            {
                afterBalue = EditorGUILayout.TextField(fieldInfo.Name, (string)fieldInfo.GetValue(targetObject));
            }

            if (afterBalue.Equals(beforValue) == false)
            {
                fieldInfo.SetValue(targetObject, afterBalue);
            }
        }

        public static void DrawSerializeProperty(SerializedProperty _sp)
        {
            if (_sp.propertyType == SerializedPropertyType.Float &&
                (_sp.name.Contains("x") || _sp.name.Contains("y") || _sp.name.Contains("z")))
            {
                return;
            }

            EditorGUILayout.PropertyField(_sp);
        }
    }
}

