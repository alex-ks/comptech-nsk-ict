using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompTech.Ict.Executor.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompTech.Ict.Executor.Controllers
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
        
        // GET api/session/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {            
            var session = _manager.GetStatusSession(id);
            return Ok(session);
        }

        // POST api/session
        [HttpPost]
        public IActionResult Post([FromBody]ComputationGraph graph)
        {
            var s = _manager.StartSession(graph);
            //validation 

            return Ok(s);
        }

        // DELETE api/session/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _manager.StopSession(id);
            return Ok();			
        }
    }
}
