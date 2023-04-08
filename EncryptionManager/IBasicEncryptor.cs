using System;
using System.Collections.Generic;
using System.Text;

namespace EncryptionManager
{
    public interface IBasicEncryptor
    {
        IBasicEncryptor SetSecret(string key, string password);

        bool TryEncrypt(string rawContent, out string encrypted);

        bool TryDecrypt(string encrypted, out string decrypted);

        string Encrypt(string rawContent);

        string Decrypt(string encrypted);  
    }
}
