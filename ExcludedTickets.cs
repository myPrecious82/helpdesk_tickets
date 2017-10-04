using System;
using System.Collections.Generic;
using System.IO;
using helpdesk_tickets.Models;

namespace helpdesk_tickets
{
    public class ExcludedTickets
    {
        public List<Ticket> Tickets { get; set; }

        public ExcludedTickets()
        {
            const string path = "C:\\LRS Docs\\mine\\Projects\\helpdesk_tickets\\ticketsToExclude.txt";
            Tickets = new List<Ticket>();

            var tixToExclude = File.ReadAllText(path).Split(',');

            foreach (var id in tixToExclude)
            {
                Int32 number;
                bool result = Int32.TryParse(id, out number);
                var newTicket = new Ticket()
                {
                    TicketId = number
                };
                Tickets.Add(newTicket);
            }
        }
    }
}