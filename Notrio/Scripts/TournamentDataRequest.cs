using GameSparks.Api.Responses;
using GameSparks.Core;
using System;
using System.Collections.Generic;
using Takuzu;
using UnityEngine;

public class TournamentDataRequest {
    public const float minimumResquestSeconds = 30;
    public const string DAILY_LB = "LB_DAILY_LV";
    public const string WEEKLY_LB = "LB_WEEKLY_LV";
    public const string LB_EXP = "LB_EXP";

    public const string DAILY_KEY = "DAILY";
    public const string WEEKLY_KEY = "WEEKLY";

    public const string COUNTRY_DAILY_LB = "LB_DAILY_COUNTRY_AVG_LV";
    public const string COUNTRY_WEEKLY_LB = "LB_WEEKLY_COUNTRY_AVG_LV";
    public const string LB_EXP_COUNTRY = "LB_EXP_COUNTRY";

    public  static Dictionary<RequestInfor, ResponseInfor> leadboardDataDictionary = new Dictionary<RequestInfor, ResponseInfor>();

    public  static Dictionary<CountRequestInfor, CountResponseInfor> countDataDictionary = new Dictionary<CountRequestInfor, CountResponseInfor>();

    public struct CountRequestInfor
    {
        public LeadboardPlayerCountType countType;
        public CountRequestInfor(LeadboardPlayerCountType type)
        {
            countType = type;
        }
    }

    public class CountResponseInfor
    {
        public List<Action<GSData>> callbacks = new List<Action<GSData>>();
        public GSData response;
        public float lastRequest = -1;
        public bool isRequesting = false;
    }

    public struct RequestInfor
    {
        public string id;
        public LeaderboardType type;
        public LeaderboardGroup group;
        public RequestInfor(string id, LeaderboardType type, LeaderboardGroup group)
        {
            this.id = id;
            this.type = type;
            this.group = group;
        }
    }

    public class ResponseInfor
    {
        public List<Action<List<LeaderboardDataResponse._LeaderboardData>>> requestCallback = new List<Action<List<LeaderboardDataResponse._LeaderboardData>>>();
        public LeaderboardDataResponse response;
        public List<LeaderboardDataResponse._LeaderboardData> fetchedData = new List<LeaderboardDataResponse._LeaderboardData>();
        public float lastRequest = -1;
        public bool isRequesting = false;
    }

    public enum LeaderboardGroup
    {
        Normal,
        Social,
        Contries
    }

    public enum LeaderboardType
    {
        SolvingTime,
        Exp
    }

    public enum LeadboardPlayerCountType
    {
        DailyWeeklyCount,
        AllLeaderboardCount,
        AllLeaderboardEntriesCount
    }

    public enum ChallengeMode
    {
        Daily,
        Weekly
    }

    public static string ChallengeIdToLBShortCode(string puzzleId, string prefix)
    {
        return prefix + (int)PuzzleManager.Instance.GetChallengeById(puzzleId).level +"_SIZE"+ (int)PuzzleManager.Instance.GetChallengeById(puzzleId).size;
    }

    public static List<LeaderboardDataResponse._LeaderboardData> GetLeaderboardDataFromLbResponse(LeaderboardDataResponse response)
    {
        if (response == null)
            return null;
        string responseLbShortCode = response.BaseData.GetString("leaderboardShortCode");
        if (string.IsNullOrEmpty(responseLbShortCode))
        {
            return null;
        }
        else
        {
            return new List<LeaderboardDataResponse._LeaderboardData>(response.Data);
        }
    }

    public static string GetLbShortCode(LeaderboardGroup leaderboardGroup, LeaderboardType leaderboardType, string challengeId = "", bool socialPrefix = false)
    {
        string LbShortCode = "";
        switch (leaderboardType)
        {
            case LeaderboardType.SolvingTime:
                LbShortCode = GetSolvingTimeLbShortCode(leaderboardGroup, challengeId, socialPrefix);
                break;
            case LeaderboardType.Exp:
                LbShortCode = GetExpLbShortCode(leaderboardGroup, socialPrefix);
                break;
            default:
                break;
        }
        return LbShortCode;
    }

    private static string GetExpLbShortCode(LeaderboardGroup leaderboardGroup, bool socialPrefix = false)
    {
        switch (leaderboardGroup)
        {
            case LeaderboardGroup.Normal:
                return LB_EXP;
            case LeaderboardGroup.Social:
                return (socialPrefix?"SOCIAL_"+ LB_EXP:LB_EXP);
            case LeaderboardGroup.Contries:
                return LB_EXP_COUNTRY;
            default:
                return "";
        }
    }

    private static string GetSolvingTimeLbShortCode(LeaderboardGroup leaderboardGroup, string tournamentId, bool socialPrefix = false)
    {
        switch (leaderboardGroup)
        {
            case LeaderboardGroup.Normal:
                if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                {
                    return ChallengeIdToLBShortCode(tournamentId, DAILY_LB);
                }
                else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                {
                    return ChallengeIdToLBShortCode(tournamentId, WEEKLY_LB);
                }
                break;
            case LeaderboardGroup.Social:
                if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                {
                    return (socialPrefix ? "SOCIAL_" + ChallengeIdToLBShortCode(tournamentId, DAILY_LB) : ChallengeIdToLBShortCode(tournamentId, DAILY_LB));
                }
                else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                {
                    return (socialPrefix ? "SOCIAL_" + ChallengeIdToLBShortCode(tournamentId, WEEKLY_LB) : ChallengeIdToLBShortCode(tournamentId, WEEKLY_LB));
                }
                break;
            case LeaderboardGroup.Contries:
                if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                {
                    return ChallengeIdToLBShortCode(tournamentId, COUNTRY_DAILY_LB);
                }
                else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                {
                    return ChallengeIdToLBShortCode(tournamentId, COUNTRY_WEEKLY_LB);
                }
                break;
            default:
                break;
        }
        return "";
    }

    public static void RequestTournamentLeaderboard(LeaderboardGroup leaderboardGroup, LeaderboardType leaderboardType, Action<List<LeaderboardDataResponse._LeaderboardData>> callback = null,string tournamentId = "", bool addMore = false)
    {
        RequestInfor request = new RequestInfor(tournamentId, leaderboardType, leaderboardGroup);
        ResponseInfor response = new ResponseInfor();
        response.requestCallback.Add(callback);
        if (!leadboardDataDictionary.ContainsKey(request))
            leadboardDataDictionary.Add(request, response);
        else
        {
            ResponseInfor res;
            leadboardDataDictionary.TryGetValue(request, out res);
            if(res.response!=null && res.lastRequest != -1 && Time.time - res.lastRequest < minimumResquestSeconds && addMore == false)
            {
                callback(res.fetchedData);
                return;
            }
            else
            {
                res.requestCallback.Add(callback);
            }
            if (addMore == false)
            {
                res.fetchedData.Clear();
            }
        }

        var newRequest = new LBRequest();
        newRequest.MakeRequest(request);
    }

    public static void RequestTournamentPlayerCount(LeadboardPlayerCountType leadboardPlayerCountType, Action<GSData> callback)
    {
        CountRequestInfor request = new CountRequestInfor(leadboardPlayerCountType);
        CountResponseInfor response = new CountResponseInfor();
        response.callbacks.Add(callback);
        if (!countDataDictionary.ContainsKey(request))
            countDataDictionary.Add(request, response);
        else
        {
            CountResponseInfor res;
            countDataDictionary.TryGetValue(request, out res);
            if (res.response != null && res.lastRequest != -1 && Time.time - res.lastRequest < minimumResquestSeconds)
            {
                callback(res.response);
                return;
            }
            else
            {
                res.callbacks.Add(callback);
            }
        }
        var newRequest = new CountRequest();
        newRequest.MakeRequest(request);
    }

    public static void ClearCatchedData()
    {
        foreach (var key in leadboardDataDictionary.Keys)
        {
            ResponseInfor res;
            leadboardDataDictionary.TryGetValue(key, out res);
            res.lastRequest = -1;
            res.isRequesting = false;
            res.response = null;
        }
        foreach (var key in countDataDictionary.Keys)
        {
            CountResponseInfor res;
            countDataDictionary.TryGetValue(key, out res);
            res.lastRequest = -1;
            res.isRequesting = false;
            res.response = null;
        }
    }

    public static void UpdateData()
    {
        foreach (var key in leadboardDataDictionary.Keys)
        {
            var newRequest = new LBRequest();
            newRequest.MakeRequest(key);
        }
        foreach (var key in countDataDictionary.Keys)
        {
            var newRequest = new CountRequest();
            newRequest.MakeRequest(key);
        }
    }

    private class CountRequest
    {
        public CountRequestInfor requestInfor;

        public void MakeRequest(CountRequestInfor requestInfor)
        {
            this.requestInfor = requestInfor;
            CountResponseInfor res;
            countDataDictionary.TryGetValue(this.requestInfor, out res);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                onResponse(null);
            };
            if (res.callbacks.Count > 0 && !res.isRequesting)
            {
                res.isRequesting = true;
                res.lastRequest = Time.time;
                switch (this.requestInfor.countType)
                {
                    case LeadboardPlayerCountType.DailyWeeklyCount:
                        CloudServiceManager.Instance.GetDailyWeeklyChallengeEntry(onResponse);
                        break;
                    case LeadboardPlayerCountType.AllLeaderboardCount:
                        CloudServiceManager.Instance.GetAllLeaderBoardCount(onResponse);
                        break;
                    case LeadboardPlayerCountType.AllLeaderboardEntriesCount:
                        CloudServiceManager.Instance.GetLeaderboardEntriesRequest(onResponse);
                        break;
                    default:
                        break;
                }
            }
        }

        private void onResponse(LogEventResponse response)
        {
            CountResponseInfor res;
            countDataDictionary.TryGetValue(this.requestInfor, out res);
            if (res != null && response!=null && !response.HasErrors)
            {
                switch (this.requestInfor.countType)
                {
                    case LeadboardPlayerCountType.DailyWeeklyCount:
                        res.response = response.ScriptData.GetGSData("ENTRY_COUNT");
                        break;
                    case LeadboardPlayerCountType.AllLeaderboardCount:
                        res.response = response.ScriptData.GetGSData("LB_COUNT");
                        break;
                    case LeadboardPlayerCountType.AllLeaderboardEntriesCount:
                        res.response = response.ScriptData.GetGSData("ALL_RANKS");
                        break;
                    default:
                        break;
                }

            }
            else
            {
                res.response = null;
            }
            foreach (var callback in res.callbacks)
            {
                if (callback.Target == null)
                    continue;
                try
                {
                    callback(res.response);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            res.callbacks.Clear();
            res.isRequesting = false;

        }
    }

    private class LBRequest
    {
        public RequestInfor requestInfor;

        public void MakeRequest(RequestInfor requestInfor)
        {
            this.requestInfor = requestInfor;
            ResponseInfor res;
            leadboardDataDictionary.TryGetValue(this.requestInfor, out res);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                onResponse(null);
            };
            if (res.requestCallback.Count > 0 && !res.isRequesting)
            {
                res.isRequesting = true;

                res.lastRequest = Time.time;
                int offset = res.fetchedData.Count;
                switch (this.requestInfor.type)
                {
                    case LeaderboardType.SolvingTime:
                        RequestTournamentSolvingTimeLeaderboard(this.requestInfor.group, this.requestInfor.id, offset);
                        break;
                    case LeaderboardType.Exp:
                        RequestExpLeaderboard(this.requestInfor.group, offset);
                        break;
                    default:
                        break;
                }
                return;
            }
        }

        private void RequestExpLeaderboard(LeaderboardGroup leaderboardGroup, int offset)
        {
            switch (leaderboardGroup)
            {
                case LeaderboardGroup.Normal:
                    CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP, false, 12, offset, onResponse);
                    break;
                case LeaderboardGroup.Social:
                    CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP, true, 12, offset, onResponse);
                    break;
                case LeaderboardGroup.Contries:
                    CloudServiceManager.Instance.RequestLeaderboardData(LB_EXP_COUNTRY, false, 12, offset, onResponse);
                    break;
                default:
                    break;
            }
        }

        private void RequestTournamentSolvingTimeLeaderboard(LeaderboardGroup leaderboardGroup, string tournamentId, int offset)
        {
            switch (leaderboardGroup)
            {
                case LeaderboardGroup.Normal:
                    if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, DAILY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 12, offset, onResponse);
                    }
                    else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, WEEKLY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 12, offset, onResponse);
                    }
                    break;
                case LeaderboardGroup.Social:
                    if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, DAILY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, true, 12, offset, onResponse);
                    }
                    else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, WEEKLY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, true, 12, offset, onResponse);
                    }
                    break;
                case LeaderboardGroup.Contries:
                    if (tournamentId.StartsWith(PuzzleManager.DAILY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, COUNTRY_DAILY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 12, offset, onResponse);
                    }
                    else if (tournamentId.StartsWith(PuzzleManager.WEEKLY_PUZZLE_PREFIX))
                    {
                        string requestLbShortCode = ChallengeIdToLBShortCode(tournamentId, COUNTRY_WEEKLY_LB);
                        CloudServiceManager.Instance.RequestLeaderboardData(requestLbShortCode, false, 12, offset, onResponse);
                    }
                    break;
                default:
                    break;
            }
        }

        private void onResponse(LeaderboardDataResponse response)
        {
            ResponseInfor res;
            leadboardDataDictionary.TryGetValue(requestInfor, out res);
            
            if (res != null && response != null && !response.HasErrors)
            {
                res.response = response;
                List<LeaderboardDataResponse._LeaderboardData> responseData = GetLeaderboardDataFromLbResponse(response);
                if(responseData != null)
                {
                    long newFetchRankOffset = -1;
                    if(responseData.Count > 0)
                        newFetchRankOffset = responseData[0].Rank??-1;
                    long currentEndRank = 0;
                    if(res.fetchedData.Count > 0)
                        currentEndRank = res.fetchedData[res.fetchedData.Count - 1].Rank ?? 0;
                    
                    if(newFetchRankOffset > currentEndRank)
                        res.fetchedData.AddRange(responseData);
                    else
                        Debug.Log("Rank duplication -> discarded");
                }
            }
            else
            {
                res.response = null;
            }

            foreach (var callback in res.requestCallback)
            {
                if (callback.Target == null)
                    continue;
                try
                {
                    callback(GetLeaderboardDataFromLbResponse(res.response));
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            res.requestCallback.Clear();
            res.isRequesting = false;
        }
    }
}
