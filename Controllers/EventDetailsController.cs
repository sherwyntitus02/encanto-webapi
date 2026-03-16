using EncantoWebAPI.Hubs;
using EncantoWebAPI.Models.Events;
using EncantoWebAPI.Models.Events.Requests;
using EncantoWebAPI.Models.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class EventDetailsController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration _config;

        public EventDetailsController(IHubContext<NotificationHub> hubContext, IConfiguration config)
        {
            _hubContext = hubContext;
            _config = config;
        }

        #region Host Event Operations

        [HttpPost("events/new")]
        public async Task<ActionResult> CreateNewEvent(CreateEventRequest newEventRequest)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (newEventRequest != null)
                {
                    var createdEvent = await eventDetailsManager.CreateNewEvent(newEventRequest);

                    var broadcastMessage = new EventUpdateMessage
                    {
                        Action = "create",
                        Event = createdEvent
                    };

                    await _hubContext.Clients.All.SendAsync("EventChanged", broadcastMessage);

                    return Ok("Event created successfully.");
                }
                else
                {
                    return BadRequest("Invaild Event Creation Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/hosted-upcoming")]
        public async Task<ActionResult<List<EventDetails>>> GetMyUpcomingHostedEvents(string hostId)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                var events = await eventDetailsManager.GetMyUpcomingHostedEvents(hostId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/hosted-past")]
        public async Task<ActionResult<List<EventDetails>>> GetMyPastHostedEvents(string hostId)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                var events = await eventDetailsManager.GetMyPastHostedEvents(hostId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/pending-requests")]
        public async Task<ActionResult<List<PrivateEventRequestPreview>>> GetAllPendingRequests(string hostId)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (hostId != null)
                {
                    var events = await eventDetailsManager.GetAllPendingRequests(hostId);
                    return Ok(events);
                }
                else
                {
                    return BadRequest("Invaild Participant Details Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("events/update-pending-request")]
        public async Task<ActionResult> UpdateEventPendingRequest(string eventId, string participantId, bool isParticipantAccepted)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (eventId != null && participantId != null)
                {
                    await eventDetailsManager.UpdateEventPendingRequest(eventId, participantId, isParticipantAccepted);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild Update Pending Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("events/update-event-status")]
        public async Task<ActionResult> UpdateEventActiveStatus(string eventId, int eventStatus)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    await eventDetailsManager.UpdateEventActiveStatus(eventId, eventStatus);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild Event Update Status Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("events/update-event-details")]
        public async Task<ActionResult> UpdateEventDetails(EditEventDetailsRequest editEventDetailsRequest)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (editEventDetailsRequest != null)
                {
                    await eventDetailsManager.UpdateEventDetails(editEventDetailsRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild Event Details Update Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Guest Event Operations

        [HttpGet("events/browse-upcoming")]
        public async Task<ActionResult<List<EventDetails>>> GetAllUpcomingEvents()
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                var events = await eventDetailsManager.GetAllUpcomingEvents();
                return Ok(events);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("events/apply")]
        public async Task<ActionResult> ApplyForUpcomingEvent([FromBody] EventApplicationRequest eventApplicationRequest)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (eventApplicationRequest != null)
                {
                    await eventDetailsManager.ApplyForUpcomingEvent(eventApplicationRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild Event Application Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/get-registered")]
        public async Task<ActionResult<List<EventDetails>>> GetMyRegisteredEvents(string guestId)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (guestId != null)
                {
                    var events = await eventDetailsManager.GetMyRegisteredEvents(guestId);
                    return Ok(events);
                }
                else
                {
                    return BadRequest("Invaild Participant Details Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("events/get-past-attended")]
        public async Task<ActionResult<List<EventDetails>>> GetMyPastAttendedEvents(string guestId)
        {
            var eventDetailsManager = new Managers.EventDetailsManager(_config);
            try
            {
                if (guestId != null)
                {
                    var events = await eventDetailsManager.GetMyPastAttendedEvents(guestId);
                    return Ok(events);
                }
                else
                {
                    return BadRequest("Invaild Participant Details Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

    }
}
