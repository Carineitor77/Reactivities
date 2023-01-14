using Application.Core;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities
{
    public class Edit
    {
        public class Command : IRequest<Result<Unit>>
        {
            public Activity Activity { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(c => c.Activity).SetValidator(new ActivityValidator());
            }
        }

        public class Handler : IRequestHandler<Command, Result<Unit>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
            }

            public async Task<Result<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var activity = await _context.Activities.FindAsync(request.Activity.Id);

                if (activity == null) return null;

                // _mapper.Map(request.Activity, activity); // mapper work doesn't correct here
                // here is must to use DTO, CreateMap<Activity, Activity>() work doesn't correct here
                // SaveChangesAsync() doesn't see changes

                activity.Title = request.Activity.Title;
                activity.Date = request.Activity.Date;
                activity.Description = request.Activity.Description;
                activity.Category = request.Activity.Category;
                activity.City = request.Activity.City;
                activity.Venue = request.Activity.Venue;

                return !(await _context.SaveChangesAsync() > 0)
                    ? Result<Unit>.Failure("Failed to update the activity")
                    : Result<Unit>.Success(Unit.Value);
            }
        }
    }
}