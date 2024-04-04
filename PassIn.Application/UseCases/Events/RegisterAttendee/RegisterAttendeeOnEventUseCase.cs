using PassIn.Communication.Requests;
using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;
using System.Net.Mail;

namespace PassIn.Application.UseCases.Events.RegisterAttendee;

public class RegisterAttendeeOnEventUseCase
{
    private readonly PassInDbContext _dbContext;
    public RegisterAttendeeOnEventUseCase()
    {
        _dbContext = new PassInDbContext();
    }
    public ResponseRegisteredJson Execute(Guid eventId, RequestRegisterEventJson request)
    {
        
        Validate(eventId, request);

        var entity = new Infrastructure.Entities.Attendee
        {
            Email = request.Email,
            Name = request.Name,
            Event_Id = eventId,
            Created_At = DateTime.UtcNow,
        };

        _dbContext.Attendees.Add(entity);
        _dbContext.SaveChanges();

        return new ResponseRegisteredJson
        {
            Id = entity.Id,
        };
    }

    private void Validate(Guid eventId, RequestRegisterEventJson request)
    {
        var eventEntity = _dbContext.Events.Find();
        if(eventEntity is null)
        {
            throw new NotFoundException("an event with this id does not exists");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ErrorOnValidationException("title does not exists");
        }

        var emailIsValid = EmailIsValid(request.Email);
        if(emailIsValid == false)
        {
            throw new ErrorOnValidationException("invalid email");
        }

        var attendeeAlreadyRegistered = _dbContext.Attendees.Any(user => user.Email.Equals(request.Email) && user.Event_Id == eventId);
        if(attendeeAlreadyRegistered)
        {
            throw new ConflictException("Attendee already registered");
        }

        var totalAttendees = _dbContext.Attendees.Count(user => user.Event_Id == eventId);
        if(totalAttendees > eventEntity.Maximum_Attendees)
        {
            throw new ConflictException("this event is full");
        }
    }

    private bool EmailIsValid(string email)
    {
        try
        {
            new MailAddress(email);

            return true;

        }
        catch
        {
            return false;
        }
    }
}
