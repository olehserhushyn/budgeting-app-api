using FamilyBudgeting.API.Extensions;
using FamilyBudgeting.Domain.DTOs.Requests.Accounts;
using FamilyBudgeting.Domain.Services.Interfaces;
using FamilyBudgeting.Domain.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FamilyBudgeting.API.Controllers
{
    [Route("[controller]")]
    [Authorize(Policy = AppConstants.AuthConfirmedEmailPolicyName)]
    public class AccountController : BaseController
    {
         private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts()
        {
            var userId = GetUserIdFromToken();
            return (await _accountService.GetAccountsAsync(userId)).ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountRequest request)
        {
            var userId = GetUserIdFromToken();
            return (await _accountService.CreateAccountAsync(userId, request)).ToActionResult();
        }

        [HttpGet("{accountId}")]
        public async Task<IActionResult> GetAccount(Guid accountId)
        {
            var userId = GetUserIdFromToken();
            return (await _accountService.GetAccountAsync(userId, accountId)).ToActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount(UpdateAccountRequest request)
        {
            var userId = GetUserIdFromToken();
            return (await _accountService.UpdateAccountAsync(userId, request)).ToActionResult();
        }

        [HttpDelete("{accountId}")]
        public async Task<IActionResult> DeleteAccount(Guid accountId)
        {
            var userId = GetUserIdFromToken();
            return (await _accountService.DeleteAccountAsync(userId, accountId)).ToActionResult();
        }
    }
}
