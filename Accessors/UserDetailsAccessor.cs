using EncantoWebAPI.Managers;
using EncantoWebAPI.Models.Auth;
using EncantoWebAPI.Models.Profiles;
using EncantoWebAPI.Models.Profiles.Requests;
using MongoDB.Driver;

namespace EncantoWebAPI.Accessors
{
    public class UserDetailsAccessor
    {
        private readonly MongoDBAccessor _db;
        private readonly IConfiguration _config;

        public UserDetailsAccessor(IConfiguration config)
        {
            _config = config;
            _db = new MongoDBAccessor(config);
        }

        #region Profile Details

        public async Task<UserProfile?> GetProfileDetails(string userId)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
            var user = await _db.Users.Find(filter).FirstOrDefaultAsync();
            return user;
        }

        public async Task<SessionDetails?> GetSessionDetails(string sessionKey)
        {
            var filter = Builders<SessionDetails>.Filter.Eq(s => s.SessionKey, sessionKey);
            var session = await _db.SessionDetails.Find(filter).FirstOrDefaultAsync();
            return session;
        }

        public async Task UpdateProfileName(UserNameUpdateRequest userNameUpdateRequest)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userNameUpdateRequest.UserId);
            var update = Builders<UserProfile>.Update
                .Set(u => u.Name, userNameUpdateRequest.Name)
                .Set(u => u.UpdatedTimestamp, userNameUpdateRequest.UpdatedTimestamp);

            var result = await _db.Users.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("User not found or name not updated.");
            }
        }
        
        public async Task UpdateProfilePhn(UserPhnUpdateRequest userPhnUpdateRequest)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userPhnUpdateRequest.UserId);
            var update = Builders<UserProfile>.Update
                .Set(u => u.PhoneNumber, userPhnUpdateRequest.PhoneNumber)
                .Set(u => u.UpdatedTimestamp, userPhnUpdateRequest.UpdatedTimestamp);

            var result = await _db.Users.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("User not found or phone number not updated.");
            }
        }

        public async Task UpdateProfileGender(UserGenderUpdateRequest userGenderUpdateRequest)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userGenderUpdateRequest.UserId);
            var update = Builders<UserProfile>.Update
                .Set(u => u.Gender, userGenderUpdateRequest.Gender)
                .Set(u => u.UpdatedTimestamp, userGenderUpdateRequest.UpdatedTimestamp);

            var result = await _db.Users.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("User not found or gender not updated.");
            }
        }

        #endregion

        #region Address Updation

        public async Task UpdateProfileBirthday(UserBirthdayUpdateRequest userBirthdayUpdateRequest)
        {
            var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userBirthdayUpdateRequest.UserId);
            var update = Builders<UserProfile>.Update
                .Set(u => u.DateOfBirth, userBirthdayUpdateRequest.DateOfBirth)
                .Set(u => u.UpdatedTimestamp, userBirthdayUpdateRequest.UpdatedTimestamp);

            var result = await _db.Users.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("User not found or DateOfBirth not updated.");
            }
        }

        public async Task CreateProfileAddress(Address userAddressDetails, string? userId_Or_OccupationId = null)
        {
            await _db.Addresses.InsertOneAsync(userAddressDetails);

            if (userAddressDetails.AddressType == "Home")
            {
                var profileFilter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId_Or_OccupationId); //userId
                var profileUpdate = Builders<UserProfile>.Update
                    .Set(u => u.AddressId, userAddressDetails.AddressId)
                    .Set(u => u.UpdatedTimestamp, userAddressDetails.UpdatedTimestamp);

                var profileUpdateResult = await _db.Users.UpdateOneAsync(profileFilter, profileUpdate);

                if (profileUpdateResult.ModifiedCount == 0)
                {
                    throw new Exception("Address Created, but Unable to Update User Profile Details.");
                }
            }
            else
            {
                if (userId_Or_OccupationId != null)
                {
                    var occupationFilter = Builders<OccupationDetails>.Filter.Eq(u => u.OccupationId, userId_Or_OccupationId); //occupationId
                    var occupationUpdate = Builders<OccupationDetails>.Update
                        .Set(u => u.AddressId, userAddressDetails.AddressId)
                        .Set(u => u.UpdatedTimestamp, userAddressDetails.UpdatedTimestamp);

                    var profileUpdateResult = await _db.OccupationDetails.UpdateOneAsync(occupationFilter, occupationUpdate);

                    if (profileUpdateResult.ModifiedCount == 0)
                    {
                        throw new Exception("Address Created, but Unable to Update User Profile Details.");
                    }
                }
            }
        }

        public async Task UpdateProfileAddress(UserAddressUpdateRequest userAddressUpdateRequest)
        {
            var filter = Builders<Address>.Filter.Eq(u => u.AddressId, userAddressUpdateRequest.AddressId);
            var update = Builders<Address>.Update
                .Set(u => u.StreetAddress, userAddressUpdateRequest.StreetAddress)
                .Set(u => u.City, userAddressUpdateRequest.City)
                .Set(u => u.State, userAddressUpdateRequest.State)
                .Set(u => u.Country, userAddressUpdateRequest.Country)
                .Set(u => u.PostalCode, userAddressUpdateRequest.PostalCode)
                .Set(u => u.Landmark, userAddressUpdateRequest.Landmark)
                .Set(u => u.AddressType, userAddressUpdateRequest.AddressType)
                .Set(u => u.UpdatedTimestamp, userAddressUpdateRequest.UpdatedTimestamp);

            var result = await _db.Addresses.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("User not found or Address not updated.");
            }

            //update 'updatedTimestamp'
            if (userAddressUpdateRequest?.AddressType.ToLower() == "home")
            {
                var profileFilter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userAddressUpdateRequest.UserId);
                var profileUpdate = Builders<UserProfile>.Update
                    .Set(u => u.UpdatedTimestamp, userAddressUpdateRequest.UpdatedTimestamp);

                var profileUpdateResult = await _db.Users.UpdateOneAsync(profileFilter, profileUpdate);

                if (profileUpdateResult.ModifiedCount == 0)
                {
                    throw new Exception("Address Created, but Unable to Update User Profile Details.");
                }
            }
            
        }

        public async Task<bool> CheckIfAddressExist(string addressId)
        {
            var filter = Builders<Address>.Filter.Eq(u => u.AddressId, addressId);
            var exists = await _db.Addresses.Find(filter).Limit(1).AnyAsync();
            return exists;
        }

        public async Task<Address> GetAddressDetails(string addressId)
        {
            var filter = Builders<Address>.Filter.Eq(u => u.AddressId, addressId);
            var address = await _db.Addresses.Find(filter).FirstOrDefaultAsync();
            return address;
        }

        #endregion

        #region Occupation Details Updation

        public async Task<bool> CheckIfOccupationExist(string occupationId)
        {
            var filter = Builders<OccupationDetails>.Filter.Eq(u => u.OccupationId, occupationId);
            var exists = await _db.OccupationDetails.Find(filter).Limit(1).AnyAsync();
            return exists;
        }

        public async Task<OccupationDetails> GetOccupationDetails(string occupationId)
        {
            var filter = Builders<OccupationDetails>.Filter.Eq(u => u.OccupationId, occupationId);
            var occupationDetails = await _db.OccupationDetails.Find(filter).FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(occupationDetails.AddressId))
            {
                occupationDetails.JobLocation = await GetAddressDetails(occupationDetails.AddressId);
            }

            return occupationDetails;
        }

        public async Task CreateProfileOccupationDetails(OccupationDetails userOccupationDetails, string userId)
        {
            await _db.OccupationDetails.InsertOneAsync(userOccupationDetails);

            var profileFilter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
            var profileUpdate = Builders<UserProfile>.Update
                .Set(u => u.OccupationId, userOccupationDetails.OccupationId)
                .Set(u => u.UpdatedTimestamp, userOccupationDetails.UpdatedTimestamp);

            var profileUpdateResult = await _db.Users.UpdateOneAsync(profileFilter, profileUpdate);

            if (profileUpdateResult.ModifiedCount == 0)
            {
                throw new Exception("Occupation Details Created, but Unable to Update User Profile Details.");
            }
        }

        public async Task UpdateProfileOccupationDetails(UserOccupationUpdateRequest userOccupationUpdateRequest)
        {
            var filter = Builders<OccupationDetails>.Filter.Eq(u => u.OccupationId, userOccupationUpdateRequest.OccupationId);

            var updates = new List<UpdateDefinition<OccupationDetails>>
            {
                Builders<OccupationDetails>.Update.Set(u => u.UpdatedTimestamp, userOccupationUpdateRequest.UpdatedTimestamp)
            };

            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.Designation))
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.Designation, userOccupationUpdateRequest.Designation));
            }
                
            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.IndustryDomain))
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.IndustryDomain, userOccupationUpdateRequest.IndustryDomain));
            }

            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.OrganizationName))
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.OrganizationName, userOccupationUpdateRequest.OrganizationName));
            }
                
            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.EmploymentType))
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.EmploymentType, userOccupationUpdateRequest.EmploymentType));
            }
               
            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.WorkEmail))
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.WorkEmail, userOccupationUpdateRequest.WorkEmail));
            }
                
            if (userOccupationUpdateRequest.WorkPhoneNumber != null)
            {
                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.WorkPhoneNumber, userOccupationUpdateRequest.WorkPhoneNumber));
            }

            // Update work address separately if provided
            if (userOccupationUpdateRequest.JobLocation != null)
            {
                var userDetailsManager = new UserDetailsManager(_config);
                await userDetailsManager.UpdateProfileAddress(userOccupationUpdateRequest.JobLocation, userOccupationUpdateRequest.OccupationId);

                updates.Add(Builders<OccupationDetails>.Update.Set(u => u.UpdatedTimestamp, userOccupationUpdateRequest.JobLocation.UpdatedTimestamp));
            }

            // Combine all non-null updates
            var updateOccupationDetails = Builders<OccupationDetails>.Update.Combine(updates);

            var result = await _db.OccupationDetails.UpdateOneAsync(filter, updateOccupationDetails);

            if (result.ModifiedCount == 0)
                throw new Exception("User not found or Occupation Details not updated.");

            // Update user's profile UpdatedTimestamp as well
            var profileFilter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userOccupationUpdateRequest.UserId);
            var profileUpdate = Builders<UserProfile>.Update
                .Set(u => u.UpdatedTimestamp, userOccupationUpdateRequest.UpdatedTimestamp);

            var profileUpdateResult = await _db.Users.UpdateOneAsync(profileFilter, profileUpdate);

            if (profileUpdateResult.ModifiedCount == 0)
                throw new Exception("Occupation Details updated, but unable to update User Profile details.");
        }


        #endregion

    }
}
