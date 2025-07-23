using System.Collections.Generic;
using UnityEngine;
using Takuzu;
using Takuzu.Generator;
using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class DailyChallenges : MonoBehaviour {
    public static DailyChallenges instance;
    public static System.Action<Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>> RetriveAllChallengeLeaderBoards = delegate { };
    public static System.Action<Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>> RetriveAllContriesLeaderBoards = delegate { };
    public static System.Action<Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>> RetriveAllSocialLeaderBoards = delegate { };
    public static System.Action<GSData> RetriveChallengeEntriesCount = delegate { };
    public static System.Action<GSData> RetriveAllLbCount = delegate { };
    public static System.Action<GSData> RetriveAllRanksDatas = delegate { };
    public static System.Action RetriveAllLBData = delegate { };

    public static bool gotAllResponses = false;
    private int retriveCount = 0;
    [HideInInspector]
    public bool isRequesting = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    
    public static string DAILY_LB = "LB_DAILY_LV";
    public static string WEEKLY_LB = "LB_WEEKLY_LV";
    public static string LB_EXP = "LB_EXP";

    public static string DAILY_KEY = "DAILY";
    public static string WEEKLY_KEY = "WEEKLY";

    public static string COUNTRY_DAILY_LB = "LB_DAILY_COUNTRY_AVG_LV";
    public static string COUNTRY_WEEKLY_LB = "LB_WEEKLY_COUNTRY_AVG_LV";
    public static string LB_EXP_COUNTRY = "LB_EXP_COUNTRY";

    public enum ChallengeMode
    {
        Daily,
        Weekly
    }
    public Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>> allChallengesLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();
    public Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>> allCountriesLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();
    public Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>> allSocialLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();
    public GSData allLeaderBoardCount = new GSData();
    public GSData ChallengeEntriesData = new GSData();
    public GSData allRanksData = new GSData();

    [HideInInspector]
    public List<string> lbShortCodeList = new List<string>();

    private int numberOfRequestSend = 0;
    private int numberOfResponse = 0;
    private int numberOfSuccessResponse = 0;

    private int numberOfCountryRequestSend = 0;
    private int numberOfCountryResponse = 0;
    private int numberOfCountrySuccessResponse = 0;

    private int numberOfSocialRequestSend = 0;
    private int numberOfSocialResponse = 0;
    private int numberOfSocialSuccessResponse = 0;

    void Start () {
        DontDestroyOnLoad(gameObject);
        PuzzleManager.onChallengeListChanged += OnDailyPuzzleListChanged;
        CloudServiceManager.onGamesparkAuthenticated += OnGSAuthenticated;
        RetriveAllChallengeLeaderBoards += OnDataRetrive;
        RetriveAllContriesLeaderBoards += OnDataRetrive;
        RetriveAllSocialLeaderBoards += OnDataRetrive;
    }

    private void OnDataRetrive(Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>> obj)
    {
        retriveCount++;
        if (retriveCount == 3)
        {
            gotAllResponses = true;
            retriveCount = 0;
            isRequesting = false;
            RetriveAllLBData();
        }
    }

    private void OnDestroy()
    {
        PuzzleManager.onChallengeListChanged -= OnDailyPuzzleListChanged;
        CloudServiceManager.onGamesparkAuthenticated -= OnGSAuthenticated;
        RetriveAllChallengeLeaderBoards -= OnDataRetrive;
        RetriveAllContriesLeaderBoards -= OnDataRetrive;
        RetriveAllSocialLeaderBoards -= OnDataRetrive;
    }

    private void OnGSAuthenticated()
    {
        if (PuzzleManager.Instance==null)
            return;
        if (PuzzleManager.Instance.challengePuzzles == null)
            return;
        if (PuzzleManager.Instance.challengePuzzles.Count == 0)
            return;
        SetupDailyChallenges(PuzzleManager.Instance.challengePuzzles);
    }

    private void OnDailyPuzzleListChanged(List<Puzzle> puzzles)
    {
        SetupDailyChallenges(puzzles);
    }

    private void SetupDailyChallenges(List<Puzzle> puzzles)
    {
        if (isRequesting)
            return;
        if (!PuzzleManager.Instance)
            return;
        StopCoroutine("WaitForChallenges");
        StartCoroutine(WaitForChallenges());
    }

    private IEnumerator WaitForChallenges()
    {
        isRequesting = true;
        yield return new WaitUntil(() => { return PuzzleManager.Instance.challengePuzzles.Count > 0; });

        gotAllResponses = false;

        allChallengesLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();
        allCountriesLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();
        allSocialLeaderBoardEntryDictionary = new Dictionary<string, GSEnumerable<LeaderboardDataResponse._LeaderboardData>>();

        List<Puzzle> puzzles = PuzzleManager.Instance.challengePuzzles;
        List<string> puzzleIds = PuzzleManager.Instance.challengeIds;

        numberOfRequestSend += puzzleIds.Count +1;
        numberOfSocialRequestSend += puzzleIds.Count + 1;
        numberOfCountryRequestSend += puzzleIds.Count + 1;
        CloudServiceManager.Instance.GetDailyWeeklyChallengeEntry(OnChallengeEntryCallBack);
        lbShortCodeList = new List<string>();
        CloudServiceManager.Instance.GetAllLeaderBoardCount(OnLeaderboardCountResponse);
        foreach (var puzzleId in puzzleIds)
        {
            //Normal leaderboard
            if (puzzleId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, DAILY_LB);
                lbShortCodeList.Add(requestLbShortCode);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 100, 0, OnRequestLeaderboardData);
            }
            else if (puzzleId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, WEEKLY_LB);
                lbShortCodeList.Add(requestLbShortCode);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 100, 0, OnRequestLeaderboardData);
            }
            //Social leaderboard
            if (puzzleId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, DAILY_LB);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, true, 100, 0, OnRequestSocialLeaderboardData);
            }
            else if (puzzleId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, WEEKLY_LB);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, true, 100, 0, OnRequestSocialLeaderboardData);
            }
            //Countries leaderboard
            if (puzzleId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, COUNTRY_DAILY_LB);
                lbShortCodeList.Add(requestLbShortCode);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 100, 0, OnRequestCountryLeaderboardData);
            }
            else if (puzzleId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
            {
                string requestLbShortCode = ChallengeIdToLBShortCode(puzzleId, COUNTRY_WEEKLY_LB);
                lbShortCodeList.Add(requestLbShortCode);
                CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 100, 0, OnRequestCountryLeaderboardData);
            }
        }
        CloudServiceManager.Instance.GetLeaderboardEntriesRequest(OnLeaderBoardEntriesResponse);

        CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP, false, 100, 0, OnRequestLeaderboardData);
        CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP, true, 100, 0, OnRequestSocialLeaderboardData);
        CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP_COUNTRY, false, 100, 0, OnRequestCountryLeaderboardData);
    }


    internal string GetPuzzleIdFromLBShortCode(string lbShortCode)
    {
        Level level = GetLBLevel(lbShortCode);
        string idMatch = "";
        if (lbShortCode.Contains(DAILY_KEY))
        {
            idMatch = PuzzleManager.Instance.challengeIds.Find(puzzleid => (puzzleid.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX) && PuzzleManager.Instance.GetChallengeById(puzzleid).level == level));
        }else if (lbShortCode.Contains(WEEKLY_KEY))
        {
            idMatch = PuzzleManager.Instance.challengeIds.Find(puzzleid => (puzzleid.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX) && PuzzleManager.Instance.GetChallengeById(puzzleid).level == level));
        }
        return idMatch;
    }

    internal Level GetLBLevel(string lbShortCode)
    {
        string[] lbShortCodeStrArr = lbShortCode.Split('_');
        return (Level) int.Parse(Regex.Match(lbShortCodeStrArr[lbShortCodeStrArr.Length - 1], @"\d+").Value);
    }

    public string ChallengeIdToLBShortCode( string puzzleId, string prefix)
    {
        return prefix + (int)PuzzleManager.Instance.GetChallengeById(puzzleId).level;
    }

    private void OnLeaderBoardEntriesResponse(LogEventResponse response)
    {
        allRanksData = new GSData();
        allRanksData = response.ScriptData.GetGSData("ALL_RANKS");
        RetriveAllRanksDatas(allRanksData);
        //Debug.Log(response.ScriptData.GetGSData("ALL_RANKS").JSON.Replace(",", ",\n"));
    }

    private void OnLeaderboardCountResponse(LogEventResponse response)
    {
        GSData gSData = response.ScriptData;
        allLeaderBoardCount = new GSData();
        GSData data = response.ScriptData.GetGSData("LB_COUNT");
        allLeaderBoardCount = data;
        RetriveAllLbCount(allLeaderBoardCount);
        //Debug.Log(data.JSON.Replace(",", ",\n"));
    }

    private void OnChallengeEntryCallBack(LogEventResponse response)
    {
        ChallengeEntriesData = new GSData();
        GSData data = response.ScriptData.GetGSData("ENTRY_COUNT");
        ChallengeEntriesData = data;
        RetriveChallengeEntriesCount(ChallengeEntriesData);
        //Debug.Log(data.JSON.Replace(",",",\n"));
    }

    private void OnRequestCountryLeaderboardData(LeaderboardDataResponse response)
    {
        if (this == null)
            return;
        numberOfCountryResponse++;
        string responseLbShortCode = response.BaseData.GetString("leaderboardShortCode");
        if (string.IsNullOrEmpty(responseLbShortCode))
        {
            ////Debug.Log("Reject LB ");
        }
        else
        {
            GSEnumerable<LeaderboardDataResponse._LeaderboardData> data = response.Data;
            if (data == null || !data.GetEnumerator().MoveNext())
            {
                ////Debug.Log(string.Format("Data not found {0}", responseLbShortCode));
            }

            ////Debug.Log(challengePuzzleId);
            if (allCountriesLeaderBoardEntryDictionary.ContainsKey(responseLbShortCode))
                allCountriesLeaderBoardEntryDictionary.Remove(responseLbShortCode);
            allCountriesLeaderBoardEntryDictionary.Add(responseLbShortCode, data);
            numberOfCountrySuccessResponse++;
        }
        if (numberOfCountryResponse == numberOfCountryRequestSend)
        {
            RetriveAllContriesLeaderBoards(allCountriesLeaderBoardEntryDictionary);
            //Debug.Log(//DebugHelper.To//DebugString(allCountriesLeaderBoardEntryDictionary));
        }
    }

    private void OnRequestSocialLeaderboardData(LeaderboardDataResponse response)
    {
        if (this == null)
            return;
        numberOfSocialResponse++;
        string responseLbShortCode = response.BaseData.GetString("leaderboardShortCode");
        if (string.IsNullOrEmpty(responseLbShortCode))
        {
            ////Debug.Log("Reject LB ");
        }
        else
        {
            GSEnumerable<LeaderboardDataResponse._LeaderboardData> data = response.Data;
            if (data == null || !data.GetEnumerator().MoveNext())
            {
                ////Debug.Log(string.Format("Data not found {0}", responseLbShortCode));
            }
            ////Debug.Log(challengePuzzleId);
            if (allSocialLeaderBoardEntryDictionary.ContainsKey(responseLbShortCode))
                allSocialLeaderBoardEntryDictionary.Remove(responseLbShortCode);
            allSocialLeaderBoardEntryDictionary.Add(responseLbShortCode, data);
            numberOfSocialSuccessResponse++;

        }
        if (numberOfSocialResponse == numberOfSocialRequestSend)
        {
            RetriveAllSocialLeaderBoards(allSocialLeaderBoardEntryDictionary);
            //Debug.Log(//DebugHelper.To//DebugString(allSocialLeaderBoardEntryDictionary));
        }
    }

    private void OnRequestLeaderboardData(LeaderboardDataResponse response)
    {
        if (this == null)
            return;
        numberOfResponse++;
        string responseLbShortCode = response.BaseData.GetString("leaderboardShortCode");
        if (string.IsNullOrEmpty(responseLbShortCode))
        {
            ////Debug.Log("Reject LB ");
        }
        else
        {
            GSEnumerable<LeaderboardDataResponse._LeaderboardData> data = response.Data;
            if (data == null || !data.GetEnumerator().MoveNext())
            {
                ////Debug.Log(string.Format("Data not found {0}", responseLbShortCode));
            }

            ////Debug.Log(challengePuzzleId);
            if (allChallengesLeaderBoardEntryDictionary.ContainsKey(responseLbShortCode))
                allChallengesLeaderBoardEntryDictionary.Remove(responseLbShortCode);
            allChallengesLeaderBoardEntryDictionary.Add(responseLbShortCode, data);
            numberOfSuccessResponse++;
        }
        if (numberOfResponse == numberOfRequestSend)
        {
            RetriveAllChallengeLeaderBoards(allChallengesLeaderBoardEntryDictionary);
            //Debug.Log(//DebugHelper.To//DebugString(allChallengesLeaderBoardEntryDictionary));
        }
    }
}

public static class DebugHelper {
    public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        string res = "";
        foreach (var key in dictionary.Keys)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            res += "{"+key +":"+ value.ToString()+"}"+'\n';
        }
        return res;
    }
}
