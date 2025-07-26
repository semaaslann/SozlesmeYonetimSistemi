namespace SozlesmeSistemi.Services
{
    using Microsoft.EntityFrameworkCore;
    using SozlesmeSistemi.Data;
    using SozlesmeSistemi.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ReminderService
    {
        private readonly SozlesmeSistemiDbContext _context;

        public ReminderService(SozlesmeSistemiDbContext context)
        {
            _context = context;
        }

        public List<Reminder> GetUpcomingRemindersWithin30Days()
        {
            var today = DateTime.Today;
            var thirtyDaysFromNow = today.AddDays(30);

            // Yaklaşan sözleşmeler için hatırlatıcıları oluştur
            GenerateAndSaveReminders();

            // Hatırlatıcıları çek
            return _context.Reminders
                .Include(r => r.Contract)
                    .ThenInclude(c => c.User)
                .Include(r => r.Contract)
                    .ThenInclude(c => c.ContractSigners)
                        .ThenInclude(cs => cs.User)
                .Where(r => r.DueDate.Date >= today && r.DueDate.Date <= thirtyDaysFromNow && !r.IsCompleted)
                .ToList();
        }

        public void GenerateAndSaveReminders()
        {
            var today = DateTime.Today;
            var thirtyDaysFromNow = today.AddDays(30);

            var contracts = _context.Contracts
                .Include(c => c.User)
                .Include(c => c.ContractSigners)
                    .ThenInclude(cs => cs.User)
                .Where(c =>
                    (c.ImzaOnayTarihi.HasValue && c.ImzaOnayTarihi.Value.Date >= today && c.ImzaOnayTarihi.Value.Date <= thirtyDaysFromNow) ||
                    (c.FinisDate.HasValue && c.FinisDate.Value.Date >= today && c.FinisDate.Value.Date <= thirtyDaysFromNow))
                .ToList();

            foreach (var contract in contracts)
            {
                // İmza onay tarihi hatırlatması
                if (contract.ImzaOnayTarihi.HasValue)
                {
                    var daysUntilImza = (contract.ImzaOnayTarihi.Value.Date - today).Days;
                    if (daysUntilImza > 30 || daysUntilImza < 0) // 30 günden uzun veya geçmişse atla
                    {
                        continue;
                    }

                    var existingReminder = _context.Reminders
                        .FirstOrDefault(r =>
                            r.ContractId == contract.Id &&
                            r.DueDate == contract.ImzaOnayTarihi.Value &&
                            r.Description.Contains("İmza"));

                    if (existingReminder == null)
                    {
                        // İmzalayan kişilerin adlarını al
                        var imzalayanlar = contract.ContractSigners
                            .Where(cs => cs.Role == "Imzalayan")
                            .Select(cs => cs.User?.Name ?? "Bilinmiyor")
                            .ToList();
                        var imzalayanlarText = imzalayanlar.Any() ? string.Join(", ", imzalayanlar) : "Bilinmiyor";

                        var reminder = new Reminder
                        {
                            Title = contract.Title,
                            Description = $"İmza tarihi yaklaşıyor. İmzalayan: {imzalayanlarText}",
                            DueDate = contract.ImzaOnayTarihi.Value,
                            IsCompleted = false,
                            ContractId = contract.Id,
                            Contract = contract,
                            IsNew = true
                        };

                        _context.Reminders.Add(reminder);
                    }
                }

                // Bitiş tarihi hatırlatması
                if (contract.FinisDate.HasValue)
                {
                    var daysUntilFinish = (contract.FinisDate.Value.Date - today).Days;
                    if (daysUntilFinish > 30 || daysUntilFinish < 0) // 30 günden uzun veya geçmişse atla
                    {
                        continue;
                    }

                    var existingReminder = _context.Reminders
                        .FirstOrDefault(r =>
                            r.ContractId == contract.Id &&
                            r.DueDate == contract.FinisDate.Value &&
                            r.Description.Contains("Bitiş"));

                    if (existingReminder == null)
                    {
                        var reminder = new Reminder
                        {
                            Title = contract.Title,
                            Description = $"Bitiş tarihi yaklaşıyor. Sorumlu: {contract.User?.Name ?? "Bilinmiyor"}",
                            DueDate = contract.FinisDate.Value,
                            IsCompleted = false,
                            ContractId = contract.Id,
                            Contract = contract,
                            IsNew = true
                        };

                        _context.Reminders.Add(reminder);
                    }
                }
            }

            _context.SaveChanges();
        }
    }
}