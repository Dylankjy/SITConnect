using System;
using System.ComponentModel.DataAnnotations;
using IdGen;

namespace SITConnect.Models
{
    public class User
    {
        // All objects will have an ID, even if they exist in the database
        // This ID is replaced with the actual stored once RetrieveData() is called.
        // [DatabaseGenerated(DatabaseGeneratedOption.None), Key]
        // public virtual long Id { get; set;} = new IdGenerator(0).CreateId();
        public long Id { get; set; } = new IdGenerator(0).CreateId();

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

        public void SetCardNo(string plainText)
        {
            // TODO: Encrypt card number before storing information
            BillingCardNo = plainText;
        }

        public string GetCardNo()
        {
            // TODO Decryption
            return BillingCardNo;
        }

        public bool ComparePassword(string incoming)
        {
            var isPasswordMatching = BCrypt.Net.BCrypt.Verify(incoming, Password);
            return isPasswordMatching;
        }
    }
}