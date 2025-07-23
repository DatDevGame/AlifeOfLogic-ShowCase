using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System.Linq;

[CreateAssetMenu(fileName = "CountryCodeMapper", menuName = "App specific/Country code mapper", order = 0)]
[System.Serializable]
public class CountryCodeMapper : ScriptableObject
{
    public List<CountryCodeMapperEntry> map;

    public CountryCodeMapper()
    {
        map = new List<CountryCodeMapperEntry>();
        List<RegionInfo> country = GetCountriesByIso3166();
        for (int i = 0; i < country.Count; ++i)
        {
            CountryCodeMapperEntry entry = new CountryCodeMapperEntry
            {
                code = country[i].TwoLetterISORegionName,
                englishName = country[i].EnglishName
            };
            map.Add(entry);
        }
    }

    private static List<RegionInfo> GetCountriesByIso3166()
    {
        List<RegionInfo> countries = new List<RegionInfo>();
        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
        {
            RegionInfo country = new RegionInfo(culture.LCID);
            if (countries.Where(p => p.Name == country.Name).Count() == 0)
                countries.Add(country);
        }
        return countries.OrderBy(p => p.EnglishName).ToList();
    }

    public string ToEnglishName(string code)
    {
        int index = map.FindIndex((entry) => { return entry.code.Equals(code); });
        if (index>=0)
        {
            return map[index].englishName;
        }
        else
        {
            return string.Format("{0} (Unknown)", code);
        }
    }
}

[System.Serializable]
public struct CountryCodeMapperEntry
{
    public string code;
    public string englishName;
}