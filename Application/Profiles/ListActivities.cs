﻿using Application.Core;
using AutoMapper;
using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles
{
    public class ListActivities
    {
        public class Query : IRequest<Result<List<UserActivityDto>>>
        {
            public string Username { get; set; }
            public string Predicate { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<UserActivityDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<UserActivityDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = _context.ActivityAttendees
                    .Where(u => u.AppUser.UserName == request.Username)
                    .OrderBy(a => a.Activity.Date)
                    .ProjectTo<UserActivityDto>(_mapper.ConfigurationProvider)
                    .AsQueryable();

                var today = DateTime.UtcNow;

                query = request.Predicate switch
                {
                    "past" => query.Where(a => a.Date <= today),
                    "hosting" => query.Where(a => a.HostUsername == request.Username),
                    _ => query.Where(a => a.Date >= today)
                };

                var activities = await query.ToListAsync();

                return Result<List<UserActivityDto>>.Success(activities);
            }
        }
    }
}
