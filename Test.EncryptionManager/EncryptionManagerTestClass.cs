using EncryptionManager;

namespace Test.EncryptionManager
{
    public class EncryptionManagerTestClass
    {
        [Fact]
        public void AesEncryptSomething()
        {
            AesEncryptor aesEncryptor = new AesEncryptor();
            aesEncryptor.SetSecret("bambu pra varal", "curtir o terra samba não é nada malll aeh");
            string originalPhrase = "Para a próxima receita vamos precisar de 2 col de sopa de azeite, 3 dentes de alho e uma pitada se sal";
            string secret = aesEncryptor.Encrypt(originalPhrase);
            string decrypt = aesEncryptor.Decrypt(secret);
            Assert.Equal(originalPhrase, decrypt);
        }

        [Fact]
        public void TryEncryptUsingDifferentSecret()
        {
            AesEncryptor aesEncryptor = new AesEncryptor();
            aesEncryptor.SetSecret("bambu pra varal", "curtir o terra samba não é nada malll aeh");
            string originalPhrase = "Para a próxima receita vamos precisar de 2 col de sopa de azeite, 3 dentes de alho e uma pitada se sal";
            string secret = aesEncryptor.Encrypt(originalPhrase);
            aesEncryptor.SetSecret("babalu", "ploc");
            string decrypt = aesEncryptor.Decrypt(secret);
            Assert.NotEqual(originalPhrase, decrypt);
        }

        [Fact]
        public void TryEncryptUsingDifferentInstancet()
        {
            AesEncryptor aesEncryptor = new AesEncryptor();
            aesEncryptor.SetSecret("bambu pra varal", "curtir o terra samba não é nada malll aeh");
            string originalPhrase = "Para a próxima receita vamos precisar de 2 col de sopa de azeite, 3 dentes de alho e uma pitada se sal";
            string secret = aesEncryptor.Encrypt(originalPhrase);
            aesEncryptor = new AesEncryptor();
            aesEncryptor.SetSecret("bambu pra varal", "curtir o terra samba não é nada malll aeh");
            string decrypt = aesEncryptor.Decrypt(secret);
            Assert.Equal(originalPhrase, decrypt);
        }
    }
}