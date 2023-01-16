using Domain;
using MediatR;
using Application.Core;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Threading;
using Persistence;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Photos
{
    public class Add
    {
        public class Command : IRequest<Result<Photo>>
        {
            public IFormFile File { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<Photo>>
        {
            private readonly DataContext _context;
            private readonly IPhotoAccessor _photoAccessor;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IPhotoAccessor photoAccessor, 
                IUserAccessor userAccessor)
            {
                _context = context;
                _photoAccessor = photoAccessor;
                _userAccessor = userAccessor;
            }

            public async Task<Result<Photo>> Handle(Command request, CancellationToken cancellationToken)
            {
                var user = await _context.Users
                    .Include(p => p.Photos)
                    .FirstOrDefaultAsync(u => u.UserName == _userAccessor.GetUsername());

                if (user == null) return null;

                var photoUploadResult = await _photoAccessor.AddPhoto(request.File);

                var photo = new Photo
                {
                    Url = photoUploadResult.Url,
                    Id = photoUploadResult.PublicId
                };

                if (!user.Photos.Any(p => p.IsMain)) photo.IsMain = true;

                user.Photos.Add(photo);

                return !(await _context.SaveChangesAsync() > 0)
                    ? Result<Photo>.Failure("Problem adding photo")
                    : Result<Photo>.Success(photo);
            }
        }
    }
}
