using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Sample.Data;
using CompTech.Ict.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompTech.Ict.Sample.Controllers
{
    [Route("api/[controller]")]
    public class SessionController : Controller
    {
        private SessionManager _manager;

        // Context will be passed via dependency injection
        public SessionController(SessionManager manager)
        {
            _manager = manager;
        }
        
        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {            
            var session = _manager.Get(id);
            return Ok(session);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]Comp_Graph graph)
        {
            var s = _manager.Create(graph);
            //validation 

            return Ok(graph);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _manager.Delete(id);
            return Ok();			
        }
    }
}
