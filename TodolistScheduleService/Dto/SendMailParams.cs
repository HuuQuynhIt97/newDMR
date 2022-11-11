using System;
using System.Collections.Generic;
using System.Text;
using QuartzExtention.Jobs;
namespace TodolistScheduleService.Dto
{
    public interface IJobMap
    {
        public string URL { get; set; }
        public string PathName { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public List<string> Emails { get; set; }
    }
   public class SendMailParams: QuartzExtention.Jobs.IJobMap
    {
        public SendMailParams()
        {
        }
        /// <summary>
        /// Đặt Identity Key cho 1 công việc
        /// </summary>
        /// <param name="name">Loại báo cáo</param>
        /// <param name="group">Báo cáo theo thời gian VD: Hàng ngày, hàng tuần, hàng tháng</param>
        public void IdentityParams(string name, string group)
        {
            Name = name;
            Group = group;
        }
        /// <summary>
        /// Thông tin request đến Web_API
        /// </summary>
        /// <param name="uRL">Đường dẫn của Web_API</param>
        /// <param name="pathName">Tên đường dẫn (Tên controller và method)</param>
        /// <param name="emails">Danh sách người nhận</param>
        public void APIInfo(string uRL, string pathName, List<string> emails)
        {
            URL = uRL;
            PathName = pathName;
            Emails = emails;
        }
        public string GetIdentityParams()
        {
            return $"{Name}-{Group}";
        }
        public string URL { get; set; }
        public string PathName { get; set; }
        public string Name { get; set; }
        public string Group { get; set; }
        public List<string> Emails { get; set; }
    }
}
