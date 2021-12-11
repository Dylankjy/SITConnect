using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using IdGen;
using SITConnect.Pages;

namespace SITConnect.Models
{
    public class User
    {
        // ID uses Twitter snowflake instead of incremental
        public long Id { get; } = new IdGenerator(0).CreateId();

        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string BillingCardNo { get; set; }
        [Required] public string Email { get; set; }

        // Password private, can only be access by self. To set, use SetPassword()
        public string Password { get; set; }

        [Required] public DateTime DateOfBirth { get; set; }
        // [Required] public SqlBinary Photo { get; set; }

        public void SetPassword(string plainText)
        {
            // Using a work factor of 12. 10 is the industry standard.
            var hash = BCrypt.Net.BCrypt.HashPassword(plainText, 12);

            Password = hash;
        }

        public bool ComparePassword(string incoming)
        {
            var isPasswordMatching = BCrypt.Net.BCrypt.Verify(incoming, Password);
            return isPasswordMatching;
        }
        
        public bool SetCardNo(string plainText)
        {
            // Validate card number
            CreditCardDetector detector = new CreditCardDetector(plainText);
            bool isValid = detector.IsValid();
            if (!isValid)
            {
                return false;
            }
            
            RandomStringGen randString = new RandomStringGen();
            
            // Use random generator to generate password (512 characters)
            var encryptionKey = randString.RandomString();
            
            // Take password and encrypt the card no.
            var encryptionResult = Encrypt(plainText, encryptionKey);

            // Set the cipher text as the prop
            BillingCardNo = encryptionResult.Item2;
            
            // Write the User ID, Initialisation Vector and encryption key to txt file stored in /CryptoStore/keys.csv
            File.AppendAllText(Environment.CurrentDirectory + @"\CryptoStore\keys.csv", $"{Id},{encryptionResult.Item1},{encryptionResult.Item3}" + Environment.NewLine);

            return true;
        }

        public dynamic GetCardNo()
        {
            // Read key file and get line by user id. Retrieve IV and key
            string[] lineInKeyFile = File.ReadAllLines(Environment.CurrentDirectory + @"\CryptoStore\keys.csv")
                .Where(s => s.Contains(Id.ToString())).ToArray();

            // Assume the first entry, since it would be the only one
            string[] elements = lineInKeyFile[0].Split(",");

            string iv = elements[1];
            string key = elements[2];
            Console.WriteLine(iv);
            Console.WriteLine(key);

            string decryptedCardNo = Decrypt(BillingCardNo, iv, key);

            return decryptedCardNo;
        }
        
        private static (string, string, string) Encrypt(string content, string password) // returns (base64Iv, base64Ciphertext, base64Key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            
            using (SymmetricAlgorithm crypt = Aes.Create())
            using (HashAlgorithm hash = SHA256.Create())
            using (MemoryStream memoryStream = new MemoryStream()) // This initialises a new memoryStream to store blobs later
            {
                // Converts the password into SHA512 hash before using it for encryption
                crypt.Key = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                
                // Generate an Initialisation Vector
                crypt.GenerateIV();
            
                // Populate the memoryStream with the blob
                using (CryptoStream cryptoStream = new CryptoStream(
                    memoryStream, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(bytes, 0, bytes.Length);
                }
            
                // Convert the initialisation vector to base64
                string base64Iv = Convert.ToBase64String(crypt.IV);
                string base64Ciphertext = Convert.ToBase64String(memoryStream.ToArray());
                string base64Key = Convert.ToBase64String(crypt.Key);
            
                return (base64Iv, base64Ciphertext, base64Key);
            }
        }

        private static string Decrypt(string cipherText, string iv, string key)
        {
            byte[] bytes = Convert.FromBase64String(cipherText);
            byte[] ivBytes = Convert.FromBase64String(iv);
            byte[] keyBytes = Convert.FromBase64String(key);

            using (SymmetricAlgorithm crypt = Aes.Create())
            {
                crypt.IV = ivBytes;
                crypt.Key = keyBytes;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = crypt.CreateDecryptor(crypt.Key, crypt.IV);
                
                byte[] decryptedText = decryptor.TransformFinalBlock(bytes, 0, bytes.Length);

                // Convert back to string and return plain text
                return Encoding.UTF8.GetString(decryptedText);
            }
        }
    }
}