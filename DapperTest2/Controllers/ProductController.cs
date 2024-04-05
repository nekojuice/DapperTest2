using Dapper;
using DapperTest2.Connection;
using DapperTest2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Transactions;

namespace DapperTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ISqlConnectionFactory _connfactory;
        public ProductController(ISqlConnectionFactory connfactory)
        {
            _connfactory = connfactory;
        }

        [HttpGet]
        [Route("{ProductID?}")]
        public Task<IEnumerable<Products>> GET(int? ProductID)
        {
            string sql = @"SELECT * FROM Products";
            var para = new DynamicParameters();
            if (ProductID != null)
            {
                sql += @" WHERE ProductID = @ProductID";
                para.Add("ProductID", ProductID);
            }

            try
            {
                var conn = _connfactory.CreateConnection();
                return conn.QueryAsync<Products>(sql, para);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [HttpPost]
        [Route("trade")]
        public IActionResult Trade([FromBody] TradeViewmodel vm)
        {
            using (var conn = _connfactory.CreateConnection())
            {
                //餘額不足判斷
                try
                {
                    string sqlcheck = @"SELECT UnitPrice FROM Products WHERE ProductID=@ProductID";
                    var para = new DynamicParameters();
                    para.Add("ProductID", vm.BuyerID);
                    var money = conn.QueryFirst<decimal>(sqlcheck, para);
                    if (money < vm.Money)
                    {
                        return BadRequest(new JsonResult(new { msg = "餘額不足" }));
                    }
                }
                catch (Exception)
                {
                    conn.Close();
                    return BadRequest(new JsonResult(new { msg = "發生錯誤，交易中止" }));
                }

                //進行trans
                string sql = @"UPDATE Products 
                SET UnitPrice=(UnitPrice+@Money) 
                WHERE ProductID=@ProductID";
                //資料
                var datas = new[]
                {
                    new{ProductID = vm.BuyerID,Money = -vm.Money},
                    new{ProductID = vm.SellerID,Money = vm.Money}
                };
                try
                {
                    using (var transactionScope = new TransactionScope())
                    {
                        conn.Execute(sql, datas);
                        transactionScope.Complete();
                        conn.Close();
                        return new JsonResult(new { msg = "交易成功" });
                    }
                }
                catch (Exception)
                {
                    conn.Close();
                    return BadRequest(new JsonResult(new { msg = "發生錯誤，交易中止" }));
                }
            }
        }
        
        //using (var conn = new SqlConnection("Server=.;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=true;"))
        [HttpPost]
        [Route("trade2")]
        public IActionResult Trade2([FromBody] TradeViewmodel vm)
        {
            //using block之外會斷開連線
            using (var conn = _connfactory.CreateConnection())
            {
                //餘額不足判斷
                try
                {
                    string sqlcheck = @"SELECT UnitPrice FROM Products WHERE ProductID=@ProductID";
                    var para = new DynamicParameters();
                    para.Add("ProductID", vm.BuyerID);
                    var money = conn.QueryFirst<decimal>(sqlcheck, para);
                    if (money < vm.Money)
                    {
                        return BadRequest(new JsonResult(new { msg = "餘額不足" }));
                    }
                }
                catch (Exception)
                {
                    conn.Close();
                    return BadRequest(new JsonResult(new { msg = "發生錯誤，交易中止" }));
                }

                //進行trans
                string sql = @"UPDATE Products 
                SET UnitPrice=(UnitPrice+@Money) 
                WHERE ProductID=@ProductID";
                //資料
                var datas = new[]
                {
                    new{ProductID = vm.BuyerID,Money = -vm.Money},
                    new{ProductID = vm.SellerID,Money = vm.Money}
                };
                try
                {
                    //沒有dapper幫忙開連線, BeginTransaction之前要自己開
                    conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            conn.Execute(sql, datas, transaction: trans);
                            trans.Commit();
                            return new JsonResult(new { msg = "交易成功" });
                        }
                        catch (Exception)
                        {
                            trans.Rollback();
                            throw;
                        }
                    }

                }
                catch (Exception)
                {
                    throw;
                    //return BadRequest(new JsonResult(new { msg = "發生錯誤，交易中止" }));
                }
            }
        }
    }

    public class TradeViewmodel
    {
        public int BuyerID { get; set; }
        public int SellerID { get; set; }
        public decimal Money { get; set; }
    }
}
