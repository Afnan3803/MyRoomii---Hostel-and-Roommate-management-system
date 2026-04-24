using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRoomii
{
    // ============================================
    // INTERFACE
    // ============================================
    
    public interface IRoommateService
    {
        int RegisterUser(string email, string fullName, string? phone, string role);
        void CreateUserPreference(int userId, int cleanlinessLevel, bool smokingAllowed, int noiseLevel, string sleepSchedule, bool petFriendly, decimal budgetMin, decimal budgetMax);
        void CreateLandlord(int userId, string? companyName, string? phone);
        int CreateListing(int landlordUserId, string title, string? description, string? city, string? hostelName, decimal rent, DateTime? availableFrom, bool hasAC, bool hasHeating, bool hasPrivateWashroom, bool allowsPets, bool hasWifi, bool hasFurniture, bool hasParking);
        List<RoomListing> GetListings();
        List<MatchResult> GetTopMatches(int userId, int topN = 10);
        List<Message> GetMessages(int chatId);
        void SendMessage(int chatId, int senderId, string text);
        void AddReview(int authorUserId, int landlordUserId, int listingId, int rating, string comment);
    }

    // ============================================
    // LINQ/EF IMPLEMENTATION
    // ============================================
    
    public class LinqRoommateService : IRoommateService
    {
        private readonly MyRoomiiContext _context;

        public LinqRoommateService(MyRoomiiContext context)
        {
            _context = context;
        }

        public int RegisterUser(string email, string fullName, string? phone, string role)
        {
            var user = new User
            {
                Email = email,
                FullName = fullName,
                Phone = phone,
                Role = role,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return user.UserID;
        }

        public void CreateUserPreference(int userId, int cleanlinessLevel, bool smokingAllowed, int noiseLevel, string sleepSchedule, bool petFriendly, decimal budgetMin, decimal budgetMax)
        {
            var preference = new UserPreference
            {
                UserID = userId,
                CleanlinessLevel = cleanlinessLevel,
                SmokingAllowed = smokingAllowed,
                NoiseLevel = noiseLevel,
                SleepSchedule = sleepSchedule,
                PetFriendly = petFriendly,
                BudgetMin = budgetMin,
                BudgetMax = budgetMax
            };
            _context.UserPreferences.Add(preference);
            _context.SaveChanges();
        }

        public void CreateLandlord(int userId, string? companyName, string? phone)
        {
            var landlord = new Landlord
            {
                UserID = userId,
                CompanyName = companyName,
                Phone = phone,
                OverallRating = 0.0m,
                TotalReviews = 0
            };
            _context.Landlords.Add(landlord);
            _context.SaveChanges();
        }

        public int CreateListing(int landlordUserId, string title, string? description, string? city, string? hostelName, decimal rent, DateTime? availableFrom, bool hasAC, bool hasHeating, bool hasPrivateWashroom, bool allowsPets, bool hasWifi, bool hasFurniture, bool hasParking)
        {
            // Use existing stored procedure: spCreateListingWithAmenities
            var listing = new RoomListing
            {
                LandlordUserID = landlordUserId,
                Title = title,
                Description = description,
                City = city,
                HostelName = hostelName,
                Rent = rent,
                AvailableFrom = availableFrom,
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            _context.RoomListings.Add(listing);
            _context.SaveChanges();

            var amenities = new RoomAmenities
            {
                ListingID = listing.ListingID,
                HasAC = hasAC,
                HasHeating = hasHeating,
                HasPrivateWashroom = hasPrivateWashroom,
                AllowsPets = allowsPets,
                HasWifi = hasWifi,
                HasFurniture = hasFurniture,
                HasParking = hasParking
            };
            _context.RoomAmenities.Add(amenities);
            _context.SaveChanges();

            return listing.ListingID;
        }

        public List<RoomListing> GetListings()
        {
            return _context.RoomListings
                .Where(l => l.IsActive)
                .OrderByDescending(l => l.CreatedAt)
                .ToList();
        }

        public List<MatchResult> GetTopMatches(int userId, int topN = 10)
        {
            var matches = _context.RoommateMatches
                .Where(m => m.UserID1 == userId || m.UserID2 == userId)
                .OrderByDescending(m => m.CompatibilityScore)
                .ThenByDescending(m => m.CreatedAt)
                .Take(topN)
                .ToList();

            var results = new List<MatchResult>();
            foreach (var m in matches)
            {
                var matchedUserId = m.UserID1 == userId ? m.UserID2 : m.UserID1;
                var matchedUser = _context.Users.FirstOrDefault(u => u.UserID == matchedUserId);
                var listing = _context.RoomListings.FirstOrDefault(l => l.ListingID == m.ListingID);

                results.Add(new MatchResult
                {
                    MatchID = m.MatchID,
                    CompatibilityScore = m.CompatibilityScore,
                    MatchedUserName = matchedUser?.FullName ?? "Unknown",
                    ListingTitle = listing?.Title ?? "Unknown",
                    Rent = listing?.Rent ?? 0,
                    City = listing?.City
                });
            }
            return results;
        }

        public List<Message> GetMessages(int chatId)
        {
            return _context.Messages
                .Where(m => m.ChatID == chatId)
                .OrderBy(m => m.SentAt)
                .ToList();
        }

        public void SendMessage(int chatId, int senderId, string text)
        {
            var message = new Message
            {
                ChatID = chatId,
                SenderID = senderId,
                Text = text,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            _context.SaveChanges();
            // Trigger trgMessage_UpdateChatLastMessage will update Chat.LastMessageAt automatically
        }

        public void AddReview(int authorUserId, int landlordUserId, int listingId, int rating, string comment)
        {
            var review = new Review
            {
                AuthorUserID = authorUserId,
                LandlordUserID = landlordUserId,
                ListingID = listingId > 0 ? listingId : null,
                Rating = rating,
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
                CreatedAt = DateTime.Now
            };
            _context.Reviews.Add(review);
            _context.SaveChanges();
            // Trigger trgReview_UpdateLandlordRating will update Landlord.OverallRating automatically
        }
    }

    // ============================================
    // STORED PROCEDURE IMPLEMENTATION
    // ============================================
    
    public class SpRoommateService : IRoommateService
    {
        private readonly string _connectionString;

        public SpRoommateService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int RegisterUser(string email, string fullName, string? phone, string role)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            string sql = @"INSERT INTO [User] (Email, FullName, Phone, Role, CreatedAt, IsActive)
                          OUTPUT INSERTED.UserID
                          VALUES (@Email, @FullName, @Phone, @Role, GETDATE(), 1)";
            
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email),
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@Phone", phone ?? (object)DBNull.Value),
                new SqlParameter("@Role", role)
            };
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            var userId = (int)command.ExecuteScalar();
            return userId;
        }

        public void CreateUserPreference(int userId, int cleanlinessLevel, bool smokingAllowed, int noiseLevel, string sleepSchedule, bool petFriendly, decimal budgetMin, decimal budgetMax)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            string sql = @"INSERT INTO UserPreference (UserID, CleanlinessLevel, SmokingAllowed, NoiseLevel, SleepSchedule, PetFriendly, BudgetMin, BudgetMax)
                          VALUES (@UserID, @CleanlinessLevel, @SmokingAllowed, @NoiseLevel, @SleepSchedule, @PetFriendly, @BudgetMin, @BudgetMax)";
            
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@CleanlinessLevel", cleanlinessLevel),
                new SqlParameter("@SmokingAllowed", smokingAllowed),
                new SqlParameter("@NoiseLevel", noiseLevel),
                new SqlParameter("@SleepSchedule", sleepSchedule),
                new SqlParameter("@PetFriendly", petFriendly),
                new SqlParameter("@BudgetMin", budgetMin),
                new SqlParameter("@BudgetMax", budgetMax)
            };
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }

        public void CreateLandlord(int userId, string? companyName, string? phone)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            string sql = @"INSERT INTO Landlord (UserID, CompanyName, Phone, OverallRating, TotalReviews)
                          VALUES (@UserID, @CompanyName, @Phone, 0.0, 0)";
            
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@CompanyName", companyName ?? (object)DBNull.Value),
                new SqlParameter("@Phone", phone ?? (object)DBNull.Value)
            };
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
        }

        public int CreateListing(int landlordUserId, string title, string? description, string? city, string? hostelName, decimal rent, DateTime? availableFrom, bool hasAC, bool hasHeating, bool hasPrivateWashroom, bool allowsPets, bool hasWifi, bool hasFurniture, bool hasParking)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Use existing stored procedure: spCreateListingWithAmenities
            var command = new SqlCommand("spCreateListingWithAmenities", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            
            command.Parameters.AddWithValue("@LandlordUserID", landlordUserId);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@City", city ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HostelName", hostelName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Rent", rent);
            command.Parameters.AddWithValue("@AvailableFrom", availableFrom ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@HasAC", hasAC);
            command.Parameters.AddWithValue("@HasHeating", hasHeating);
            command.Parameters.AddWithValue("@HasPrivateWashroom", hasPrivateWashroom);
            command.Parameters.AddWithValue("@AllowsPets", allowsPets);
            command.Parameters.AddWithValue("@HasWifi", hasWifi);
            command.Parameters.AddWithValue("@HasFurniture", hasFurniture);
            command.Parameters.AddWithValue("@HasParking", hasParking);
            
            var listingIdParam = new SqlParameter("@ListingID", System.Data.SqlDbType.Int)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            command.Parameters.Add(listingIdParam);
            
            command.ExecuteNonQuery();
            
            return (int)listingIdParam.Value;
        }

        public List<RoomListing> GetListings()
        {
            var listings = new List<RoomListing>();
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Simple SELECT query (can also use view vwActiveListingsWithLandlord)
            string sql = "SELECT ListingID, LandlordUserID, Title, Description, City, HostelName, Rent, AvailableFrom, IsActive, CreatedAt FROM RoomListing WHERE IsActive = 1 ORDER BY CreatedAt DESC";
            
            using var reader = DbHelper.ExecuteQuery(connection, sql);
            while (reader.Read())
            {
                listings.Add(new RoomListing
                {
                    ListingID = (int)reader["ListingID"],
                    LandlordUserID = (int)reader["LandlordUserID"],
                    Title = (string)reader["Title"],
                    Description = reader["Description"] == DBNull.Value ? null : (string)reader["Description"],
                    City = reader["City"] == DBNull.Value ? null : (string)reader["City"],
                    HostelName = reader["HostelName"] == DBNull.Value ? null : (string)reader["HostelName"],
                    Rent = (decimal)reader["Rent"],
                    AvailableFrom = reader["AvailableFrom"] == DBNull.Value ? null : (DateTime?)reader["AvailableFrom"],
                    IsActive = (bool)reader["IsActive"],
                    CreatedAt = (DateTime)reader["CreatedAt"]
                });
            }
            return listings;
        }

        public List<MatchResult> GetTopMatches(int userId, int topN = 10)
        {
            var matches = new List<MatchResult>();
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Use existing stored procedure: spGetTopMatchesForUser
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@UserID", userId),
                new SqlParameter("@TopN", topN)
            };
            
            using var reader = DbHelper.ExecuteStoredProcedure(connection, "spGetTopMatchesForUser", parameters);
            while (reader.Read())
            {
                matches.Add(new MatchResult
                {
                    MatchID = (int)reader["MatchID"],
                    CompatibilityScore = (int)reader["CompatibilityScore"],
                    MatchedUserName = (string)reader["MatchedUserName"],
                    ListingTitle = (string)reader["ListingTitle"],
                    Rent = (decimal)reader["Rent"],
                    City = reader["City"] == DBNull.Value ? null : (string)reader["City"]
                });
            }
            return matches;
        }

        public List<Message> GetMessages(int chatId)
        {
            var messages = new List<Message>();
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Simple SELECT query
            string sql = "SELECT MessageID, ChatID, SenderID, Text, SentAt, IsRead FROM Message WHERE ChatID = @ChatID ORDER BY SentAt ASC";
            var param = new SqlParameter("@ChatID", chatId);
            
            using var reader = DbHelper.ExecuteQuery(connection, sql, param);
            while (reader.Read())
            {
                messages.Add(new Message
                {
                    MessageID = (long)reader["MessageID"],
                    ChatID = (int)reader["ChatID"],
                    SenderID = (int)reader["SenderID"],
                    Text = (string)reader["Text"],
                    SentAt = (DateTime)reader["SentAt"],
                    IsRead = (bool)reader["IsRead"]
                });
            }
            return messages;
        }

        public void SendMessage(int chatId, int senderId, string text)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Use existing stored procedure: spSendMessage
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@ChatID", chatId),
                new SqlParameter("@SenderID", senderId),
                new SqlParameter("@Text", text)
            };
            
            DbHelper.ExecuteNonQuery(connection, "spSendMessage", parameters);
            // Trigger trgMessage_UpdateChatLastMessage will update Chat.LastMessageAt automatically
        }

        public void AddReview(int authorUserId, int landlordUserId, int listingId, int rating, string comment)
        {
            using var connection = DbHelper.CreateConnection(_connectionString);
            connection.Open();
            
            // Simple INSERT query (trigger will handle landlord rating update)
            string sql = @"INSERT INTO Review (AuthorUserID, LandlordUserID, ListingID, Rating, Comment, CreatedAt)
                          VALUES (@AuthorUserID, @LandlordUserID, @ListingID, @Rating, @Comment, GETDATE())";
            
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@AuthorUserID", authorUserId),
                new SqlParameter("@LandlordUserID", landlordUserId),
                new SqlParameter("@ListingID", listingId > 0 ? (object)listingId : DBNull.Value),
                new SqlParameter("@Rating", rating),
                new SqlParameter("@Comment", string.IsNullOrWhiteSpace(comment) ? (object)DBNull.Value : comment)
            };
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            command.ExecuteNonQuery();
            // Trigger trgReview_UpdateLandlordRating will update Landlord.OverallRating automatically
        }
    }

    // ============================================
    // FACTORY PATTERN
    // ============================================
    
    public static class ServiceFactory
    {
        /// <summary>
        /// Creates a service instance based on mode ("LINQ" or "SP")
        /// </summary>
        public static IRoommateService Create(string mode, string connString)
        {
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentException("Connection string cannot be empty", nameof(connString));

            if (mode.ToUpper() == "LINQ")
            {
                var optionsBuilder = new DbContextOptionsBuilder<MyRoomiiContext>();
                optionsBuilder.UseSqlServer(connString);
                var context = new MyRoomiiContext(optionsBuilder.Options);
                return new LinqRoommateService(context);
            }
            else
            {
                return new SpRoommateService(connString);
            }
        }
    }
}
