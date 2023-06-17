using CommonAccessDataObjectHelper;
using EncryptionManager;
using FileHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LocalCryptoDb
{
    public class LocalCryptoDbManager : ICommonDbManager, IDisposable
    {
        private readonly IBasicEncryptor _encryptor;
        private readonly ICommonDbManager _dbManager;
        private string _workingFilePath;
        private Action<byte[]> _writeEncryptDb;
        private bool disposedValue;
        private bool initialized;

        private LocalCryptoDbManager()
        {
        }

        public LocalCryptoDbManager(Func<IBasicEncryptor> getEncryptor, Func<ICommonDbManager> getDbManager)
        {
            if (getEncryptor == null)
                throw new ArgumentNullException(nameof(getEncryptor));
            if (getDbManager == null)
                throw new ArgumentNullException(nameof(getDbManager));
            _encryptor = getEncryptor.Invoke();
            if (_encryptor == null)
                throw new NotSupportedException("getEncryptor returns null");
            _dbManager = getDbManager.Invoke();
            if (_dbManager == null)
                throw new NotSupportedException("getDbManager returns null");
        }

        public LocalCryptoDbManager Initialize(Func<byte[]> getEcryptedDb, Action<byte[]> writeEncryptDb, string workingFilePath)
        {
            CheckIfIsDisposed();
            if (initialized)
                throw new NotSupportedException("the system is already initialized");
            _workingFilePath = workingFilePath ?? $"{Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())}.db";
            _writeEncryptDb = writeEncryptDb ?? throw new ArgumentNullException(nameof(writeEncryptDb));
            var encryptedBytes = getEcryptedDb?.Invoke();
            if (encryptedBytes != null)
            {
                LoadAndDecryptExtantDb(encryptedBytes);
            }
            _dbManager.Initialize(new KeyValuePair<string, string>("path", _workingFilePath));
            initialized = true;
            return this;
        }

        public LocalCryptoDbManager Initialize(string encryptedDbPath, string workingFilePath)
        {
            CheckIfIsDisposed();
            Func<byte[]> getEcryptedDb = () => File.Exists(encryptedDbPath) ? File.ReadAllBytes(encryptedDbPath) : null;
            Action<byte[]> writeEncryptDb = (bytes) => File.WriteAllBytes(encryptedDbPath, bytes);
            return Initialize(getEcryptedDb, writeEncryptDb, workingFilePath);
        }

        public LocalCryptoDbManager Initialize(string encryptedDbPath)
        {
            string tempDbPath = $@"{Path.GetTempPath()}\lcdb_{Path.GetRandomFileName()}"; 
            return Initialize(encryptedDbPath, tempDbPath);
        }

        public ICommonDbManager Initialize(params KeyValuePair<string, string>[] config)
        {
            CheckIfIsDisposed();
            string encryptedDbPath = config.FirstOrDefault(c => Regex.IsMatch(c.Key, @"crypt", RegexOptions.IgnoreCase)).Value;
            string workingFilePath = config.FirstOrDefault(c => Regex.IsMatch(c.Key, @"work", RegexOptions.IgnoreCase)).Value;
            if (string.IsNullOrEmpty(encryptedDbPath))
            {
                throw new ArgumentException($"config needs to contain parameter {nameof(encryptedDbPath)}", nameof(config));
            }
            if (string.IsNullOrEmpty(workingFilePath))
            {
                throw new ArgumentException($"config needs to contain parameter {nameof(workingFilePath)}", nameof(config));
            }
            return Initialize(encryptedDbPath, workingFilePath);
        }

        public void ExecuteCommand(StringSqlCommand stringSqlCommand)
        {
            CheckIfIsDisposed();
            _dbManager.ExecuteCommand(stringSqlCommand);
        }

        public DataTable GetData(StringSqlCommand stringSqlCommand)
        {
            CheckIfIsDisposed();
            return _dbManager.GetData(stringSqlCommand);
        }

        public IEnumerable<T> GetEntity<T>(StringSqlCommand stringSqlCommand, Func<IDataRecord, T> parse)
        {
            CheckIfIsDisposed();
            return _dbManager.GetEntity<T>(stringSqlCommand, parse);
        }

        private void LoadAndDecryptExtantDb(byte[] encryptedDb)
        {
            if (encryptedDb != null)
            {
                var encryptedString = UTF8Encoding.UTF8.GetString(encryptedDb);
                var decryptedBase64 = _encryptor.Decrypt(encryptedString);
                if (string.IsNullOrEmpty(decryptedBase64))
                {
                    throw new InvalidDataException("fail on decrypt db! check your encryptor, key and password");
                }
                var decryptedBytes = Convert.FromBase64String(decryptedBase64);
                File.WriteAllBytes(_workingFilePath, decryptedBytes);
            }
            else
            {
                throw new ArgumentNullException(nameof(encryptedDb));
            }
        }

        public void PersistAndEncryptDb()
        {
            CheckIfIsDisposed();
            FileOperations.WaitForReleaseFile(_workingFilePath, TimeSpan.FromSeconds(30), true);
            var decryptedBytes = File.ReadAllBytes(_workingFilePath);
            var decryptedBase64 = Convert.ToBase64String(decryptedBytes);
            var encryptedString = _encryptor.Encrypt(decryptedBase64);
            var encryptedBytes = Encoding.UTF8.GetBytes(encryptedString);
            _writeEncryptDb.Invoke(encryptedBytes);
        }

        private void CheckIfIsDisposed()
        {
            if (disposedValue)
                throw new ObjectDisposedException("the object has already been disposed");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    (_encryptor as IDisposable)?.Dispose();
                    (_dbManager as IDisposable)?.Dispose();
                    PersistAndEncryptDb();
                    if (File.Exists(_workingFilePath))
                        File.Delete(_workingFilePath);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


    }
}
