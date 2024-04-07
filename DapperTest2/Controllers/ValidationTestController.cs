using Dapper;
using DapperTest2.Connection;
using DapperTest2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace DapperTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValidationTestController : ControllerBase
    {
        private readonly ISqlConnectionFactory _connfactory;
        public ValidationTestController(ISqlConnectionFactory connfactory)
        {
            _connfactory = connfactory;
        }

        /// <summary>
        /// 很醜的資料驗證練習，查詢id和Phone時一點關係也沒有
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] QueryCustomerVM val)
        {
            var conn = _connfactory.CreateConnection();
            string sql = @"SELECT * FROM Customers WHERE CustomerID=@CustomerID";

            //自訂回傳的格式
            return Ok(new ResponseMessage()
            {
                Data = await conn.QueryAsync<Customers>(sql, val),
                Message = "cathi",
                Status = "success"
            });
        }


    }

    /// <summary>
    /// 電話號碼驗證，驗證是否過長(>10字)或電話重複
    /// </summary>
    public class PhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //DI注入連線
            var _connfactory = validationContext.GetServices(typeof(ISqlConnectionFactory));
            var conn = ((ISqlConnectionFactory)_connfactory.FirstOrDefault()).CreateConnection();

            if (((string)value).Length > 10)
            {
                return new ValidationResult("電話號碼過長");
            }

            int count = PhoneStringLength((string)value, conn).Count;
            Console.WriteLine(count);

            if (count > 0)
            {
                return new ValidationResult("電話號碼重複使用");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// 查詢有沒有電話號碼重複
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="conn">連線物件</param>
        /// <returns></returns>
        private CheckVM PhoneStringLength(string value, SqlConnection conn)
        {
            string sql = @"SELECT COUNT(Phone) AS Count FROM Customers WHERE Phone = @Phone";
            var para = new DynamicParameters();
            para.Add("Phone", (string)value);

            return conn.Query<CheckVM>(sql, para).FirstOrDefault();
        }
    }

    /// <summary>
    /// 用來接檢查query到的資料格式
    /// </summary>
    public class CheckVM
    {
        public int Count { get; set; }
    }

    /// <summary>
    /// API接收的資料格式
    /// </summary>
    public class QueryCustomerVM 
    {
        public string CustomerID { get; set; }

        [PhoneAttribute]
        public string Phone {  get; set; }
    }
}
