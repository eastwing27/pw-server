using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using pwServer.DTO;
using pwServer.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace pwServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class TransactController : Controller
    {
        private PwContext db;

        public TransactController(PwContext db)
        {
            this.db = db;
        }
        
        [HttpPost]
        [Route("send")]
        public async Task<IActionResult> Send([FromBody]SendDTO dto)
        {
            if (dto == null)
            {
                return new BadRequestResult();
            }

            var sender   = db.Users.First(u => u.Id == dto.SenderId);
            var receiver = db.Users.First(u => u.Id == dto.ReceiverId);

            if (sender.Balance < dto.Amount)
            {
                return new StatusCodeResult(422);
            }
            sender.Balance -= dto.Amount;
            receiver.Balance += dto.Amount;

            var transaction = new Transaction();
            transaction.SenderId = sender.Id;
            transaction.ReceiverId = receiver.Id;
            transaction.Amount = dto.Amount;
            transaction.SenderResult = sender.Balance;
            transaction.ReceiverResult = receiver.Balance;
            transaction.TransactionTime = DateTime.UtcNow;

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            return new OkResult();
        }

        [HttpGet("{id}")]
        [Route("{id}/outgoing")]
        public async Task<IActionResult> GetOutgoing(int id)
        {
            var json = await GetTransactions(t => t.SenderId == id);
            return new ContentResult() { Content = json };
        }

        [HttpGet("{id}")]
        [Route("{id}/incoming")]
        public async Task<IActionResult> GetIncoming(int id)
        {
            var json = await GetTransactions(t => t.ReceiverId == id);
            return new ContentResult() { Content = json };
        }

        [HttpGet("{id}")]
        [Route("{id}/all")]
        public async Task<IActionResult> GetAll(int id)
        {
            var json = await GetTransactions(t => t.SenderId == id | t.ReceiverId == id);
            return new ContentResult() { Content = json };
        }

        private async Task<string> GetTransactions(Func<Transaction, bool> predicate)
        {
            var transactions = await db.Transactions.Where(t => predicate(t))
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Select(t => new
                {
                    TransactTime = t.TransactionTime,
                    SenderId = t.SenderId,
                    ReceiverId = t.ReceiverId,
                    SenderName = $"{t.Sender.FirstName} {t.Sender.LastName}",
                    ReceiverName = $"{t.Receiver.FirstName} {t.Receiver.LastName}",
                    Amount = t.Amount,
                    SenderResult = t.SenderResult,
                    ReceiverResult = t.ReceiverResult
                })
                .ToArrayAsync();

            return JsonConvert.SerializeObject(transactions);
        }
    }
}
