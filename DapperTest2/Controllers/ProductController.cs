using Dapper;
using DapperTest2.Connection;
using DapperTest2.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Transactions;

//使用北風資料庫
namespace DapperTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        //DI注入
        private readonly ISqlConnectionFactory _connfactory;
        public ProductController(ISqlConnectionFactory connfactory)
        {
            _connfactory = connfactory;
        }

        /// <summary>
        /// Route沒寫，會以 api?ProductID=23 傳遞，有寫會以api/23 傳遞
        /// {ProductID?}表示路徑上非必填(swagger不會理你)
        /// 參數的int? ProductID表示參數非必填
        /// </summary>
        /// <param name="ProductID"></param>
        /// <returns></returns>
        [HttpGet]
        //[Route("{ProductID?}")]
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

        /// <summary>
        /// 交易使用TransactionScope，可用於不同伺服器之間的查詢
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
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

        //直接使用連接字串的樣子
        //using (var conn = new SqlConnection("Server=.;Database=Northwind;Trusted_Connection=True;TrustServerCertificate=true;"))

        /// <summary>
        /// 交易使用BeginTransaction，只能用於同伺服器查詢，效能較好。
        /// </summary>
        /// <param name="tradeVM">API接收的資料格式</param>
        /// <returns></returns>
        [HttpPost]
        [Route("trade2")]
        public IActionResult Trade2([FromBody] TradeViewmodel tradeVM)
        {
            //using block之外會斷開連線
            using (var conn = _connfactory.CreateConnection())
            {
                //餘額不足判斷
                try
                {
                    string sqlcheck = @"SELECT UnitPrice FROM Products WHERE ProductID=@ProductID";
                    var para = new DynamicParameters();
                    para.Add("ProductID", tradeVM.BuyerID);
                    var money = conn.QueryFirst<decimal>(sqlcheck, para);
                    if (money < tradeVM.Money)
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
                    new{ProductID = tradeVM.BuyerID,Money = -tradeVM.Money},
                    new{ProductID = tradeVM.SellerID,Money = tradeVM.Money}
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
                    //throw;
                    return BadRequest(new JsonResult(new { msg = "發生錯誤，交易中止" }));
                }
            }
            //using結束後，物件消失dapper提供同時自動觸發Close()斷開連線，理論上中途不需要手動斷線
        }
    }

    public class TradeViewmodel
    {
        public int? BuyerID { get; set; }
        public int? SellerID { get; set; }
        public decimal? Money { get; set; }
        //? nullable類別表示非必填欄位
        public string? msg { get; set; }
    }
}
