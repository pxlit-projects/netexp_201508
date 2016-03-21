﻿using System.Threading.Tasks;
using TimeView.data;

namespace TimeView.DAL.Repositories.Employee
{
    public interface IEmployeeRepository
    {
        Task<TimeView.data.Employee> GetEmployee(string username, string password);
        Task<TimeView.data.Employee> GetEmployee(int employeeId);
        Task<TimeView.data.Employee> CreateEmployee(TimeView.data.Employee employee);
    }
}