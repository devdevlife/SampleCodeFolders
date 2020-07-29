using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;
using UnityEngine.Playables;
using TMPro.EditorUtilities;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using System.Runtime.InteropServices.WindowsRuntime;

public partial class TrackToolWindow : EditorWindow
{
    private readonly string ResetButton = "Reset";
    private readonly string TrackToolSceneName = "SampleScene.unity";
    private readonly string ScenesFolderpath = "Assets/Scenes/";
    private readonly string TargetParentObjectName = "Target";

    private PlayerData[] m_PlayerDataList;
    private MonsterData[] m_MonsterList;
    private List<string> ActorPopupList = new List<string>();
    private List<string> m_ActionPopupList = new List<string>();
    private List<string> m_NewActionPopupList = new List<string>();

    private GameObject m_ActorParentObject { get; set; }
    private GameObject m_ActorObject { get; set; }
    private PlayableDirector m_PlayerableDirector { get; set; }
    private TimelineAsset m_TimelineAsset { get; set; }
    private TrackData m_TrackDataCache { get; set; }

    private int m_SelectedActorIndex { get; set; }
    private int m_SelectedActionIndex { get; set; }
    private int m_SelectedNewActionIndex { get; set; }

    protected bool IsPlayer
    {
        get
        {
            if (m_PlayerDataList == null)
                return true;

            return m_SelectedActorIndex < m_PlayerDataList.Length ? true : false;
        }
    }

    protected PlayerData CurrentPlayerData
    {
        get
        {
            if (m_PlayerDataList == null ||
                m_PlayerDataList.Length <= 0)
                return null;

            return Array.Find<PlayerData>(m_PlayerDataList, e => e.Name.Contains(CurrentActorName));
        }
    }

    protected MonsterData CurrentMonterData
    {
        get
        {
            if (m_MonsterList == null ||
                m_MonsterList.Length <= 0)
                return null;

            return Array.Find<MonsterData>(m_MonsterList, e => e.Name.Contains(CurrentActorName));
        }
    }

    protected string CurrentActorName 
    {
        get
        {
            if (ActorPopupList == null ||
                ActorPopupList.Count <= m_SelectedActorIndex)
                return string.Empty;

            if(m_SelectedActorIndex == -1)
                return string.Empty;

            return ActorPopupList[m_SelectedActorIndex].Split('/')[1];
        }
    }

    protected TrackData.eAction CurrentActionType
    {
        get
        {
            if (m_ActionPopupList == null ||
                m_ActionPopupList.Count <= m_SelectedActionIndex)
                return TrackData.eAction.None;

            var actionName = m_ActionPopupList[m_SelectedActionIndex];
            var parseType = Enum.Parse(typeof(TrackData.eAction), actionName);

            return (TrackData.eAction)parseType;
        }
    }

    protected TrackData.eAction CurrentNewActionType
    {
        get
        {
            if (m_NewActionPopupList == null ||
                m_NewActionPopupList.Count <= m_SelectedNewActionIndex)
                return TrackData.eAction.None;

            var newActionName = m_NewActionPopupList[m_SelectedNewActionIndex];
            var parseType = Enum.Parse(typeof(TrackData.eAction), newActionName);

            return (TrackData.eAction)parseType;
        }
    }

    public void OnGuiTrack()
    {
        GUILayout.Space(10);
        if (GUILayout.Button(ResetButton, buttonStyle))
        {
            InitTrackTool();
        }

        DrawActorList();

        DrawActionList();

        DrawNewJson();

        DrawSaveJson();
    }

    private void InitTrackTool()
    {
        OpenScene();

        LoadGameDesignData();

        LoadActorListInfo();

        LoadActionList();

        LoadTrackData();

        CreateActor();

        SetTimeline();        
    }

    private void DrawActorList()
    {
        var changeActorNumber = EditorGUILayout.Popup("Selected Actor", m_SelectedActorIndex, ActorPopupList.ToArray());
        if (changeActorNumber != m_SelectedActorIndex)
        {
            m_SelectedActorIndex = changeActorNumber;
            ChangeActor();
        }
    }

    private void DrawActionList()
    {
        var changeActionIndex = EditorGUILayout.Popup("Action Type", m_SelectedActionIndex, m_ActionPopupList.ToArray());
        if (changeActionIndex != m_SelectedActionIndex)
        {
            m_SelectedActionIndex = changeActionIndex;
            ChangeAction();
        }
    }

    private void DrawNewActionList()
    {
        var changeActionIndex = EditorGUILayout.Popup("New Action List", m_SelectedNewActionIndex, m_NewActionPopupList.ToArray());
        if (changeActionIndex != m_SelectedNewActionIndex)
        {
            m_SelectedNewActionIndex = changeActionIndex;
        }
    }

    private void DrawNewJson()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("HelpBox");
        {
            EditorGUILayout.BeginHorizontal();
            {
                DrawNewActionList();

                BeginDisableGroup(CurrentNewActionType == TrackData.eAction.None);
                if (GUILayout.Button("New Action"))
                {
                    NewAction();
                }
                EndDisableGroup(CurrentNewActionType == TrackData.eAction.None);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawSaveJson()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginVertical("HelpBox");
        {
            EditorGUILayout.BeginHorizontal();
            {
                BeginDisableGroup(CurrentActionType == TrackData.eAction.None);
                {
                    if (GUILayout.Button("Save Json"))
                    {
                        SaveJsonEventData(CurrentActorName, CurrentActionType);
                    }

                    if (GUILayout.Button("ReLoad Json"))
                    {
                        SetTimeline();
                    }
                }
                EndDisableGroup(CurrentActionType == TrackData.eAction.None);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void LoadActorListInfo()
    {
        ActorPopupList.Clear();

        m_PlayerDataList = GameDesignDataManager.Instance.FindAll<PlayerData>().ToArray();
        m_MonsterList = GameDesignDataManager.Instance.FindAll<MonsterData>().ToArray();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var player in m_PlayerDataList)
        {
            sb.Clear();
            sb.Append("Player/");
            sb.Append(player.Name);
            ActorPopupList.Add(sb.ToString());
        }

        foreach (var monster in m_MonsterList)
        {
            sb.Clear();
            sb.Append("Monster/");
            sb.Append(monster.Name);
            ActorPopupList.Add(sb.ToString());
        }

        // 마지막에 사용한 캐릭터 인덱스 저장 
        
        m_SelectedActionIndex = 0;
    }

    private void LoadTrackData()
    {
        if (CurrentActionType == TrackData.eAction.None)
            return;

        var directoryPath = string.Format("{0}/{1}", JsonRootDirectory, CurrentActorName);
        var jsonFileName = string.Format("{0}.json", NamingHelper.GetTrackDataID(CurrentActorName, CurrentActionType));

        m_TrackDataCache = JsonHelper.LoadJson<TrackData>(directoryPath, jsonFileName);
    }

    private void LoadActionList()
    {
        m_ActionPopupList.Clear();
        m_ActionPopupList.Add("None");
        m_NewActionPopupList.Clear();
        m_NewActionPopupList.Add("None");

        if (string.IsNullOrEmpty(CurrentActorName))
            return;

        var targetDirectoryPath = string.Format("{0}/{1}", JsonRootDirectory, CurrentActorName);
        var dictionaryInfo = new DirectoryInfo(targetDirectoryPath);
        if(dictionaryInfo.Exists == false)
        {
            dictionaryInfo = Directory.CreateDirectory(targetDirectoryPath);
        }

        var fileInfos = dictionaryInfo.GetFiles();
        foreach (var fileInfo in fileInfos)
        {
            if (fileInfo.Extension.Contains(".meta"))
                continue;

            var splits = Path.GetFileNameWithoutExtension(fileInfo.Name).Split('_');
            var actionName = splits[1];
            m_ActionPopupList.Add(actionName);
        }
        
        foreach (var actionTypeName in Enum.GetNames(typeof(TrackData.eAction)))
        {
            if(m_ActionPopupList.Contains(actionTypeName) == false)
            {
                m_NewActionPopupList.Add(actionTypeName);
            }
        }

        m_SelectedActionIndex = 0;
        m_SelectedNewActionIndex = 0;
    }

    private void OpenScene()
    {
        // open scene timeline
        var currentActiveScene = EditorSceneManager.GetActiveScene();
        if (currentActiveScene.name.Contains(TrackToolSceneName) == false)
        {
            EditorSceneManager.OpenScene(ScenesFolderpath + TrackToolSceneName);
        }

        // selected target object
        if (m_ActorParentObject == null)
        {
            m_ActorParentObject = GameObject.Find(TargetParentObjectName);
        }
    }

    private void LoadGameDesignData()
    {
        GameDesignDataManager.Instance.Initialize();
    }

    private void CreateActor()
    {
        if (m_SelectedActorIndex == -1)
            return;

        var prefabName = string.Empty;
        if (m_SelectedActorIndex < m_PlayerDataList.Length)
        {
            prefabName = m_PlayerDataList[m_SelectedActorIndex].prefabName;
        }
        else
        {
            prefabName = m_MonsterList[m_SelectedActorIndex].prefabName;
        }

        if (string.IsNullOrEmpty(prefabName))
            return;

        var fullPath = string.Format("{0}/{1}/{2}", "Prefabs", IsPlayer ? "Player" : "Monster", prefabName);
        var actorPrefab = Resources.Load<GameObject>(fullPath);
        if (actorPrefab == null)
        {
            Debug.LogWarning(string.Format("Not Found Prefab / {0}", fullPath));
            return;
        }

        ClearTargetChildObject();

        m_ActorObject = Instantiate<GameObject>(actorPrefab, m_ActorParentObject ? m_ActorParentObject.transform : null, false);
    }

    private void ClearTargetChildObject()
    {
        if (m_ActorParentObject == null)
            return;

        if (m_ActorParentObject.transform.childCount > 0)
        {
            for (int icnt = 0; icnt < m_ActorParentObject.transform.childCount; icnt++)
            {
                GameObject.DestroyImmediate(m_ActorParentObject.transform.GetChild(icnt).gameObject);
            }
        }

        m_ActorObject = null;
        m_PlayerableDirector = null;
        m_TimelineAsset = null;
    }

    private void SetTimeline()
    {
        if (m_ActorObject == null)
            return;

        var timelineTrackInfo = new TimelineAssetControler.TimelineTrackInfo()
        {
            TimelineAsset = GetDefaultTimelineAsset(),
            PlayableDirector = m_PlayerableDirector,
            ActorName = CurrentActorName,
            IsPlayer = IsPlayer,
            PrefabName = GetPrefabName(), 
            ActionType = CurrentActionType,
            ActorAnimator = GetActorAnimator(),
            TrackData = m_TrackDataCache,
        };

        TMP_EditorCoroutine.StartCoroutine(AddPlayableDirector(timelineTrackInfo));
    }

    private string GetPrefabName()
    {
        if(IsPlayer)
        {
            return CurrentPlayerData != null ? CurrentPlayerData.prefabName : string.Empty;
        }
        else
        {
            return CurrentMonterData != null ? CurrentMonterData.prefabName : string.Empty;
        }
    }

    private Animator GetActorAnimator()
    {
        if (m_ActorObject == null)
            return null;

        return m_ActorObject.GetComponent<Animator>();
    }

    IEnumerator AddPlayableDirector(TimelineAssetControler.TimelineTrackInfo _trackInfo)
    {
        Selection.activeGameObject = m_ActorParentObject;

        yield return new WaitForEndOfFrame();

        m_PlayerableDirector = m_ActorObject.GetComponent<PlayableDirector>();
        if (m_PlayerableDirector == null)
        {
            m_PlayerableDirector = m_ActorObject.AddComponent<PlayableDirector>();
        }

        yield return new WaitForEndOfFrame();

        m_PlayerableDirector.playableAsset = _trackInfo.TimelineAsset;

        yield return new WaitForEndOfFrame();

        TimelineAssetControler.SetTimelineTrack(_trackInfo);

        yield return new WaitForEndOfFrame();

        Selection.activeGameObject = m_ActorObject;
    }

    private TimelineAsset GetDefaultTimelineAsset()
    {
        if(CurrentActionType ==  TrackData.eAction.None)
        {
            return null;
        }

        m_TimelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>(DefaultPlayableAssetFullPathWithName);
        if(m_TimelineAsset == null)
        {
            Debug.LogWarning(string.Format("Not Found File / Default TimelineAsset / {0}", DefaultPlayableAssetFullPathWithName));
        }

        return m_TimelineAsset;
    }

    private void ChangeActor()
    {
        LoadActionList();

        LoadTrackData();

        CreateActor();

        SetTimeline();
    }

    private void ChangeAction()
    {
        // 트랙데이터 다시 로드.
        LoadTrackData();

        // 타임라인 다시 셋팅.
        SetTimeline();
    }

    private void SaveJsonEventData(string _actorName, TrackData.eAction _actionType)
    {
        var currentEventDataList = TimelineAssetControler.GetEventDataList(m_TimelineAsset);
        if (currentEventDataList == null)
            return;

        var newTrackData = new TrackData()
        {
            ID = NamingHelper.GetTrackDataID(_actorName, _actionType),
            AnimationName = TimelineAssetControler.GetAnimationName(m_TimelineAsset),
            ActionType = _actionType.ToString(),
            EventDataArray = currentEventDataList.ToArray(),
        };

        var directoryPath = string.Format("{0}/{1}", JsonRootDirectory, _actorName);
        var jsonFileName = string.Format("{0}.json", newTrackData.ID);

        JsonHelper.SaveJson<TrackData>(directoryPath, jsonFileName, newTrackData);
    }

    private void NewAction()
    {
        var newActionTypeName = CurrentNewActionType.ToString();

        if (m_TrackDataCache == null)
            m_TrackDataCache = new TrackData();

        m_TrackDataCache.ID = NamingHelper.GetTrackDataID(CurrentActorName, CurrentNewActionType);
        m_TrackDataCache.ActionType = newActionTypeName;
        m_TrackDataCache.AnimationName = newActionTypeName;
        m_TrackDataCache.EventDataArray = null;

        m_ActionPopupList.Add(newActionTypeName);
        m_SelectedActionIndex = m_ActionPopupList.Count - 1;

        m_NewActionPopupList.Remove(newActionTypeName);
        m_SelectedNewActionIndex = (int)TrackData.eAction.None;

        SetTimeline();
    }
}
