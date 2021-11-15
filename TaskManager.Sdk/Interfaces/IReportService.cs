using System;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces
{
    public interface IReportService
    {
        ReportInfo Report { get; set; }

        String ReportPath { get; set; }

        void CreateMonthReport(UsersInfo CurrentUser);
    }
}