using Ardalis.Result;
using FamilyBudgeting.Domain.DTOs.Models.Ledgers;
using FamilyBudgeting.Domain.DTOs.Requests.Ledgers;
using FamilyBudgeting.Domain.Interfaces.Queries;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Data.Ledgers;
using FamilyBudgeting.Domain.Data.UserLedgers;

namespace FamilyBudgeting.Domain.Services
{
    public class LedgerService : ILedgerService
    {
        private readonly ILedgerRepository _ledgerRepository;
        private readonly ILedgerQueryService _ledgerQueryService;

        public LedgerService(ILedgerRepository ledgerRepository, ILedgerQueryService ledgerQueryService)
        {
            _ledgerRepository = ledgerRepository;
            _ledgerQueryService = ledgerQueryService;
        }

        public async Task<Result<Guid>> CreateLedgerAsync(CreateLedgerRequest request, Guid userId, Guid roleId)
        {
            var ledger = new Ledger(request.LedgerTitle);
            var userLedger = new UserLedger(userId, roleId, Guid.Empty);

            var result = await _ledgerRepository.CreateLedgerAsync(ledger, userLedger);

            if (result.LedgerId == Guid.Empty || result.UserLedgerId == Guid.Empty)
            {
                return Result<Guid>.Error("Unable to create Ledger");
            }

            return Result.Success(result.LedgerId);
        }

        public async Task<Result<IEnumerable<LedgerDto>>> GetLedgersFromUserAsync(Guid userId)
        {
            var ledgers = await _ledgerQueryService.GetUserLedgersAsync(userId);
            return Result<IEnumerable<LedgerDto>>.Success(ledgers);
        }

        public async Task<Result<bool>> UpdateLedgerAsync(UpdateLedgerRequest request)
        {
            var ledgerDto = await _ledgerQueryService.GetUserLedgerFirstAsync(request.Id); // TODO: Replace Guid.Empty with actual userId if available
            if (ledgerDto is null || ledgerDto.Id != request.Id)
            {
                return Result<bool>.NotFound("Ledger not found");
            }
            var ledger = new Ledger(ledgerDto.Title)
            {
                Id = ledgerDto.Id
            };
            ledger.ChangeTitle(request.Title);
            var updated = await _ledgerRepository.UpdateLedgerAsync(ledger);
            if (!updated)
                return Result<bool>.Error("Unable to update Ledger");
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteLedgerAsync(DeleteLedgerRequest request)
        {
            var ledgerDto = await _ledgerQueryService.GetUserLedgerFirstAsync(request.Id);
            if (ledgerDto is null || ledgerDto.Id != request.Id)
            {
                return Result<bool>.NotFound("Ledger not found");
            }
            var ledger = new Ledger(ledgerDto.Title)
            {
                Id = ledgerDto.Id
            };

            ledger.Delete();
            var updated = await _ledgerRepository.UpdateLedgerAsync(ledger);
            if (!updated)
            {
                return Result<bool>.Error("Unable to delete Ledger");
            }

            return Result<bool>.Success(true);
        }
    }
}
