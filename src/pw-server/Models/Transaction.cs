using System;

namespace pwServer.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime TransactionTime { get; set; }
        public double Amount { get; set; }
        public double SenderResult { get; set; }
        public double ReceiverResult { get; set; }

        public int SenderId { get; set; }
        public User Sender { get; set; }

        public int ReceiverId { get; set; }
        public User Receiver { get; set; }
    }
}
