using SozlesmeSistemi.Models;

namespace SozlesmeSistemi.Services
{
    public interface IContractRequestService
    {
        Task<List<Unit>> GetAllUnitsAsync(int userId);
        Task<List<User>> GetManagersByUnitAsync(int unitId);
        Task<List<User>> GetManagersByUserUnitAsync(int userId); // Yeni metot
        Task CreateRequestAsync(ContractRequest request, int requestedById);
        Task<List<ContractRequest>> GetIncomingRequestsAsync(int userId);
        Task ApproveRequestAsync(int id);
        Task<List<User>> GetSubordinatesAsync(int managerId);
        Task AssignRequestToUserAsync(int requestId, int assignedToUserId);
        Task<List<ContractRequest>> GetAssignedRequestsAsync(int userId);
        Task<ContractRequest> GetRequestByIdAsync(int requestId);
        Task UpdateRequestAsync(ContractRequest request);
        Task<List<ContractRequest>> GetSentRequestsAsync(int userId);


        Task CreateNotificationAsync(Notification notification);
    }
}