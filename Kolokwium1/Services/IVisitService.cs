using Kolokwium1.Models.DTOs;

namespace Kolokwium1.Services;

public interface IVisitService
{
    Task<VisitDto> GetVisit(int id);
    Task PostVisit(VisitPostDto visitDto);
}