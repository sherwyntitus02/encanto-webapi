using EncantoWebAPI.Models.Events;
using EncantoWebAPI.Models.Events.Requests;
using MongoDB.Driver;

namespace EncantoWebAPI.Accessors
{
    public class EventDetailsAccessor
    {
        private readonly MongoDBAccessor _db;

        public EventDetailsAccessor(MongoDBAccessor db)
        {
            _db = db;
        }

        #region Host Event Operations

        public async Task CreateNewEvent(EventDetails newEvent, EventFeedback newEventFeedback)
        {
            await _db.Events.InsertOneAsync(newEvent);
            await _db.EventFeedbacks.InsertOneAsync(newEventFeedback);
        }

        public async Task<List<EventDetails>> GetMyUpcomingHostedEvents(string hostId)
        {
            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.Eq(e => e.Active, 1),
                Builders<EventDetails>.Filter.Eq(e => e.OrganizerDetails.OrganizerId, hostId));

            var activeEvents = await _db.Events
                .Find(filter)
                .ToListAsync();

            return activeEvents;
        }

        public async Task<List<EventDetails>> GetMyPastHostedEvents(string hostId)
        {
            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.Eq(e => e.Active, 0),
                Builders<EventDetails>.Filter.Eq(e => e.OrganizerDetails.OrganizerId, hostId));

            var activeEvents = await _db.Events
                .Find(filter)
                .ToListAsync();

            return activeEvents;
        }

        public async Task UpdateEventPendingRequest(string eventId, string participantId, bool isParticipantAccepted)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var participantUpdationStatus = isParticipantAccepted ? 1 : -1;
            var incrementAmount = isParticipantAccepted ? 1 : 0;

            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.Eq(e => e.EventId, eventId),
                Builders<EventDetails>.Filter.ElemMatch(
                    e => e.Participants,
                    p => p.ParticipantId == participantId));

            var update = Builders<EventDetails>.Update
                .Set("Participants.$.RegistrationStatus", participantUpdationStatus)
                .Set("Participants.$.UpdatedTimestamp", currentTimestamp)
                .Inc(e => e.TotalRegisteredParticipants, incrementAmount)
                .Set(e => e.UpdatedTimestamp, currentTimestamp);

            var result = await _db.Events.UpdateOneAsync(filter, update);
        }

        public async Task<EventDetails> GetEventDetailsFromEventId(string eventId)
        {
            var filter = Builders<EventDetails>.Filter.Eq(u => u.EventId, eventId);
            var eventDetails = await _db.Events.Find(filter).FirstOrDefaultAsync();
            return eventDetails;
        }

        public async Task UpdateEventActiveStatus(string eventId, int eventStatus)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            bool is_participant_acccepting_status = eventStatus == 1; // if eventStatus == 1 -> true ; else false

            var filter = Builders<EventDetails>.Filter.Eq(u => u.EventId, eventId);
            var update = Builders<EventDetails>.Update
                .Set(u => u.Active, eventStatus)
                .Set(u => u.Is_accepting_participants, is_participant_acccepting_status)
                .Set(u => u.UpdatedTimestamp, currentTimestamp); // update timestamp

            var result = await _db.Events.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Event not found or Active Status not updated.");
            }
        }

        public async Task UpdateEventDetails(EditEventDetailsRequest editEventDetailsRequest)
        {
            var filter = Builders<EventDetails>.Filter.Eq(u => u.EventId, editEventDetailsRequest.EventId);
            var update = Builders<EventDetails>.Update
                .Set(u => u.Title, editEventDetailsRequest.Title)
                .Set(u => u.Description, editEventDetailsRequest.Description)
                .Set(u => u.MeetingLink, editEventDetailsRequest.MeetingLink)
                .Set(u => u.StartTimestamp, editEventDetailsRequest.StartTimestamp)
                .Set(u => u.EndTimestamp, editEventDetailsRequest.EndTimestamp)
                .Set(u => u.Is_accepting_participants, editEventDetailsRequest.Is_accepting_participants)
                .Set(u => u.UpdatedTimestamp, editEventDetailsRequest.UpdatedTimestamp);

            var result = await _db.Events.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Event not found or Event Details not updated.");
            }
        }

        public async Task<List<EventDetails>> GetMyCancelledHostedEvents(string hostId)
        {
            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.Eq(e => e.Active, -1),
                Builders<EventDetails>.Filter.Eq(e => e.OrganizerDetails.OrganizerId, hostId));

            var activeEvents = await _db.Events
                .Find(filter)
                .ToListAsync();

            return activeEvents;
        }
        public async Task<EventFeedback> GetFeedbacksByEventId(string eventId)
        {
            var filter = Builders<EventFeedback>.Filter.Eq(e => e.EventId, eventId);
            return await _db.EventFeedbacks.Find(filter).FirstOrDefaultAsync();
        }

        #endregion

        #region Guest Event Operations

        public async Task<List<EventDetails>> GetAllUpcomingEvents()
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.Eq(e => e.Active, 1),
                Builders<EventDetails>.Filter.Gt(e => e.StartTimestamp, currentTimestamp));

            var activeEvents = await _db.Events
                .Find(filter)
                .ToListAsync();

            return activeEvents;
        }

        public async Task ApplyForUpcomingEvent(ParticipantDetails participantDetails, string eventId, int totalParticipantsCount)
        {
            var filter = Builders<EventDetails>.Filter.Eq(u => u.EventId, eventId);
            var update = Builders<EventDetails>.Update
                .Push(u => u.Participants, participantDetails) // append to list
                .Set(u => u.TotalRegisteredParticipants, totalParticipantsCount)
                .Set(u => u.UpdatedTimestamp, participantDetails.UpdatedTimestamp); // update timestamp

            var result = await _db.Events.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                throw new Exception("Event not found or Participant Details not updated.");
            }
        }

        public async Task<List<EventDetails>> GetMyRegisteredEvents(string guestId)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.In(e => e.Active, [1, -1]),
                Builders<EventDetails>.Filter.Gt(e => e.EndTimestamp, currentTimestamp),
                Builders<EventDetails>.Filter.ElemMatch(e => e.Participants, p => p.ParticipantId == guestId));
                // Use ElemMatch to query inside the Participants list

            var events = await _db.Events
                .Find(filter)
                .ToListAsync();

            return events;
        }

        public async Task<List<EventDetails>> GetMyPastAttendedEvents(string guestId)
        {
            var currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var filter = Builders<EventDetails>.Filter.And(
                Builders<EventDetails>.Filter.In(e => e.Active, [0, 1]),// Use "In" for multiple values
                Builders<EventDetails>.Filter.Lt(e => e.EndTimestamp, currentTimestamp),
                Builders<EventDetails>.Filter.ElemMatch(e => e.Participants, p => p.ParticipantId == guestId),
                Builders<EventDetails>.Filter.ElemMatch(e => e.Participants, p => p.RegistrationStatus == 1));// Use ElemMatch to query inside the Participants list

            var events = await _db.Events
                .Find(filter)
                .ToListAsync();

            return events;
        }

        #endregion

    }
}
