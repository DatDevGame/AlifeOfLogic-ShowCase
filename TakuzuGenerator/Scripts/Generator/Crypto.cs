using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Text;
using System.IO;

namespace Takuzu.Generator
{
    public static class Crypto
    {
        private static int blockSize = 256;
        public static int BlockSize
        {
            get
            {
                return blockSize;
            }
            set
            {
                if (blockSize != 128 && blockSize != 192 && blockSize != 256)
                {
                    throw new CryptographicException("Block size must be 128, 192 or 256");
                }
                else
                {
                    blockSize = value;
                }
            }
        }

        private static string key = "qwertyuiopasdfghjklzxcvbnm123456";
        public static string Key
        {
            get
            {
                return key;
            }
            set
            {
                if (value.Length * 8 != blockSize)
                {
                    throw new CryptographicException("Key length must be " + blockSize / 8);
                }
                else
                {
                    key = value;
                }
            }
        }

        private static string iv = "mnbvcxzasdfghjklpoiuytrewq162534";
        public static string Iv
        {
            get
            {
                return iv;
            }
            set
            {
                if (value.Length * 8 != blockSize)
                {
                    throw new CryptographicException("IV length must be " + blockSize / 8);
                }
                else
                {
                    iv = value;
                }
            }
        }

        public static void SetDefaultKey(CryptoKey key)
        {
            BlockSize = (int)key.blockSize;
            Key = key.key;
            Iv = key.iv;
        }

        public static string Encrypt(string src, CryptoKey cryptoKey = null)
        {
            if (cryptoKey && cryptoKey.preEncrypted != null)
            {
                PreEncryptPair pair = cryptoKey.preEncrypted.Find((p) =>
                {
                    return p.value.Equals(src);
                });
                if (!string.IsNullOrEmpty(pair.encryptedValue))
                {
                    return pair.encryptedValue;
                }
            }

            byte[] keyBytes = ASCIIEncoding.UTF8.GetBytes(cryptoKey ? cryptoKey.key : key);
            byte[] ivBytes = ASCIIEncoding.UTF8.GetBytes(cryptoKey ? cryptoKey.iv : iv);
            int size = cryptoKey ? (int)cryptoKey.blockSize : blockSize;
            string encrypted = string.Empty;

            try
            {
                // Create an Rijndael object
                // with the specified key and IV.
                using (Rijndael rj = new RijndaelManaged())
                {
                    rj.BlockSize = size;
                    rj.Key = keyBytes;
                    rj.IV = ivBytes;

                    // Create an encryptor to perform the stream transform.
                    ICryptoTransform encryptor = rj.CreateEncryptor();

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {

                                //Write all data to the stream.
                                swEncrypt.Write(src);
                            }
                            encrypted = Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: {0}", e.Message);
            }

            return encrypted;
        }

        public static string Decrypt(string src, CryptoKey cryptoKey = null)
        {
            if (cryptoKey && cryptoKey.preEncrypted != null)
            {
                PreEncryptPair pair = cryptoKey.preEncrypted.Find((p) =>
                {
                    return p.encryptedValue.Equals(src);
                });
                if (!string.IsNullOrEmpty(pair.encryptedValue))
                {
                    return pair.value.ToString();
                }
            }

            byte[] srcBytes = Convert.FromBase64String(src);
            byte[] keyBytes = ASCIIEncoding.UTF8.GetBytes(cryptoKey ? cryptoKey.key : key);
            byte[] ivBytes = ASCIIEncoding.UTF8.GetBytes(cryptoKey ? cryptoKey.iv : iv);
            int size = cryptoKey ? (int)cryptoKey.blockSize : blockSize;
            string decrypted = "";
            try
            {
                // Create an Rijndael object
                // with the specified key and IV.
                using (RijndaelManaged rj = new RijndaelManaged())
                {
                    rj.BlockSize = size;
                    rj.Key = keyBytes;
                    rj.IV = ivBytes;

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = rj.CreateDecryptor();

                    // Create the streams used for decryption.
                    using (MemoryStream msDecrypt = new MemoryStream(srcBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {

                                // Read the decrypted bytes from the decrypting stream
                                // and place them in a string.
                                decrypted = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: {0}", e.Message);
            }

            return decrypted;
        }
    }
}