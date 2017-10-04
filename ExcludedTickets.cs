using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using helpdesk_tickets.Models;

namespace helpdesk_tickets
{
    public class ExcludedTickets
    {
        public List<Ticket> Tickets { get; set; }

        public ExcludedTickets()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var path = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), @"..\..\"));
            var templatePath = $"{path}ticketsToExclude.txt";

            Tickets = new List<Ticket>();

            var tixToExclude = File.ReadAllText(templatePath).Split(',');

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