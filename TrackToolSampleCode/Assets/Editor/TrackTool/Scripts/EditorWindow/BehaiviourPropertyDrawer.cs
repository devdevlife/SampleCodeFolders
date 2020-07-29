using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.Reflection;
using System.CodeDom;
using System;

using ReflectionHelper;

namespace TrackTool
{
    [CustomPropertyDrawer(typeof(EffectEventBehaiviour))]
    [CustomPropertyDrawer(typeof(SoundEventBehaiviour))]
    [CustomPropertyDrawer(typeof(ScreenEffectEventBehaiviour))]
    [CanEditMultipleObjects]
    public class BehaiviourPropertyDrawer : PropertyDrawer
    {
        private bool isLoadedFieldInfo = false;
        private List<FieldInfoData> fieldInfoDataList = new List<FieldInfoData>();
        private List<FieldInfoData> myCustomFieldInfoDataList = new List<FieldInfoData>();
        private object templateObject;
        private object eventDataObject;
        private FieldInfoData disableAllFieldInfoData = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (isLoadedFieldInfo == false)
            {
                LoadFieldInfoData(property.serializedObject.targetObject);
                myCustomFieldInfoDataList.AddRange(fieldInfoDataList.FindAll(e => e.FiedlInfoData.GetCustomAttribute<ShowInspectorAttribute>() != null));
                disableAllFieldInfoData = DrawerHelper.FindDisableAllField(ref myCustomFieldInfoDataList);
                isLoadedFieldInfo = true;
            }

            if(disableAllFieldInfoData != null)
            {
                disableAllFieldInfoData.DrawInspector();
                EditorGUI.BeginDisabledGroup((bool)disableAllFieldInfoData.GetValue());
            }

            var ppInumer = property.GetEnumerator();
            while(ppInumer.MoveNext())
            {
                var convertPP = ppInumer.Current as SerializedProperty;
                if (convertPP.propertyType == SerializedPropertyType.Generic)
                    continue;

                if (convertPP.name.Contains(disableAllFieldInfoData.FiedlInfoData.Name))
                    continue;

                var isMyCustomAttribute = myCustomFieldInfoDataList.Find(e => e.FiedlInfoData.Name.Contains(convertPP.name));
                if(isMyCustomAttribute != null)
                {
                    isMyCustomAttribute.DrawInspector();
                }
                else
                {
                    DrawerHelper.DrawSerializeProperty(convertPP);
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void LoadFieldInfoData(UnityEngine.Object clipSerializeTargetObject)
        {
            if (clipSerializeTargetObject == null)
            {
                return;
            }

            var templateFieldInfoData = DrawerHelper.GetFieldInfo(clipSerializeTargetObject, "template");
            if (templateFieldInfoData == null)
            {
                return;
            }

            templateObject = templateFieldInfoData.FiedlInfoData.GetValue(clipSerializeTargetObject);
            fieldInfoDataList.AddRange(DrawerHelper.GetAllFieldInfo(templateObject));

            var eventDataFieldInfo = DrawerHelper.GetFieldInfo(templateObject, "EventData");
            if (eventDataFieldInfo == null)
            {
                return;
            }

            eventDataObject = eventDataFieldInfo.FiedlInfoData.GetValue(templateObject);
            fieldInfoDataList.AddRange(DrawerHelper.GetAllFieldInfo(eventDataObject));
        }
    }
}
