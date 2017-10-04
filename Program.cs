using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using helpdesk_tickets.Models;
using System.Net.Mail;
using System.Configuration;

namespace helpdesk_tickets
{
    public class Program
    {
        public static void Main()
        {
            var appSettings = ConfigurationManager.AppSettings;

            var dbConnection = appSettings["ConnectionString"];
            var ticketAreas = appSettings["TicketAreas"];
            var assigneeUserName = appSettings["AssigneeUserName"];
            var responsibilityGroups = appSettings["ResponsibilityGroups"];

            var myDS = new DataSet();
            var myConn = new SqlConnection(dbConnection);
            var myQuery =
                "SELECT TOP 1000 T.TicketId,C.FirstName,C.LastName,C.Email,TA.Name AS Area,TD.Name AS Detail,TS.Name AS Status,P.Name AS Priority,T.Issue,TD2.Description,RG.Name AS ResponsibilityGroup,T.Created " +
                "FROM Ticket T " +
                "INNER JOIN Client C ON T.ClientId = C.ClientId " +
                "INNER JOIN TicketDetail TD ON T.TicketDetailId = TD.TicketDetailId " +
                "INNER JOIN TicketArea TA ON TD.TicketAreaId = TA.TicketAreaId " +
                "INNER JOIN TicketStatus TS ON T.TicketStatusId = TS.TicketStatusId " +
                "INNER JOIN Priority P ON T.PriorityId = P.PriorityId " +
                "INNER JOIN TicketDescription TD2 ON T.TicketId = TD2.TicketId " +
                "INNER JOIN ResponsibilityGroup RG ON T.ResponsibilityGroupId = RG.ResponsibilityGroupId " +
                "WHERE T.TicketStatusId <> 3 AND AssigneeUserName = " + assigneeUserName + " " +
                "ORDER BY Status, Priority, Area, Detail, TicketId";

            var myCmd = new SqlCommand(myQuery, myConn);
            var myDataAdapter = new SqlDataAdapter(myCmd);

            myConn.Open();
            myDataAdapter.Fill(myDS, "MyTickets");

            myQuery =
                "SELECT TOP 1000 T.TicketId,C.FirstName,C.LastName,C.Email,TA.Name AS Area,TD.Name AS Detail,TS.Name AS Status,P.Name AS Priority,T.Issue,TD2.Description,RG.Name AS ResponsibilityGroup,T.Created " +
                "FROM Ticket T " +
                "INNER JOIN Client C ON T.ClientId = C.ClientId " +
                "INNER JOIN TicketDetail TD ON T.TicketDetailId = TD.TicketDetailId " +
                "INNER JOIN TicketArea TA ON TD.TicketAreaId = TA.TicketAreaId " +
                "INNER JOIN TicketStatus TS ON T.TicketStatusId = TS.TicketStatusId " +
                "INNER JOIN Priority P ON T.PriorityId = P.PriorityId " +
                "INNER JOIN TicketDescription TD2 ON T.TicketId = TD2.TicketId " +
                "INNER JOIN ResponsibilityGroup RG ON T.ResponsibilityGroupId = RG.ResponsibilityGroupId " +
                "WHERE T.TicketStatusId = 2 AND TA.Name IN(" + ticketAreas + ") AND RG.Name IN(" + responsibilityGroups + ") AND AssigneeUserName IS NULL " +
                "ORDER BY Status, Priority, Area, Detail, TicketId";

            myCmd = new SqlCommand(myQuery, myConn);
            myDataAdapter = new SqlDataAdapter(myCmd);
            myDataAdapter.Fill(myDS, "OpenTickets");

            myConn.Close();

            var dta = myDS.Tables;

            var et = new ExcludedTickets();

            var myTix = new List<Ticket>();
            foreach (DataRow row in dta[0].Rows)
            {
                Int32 number;
                bool result = Int32.TryParse(row.ItemArray[0].ToString(), out number);
                var ticket = new Ticket()
                {
                    TicketId = number,
                    FirstName = row.ItemArray[1].ToString(),
                    LastName = row.ItemArray[2].ToString(),
                    Email = row.ItemArray[3].ToString(),
                    Area = row.ItemArray[4].ToString(),
                    Detail = row.ItemArray[5].ToString(),
                    Status = row.ItemArray[6].ToString(),
                    Priority = row.ItemArray[7].ToString(),
                    Issue = row.ItemArray[8].ToString(),
                    Description = row.ItemArray[9].ToString(),
                    ResponsibilityGroup = row.ItemArray[10].ToString(),
                    Created = DateTime.Parse(row.ItemArray[11].ToString()),
                    Url = $"<a href='http://helpdesk/Users/Ticket.aspx?TicketId={number.ToString().Substring(0, 7)}'>{number}</a>"
                };
                myTix.Add(ticket);
            }

            var openTix = new List<Ticket>();
            foreach (DataRow row in dta[1].Rows)
            {
                Int32 number;
                bool result = Int32.TryParse(row.ItemArray[0].ToString(), out number);

                if (et.Tickets.Count(x => x.TicketId == number) > 0) continue;
                var ticket = new Ticket()
                {
                    TicketId = number,
                    FirstName = row.ItemArray[1].ToString(),
                    LastName = row.ItemArray[2].ToString(),
                    Email = row.ItemArray[3].ToString(),
                    Area = row.ItemArray[4].ToString(),
                    Detail = row.ItemArray[5].ToString(),
                    Status = row.ItemArray[6].ToString(),
                    Priority = row.ItemArray[7].ToString(),
                    Issue = row.ItemArray[8].ToString(),
                    Description = row.ItemArray[9].ToString(),
                    ResponsibilityGroup = row.ItemArray[10].ToString(),
                    Created = DateTime.Parse(row.ItemArray[11].ToString()),
                    Url = $"<a href='http://helpdesk/Users/Ticket.aspx?TicketId={number.ToString().Substring(0, 7)}'>{number}</a>"
                };
                openTix.Add(ticket);
            }

            var emailSmtpHost = appSettings["EmailSmtpHost"];
            var emailSendTo = appSettings["EmailSendTo"];
            var emailSendFrom = appSettings["EmailSendFrom"];
            var emailSendFromDisplay = appSettings["EmailSendFromDisplay"];

            var emailSubject = $"PDR Scripts for {DateTime.Now.Date:MM.dd.yyyy}";
            var crlf = "<br />";
            var strPending = $"<strong>Pending Tickets</strong>{crlf}";

            var body = $"<strong>My Tickets</strong> {crlf}";

            foreach (var t in myTix)
            {
                var addLine = $"{t.Url} ~ {t.Status} ~ {t.Area}/{t.Detail}";

                if (!t.Status.ToLower().Contains("pending"))
                {
                    body += addLine;

                    if (t.Issue.ToLower().Contains("expunged") || t.Issue.ToLower().Contains("purged"))
                    {
                        body += " <em>(expunged/purged)</em>";
                    }
                    body += crlf;
                }
                else
                {
                    strPending += addLine;

                    if (t.Issue.ToLower().Contains("expunged") || t.Issue.ToLower().Contains("purged"))
                    {
                        strPending += " <em>(expunged/purged)</em>";
                    }
                    strPending += crlf;
                }
            }

            body += crlf + strPending;

            if (openTix.Count > 0)
            {
                body += $"{crlf}<strong>Open Tickets To Check</strong>{crlf}";

                foreach (var t in openTix)
                {
                    var addLine = $"{t.Url} ~ {t.Status} ~ {t.Area}/{t.Detail}";
                    body += addLine;
                    body += crlf;
                }
            }

            var client = new SmtpClient(emailSmtpHost);

            var from = new MailAddress(emailSendFrom, emailSendFromDisplay, System.Text.Encoding.UTF8);
            var to = new MailAddress(emailSendTo);
            var message = new MailMessage(from, to)
            {
                IsBodyHtml = true,
                BodyEncoding = System.Text.Encoding.UTF8,
                Subject = emailSubject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Bcc = { new MailAddress(emailSendFrom) }
            };
            message.Body += "<span style='font-size:11pt;font-family:Calibri'>" + body + "</span>";

            client.Send(message);

            message.Dispose();
        }
    }
}
