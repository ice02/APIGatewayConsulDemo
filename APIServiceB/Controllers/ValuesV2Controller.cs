namespace APIServiceB.Controllers
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/v2/values")]
    [ApiController]
    public class ValuesV2Controller : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var port = Request.Host.Port;

            return new string[] { "value1", "value2", port.Value.ToString(), "Netcore 2.2" };
        }
    }
}
