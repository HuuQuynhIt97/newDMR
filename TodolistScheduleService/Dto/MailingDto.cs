using System;
using System.Collections.Generic;
using System.Text;

namespace TodolistScheduleService.Dto
{
    public class MailingDto
    {
        public int ID { get; set; }
        public List<string> UserNames { get; set; }
        public List<int> UserIDList { get; set; }
        public List<UserList> UserList { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string PathName { get; set; }
        public int UserID { get; set; }
        public string Frequency { get; set; }
        public string Report { get; set; }
        public DateTime TimeSend { get; set; }
    }
    public class UserList
    {
        public int ID { get; set; }
        public int MailingID { get; set; }
        public string Email { get; set; }
    }

}
