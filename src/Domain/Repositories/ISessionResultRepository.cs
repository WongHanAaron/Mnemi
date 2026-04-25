using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Repositories;

public interface ISessionResultRepository
{
    Task<SessionResult> AddAsync(SessionResult result);
    Task<IReadOnlyCollection<SessionResult>> GetBySessionIdAsync(Guid sessionId);
    Task DeleteAsync(Guid id);
}
