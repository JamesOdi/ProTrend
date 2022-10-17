﻿using System.ComponentModel.DataAnnotations;

namespace ProTrendAPI.Models.User
{
    public class ProfileDTO
    {
        [Required]
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string BackgroundImageUrl { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
        public string PaymentPin { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? Phone { get; set; } = null;
    }
}
