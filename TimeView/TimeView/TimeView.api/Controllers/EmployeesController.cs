﻿using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using TimeView.context;
using TimeView.data;

namespace TimeView.api.Controllers
{
    public class EmployeesController : ApiController
    {
        private readonly TimeViewContext db = new TimeViewContext();

        [ResponseType(typeof(Employee))]
        public Employee GetEmployee(string username, string password)
        {
            try
            {
                var employee =
                    db.Employee.Include(e => e.Company)
                        .Where(e => e.Username == username)
                        .Where(e => e.Password == password)
                        .First();

                employee.Company.Employees = null;

                return employee;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // GET: api/Employees
        public IQueryable<Employee> GetEmployees()
        {
            return db.Employee.Include(e => e.Following);
        }

        // GET: api/Employees/5
        [ResponseType(typeof(Employee))]
        public IHttpActionResult GetEmployee(int id)
        {
            var employee = db.Employee
                .Include(e => e.Following.Select(f => f.Company))
                .Include(e => e.Following)
                .Where(e => e.Id == id).First();

            // Avoid loops

            for (var i = 0; i < employee.Following.Count; i++)
            {
                employee.Following[i].Follower = null;
                employee.Following[i].Company.Employees = null;
            }

            // End avoid loops

            return Ok(employee);
        }

        // PUT: api/Companies/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutEmployee(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var emp in db.Employee)
            {
                if (employee.Username.Equals(emp.Username))
                {
                    return StatusCode(HttpStatusCode.Conflict);
                }
            }

            db.Employee.Add(employee);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Employees
        public IQueryable<Employee> PostEmployee(Employee employee)
        {
            Employee _found = new Employee { Id = -1 };
            try
            {
                _found = db.Employee.Where(e => e.Username == employee.Username).First();
            }
            catch (Exception)
            {
                // No found
            }

            if (_found.Id != -1)
            {
                return null;
            }

            // 128 salt characters
            try
            {
                String salt = GetSalt();
                employee.Password = salt.ToString() + Sha256(salt + employee.Password);
            }
            catch (Exception)
            {
                return null;
            }

            // Temp data
            employee.CompanyId = 1;

            try
            {
                db.Employee.Add(employee);
                db.SaveChanges();
            }
            catch (Exception)
            {
                return null;
            }

            return db.Employee.Where(e => e.Username == employee.Username);
        }

        // DELETE: api/Employees/5
        [ResponseType(typeof(Employee))]
        public IHttpActionResult DeleteEmployee(int id)
        {
            var employee = db.Employee.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            db.Employee.Remove(employee);
            db.SaveChanges();

            return Ok(employee);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EmployeeExists(int id)
        {
            return db.Employee.Count(e => e.Id == id) > 0;
        }

        static string Sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetByteCount(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        private String GetSalt()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[128];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            var finalString = new String(stringChars);

            return finalString;
        }
    }
}