using Dapper;
using DapperTest2.Model;
using DapperTest2.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace DapperTest2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerTwoController : ControllerBase
    {
        private readonly ICustomersRepository _repository;

        public CustomerTwoController(ICustomersRepository repository)
        {
            _repository = repository;
        }


        [HttpGet]
        [Route("{CustomerID?}")]
        async public Task<IActionResult> GET(string? CustomerID)
        {
            try
            {
                return Ok(new ResponseMessage()
                {
                    Status = "success",
                    Message = "已回傳資料",
                    Data = await _repository.Get(CustomerID)
                });
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
            int result = await _repository.Post(c);
            Console.WriteLine(result);

            switch (result)
            {
                case 1:
                    return Created(Url.Content("~/api/[controller]"), new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已新增"
                    });
                case -1:
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料新增失敗"
                    });
                default:
                    return NoContent();
            }
        }

        [HttpPut]
        async public Task<IActionResult> PUT([FromBody] Customers c)
        {
            var result = await _repository.Put(c);
            Console.WriteLine(result);

            switch (result)
            {
                case 1:
                    return Ok(new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已修改"
                    });
                case 0:
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "找不到資料，沒有資料被修改"
                    });
                case -1:
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料新增失敗"
                    });
                default:
                    return NoContent();
            }
        }

        [HttpDelete]
        async public Task<IActionResult> DELETE([FromBody] string CustomerID)
        {
            var result = await _repository.Delete(CustomerID);
            Console.WriteLine(result);

            switch (result)
            {
                case 1:
                    return Ok(new ResponseMessage()
                    {
                        Status = "success",
                        Message = "資料已刪除"
                    });
                case 0:
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "找不到資料，沒有資料被刪除"
                    });
                case -1:
                    return BadRequest(new ResponseMessage()
                    {
                        Status = "error",
                        Message = "發生錯誤，資料刪除失敗"
                    });
                default:
                    return NoContent();
            }
        }
    }
}