using EncantoWebAPI.Accessors;
using EncantoWebAPI.Models.Events;
using EncantoWebAPI.Models.Events.Requests;
using System.Text.RegularExpressions;

namespace EncantoWebAPI.Managers
{
    public class EventDetailsManager
    {
        private readonly EventDetailsAccessor _eventDetailsAccessor;
        private readonly UserDetailsAccessor _userDetailsAccessor;

        public EventDetailsManager(EventDetailsAccessor eventDetailsAccessor, UserDetailsAccessor userDetailsAccessor)
        {
            _eventDetailsAccessor = eventDetailsAccessor;
            _userDetailsAccessor = userDetailsAccessor;
        }

        #region Host Event Operations

        public async Task<EventDetails> CreateNewEvent(CreateEventRequest newEventRequest)
        {
            var eventId = GenerateEventId(newEventRequest);

            var hostDetails = await _userDetailsAccessor.GetProfileDetails(newEventRequest.OrganizerId);

            if (hostDetails == null)
            {
                throw new InvalidOperationException("Unable to retreive Organizer Details");
            }

            string organizerDesignationString = string.Empty;

            if (!string.IsNullOrWhiteSpace(hostDetails.OccupationId))
            {
                var hostOccupationDetails = await _userDetailsAccessor.GetOccupationDetails(hostDetails.OccupationId);

                var designationParts = new[] { hostOccupationDetails?.Designation, hostOccupationDetails?.OrganizationName }
                .Where(s => !string.IsNullOrWhiteSpace(s));

                organizerDesignationString = string.Join(", ", designationParts);
            }

            var organizerDetails = new OrganizerDetails
            {
                OrganizerId = hostDetails.UserId,
                OrganizerName = hostDetails.Name,
                BackgroundColour = hostDetails.BackgroundColour,
                ForegroundColour = hostDetails.ForegroundColour,
                OrganizerDesignation = organizerDesignationString
            };

            var eventFeedback = new EventFeedback
            {
                EventId = eventId,
                EnableRating = newEventRequest.EnableRating,
                EnableComments = newEventRequest.EnableComments,
                TotalRatings = 0,
            };

            var newEvent = new EventDetails
            {
                EventId = eventId,
                Title = newEventRequest.Title,
                Description = newEventRequest.Description,
                OrganizerDetails = organizerDetails,
                MeetingLink = newEventRequest.MeetingLink,
                EventImageBitCode = newEventRequest.EventImageBitCode,
                StartTimestamp = newEventRequest.StartTimestamp,
                EndTimestamp = newEventRequest.EndTimestamp,
                CreatedTimestamp = newEventRequest.CreatedTimestamp,
                UpdatedTimestamp = newEventRequest.CreatedTimestamp,
                IsPrivate = newEventRequest.IsPrivate,
                Participants = [],
                Is_accepting_participants = true,
                TotalRegisteredParticipants = 0,
                Active = 1,
            };

            await _eventDetailsAccessor.CreateNewEvent(newEvent, eventFeedback);

            return newEvent;
        }

        public static string GenerateEventId(CreateEventRequest newEventRequest)
        {
            string eventType = newEventRequest.IsPrivate ? "Priv" : "Pub";
            string shortEventName = Regex.Replace(newEventRequest.Title, "[^a-zA-Z0-9]", ""); //remove whitespaces
            shortEventName = shortEventName.Length <= 7 ? shortEventName : shortEventName.Substring(0, 7);

            return $"Event_{eventType}_{newEventRequest.CreatedTimestamp}_{shortEventName}";
        }

        public async Task<List<EventDetails>> GetMyUpcomingHostedEvents(string hostId)
        {
            var events = await _eventDetailsAccessor.GetMyUpcomingHostedEvents(hostId);
            return events;
        }

        public async Task<List<EventDetails>> GetMyPastHostedEvents(string hostId)
        {
            var events = await _eventDetailsAccessor.GetMyPastHostedEvents(hostId);
            return events;
        }

        public async Task<List<PrivateEventRequestPreview>> GetAllPendingRequests(string hostId)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var pendingRequests = new List<PrivateEventRequestPreview>();
            var events = await _eventDetailsAccessor.GetMyUpcomingHostedEvents(hostId);

            var upcomingEvents = events.Where(e => e.StartTimestamp > currentTimestamp);

            foreach (var Event in upcomingEvents)
            {
                if (Event.Is_accepting_participants == true && Event.Participants != null)
                {
                    var pendingParticipantsInThisEvent = Event.Participants
                        .Where(p => p.RegistrationStatus == 0);

                    foreach (var participant in pendingParticipantsInThisEvent)
                    {
                        PrivateEventRequestPreview pendingRequest = new()
                        {
                            EventId = Event.EventId,
                            EventName = Event.Title,
                            StartTimestamp = Event.StartTimestamp,
                            EndTimestamp = Event.EndTimestamp,
                            ParticipantDetails = participant,
                            ParticipantRequestTimestamp = participant.UpdatedTimestamp
                        };

                        pendingRequests.Add(pendingRequest);
                    }
                }
            }

            var sortedPendingRequests = pendingRequests
                .OrderByDescending(r => r.ParticipantRequestTimestamp)
                .ToList();

            return sortedPendingRequests;
        }

        public async Task UpdateEventPendingRequest(string eventId, string participantId, bool isParticipantAccepted)
        {
            await _eventDetailsAccessor.UpdateEventPendingRequest(eventId, participantId, isParticipantAccepted);
        }

        public async Task UpdateEventActiveStatus(string eventId, int eventStatus)
        {
            var eventDetails = await _eventDetailsAccessor.GetEventDetailsFromEventId(eventId);

            if (eventDetails.Active == eventStatus)
            {
                throw new InvalidOperationException($"Event's Active Status is already {eventStatus}");
            }

            await _eventDetailsAccessor.UpdateEventActiveStatus(eventId, eventStatus);
        }

        public async Task UpdateEventDetails(EditEventDetailsRequest editEventDetailsRequest)
        {
            await _eventDetailsAccessor.UpdateEventDetails(editEventDetailsRequest);
        }

        #endregion

        #region Guest Event Operations

        public async Task<List<EventDetails>> GetAllUpcomingEvents()
        {
            var events = await _eventDetailsAccessor.GetAllUpcomingEvents();
            return events;
        }

        public async Task ApplyForUpcomingEvent(EventApplicationRequest eventApplicationRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_userDetailsAccessor);

            try
            {
                var profileDetails = await userDetailsManager.GetProfileDetailsForEventCreationFromUserId(eventApplicationRequest.UserId);
                var eventDetails = await _eventDetailsAccessor.GetEventDetailsFromEventId(eventApplicationRequest.EventId);
                bool isParticipantAlreadyRegistered = CheckIfUserAlreadyRegisteredForTheEvent(profileDetails.UserId, eventDetails);

                bool isEventPrivate = eventDetails.IsPrivate;
                int totalParticipantsCount = eventDetails.TotalRegisteredParticipants;
                int registrationStatus;

                if (!eventDetails.Is_accepting_participants)
                {
                    throw new InvalidOperationException("Organizer has stopped accepting participants.");
                }

                if (isParticipantAlreadyRegistered)
                {
                    throw new InvalidOperationException("Participant Already Registered for the Event.");
                }

                if (isEventPrivate) //private event
                {
                    registrationStatus = 0;
                }
                else //public event
                {
                    registrationStatus = 1;
                    totalParticipantsCount += 1;
                }

                ParticipantDetails participantDetails = new() 
                {
                    ParticipantId = profileDetails.UserId,
                    ParticipantName = profileDetails.Name,
                    BackgroundColour = profileDetails.BackgroundColour,
                    ForegroundColour = profileDetails.ForegroundColour,
                    RegistrationStatus = registrationStatus,
                    RegisteredTimestamp = eventApplicationRequest.UpdatedTimestamp,
                    UpdatedTimestamp = eventApplicationRequest.UpdatedTimestamp
                };

                await _eventDetailsAccessor.ApplyForUpcomingEvent(participantDetails, eventDetails.EventId, totalParticipantsCount);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
        }

        public bool CheckIfUserAlreadyRegisteredForTheEvent(string userId, EventDetails eventDetails)
        {
            var participantsList = eventDetails.Participants;

            if (participantsList == null || participantsList.Count == 0)
            {
                return false;
            }

            foreach (var participant in participantsList)
            {
                if (participant.ParticipantId == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<List<EventDetails>> GetMyRegisteredEvents(string guestId)
        {
            var events = await _eventDetailsAccessor.GetMyRegisteredEvents(guestId);
            return events;
        }

        public async Task<List<EventDetails>> GetMyPastAttendedEvents(string guestId)
        {
            var events = await _eventDetailsAccessor.GetMyPastAttendedEvents(guestId);
            return events;
        }

        #endregion

    }
}
