using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Takuzu.Generator;

[System.Serializable]
[CreateAssetMenu(fileName = "New grading profile", menuName = "App specific/Grading Profile", order = 0)]
public class GradingProfile : ScriptableObject
{
    public static GradingProfile active;

    public Dictionary<string, LevelDef> map;

    public LevelDef UnGraded;

    public LevelDef SixEasy;
    public LevelDef SixMedium;
    public LevelDef SixHard;
    public LevelDef SixEvil;
    public LevelDef SixInsane;

    public LevelDef EightEasy;
    public LevelDef EightMedium;
    public LevelDef EightHard;
    public LevelDef EightEvil;
    public LevelDef EightInsane;

    public LevelDef TenEasy;
    public LevelDef TenMedium;
    public LevelDef TenHard;
    public LevelDef TenEvil;
    public LevelDef TenInsane;

    public LevelDef TwelveEasy;
    public LevelDef TwelveMedium;
    public LevelDef TwelveHard;
    public LevelDef TwelveEvil;
    public LevelDef TwelveInsane;

    public LevelDef FourteenEasy;
    public LevelDef FourteenMedium;
    public LevelDef FourteenHard;
    public LevelDef FourteenEvil;
    public LevelDef FourteenInsane;

    public GradingProfile()
    {
        map = new Dictionary<string, LevelDef>();

        UnGraded = LevelDef.UnGraded.Clone() ;
        AddToMap(UnGraded);

        SixEasy = LevelDef.SixEasy.Clone();
        SixMedium = LevelDef.SixMedium.Clone();
        SixHard = LevelDef.SixHard.Clone();
        SixEvil = LevelDef.SixEvil.Clone();
        SixInsane = LevelDef.SixInsane.Clone();
        AddToMap(SixEasy, SixMedium, SixHard, SixEvil, SixInsane);

        EightEasy = LevelDef.EightEasy.Clone();
        EightMedium = LevelDef.EightMedium.Clone();
        EightHard = LevelDef.EightHard.Clone();
        EightEvil = LevelDef.EightEvil.Clone();
        EightInsane = LevelDef.EightInsane.Clone();
        AddToMap(EightEasy, EightMedium, EightHard, EightEvil, EightInsane);

        TenEasy = LevelDef.TenEasy.Clone();
        TenMedium = LevelDef.TenMedium.Clone();
        TenHard = LevelDef.TenHard.Clone();
        TenEvil = LevelDef.TenEvil.Clone();
        TenInsane = LevelDef.TenInsane.Clone();
        AddToMap(TenEasy, TenMedium, TenHard, TenEvil, TenInsane);

        TwelveEasy = LevelDef.TwelveEasy.Clone();
        TwelveMedium = LevelDef.TwelveMedium.Clone();
        TwelveHard = LevelDef.TwelveHard.Clone();
        TwelveEvil = LevelDef.TwelveEvil.Clone();
        TwelveInsane = LevelDef.TwelveInsane.Clone();
        AddToMap(TwelveEasy, TwelveMedium, TwelveHard, TwelveEvil, TwelveInsane);

        FourteenEasy = LevelDef.FourteenEasy.Clone();
        FourteenMedium = LevelDef.FourteenMedium.Clone();
        FourteenHard = LevelDef.FourteenHard.Clone();
        FourteenEvil = LevelDef.FourteenEvil.Clone();
        FourteenInsane = LevelDef.FourteenInsane.Clone();
        AddToMap(FourteenEasy, FourteenMedium, FourteenHard, FourteenEvil, FourteenInsane);
    }

    private void AddToMap(params LevelDef[] levelDefs)
    {
        foreach (LevelDef ld in levelDefs)
        {
            string code = LevelDef.GetLevelCode(ld.size, ld.level);
            map.Add(code, ld);
        }
    }
}
