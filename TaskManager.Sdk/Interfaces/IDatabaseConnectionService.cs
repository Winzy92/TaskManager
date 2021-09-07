using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Npgsql;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces
{
    public interface IDatabaseConnectionService
    {
        void CheckDbConnection();

        void CloseConnection();
        
        Boolean IsConnected { get; set; }
        
        NpgsqlConnection Connection { get; set; }
        
    }
}