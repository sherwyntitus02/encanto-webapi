using EncantoWebAPI.Accessors;
using EncantoWebAPI.Models.Auth;
using EncantoWebAPI.Models.Profiles;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace EncantoWebAPI.Managers
{
    public class AuthenticationManager
    {
        private readonly AuthenticationAccessor _authenticationAccessor;

        public AuthenticationManager(AuthenticationAccessor authenticationAccessor)
        {
            _authenticationAccessor = authenticationAccessor;
        }

        #region Signup

        public async Task CreateNewUser(SignupRequest signupRequest)
        {
            var isEmailExisting = await _authenticationAccessor.CheckIfEmailExists(signupRequest.Email);

            if (isEmailExisting) //if email already exists
            {
                throw new InvalidOperationException($"User with Mail Id '{signupRequest.Email}' already exists.");
            }

            var loginCredential = GenerateLoginCredential(signupRequest);
            await _authenticationAccessor.CreateNewLoginCredential(loginCredential);

            var userProfile = GenerateUserProfile(signupRequest);
            await _authenticationAccessor.CreateNewUser(userProfile);

        }

        private static UserProfile GenerateUserProfile(SignupRequest signupRequest)
        {
            string userId = GenerateUserId(signupRequest.ProfileType, signupRequest.CreatedTimestamp, signupRequest.Email);

            var (Background, Foreground) = GetColorForString(userId);

            UserProfile userProfile = new()
            {
                UserId = userId,
                Email = signupRequest.Email.ToLower(),
                Name = GetValidName(signupRequest.Name), // Capitalize first letter of each word
                ProfileType = signupRequest.ProfileType.ToLower(),
                BackgroundColour = Background,
                ForegroundColour = Foreground,
                CreatedTimestamp = signupRequest.CreatedTimestamp,
                UpdatedTimestamp = signupRequest.UpdatedTimestamp,
                Is_email_verified = false,
                Active = 1
            };

            return userProfile;
        }

        private static LoginCredential GenerateLoginCredential(SignupRequest signupRequest)
        {
            string userId = GenerateUserId(signupRequest.ProfileType, signupRequest.CreatedTimestamp, signupRequest.Email);

            string passwordHash = signupRequest.PasswordHash;

            LoginCredential loginCredential = new()
            {
                UserId = userId,
                Username = signupRequest.Email,
                PasswordHash = passwordHash,
                CreatedTimestamp = signupRequest.CreatedTimestamp,
                UpdatedTimestamp = signupRequest.UpdatedTimestamp
            };

            return loginCredential;
        }

        private static string GenerateUserId(string profileType, long createdTimestamp, string email)
        {
            string slicedMail = email.Split('@')[0].ToLower();
            slicedMail = Regex.Replace(slicedMail, @"[^0-9a-z]", "");
            return $"{profileType}_{createdTimestamp}_{slicedMail}";
        }

        public static string GetValidName(string name)
        {
            string validName = string.Join(" ",
                name.Trim()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower())
            );

            return validName;
        }

        public static (string Background, string Foreground) GetColorForString(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                int hashValue = BitConverter.ToInt32(hashBytes, 0);
                int index = Math.Abs(hashValue % CardColors.Count);

                return CardColors[index];
            }
        }

        private static readonly List<(string Background, string Foreground)> CardColors =
        [
            ("#FFEBEE", "#E57373"),
            ("#FFCDD2", "#F44336"),
            ("#F8BBD0", "#E91E63"),
            ("#E1BEE7", "#9C27B0"),
            ("#D1C4E9", "#673AB7"),
            ("#C5CAE9", "#3F51B5"),
            ("#BBDEFB", "#2196F3"),
            ("#D6E3F0", "#083B5A"),
            ("#B2DFDB", "#009688"),
            ("#E6F4EC", "#145A32"),
            ("#FFE0B2", "#FF9800"),
            ("#FFCCBC", "#FF5722"),
            ("#D7CCC8", "#607D8B"),
            ("#D9D9D9", "#3A3B3F")
        ];

        #endregion

        #region Login/Logout
        public async Task<string> LoginExistingUser(LoginRequest loginRequest)
        {
            var userId = await _authenticationAccessor.LoginExistingUser(loginRequest);

            if (userId != null)
            {
                return userId;
            }
            else
            {
                throw new InvalidOperationException("Invalid email or password.");
            }
        }

        public string GenerateSessionKey(string userId)
        {
            // Random salt (ensures uniqueness)
            var salt = Guid.NewGuid().ToString();

            //A salt is random data added to your input before hashing to make the output unique.
            //It prevents predictable or duplicate hashes, ensuring each session key is different even for the same input.

            // Combine input + salt + timestamp
            var rawData = userId + salt + DateTime.UtcNow.Ticks;

            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }

        public async Task StoreSessionKey(string userId, string sessionKey)
        {
            var sessionExpiry = DateTime.UtcNow.AddDays(7); // Example: 7 days expiry

            var sessionDetails = new SessionDetails
            {
                SessionKey = sessionKey,
                UserId = userId,
                ExpirationTimestamp = new DateTimeOffset(sessionExpiry).ToUnixTimeMilliseconds()
            };

            await _authenticationAccessor.StoreSessionKey(sessionDetails);

        }

        public async Task DeleteSessionKey(string sessionKey)
        {
            await _authenticationAccessor.DeleteSessionKey(sessionKey);
        }

        #endregion

    }
}
