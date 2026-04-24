using System;
using System.Collections.Generic;

namespace MyRoomii
{
    class Program
    {
        private static IRoommateService? service;
        private static string mode = "LINQ";
        private const string ConnectionString = "Server=localhost,1433;Database=myRoomii;User Id=sa;Password=Test123!@#;TrustServerCertificate=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   MyRoomii - Roommate Matching System");
            Console.WriteLine("========================================\n");

            // Initialize service with default mode
            service = ServiceFactory.Create(mode, ConnectionString);
            Console.WriteLine($"Current backend mode: {mode}\n");

            bool running = true;
            while (running)
            {
                ShowMenu();
                string? choice = Console.ReadLine();

                switch (choice?.Trim())
                {
                    case "1":
                        ViewListings();
                        break;
                    case "2":
                        ViewMatches();
                        break;
                    case "3":
                        ViewMessages();
                        break;
                    case "4":
                        SendMessage();
                        break;
                    case "5":
                        AddReview();
                        break;
                    case "6":
                        RegisterUser();
                        break;
                    case "7":
                        PostListing();
                        break;
                    case "8":
                        SwitchMode();
                        break;
                    case "0":
                        running = false;
                        Console.WriteLine("\nGoodbye!");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.\n");
                        break;
                }
            }
        }

        static void ShowMenu()
        {
            Console.WriteLine("--- MAIN MENU ---");
            Console.WriteLine("1. View room listings");
            Console.WriteLine("2. View roommate matches (for user ID 1)");
            Console.WriteLine("3. View chat messages");
            Console.WriteLine("4. Send a new message");
            Console.WriteLine("5. Add a review");
            Console.WriteLine("6. Register new user (Student/Landlord)");
            Console.WriteLine("7. Post a listing (Landlords only)");
            Console.WriteLine("8. Switch backend mode (LINQ/SP)");
            Console.WriteLine("0. Exit");
            Console.Write("\nEnter your choice: ");
        }

        static void ViewListings()
        {
            Console.WriteLine("\n--- ROOM LISTINGS ---");
            try
            {
                var listings = service!.GetListings();
                if (listings.Count == 0)
                {
                    Console.WriteLine("No listings found.\n");
                    return;
                }

                foreach (var listing in listings)
                {
                    Console.WriteLine($"ID: {listing.ListingID}");
                    Console.WriteLine($"Title: {listing.Title}");
                    Console.WriteLine($"City: {listing.City ?? "N/A"}");
                    Console.WriteLine($"Rent: ${listing.Rent:F2}");
                    Console.WriteLine($"Active: {listing.IsActive}");
                    Console.WriteLine("---");
                }
                Console.WriteLine($"\nTotal: {listings.Count} listings\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void ViewMatches()
        {
            Console.WriteLine("\n--- ROOMMATE MATCHES (User ID 1) ---");
            try
            {
                var matches = service!.GetTopMatches(1, 10);
                if (matches.Count == 0)
                {
                    Console.WriteLine("No matches found for user ID 1.\n");
                    return;
                }

                foreach (var match in matches)
                {
                    Console.WriteLine($"Match ID: {match.MatchID}");
                    Console.WriteLine($"Matched with: {match.MatchedUserName}");
                    Console.WriteLine($"Listing: {match.ListingTitle}");
                    Console.WriteLine($"Compatibility: {match.CompatibilityScore}%");
                    Console.WriteLine($"Rent: ${match.Rent:F2}");
                    Console.WriteLine($"City: {match.City ?? "N/A"}");
                    Console.WriteLine("---");
                }
                Console.WriteLine($"\nTotal: {matches.Count} matches\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void ViewMessages()
        {
            Console.Write("\nEnter Chat ID: ");
            if (!int.TryParse(Console.ReadLine(), out int chatId))
            {
                Console.WriteLine("Invalid Chat ID.\n");
                return;
            }

            Console.WriteLine($"\n--- MESSAGES (Chat ID {chatId}) ---");
            try
            {
                var messages = service!.GetMessages(chatId);
                if (messages.Count == 0)
                {
                    Console.WriteLine("No messages found.\n");
                    return;
                }

                foreach (var msg in messages)
                {
                    Console.WriteLine($"[{msg.SentAt:yyyy-MM-dd HH:mm}] User {msg.SenderID}: {msg.Text}");
                }
                Console.WriteLine($"\nTotal: {messages.Count} messages\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void SendMessage()
        {
            Console.Write("\nEnter Chat ID: ");
            if (!int.TryParse(Console.ReadLine(), out int chatId))
            {
                Console.WriteLine("Invalid Chat ID.\n");
                return;
            }

            Console.Write("Enter Sender ID: ");
            if (!int.TryParse(Console.ReadLine(), out int senderId))
            {
                Console.WriteLine("Invalid Sender ID.\n");
                return;
            }

            Console.Write("Enter message text: ");
            string? text = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(text))
            {
                Console.WriteLine("Message cannot be empty.\n");
                return;
            }

            try
            {
                service!.SendMessage(chatId, senderId, text);
                Console.WriteLine("Message sent successfully! (Trigger updated chat timestamp)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void AddReview()
        {
            Console.Write("\nEnter Reviewer User ID: ");
            if (!int.TryParse(Console.ReadLine(), out int reviewerId))
            {
                Console.WriteLine("Invalid Reviewer ID.\n");
                return;
            }

            Console.Write("Enter Landlord User ID: ");
            if (!int.TryParse(Console.ReadLine(), out int landlordId))
            {
                Console.WriteLine("Invalid Landlord ID.\n");
                return;
            }

            Console.Write("Enter Listing ID (0 for none): ");
            if (!int.TryParse(Console.ReadLine(), out int listingId))
            {
                listingId = 0;
            }

            Console.Write("Enter Rating (1-5): ");
            if (!int.TryParse(Console.ReadLine(), out int rating) || rating < 1 || rating > 5)
            {
                Console.WriteLine("Invalid rating. Must be 1-5.\n");
                return;
            }

            Console.Write("Enter comment (optional): ");
            string? comment = Console.ReadLine();

            try
            {
                service!.AddReview(reviewerId, landlordId, listingId, rating, comment ?? "");
                Console.WriteLine("Review added successfully! (Trigger updated landlord rating)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void RegisterUser()
        {
            Console.WriteLine("\n--- REGISTER NEW USER ---");
            
            Console.Write("Enter Email: ");
            string? email = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email is required.\n");
                return;
            }

            Console.Write("Enter Full Name: ");
            string? fullName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                Console.WriteLine("Full Name is required.\n");
                return;
            }

            Console.Write("Enter Phone (optional): ");
            string? phone = Console.ReadLine();

            Console.Write("Enter Role (Student or Landlord): ");
            string? role = Console.ReadLine()?.Trim();
            if (role != "Student" && role != "Landlord")
            {
                Console.WriteLine("Invalid role. Must be 'Student' or 'Landlord'.\n");
                return;
            }

            try
            {
                int userId = service!.RegisterUser(email, fullName, string.IsNullOrWhiteSpace(phone) ? null : phone, role);
                Console.WriteLine($"User registered successfully! User ID: {userId}\n");

                if (role == "Student")
                {
                    Console.WriteLine("--- SET USER PREFERENCES ---");
                    Console.Write("Cleanliness Level (1-10, default 5): ");
                    if (!int.TryParse(Console.ReadLine(), out int cleanliness) || cleanliness < 1 || cleanliness > 10)
                        cleanliness = 5;

                    Console.Write("Smoking Allowed? (y/n, default n): ");
                    bool smoking = Console.ReadLine()?.Trim().ToLower() == "y";

                    Console.Write("Noise Level (1-10, default 5): ");
                    if (!int.TryParse(Console.ReadLine(), out int noise) || noise < 1 || noise > 10)
                        noise = 5;

                    Console.Write("Sleep Schedule (Early/Normal/Late, default Normal): ");
                    string? sleep = Console.ReadLine()?.Trim();
                    if (string.IsNullOrWhiteSpace(sleep) || (sleep != "Early" && sleep != "Normal" && sleep != "Late"))
                        sleep = "Normal";

                    Console.Write("Pet Friendly? (y/n, default n): ");
                    bool pets = Console.ReadLine()?.Trim().ToLower() == "y";

                    Console.Write("Budget Min (default 0): ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal budgetMin))
                        budgetMin = 0;

                    Console.Write("Budget Max (default 50000): ");
                    if (!decimal.TryParse(Console.ReadLine(), out decimal budgetMax))
                        budgetMax = 50000;

                    service.CreateUserPreference(userId, cleanliness, smoking, noise, sleep, pets, budgetMin, budgetMax);
                    Console.WriteLine("User preferences created successfully!\n");
                }
                else if (role == "Landlord")
                {
                    Console.WriteLine("--- SET LANDLORD INFORMATION ---");
                    Console.Write("Company Name (optional): ");
                    string? companyName = Console.ReadLine();

                    Console.Write("Phone (optional): ");
                    string? landlordPhone = Console.ReadLine();

                    service.CreateLandlord(userId, string.IsNullOrWhiteSpace(companyName) ? null : companyName, 
                                         string.IsNullOrWhiteSpace(landlordPhone) ? null : landlordPhone);
                    Console.WriteLine("Landlord profile created successfully!\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void PostListing()
        {
            Console.WriteLine("\n--- POST NEW LISTING (Landlords Only) ---");
            
            Console.Write("Enter Landlord User ID: ");
            if (!int.TryParse(Console.ReadLine(), out int landlordId))
            {
                Console.WriteLine("Invalid Landlord User ID.\n");
                return;
            }

            Console.Write("Enter Title: ");
            string? title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title is required.\n");
                return;
            }

            Console.Write("Enter Description (optional): ");
            string? description = Console.ReadLine();

            Console.Write("Enter City (optional): ");
            string? city = Console.ReadLine();

            Console.Write("Enter Hostel Name (optional): ");
            string? hostelName = Console.ReadLine();

            Console.Write("Enter Rent: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal rent) || rent < 0)
            {
                Console.WriteLine("Invalid rent amount.\n");
                return;
            }

            Console.Write("Enter Available From Date (YYYY-MM-DD, optional): ");
            string? dateStr = Console.ReadLine();
            DateTime? availableFrom = null;
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out DateTime date))
            {
                availableFrom = date;
            }

            Console.WriteLine("\n--- AMENITIES ---");
            Console.Write("Has AC? (y/n): ");
            bool hasAC = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.Write("Has Heating? (y/n): ");
            bool hasHeating = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.Write("Has Private Washroom? (y/n): ");
            bool hasPrivateWashroom = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.Write("Allows Pets? (y/n): ");
            bool allowsPets = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.Write("Has Wifi? (y/n, default y): ");
            string? wifi = Console.ReadLine();
            bool hasWifi = string.IsNullOrWhiteSpace(wifi) || wifi.Trim().ToLower() == "y";

            Console.Write("Has Furniture? (y/n): ");
            bool hasFurniture = Console.ReadLine()?.Trim().ToLower() == "y";

            Console.Write("Has Parking? (y/n): ");
            bool hasParking = Console.ReadLine()?.Trim().ToLower() == "y";

            try
            {
                int listingId = service!.CreateListing(landlordId, title, description, city, hostelName, rent, availableFrom,
                    hasAC, hasHeating, hasPrivateWashroom, allowsPets, hasWifi, hasFurniture, hasParking);
                Console.WriteLine($"Listing created successfully! Listing ID: {listingId} (Uses stored procedure spCreateListingWithAmenities)\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}\n");
            }
        }

        static void SwitchMode()
        {
            Console.WriteLine($"\nCurrent mode: {mode}");
            Console.Write("Enter new mode (LINQ or SP): ");
            string? newMode = Console.ReadLine()?.Trim().ToUpper();

            if (newMode != "LINQ" && newMode != "SP")
            {
                Console.WriteLine("Invalid mode. Use 'LINQ' or 'SP'.\n");
                return;
            }

            mode = newMode;
            service = ServiceFactory.Create(mode, ConnectionString);
            Console.WriteLine($"Mode switched to: {mode}\n");
        }
    }
}
