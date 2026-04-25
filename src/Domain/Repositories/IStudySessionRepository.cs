using System;
using System.Threading.Tasks;
using Mnemi.Domain.Entities;

namespace Mnemi.Domain.Repositories;

public interface IStudySessionRepository
{
    Task<StudySession> AddAsync(StudySession session);
    Task<StudySession?> GetByIdAsync(Guid id);
    Task<StudySession> UpdateAsync(StudySession session);
    Task DeleteAsync(Guid id);
}
