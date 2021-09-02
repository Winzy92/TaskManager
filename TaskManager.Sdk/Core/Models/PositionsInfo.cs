using System;

namespace TaskManager.Sdk.Core.Models
{
    public class PositionsInfo
    {
        public Int32 Id { get; set; }
        
        public String Name { get; set; }

        public Boolean CanModify { get; set; }
    }
}