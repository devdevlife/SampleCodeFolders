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

public partial class TrackToolWindow : EditorWindow
{
    private readonly string ResetButton = "Reset";
    private readonly string TrackToolSceneName = "SampleScene.unity";
    private readonly string ScenesFolderpath = "Assets/Scenes/";
    private readonly string TargetParentObjectName = "Target";

    private PlayerData[] PlayerList;
    private MonsterData[] MonsterList;
    private List<string> ActorPopupList = new List<string>();
    private Dictionary<string, FileInfo> m_JsonFileInfoList = new Dictionary<string, FileInfo>();
    private List<string> ActionPopupList = new List<string>();

    private GameObject ActorParentObject { get; set; }
    private GameObject ActorObject { get; set; }
    private PlayableDirector m_PlayerableDirector { get; set; }
    private PlayableAsset m_TimelinePlayableAsset { get; set; }
    private TrackData m_TrackDataCache { get; set; }

    private int SelectedActorIndex { get; set; }
    private int SelectedActionIndex { get; set; }

    
    protected bool IsPlayer
    {
        get
        {
            if (PlayerList == null)
                return true;

            return SelectedActorIndex < PlayerList.Length ? true : false;
        }
    }

    protected string CurrentActorName 
    {
        get
        {
            if (ActorPopupList == null ||
                ActorPopupList.Count <= SelectedActorIndex)
                return string.Empty;

            if(SelectedActorIndex == -1)
                return string.Empty;

            return ActorPopupList[SelectedActorIndex].Split('/')[1];
        }
    }

    protected TrackData.eAction CurrentActionType
    {
        get
        {
            if (ActionPopupList == null ||
                ActionPopupList.Count <= SelectedActionIndex)
                return TrackData.eAction.None;

            return (TrackData.eAction)Enum.ToObject(typeof(TrackData.eAction), SelectedActionIndex);
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

        DrawTrackList();

        DrawJsonOption();
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
        var changeActorNumber = EditorGUILayout.Popup("Selected Actor", SelectedActorIndex, ActorPopupList.ToArray());
        if (changeActorNumber != SelectedActorIndex)
        {
            SelectedActorIndex = changeActorNumber;
            ChangeActor();
        }
    }

    private void DrawTrackList()
    {
        var changeActionIndex = EditorGUILayout.Popup("Action Type", SelectedActionIndex, ActionPopupList.ToArray());
        if (changeActionIndex != SelectedActionIndex)
        {
            SelectedActionIndex = changeActionIndex;
            ChangeAction();
        }
    }

    private void DrawJsonOption()
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

                }
            }
            EndDisableGroup(CurrentActionType == TrackData.eAction.None);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void LoadActorListInfo()
    {
        ActorPopupList.Clear();

        PlayerList = GameDesignDataManager.Instance.FindAll<PlayerData>().ToArray();
        MonsterList = GameDesignDataManager.Instance.FindAll<MonsterData>().ToArray();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var player in PlayerList)
        {
            sb.Clear();
            sb.Append("Player/");
            sb.Append(player.Name);
            ActorPopupList.Add(sb.ToString());
        }

        foreach (var monster in MonsterList)
        {
            sb.Clear();
            sb.Append("Monster/");
            sb.Append(monster.Name);
            ActorPopupList.Add(sb.ToString());
        }

        // 마지막에 사용한 캐릭터 인덱스 저장 
        
        SelectedActionIndex = 0;
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
        ActionPopupList.Clear();
        ActionPopupList.Add("None");
        m_JsonFileInfoList.Clear();

        if (string.IsNullOrEmpty(CurrentActorName))
            return;

        var targetDirectoryPath = string.Format("{0}/{1}", JsonRootDirectory, CurrentActorName);
        var dictionaryInfo = new DirectoryInfo(targetDirectoryPath);
        var fileInfos = dictionaryInfo.GetFiles();
        foreach (var fileInfo in fileInfos)
        {
            if (fileInfo.Extension.Contains(".meta"))
                continue;

            var splits = Path.GetFileNameWithoutExtension(fileInfo.Name).Split('_');
            var actionName = splits[1];
            ActionPopupList.Add(actionName);
            m_JsonFileInfoList.Add(actionName, fileInfo);
        }

        SelectedActionIndex = 0;
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
        if (ActorParentObject == null)
        {
            ActorParentObject = GameObject.Find(TargetParentObjectName);
        }
    }

    private void LoadGameDesignData()
    {
        GameDesignDataManager.Instance.Initialize();
    }

    private void CreateActor()
    {
        if (SelectedActorIndex == -1)
            return;

        var prefabName = string.Empty;
        if(SelectedActorIndex < PlayerList.Length)
        {
            prefabName = PlayerList[SelectedActorIndex].prefabName;
        }
        else
        {
            prefabName = MonsterList[SelectedActorIndex].prefabName;
        }

        if (string.IsNullOrEmpty(prefabName))
            return;

        var fullPath = string.Format("{0}/{1}/{2}", "Prefabs", IsPlayer ? "Player" : "Monster", prefabName);
        var actorPrefab = Resources.Load<GameObject>(fullPath);
        if(actorPrefab == null)
        {
            Debug.LogWarning(string.Format("Not Found Prefab / {0}", fullPath));
            return;
        }

        if(ActorObject != null)
        {
            ClearActorData();
        }

        ActorObject = Instantiate<GameObject>(actorPrefab, ActorParentObject ? ActorParentObject.transform : null, false);

        TMP_EditorCoroutine.StartCoroutine(AddPlayableDirector());
    }

    private void SetTimeline()
    {
        TimelineAssetControler.SetTimelineTrack(m_TimelinePlayableAsset, m_TrackDataCache);
    }

    private void ClearActorData()
    {
        GameObject.DestroyImmediate(ActorObject);
        ActorObject = null;
        m_PlayerableDirector = null;
        m_TimelinePlayableAsset = null;
    }

    IEnumerator AddPlayableDirector()
    {
        yield return new WaitForEndOfFrame();

        m_PlayerableDirector = ActorObject.GetComponent<PlayableDirector>();
        if (m_PlayerableDirector == null)
        {
            m_PlayerableDirector = ActorObject.AddComponent<PlayableDirector>();
        }

        yield return new WaitForEndOfFrame();

        m_PlayerableDirector.playableAsset = GetDefaultTimelinePlayableAsset();

        yield return new WaitForEndOfFrame();

        Selection.activeGameObject = ActorObject;
    }

    private PlayableAsset GetDefaultTimelinePlayableAsset()
    {
        m_TimelinePlayableAsset = AssetDatabase.LoadAssetAtPath<PlayableAsset>(DefaultPlayableAssetFullPathWithName);
        return m_TimelinePlayableAsset;
    }

    private void ChangeActor()
    {
        CreateActor();

        SetTimeline();
    }

    private void ChangeAction()
    {
        // 트랙데이터 다시 로드.

        // 타임라인 다시 셋팅.
    }

    private void SaveJsonEventData(string _actorName, TrackData.eAction _actionType)
    {
        var currentEventDataList = TimelineAssetControler.GetEventDataList(m_TimelinePlayableAsset);
        if (currentEventDataList == null)
            return;

        var newTrackData = new TrackData()
        {
            ID = NamingHelper.GetTrackDataID(_actorName, _actionType),
            AnimationName = TimelineAssetControler.GetAnimationName(m_TimelinePlayableAsset),
            EventDataArray = currentEventDataList.ToArray(),
        };

        var directoryPath = string.Format("{0}/{1}", JsonRootDirectory, _actorName);
        var jsonFileName = string.Format("{0}.json", newTrackData.ID);

        JsonHelper.SaveJson<TrackData>(directoryPath, jsonFileName, newTrackData);
    }
}
