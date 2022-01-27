using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CreditCardValidator;
using IdGen;
using Newtonsoft.Json;
using SITConnect.Pages;

namespace SITConnect.Models
{
    public class User
    {
        // ID uses Twitter snowflake instead of incremental
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public long Id { get; set; } = new IdGenerator(0).CreateId();

        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string BillingCardNo { get; set; }
        [Required, EmailAddress] public string Email { get; set; }

        // Password private, can only be access by self. To set, use SetPassword()
        public string Password { get; set; }

        [Required] public DateTime DateOfBirth { get; set; }
        [Required] public string Photo { get; set; }

        public void SetPassword(string plainText)
        {
            // Using a work factor of 12. 10 is the industry standard.
            var hash = BCrypt.Net.BCrypt.HashPassword(plainText, 12);

            if (Password == null)
            {
                Password = $"{hash}";
            }
            else
            {
                Password = $"{hash};~;{Password}";
            }
        }

        public bool ComparePassword(string incoming)
        {
            var currentPassword = Password.Split(";~;")[0];

            var isPasswordMatching = BCrypt.Net.BCrypt.Verify(incoming, currentPassword);
            return isPasswordMatching;
        }

        public bool SetCardNo(string plainText)
        {
            // Validate card number
            var detector = new CreditCardDetector(plainText);
            var isValid = detector.IsValid();
            if (!isValid) return false;

            var randString = new RandomStringGen();

            // Use random generator to generate password (512 characters)
            var encryptionKey = randString.RandomString();

            // Take password and encrypt the card no.
            var encryptionResult = Encrypt(plainText, encryptionKey);

            // Set the cipher text as the prop
            BillingCardNo = encryptionResult.Item2;

            // Write the User ID, Initialisation Vector and encryption key to txt file stored in /CryptoStore/keys.csv
            File.AppendAllText(Environment.CurrentDirectory + @"\CryptoStore\keys.csv",
                $"{Id},{encryptionResult.Item1},{encryptionResult.Item3}" + Environment.NewLine);

            return true;
        }

        public dynamic GetCardNo()
        {
            // Read key file and get line by user id. Retrieve IV and key
            var lineInKeyFile = File.ReadAllLines(Environment.CurrentDirectory + @"\CryptoStore\keys.csv")
                .Where(s => s.Contains(Id.ToString())).ToArray();

            // Assume the first entry, since it would be the only one
            var elements = lineInKeyFile[0].Split(",");

            var iv = elements[1];
            var key = elements[2];

            var decryptedCardNo = Decrypt(BillingCardNo, iv, key);

            return decryptedCardNo;
        }

        private static (string, string, string)
            Encrypt(string content, string password) // returns (base64Iv, base64Ciphertext, base64Key)
        {
            var bytes = Encoding.UTF8.GetBytes(content);

            using (SymmetricAlgorithm crypt = Aes.Create())
            using (HashAlgorithm hash = SHA256.Create())
            using (var memoryStream = new MemoryStream()) // This initialises a new memoryStream to store blobs later
            {
                // Converts the password into SHA512 hash before using it for encryption
                crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Generate an Initialisation Vector
                crypt.GenerateIV();

                // Populate the memoryStream with the blob
                using (var cryptoStream = new CryptoStream(
                    memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }

                // Convert the initialisation vector to base64
                var base64Iv = Convert.ToBase64String(crypt.IV);
                var base64Ciphertext = Convert.ToBase64String(memoryStream.ToArray());
                var base64Key = Convert.ToBase64String(crypt.Key);

                return (base64Iv, base64Ciphertext, base64Key);
            }
        }

        private static string Decrypt(string cipherText, string iv, string key)
        {
            var bytes = Convert.FromBase64String(cipherText);
            var ivBytes = Convert.FromBase64String(iv);
            var keyBytes = Convert.FromBase64String(key);

            using (SymmetricAlgorithm crypt = Aes.Create())
            {
                crypt.IV = ivBytes;
                crypt.Key = keyBytes;

                // Create a decryptor to perform the stream transform.
                var decryptor = crypt.CreateDecryptor(crypt.Key, crypt.IV);

                var decryptedText = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

                // Convert back to string and return plain text
                return Encoding.UTF8.GetString(decryptedText);
            }
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public User FromJson(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<User>(jsonString);
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }
    }
}