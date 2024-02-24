﻿using System;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.DirectoryServices;
using FleszynChat.Classes;
using System.Text.RegularExpressions;


namespace FleszynChat.Scripts
{
    public class ActiveDirectory
    {
        public void ConntectToAD(string domainController, string ADusername, string ADpassword)
        {
            try
            {
                using (DirectoryEntry entry = new DirectoryEntry("LDAP://" + domainController, ADusername, ADpassword))
                using (DirectorySearcher searcher = new DirectorySearcher(entry))
                {
                    searcher.Filter = $"(objectCategory=user)";
                    searcher.PropertiesToLoad.Add("samAccountName");
                    searcher.PropertiesToLoad.Add("givenName");
                    searcher.PropertiesToLoad.Add("sn");
                    searcher.PropertiesToLoad.Add("mail");
                    searcher.PropertiesToLoad.Add("thumbnailPhoto");

                    SearchResultCollection results = searcher.FindAll();

                    foreach (SearchResult result in results)
                    {
                        string username = GetPropertyValue(result, "samAccountName");
                        string firstName = GetPropertyValue(result, "givenName");
                        string lastName = GetPropertyValue(result, "sn");
                        string email = GetPropertyValue(result, "mail");

                        // Retrieve profile picture as byte array
                        byte[] profilePictureBytes = null;
                        if (result.Properties.Contains("thumbnailPhoto"))
                        {
                            profilePictureBytes = result.Properties["thumbnailPhoto"][0] as byte[];
                        }

                        // Save profile picture to file (optional)
                        string profilePicturePath = null;
                        if (profilePictureBytes != null && profilePictureBytes.Length > 0)
                        {
                            profilePicturePath = SaveProfilePicture(username, profilePictureBytes);
                        }

                        // Output user information
                        Console.WriteLine($"Username: {username}");
                        Console.WriteLine($"First Name: {firstName}");
                        Console.WriteLine($"Last Name: {lastName}");
                        Console.WriteLine($"Email: {email}");
                        Console.WriteLine($"Profile Picture Path: {profilePicturePath}");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private string GetPropertyValue(SearchResult result, string propertyName)
        {
            if (result.Properties.Contains(propertyName))
            {
                return result.Properties[propertyName][0].ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private string SaveProfilePicture(string username, byte[] profilePictureBytes)
        {
            string directoryPath = "ProfilePictures"; // Directory to save profile pictures
            string filePath = Path.Combine(directoryPath, $"{username}.jpg");

            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(directoryPath);

                // Save profile picture to file
                File.WriteAllBytes(filePath, profilePictureBytes);

                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save profile picture: {ex.Message}");
                return null;
            }
        }
    }
}