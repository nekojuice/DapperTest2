using Dapper;
using DapperTest2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DapperTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private string _connectString = @"Server=.;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=true;";

        [HttpGet]
        [Route("{id?}")]
        async public Task<IEnumerable<Customers>> GET(string? CustomerID)
        {
            using (SqlConnection conn = new SqlConnection(_connectString))
            {
                string sql = @"SELECT * FROM Customers";
                if (!string.IsNullOrEmpty(CustomerID))
                {
                    sql += " WHERE CustomerID = @ID";
                    var para = new DynamicParameters();
                    para.Add("@ID", CustomerID);
                    return await conn.QueryAsync<Customers>(sql, para);
                }
                return await conn.QueryAsync<Customers>(sql);
            }
        }

        [HttpPost]
        async public Task<IActionResult> POST([FromBody] Customers c)
        {
            using (SqlConnection conn = new SqlConnection(_connectString))
            {
                string sql = @"INSERT INTO 
                    Customers(CustomerID,CompanyName,ContactName,ContactTitle,Address,City,Region,PostalCode,Country,Phone,Fax) 
                    VALUES (@CustomerID,@CompanyName,@ContactName,@ContactTitle,@Address,@City,@Region,@PostalCode,@Country,@Phone,@Fax)";
                var para = new DynamicParameters();
                para.Add("CustomerID", c.CustomerID);
                para.Add("CompanyName", c.CompanyName);
                para.Add("ContactName", c.ContactName);
                para.Add("ContactTitle", c.ContactTitle);
                para.Add("Address", c.Address);
                para.Add("City", c.City);
                para.Add("Region", c.Region);
                para.Add("PostalCode", c.PostalCode);
                para.Add("Country", c.Country);
                para.Add("Phone", c.Phone);
                para.Add("Fax", c.Fax);
                try
                {
                    await conn.ExecuteAsync(sql, para);
                    await conn.CloseAsync();
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }

            }
        }

        [HttpPut]
        async public Task<IActionResult> PUT([FromBody] Customers c)
        {
            using (SqlConnection conn = new SqlConnection(_connectString))
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
                var para = new DynamicParameters();
                para.Add("CustomerID", c.CustomerID);
                para.Add("CompanyName", c.CompanyName);
                para.Add("ContactName", c.ContactName);
                para.Add("ContactTitle", c.ContactTitle);
                para.Add("Address", c.Address);
                para.Add("City", c.City);
                para.Add("Region", c.Region);
                para.Add("PostalCode", c.PostalCode);
                para.Add("Country", c.Country);
                para.Add("Phone", c.Phone);
                para.Add("Fax", c.Fax);
                try
                {
                    await conn.ExecuteAsync(sql, para);
                    await conn.CloseAsync();
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
        }

        [HttpDelete]
        async public Task<IActionResult> DELETE([FromBody] string CustomerID)
        {
            string sql = @"DELETE Customers WHERE CustomerID=@CustomerID";
            var para = new DynamicParameters();
            para.Add("CustomerID", CustomerID);
            using (SqlConnection conn = new SqlConnection(_connectString))
            {
                try
                {
                    await conn.ExecuteAsync(sql, para);
                    await conn.CloseAsync();
                    return Ok();
                }
                catch (Exception)
                {
                    return BadRequest();
                }
            }
        }
    }
}
