using System;
using System.Collections.Generic;

namespace SITConnect.Pages
{
    public class RandomStringGen
    {
        private static readonly Random Random = new Random(); 
        
        public string RandomString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string randomString = "";

            for (int i = 0; i < 512; i++)
            {
                randomString += chars[Random.Next(0, chars.Length - 1)];
            }

            return randomString;
        }
    }
}