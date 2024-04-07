using Dapper;
using DapperTest2.Connection;
using DapperTest2.Controllers;
using DapperTest2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;

namespace DapperTest2.Repository
{
    public interface ICustomersRepository
    {
        Task<IEnumerable<Customers>> Get(string? CustomerID);
        Task<int> Post(Customers c);
        Task<int> Put([FromBody] Customers c);
        Task<int> Delete([FromBody] string CustomerID);
    }
    public class CustomersRepository : ICustomersRepository
    {
        private readonly ISqlConnectionFactory _connfactory;

        public CustomersRepository(ISqlConnectionFactory connfactory)
        {
            _connfactory = connfactory;
        }

        public async Task<IEnumerable<Customers>> Get(string? CustomerID)
        {
            using (SqlConnection conn = _connfactory.CreateConnection())
            {
                string sql = @"SELECT * FROM Customers";
                var para = new DynamicParameters();
                if (!string.IsNullOrEmpty(CustomerID))
                {
                    sql += " WHERE CustomerID = @CustomerID";
                    para.Add("@CustomerID", CustomerID);
                }
                return await conn.QueryAsync<Customers>(sql, para);
            }
        }

        public async Task<int> Post(Customers c)
        {
            using (SqlConnection conn = _connfactory.CreateConnection())
            {
                string sql = @"INSERT INTO 
                    Customers(CustomerID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax) 
                    VALUES (@CustomerID,@CompanyName,@ContactName,@ContactTitle,@Address,@City,@Region,@PostalCode,@Country,@Phone,@Fax)";
                try
                {
                    return await conn.ExecuteAsync(sql, c);
                }
                catch (Exception)
                {
                    return -1;
                }
                
            }
        }

        public async Task<int> Put([FromBody] Customers c)
        {
            using (SqlConnection conn = _connfactory.CreateConnection())
            {
                string sql = @"UPDATE Customers SET 
                CompanyName=@CompanyName,
                ContactName=@ContactName,
                ContactTitle=@ContactTitle,
                Address=@Address,
                City=@City,
                Region=@Region,
                PostalCode=@PostalCode,
                Country=@Country,
                Phone=@Phone,
                Fax=@Fax 
                WHERE CustomerID = @CustomerID";

                try
                {
                    return await conn.ExecuteAsync(sql, c);
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }

        public async Task<int> Delete([FromBody] string CustomerID)
        {
            string sql = @"DELETE Customers WHERE CustomerID=@CustomerID";
            var para = new DynamicParameters();
            para.Add("CustomerID", CustomerID);
            using (SqlConnection conn = _connfactory.CreateConnection())
            {
                try
                {
                    return await conn.ExecuteAsync(sql, para);
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }
    }
}
