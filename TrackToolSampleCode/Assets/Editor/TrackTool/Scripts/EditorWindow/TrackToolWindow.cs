using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.PackageManager.UI;
using System.Runtime.Serialization;
using System;
using System.Linq;
using UnityEditor.Experimental.TerrainAPI;
using System.ComponentModel;
using UnityEditor.Experimental.Networking.PlayerConnection;
using System.Reflection;

public partial class TrackToolWindow : EditorWindow
{
    public enum eTrackToolTab
    {
        Track,
        Setting,
    }

    private static MethodInfo[] m_OnGuiUpdateList;

    private int selectedTabIndex;

    // editor GUI Style
    private GUIStyle buttonStyle;

    // path
    private static readonly string DefaultPlayableAssetFullPathWithName = "Assets/Editor/TrackTool/TimelinePlayables/TimelinePlayable.playable";
    private static string JsonRootDirectory;

    [MenuItem("Window/CustomEditor/TrackTool")]
    static public void Init()
    {
        var trackToolWindow = (TrackToolWindow)EditorWindow.GetWindow(typeof(TrackToolWindow));
        if (trackToolWindow == null)
            return;

        trackToolWindow.Show();
        
    }

    private void Initialize()
    {
        InitGUIStyle();
        InitPath();
        InitGuiTabMethod();
    }

    private void InitGuiTabMethod()
    {
        if (m_OnGuiUpdateList != null)
            return;

        var tabList = Enum.GetNames(typeof(eTrackToolTab));
        m_OnGuiUpdateList = new MethodInfo[(int)tabList.Length];
        for (int icnt = 0; icnt < tabList.Length; icnt++)
        {
            m_OnGuiUpdateList[icnt] = this.GetType().GetMethod(string.Format("OnGui{0}", Enum.GetName(typeof(eTrackToolTab), icnt)));
        }
    }

    private void InitPath()
    {
        var assetsPath = Application.dataPath;
        JsonRootDirectory = string.Format("{0}/Bundle/_Resource/TrackGroup", assetsPath);
    }

    private void InitGUIStyle()
    {
        buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
        buttonStyle.fontSize = 15;
    }

    private void OnGUI()
    {
        Initialize();

        var changeSelectedTabIndex = GUILayout.Toolbar(selectedTabIndex, Enum.GetNames(typeof(eTrackToolTab)));
        if (changeSelectedTabIndex != selectedTabIndex)
        {
            selectedTabIndex = changeSelectedTabIndex;
        }

        if (m_OnGuiUpdateList[selectedTabIndex] != null)
            m_OnGuiUpdateList[selectedTabIndex].Invoke(this, null);
    }

    protected void BeginDisableGroup(bool isDisable)
    {
        UnityEditor.EditorGUI.BeginDisabledGroup(isDisable);
    }

    protected void EndDisableGroup(bool isDisable)
    {
        if(isDisable)
            UnityEditor.EditorGUI.EndDisabledGroup();
    }
}

