using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace FishDotNetKeyGen
{
    class Program
    {
        static void Main(string[] args)
        {
            License keyGen = new License();
            string key = keyGen.GenerateKey("sss", "5000", License.emProductName.fishConvert);
            Console.WriteLine(key);

            Console.ReadKey();
        }

    }

    class License
    {
        public License()
        {
            this.tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
            this.sha256Managed = new SHA256Managed();
            this.ProductName = new string[]
            {
                "fishDatabase",
                "fishDictionary",
                "fishConvert",
                "fishCodeLib",
                "fishCapture",
                "fishResource",
                "fishVocabulary",
                "fishDBMigration"
            };
        }

        public string GenerateKey(string user, string count, emProductName name)
        {
            string Result = string.Empty;
            string info = string.Empty;
            byte[] seed = null;
            byte[] key = null;

            switch (name)
            {
                case emProductName.fishDatabase:
                case emProductName.fishDictionary:
                case emProductName.fishConvert:
                case emProductName.fishCodeLib:
                case emProductName.fishCapture:
                case emProductName.fishResource:
                case emProductName.fishVocabulary:
                    seed = Encoding.UTF8.GetBytes(this.ProductName[(int)name]);

                    break;
                case emProductName.fishDBMigration:
                    int ProductType = 0;
                    string TimeOps = char.ConvertFromUtf32(2468 + 41);
                    string rtime = TimeOps + Convert.ToString(DateTime.Now.Subtract(DateTime.Now).Seconds);
                    seed = Encoding.UTF8.GetBytes(this.ProductName[(int)name] + rtime + Convert.ToString(ProductType));

                    break;
                default:
                    throw new Exception("Invalid Product Name");
            }

            info = this.ProductName[(int)name] + this.Seperator + user + this.Seperator + count;
            key = this.GetDesKey(seed);
            Result = DesEncrypt(info, key);

            return Result;
        }

        private string DesEncrypt(string info, byte[] key)
        {
            string Result = string.Empty;

            try
            {
                byte[] inputArray = Encoding.UTF8.GetBytes(info);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ICryptoTransform crptoTransform = this.tripleDESCryptoServiceProvider.CreateEncryptor(key,
                        this.GetDesIV(key, this.tripleDESCryptoServiceProvider.IV.Length)))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, crptoTransform, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(inputArray, 0, inputArray.Length);
                            cryptoStream.FlushFinalBlock();
                        }
                    }

                    Result = Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            catch (Exception)
            {
                throw;
            }

            return Result;
        }

        private byte[] GetDesKey(byte[] seed)
        {
            int num = this.tripleDESCryptoServiceProvider.Key.Length;
            checked
            {
                byte[] result = null;
                if (seed.Length != num)
                {
                    byte[] array = new byte[num - 1 + 1];
                    Array.Copy(this.sha256Managed.ComputeHash(seed), array, num - 1);
                    result = array;
                }
                else
                {
                    result = seed;
                }

                return result;
            }
        }

        private byte[] GetDesIV(byte[] key, int len)
        {
            checked
            {
                byte[] array = new byte[len - 1 + 1];
                Array.Copy(this.sha256Managed.ComputeHash(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(key))), array, len - 1);
                return array;
            }
        }

        public enum emProductName
        {
            fishDatabase,
            fishDictionary,
            fishConvert,
            fishCodeLib,
            fishCapture,
            fishResource,
            fishVocabulary,
            fishDBMigration
        }

        private TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider;
        private SHA256Managed sha256Managed;
        private string[] ProductName;
        private string Seperator = "|";
    }
}
