using System;

namespace TaskManager.Sdk.Core.Models
{
    public class DbConnectionInfo
    {
        public String Host { get; set; }
        public Int32 Port { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }
        public String DbName { get; set; }
    }
}