using System;

namespace MyRoomii
{
    // ============================================
    // ENTITY CLASSES - Matching group59_p2.sql exactly
    // ============================================

    public class User
    {
        public int UserID { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string Role { get; set; } = "Student";
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UserPreference
    {
        public int UserID { get; set; }
        public int CleanlinessLevel { get; set; } = 5;
        public bool SmokingAllowed { get; set; } = false;
        public int NoiseLevel { get; set; } = 5;
        public string SleepSchedule { get; set; } = "Normal";
        public bool PetFriendly { get; set; } = false;
        public decimal BudgetMin { get; set; } = 0;
        public decimal BudgetMax { get; set; } = 50000;
    }

    public class Verification
    {
        public int UserID { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Method { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }

    public class Landlord
    {
        public int UserID { get; set; }
        public string? CompanyName { get; set; }
        public string? Phone { get; set; }
        public decimal OverallRating { get; set; } = 0.0m;
        public int TotalReviews { get; set; } = 0;
    }

    public class RoomListing
    {
        public int ListingID { get; set; }
        public int LandlordUserID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? City { get; set; }
        public string? HostelName { get; set; }
        public decimal Rent { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class RoomAmenities
    {
        public int ListingID { get; set; }
        public bool HasAC { get; set; } = false;
        public bool HasHeating { get; set; } = false;
        public bool HasPrivateWashroom { get; set; } = false;
        public bool AllowsPets { get; set; } = false;
        public bool HasWifi { get; set; } = true;
        public bool HasFurniture { get; set; } = false;
        public bool HasParking { get; set; } = false;
    }

    public class RoommateMatch
    {
        public int MatchID { get; set; }
        public int UserID1 { get; set; }
        public int UserID2 { get; set; }
        public int ListingID { get; set; }
        public int CompatibilityScore { get; set; } = 0;
        public string Status { get; set; } = "Suggested";
        public DateTime CreatedAt { get; set; }
    }

    public class Chat
    {
        public int ChatID { get; set; }
        public int User1ID { get; set; }
        public int User2ID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public bool IsBlocked { get; set; } = false;
    }

    public class Message
    {
        public long MessageID { get; set; }
        public int ChatID { get; set; }
        public int SenderID { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false;
    }

    public class Review
    {
        public int ReviewID { get; set; }
        public int AuthorUserID { get; set; }
        public int LandlordUserID { get; set; }
        public int? ListingID { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============================================
    // DTOs (Data Transfer Objects)
    // ============================================

    public class MatchResult
    {
        public int MatchID { get; set; }
        public int CompatibilityScore { get; set; }
        public string MatchedUserName { get; set; } = string.Empty;
        public string ListingTitle { get; set; } = string.Empty;
        public decimal Rent { get; set; }
        public string? City { get; set; }
    }

    public class ListingDTO
    {
        public int ListingID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? City { get; set; }
        public decimal Rent { get; set; }
        public bool IsActive { get; set; }
    }

    public class MessageDTO
    {
        public long MessageID { get; set; }
        public int SenderID { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }
}
