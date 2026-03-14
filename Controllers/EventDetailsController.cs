using EncantoWebAPI.Hubs;
using EncantoWebAPI.Managers;
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
        private readonly EventDetailsManager _eventDetailsManager;

        public EventDetailsController(IHubContext<NotificationHub> hubContext, EventDetailsManager eventDetailsManager)
        {
            _hubContext = hubContext;
            _eventDetailsManager = eventDetailsManager;
        }

        #region Host Event Operations

        [HttpPost("events/new")]
        public async Task<ActionResult> CreateNewEvent(CreateEventRequest newEventRequest)
        {
            try
            {
                if (newEventRequest != null)
                {
                    var createdEvent = await _eventDetailsManager.CreateNewEvent(newEventRequest);

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
            try
            {
                var events = await _eventDetailsManager.GetMyUpcomingHostedEvents(hostId);
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
            try
            {
                var events = await _eventDetailsManager.GetMyPastHostedEvents(hostId);
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
            try
            {
                if (hostId != null)
                {
                    var events = await _eventDetailsManager.GetAllPendingRequests(hostId);
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
            try
            {
                if (eventId != null && participantId != null)
                {
                    await _eventDetailsManager.UpdateEventPendingRequest(eventId, participantId, isParticipantAccepted);
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
            try
            {
                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    await _eventDetailsManager.UpdateEventActiveStatus(eventId, eventStatus);
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
            try
            {
                if (editEventDetailsRequest != null)
                {
                    await _eventDetailsManager.UpdateEventDetails(editEventDetailsRequest);
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
            try
            {
                var events = await _eventDetailsManager.GetAllUpcomingEvents();
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
            try
            {
                if (eventApplicationRequest != null)
                {
                    await _eventDetailsManager.ApplyForUpcomingEvent(eventApplicationRequest);
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
            try
            {
                if (guestId != null)
                {
                    var events = await _eventDetailsManager.GetMyRegisteredEvents(guestId);
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
            try
            {
                if (guestId != null)
                {
                    var events = await _eventDetailsManager.GetMyPastAttendedEvents(guestId);
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
