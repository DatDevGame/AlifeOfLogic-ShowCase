using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberToStringUltility{
    public static string ShortenNumberCount(int count)
    {
        float m = count / 1000000f;
        float k = count / 1000f;
        if (m > 1)
        {
            return string.Format("{0:F1} M", m);
        }else if(k > 1)
        {
            return string.Format("{0:F1} K", k);
        }
        else
        {
            return count.ToString();
        }
    }
}
