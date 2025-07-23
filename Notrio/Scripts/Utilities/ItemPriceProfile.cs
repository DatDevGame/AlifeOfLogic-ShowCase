using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu
{
    [CreateAssetMenu(fileName = "New Item price profile", menuName = "App specific/Item Price", order = 0)]
    public class ItemPriceProfile : ScriptableObject
    {
        public static ItemPriceProfile active;

        public int flagPowerup;
        public int revealPowerup;
        public int clearPowerup;
        public int undoPowerup;
    }
}