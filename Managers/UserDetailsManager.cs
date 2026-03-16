using EncantoWebAPI.Accessors;
using EncantoWebAPI.Models.Profiles;
using EncantoWebAPI.Models.Profiles.Requests;

namespace EncantoWebAPI.Managers
{
    public class UserDetailsManager
    {
        private readonly IConfiguration _config;

        public UserDetailsManager(IConfiguration config)
        {
            _config = config;
        }

        #region Profile Details
        public async Task<UserProfile> GetProfileDetailsFromUserId(string UserId)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            var profileDetails = await userDetailsAccessor.GetProfileDetails(UserId);

            if (profileDetails == null)
            {
                throw new InvalidOperationException($"User profile with ID '{UserId}' not found.");
            }

            if (!string.IsNullOrWhiteSpace(profileDetails.AddressId))
            {
                profileDetails.Address = await userDetailsAccessor.GetAddressDetails(profileDetails.AddressId);
            }

            if (!string.IsNullOrWhiteSpace(profileDetails.OccupationId))
            {
                profileDetails.OccupationDetails = await userDetailsAccessor.GetOccupationDetails(profileDetails.OccupationId);
            }

            return profileDetails;
        }

        public async Task<UserProfile> GetProfileDetailsForEventCreationFromUserId(string UserId)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            var profileDetails = await userDetailsAccessor.GetProfileDetails(UserId);

            if (profileDetails == null)
            {
                throw new InvalidOperationException($"User profile with ID '{UserId}' not found.");
            }

            return profileDetails;
        }

        public async Task<string> GetUserIdFromSessionDetails(string sessionKey)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            var sessionDetails = await userDetailsAccessor.GetSessionDetails(sessionKey);
            if (sessionDetails == null)
            {
                throw new InvalidOperationException($"Session not found.");
            }
            return sessionDetails.UserId;
        }

        public async Task UpdateProfileName(UserNameUpdateRequest userNameUpdateRequest)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            await userDetailsAccessor.UpdateProfileName(userNameUpdateRequest);
        }

        public async Task UpdateProfilePhn(UserPhnUpdateRequest userPhnUpdateRequest)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            await userDetailsAccessor.UpdateProfilePhn(userPhnUpdateRequest);
        }

        public async Task UpdateProfileGender(UserGenderUpdateRequest userGenderUpdateRequest)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            await userDetailsAccessor.UpdateProfileGender(userGenderUpdateRequest);
        }

        public async Task UpdateProfileBirthday(UserBirthdayUpdateRequest userBirthdayUpdateRequest)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            await userDetailsAccessor.UpdateProfileBirthday(userBirthdayUpdateRequest);
        }

        #endregion

        #region Address Details

        public async Task UpdateProfileAddress(UserAddressUpdateRequest userAddressUpdateRequest, string? occupationId = null)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            var doesAddressExist = false;

            if (!string.IsNullOrWhiteSpace(userAddressUpdateRequest.AddressId))
            {
                doesAddressExist = await userDetailsAccessor.CheckIfAddressExist(userAddressUpdateRequest.AddressId);
            }
            
            if (doesAddressExist)
            {
                await userDetailsAccessor.UpdateProfileAddress(userAddressUpdateRequest);
            }
            else
            {
                string addressId = GenerateAddressId(userAddressUpdateRequest.AddressType, userAddressUpdateRequest.UserId, userAddressUpdateRequest.UpdatedTimestamp);

                Address userAddressDetails = new()
                {
                    AddressId = addressId,
                    StreetAddress = userAddressUpdateRequest.StreetAddress,
                    City = userAddressUpdateRequest.City,
                    State = userAddressUpdateRequest.State,
                    Country = userAddressUpdateRequest.Country,
                    PostalCode = userAddressUpdateRequest.PostalCode,
                    Landmark = userAddressUpdateRequest.Landmark ?? null,
                    AddressType = userAddressUpdateRequest.AddressType,
                    UpdatedTimestamp = userAddressUpdateRequest.UpdatedTimestamp,
                    CreatedTimestamp = userAddressUpdateRequest.UpdatedTimestamp
                };

                await userDetailsAccessor.CreateProfileAddress(userAddressDetails, occupationId ?? userAddressUpdateRequest.UserId);

            }
        }

        public string GenerateAddressId(string addressType, string userId, long updatedTimestamp)
        {
            string addressId = $"{addressType}_{userId}_{updatedTimestamp}_Address";
            return addressId;
        }

        #endregion

        #region Occupation Details

        public async Task UpdateProfileOccupation(UserOccupationUpdateRequest userOccupationUpdateRequest)
        {
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            var doesOccupationExist = false;

            if (!string.IsNullOrWhiteSpace(userOccupationUpdateRequest.OccupationId))
            {
                doesOccupationExist = await userDetailsAccessor.CheckIfOccupationExist(userOccupationUpdateRequest.OccupationId);
            }

            if (doesOccupationExist)
            {
                await userDetailsAccessor.UpdateProfileOccupationDetails(userOccupationUpdateRequest);
            }
            else
            {
                string occupationId = GenerateOccupationId(userOccupationUpdateRequest.UserId, userOccupationUpdateRequest.UpdatedTimestamp);

                OccupationDetails userOccupationDetails = new()
                {
                    OccupationId = occupationId,
                    Designation = userOccupationUpdateRequest.Designation ?? null,
                    IndustryDomain = userOccupationUpdateRequest.IndustryDomain ?? null,
                    OrganizationName = userOccupationUpdateRequest.OrganizationName ?? null,
                    EmploymentType = userOccupationUpdateRequest.EmploymentType ?? null,
                    AddressId = (userOccupationUpdateRequest.JobLocation != null) ? await JobLocationHandler(userOccupationUpdateRequest.JobLocation, occupationId) : null,
                    WorkEmail = userOccupationUpdateRequest.WorkEmail ?? null,
                    WorkPhoneNumber = userOccupationUpdateRequest.WorkPhoneNumber ?? null,
                    CreatedTimestamp = userOccupationUpdateRequest.UpdatedTimestamp,
                    UpdatedTimestamp = userOccupationUpdateRequest.UpdatedTimestamp
                };

                await userDetailsAccessor.CreateProfileOccupationDetails(userOccupationDetails, userOccupationUpdateRequest.UserId);
            }
        }

        public async Task<string> JobLocationHandler(UserAddressUpdateRequest jobLocationRequest, string occupationId)
        {
            //since Occupation Does not exists, we create a new record in addresses collection, and return the 'addressId' to store in OccupationDetails Collection in DB
            var userDetailsAccessor = new UserDetailsAccessor(_config);
            string addressId = GenerateAddressId(jobLocationRequest.AddressType, jobLocationRequest.UserId, jobLocationRequest.UpdatedTimestamp);

            Address userAddressDetails = new()
            {
                AddressId = addressId,
                StreetAddress = jobLocationRequest.StreetAddress,
                City = jobLocationRequest.City,
                State = jobLocationRequest.State,
                Country = jobLocationRequest.Country,
                PostalCode = jobLocationRequest.PostalCode,
                Landmark = jobLocationRequest.Landmark ?? null,
                AddressType = jobLocationRequest.AddressType,
                UpdatedTimestamp = jobLocationRequest.UpdatedTimestamp,
                CreatedTimestamp = jobLocationRequest.UpdatedTimestamp
            };

            await userDetailsAccessor.CreateProfileAddress(userAddressDetails);

            return addressId;
        }

        public string GenerateOccupationId( string userId, long updatedTimestamp)
        {
            string occupationId = $"{userId}_{updatedTimestamp}_Occupation";
            return occupationId;
        }

        #endregion
    }
}
