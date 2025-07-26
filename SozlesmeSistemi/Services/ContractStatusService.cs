using SozlesmeSistemi.Data;
using SozlesmeSistemi.Models;

using Microsoft.EntityFrameworkCore;

public interface IContractStatusService
{
    void AddStatus(int contractId, string status, int changedByUserId, string rejectionReason = null); List<ContractStatusHistory> GetHistoryByContractId(int contractId);
}

public class ContractStatusService : IContractStatusService
{
    private readonly SozlesmeSistemiDbContext _context;

    public ContractStatusService(SozlesmeSistemiDbContext context)
    {
        _context = context;
    }

    public void AddStatus(int contractId, string status, int changedByUserId, string rejectionReason = null)
    {
        var contract = _context.Contracts.Find(contractId);
        if (contract == null)
        {
            System.Diagnostics.Debug.WriteLine($"Sözleşme bulunamadı: ContractId: {contractId}");
            throw new Exception("Sözleşme bulunamadı.");
        }

        contract.CurrentStatus = status;
        contract.RejectionReason = rejectionReason;

        var statusHistory = new ContractStatusHistory
        {
            ContractId = contractId,
            Status = status,
            ChangeDate = DateTime.Now,
            ChangedByUserId = changedByUserId,
            RejectionReason = rejectionReason
        };

        _context.ContractStatusHistories.Add(statusHistory);
        _context.SaveChanges();
        System.Diagnostics.Debug.WriteLine($"Durum güncellendi: ContractId: {contractId}, Status: {status}");
    }

    public List<ContractStatusHistory> GetHistoryByContractId(int contractId)
    {
        return _context.ContractStatusHistories
            .Include(h => h.ChangedByUser)
            .Where(h => h.ContractId == contractId)
            .OrderByDescending(h => h.ChangeDate)
            .ToList();
    }
}