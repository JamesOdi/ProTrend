﻿using System.Text.Json.Serialization;

namespace ProTrendAPI.Models.User
{
    public class Profile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [JsonPropertyName("identifier")]
        public Guid Identifier { get; set; }
        [JsonPropertyName("username")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("fullname")]
        public string FullName { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        [JsonPropertyName("acctype")]
        public string AccountType { get; set; } = string.Empty;
        [JsonPropertyName("bgimg")]
        public string BackgroundImageUrl { get; set; } = string.Empty;
        [JsonPropertyName("profileimg")]
        public string ProfileImage { get; set; } = string.Empty;
        [JsonPropertyName("paymentpin")]
        public string PaymentPin { get; set; } = string.Empty;
        [JsonPropertyName("accountnumber")]
        public string AccountNumber { get; set; } = string.Empty;
        [JsonPropertyName("bank")]
        public string Bank { get; set; } = string.Empty; 
        [JsonPropertyName("location")]
        public string Location { get; set; } = string.Empty;
        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
        [JsonPropertyName("phone")]
        public string? Phone { get; set; } = null;
        [JsonPropertyName("regdate")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        [JsonPropertyName("disabled")]
        public bool Disabled { get; set; } = false;
    }
}