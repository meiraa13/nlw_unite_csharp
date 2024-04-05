using PassIn.Communication.Responses;
using PassIn.Exceptions;
using PassIn.Infrastructure;

namespace PassIn.Application.UseCases.CheckIns.CheckIn;

public class CheckInUseCase
{
    private readonly PassInDbContext _dbContext;
    public CheckInUseCase()
    {
        _dbContext = new PassInDbContext();
    }
    public ResponseRegisteredJson Execute(Guid attendeeId)
    {
        Validate(attendeeId);

        var entity = new Infrastructure.Entities.CheckIn
        {
            Attendee_id = attendeeId,
            Created_at = DateTime.UtcNow,

        };

        _dbContext.CheckIns.Add(entity);
        _dbContext.SaveChanges();
        return new ResponseRegisteredJson
        {
            Id = entity.Id,

        };
    }

    private void Validate(Guid attendeeId)
    {
       var attendee =  _dbContext.Attendees.Any(att => att.Id == attendeeId);
        if (attendee == false)
        {
            throw new NotFoundException("attendee id not found");
        }

        var existCheckIn = _dbContext.CheckIns.Any(ch => ch.Attendee_id == attendeeId);
        if(existCheckIn)
        {
            throw new ConflictException("attendee already made checkin");
        }
    
    }
}
