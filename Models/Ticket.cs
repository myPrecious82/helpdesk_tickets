using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helpdesk_tickets.Models
{
    public class Ticket
    {
        public Int32 TicketId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Area { get; set; }

        public string Detail { get; set; }

        public string Status { get; set; }

        public string Priority { get; set; }

        public string Issue { get; set; }

        public string Description { get; set; }

        public string ResponsibilityGroup { get; set; }

        public DateTime Created { get; set; }

        public string Url { get; set; }

        public string MyNotes { get; set; }
    }
}
