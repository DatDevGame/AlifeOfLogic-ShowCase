using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Takuzu.Generator
{

    [CreateAssetMenu(fileName = "Key", menuName = "App specific/Key", order = 0)]
    public class CryptoKey : ScriptableObject
    {
        public CryptoBlockSize blockSize = CryptoBlockSize.Size256;
        public string key = "mnbvcxzasdfghjklpoiuytrewq123456";
        public string iv = "654321qwertyuioplkjhgfdsazxcvbnm";

        public List<PreEncryptPair> preEncrypted;

        public void BakeValue(int min, int max)
        {
            for (int i = min; i <= max; ++i)
            {
                string s = Crypto.Encrypt(i.ToString(), this);
                PreEncryptPair pep = new PreEncryptPair
                {
                    value = i.ToString(),
                    encryptedValue = s
                };
                preEncrypted.Add(pep);
            }
        }
    }

    [System.Serializable]
    public struct PreEncryptPair
    {
        public string value;
        public string encryptedValue;
    }

    public enum CryptoBlockSize
    {
        Size128 = 128,
        Size192 = 192,
        Size256 = 256
    }
}