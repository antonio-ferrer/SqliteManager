using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionManager
{
    public class AesEncryptor : IBasicEncryptor
    {
        private byte[] key;

        public IBasicEncryptor SetSecret(string key, string password)
        {
            var sha256 = new SHA256Managed();
            this.key = sha256.ComputeHash(Encoding.UTF8.GetBytes(key + password));
            return this;
        }

        public string Encrypt(string rawContent) => TryEncrypt(rawContent, out string encrypted) ? encrypted : null;


        public string Decrypt(string encrypted) => TryDecrypt(encrypted, out string decrypted) ? decrypted : null;
       

        public bool TryEncrypt(string rawContent, out string encrypted)
        {
            encrypted = null;
            if (string.IsNullOrEmpty(rawContent))
            {
                return false;
            }
            try
            {
                var plainText = Encoding.UTF8.GetBytes(rawContent);
                using (var aesAlg = Aes.Create())
                {
                    aesAlg.Key = this.key;
                    aesAlg.GenerateIV();
                    var iv = aesAlg.IV;

                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, iv);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainText, 0, plainText.Length);

                    var result = new byte[iv.Length + encryptedBytes.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);
                    encrypted = Convert.ToBase64String(result);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool TryDecrypt(string encrypted, out string decrypted)
        {
            decrypted = null;
            if (string.IsNullOrEmpty(encrypted))
            {
                return false;
            }
            var cipherText = Convert.FromBase64String(encrypted);
            var ivSize = Aes.Create().BlockSize / 8;
            var iv = new byte[ivSize];
            Buffer.BlockCopy(cipherText, 0, iv, 0, ivSize);

            var encryptedBytes = new byte[cipherText.Length - ivSize];
            Buffer.BlockCopy(cipherText, ivSize, encryptedBytes, 0, encryptedBytes.Length);

            try
            {
                using (var aesAlg = Aes.Create())
                {
                    aesAlg.Key = this.key;
                    aesAlg.IV = iv;
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    decrypted = Encoding.UTF8.GetString(decryptedBytes);
                }
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
