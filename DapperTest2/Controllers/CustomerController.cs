using Dapper;
using DapperTest2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
        [Route("{CustomerID?}")]
        async public Task<IActionResult> GET(string? CustomerID)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectString))
                {
                    string sql = @"SELECT * FROM Customers";
                    var para = new DynamicParameters();
                    if (!string.IsNullOrEmpty(CustomerID))
                    {
                        sql += " WHERE CustomerID = @CustomerID";
                        para.Add("@CustomerID", CustomerID);
                    }
                    return Ok(new ResponseMessage()
                    {
                        Status = "success",
                        Message = "已回傳資料",
                        Data = await conn.QueryAsync<Customers>(sql, para)
                    });
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ResponseMessage()
                {
                    Status = "error",
                    Message = "發生嚴重錯誤"
                });
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
                try
                {
                    await conn.ExecuteAsync(sql, c);
                    await conn.CloseAsync();
                    return Created(Url.Content("~/api/[controller]"), new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已新增"
                    });
                }
                catch (Exception)
                {
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料新增失敗"
                    });
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

                try
                {
                    await conn.ExecuteAsync(sql, c);
                    await conn.CloseAsync();
                    return Ok(new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已修改"
                    });
                }
                catch (Exception)
                {
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料新增失敗"
                    });
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
                    return Ok(new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已刪除"
                    });
                }
                catch (Exception)
                {
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料刪除失敗"
                    });
                }
            }
        }
    }

    public class ResponseMessage
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public dynamic? Data { get; set; }
    }
}
