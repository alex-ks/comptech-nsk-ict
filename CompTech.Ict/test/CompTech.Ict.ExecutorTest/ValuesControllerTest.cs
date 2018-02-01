using System;
using System.Linq;
using CompTech.Ict.Sample.Controllers;
using CompTech.Ict.Sample.Data;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CompTech.Ict.ExecutorTest
{
    public class ValuesControllerTest : IDisposable
    {
        private const string HelloWorld = "Hello, world!";
        private const string HelloXunit = "Hello, XUnit!";
        private const string HelloDotNet = "Hello, .Net!";

        private readonly ApplicationContext _context;
        private readonly ValuesController _controller;

        public ValuesControllerTest()
        {
            _context = new ApplicationContext();
            _controller = new ValuesController(_context);
        }

        [Fact]
        public void Post_AddHelloWorldAndGetAll_ResultContainsHelloWorld()
        {
            _controller.Post(HelloWorld);
            var returned = _controller.Get();

            Assert.Contains(HelloWorld, returned);
        }

        [Fact]
        public void Post_AddHelloDotNet_IdReturned()
        {
            var result = _controller.Post(HelloDotNet);
            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<int>((result as OkObjectResult).Value);
        }

        [Fact]
        public void Delete_PostHelloXunitAndDeleteById_BadRequestOnGet()
        {
            var idResult = _controller.Post(HelloXunit) as OkObjectResult;
            Assert.NotNull(idResult);

            var id = (int)idResult.Value;
            _controller.Delete(id);
            
            var result = _controller.Get(id);
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_PostHelloXunitAndDeleteById_NoXunitOnGetAll()
        {
            var idResult = _controller.Post(HelloXunit) as OkObjectResult;
            Assert.NotNull(idResult);

            var id = (int)idResult.Value;
            _controller.Delete(id);
            
            Assert.DoesNotContain(HelloXunit, _controller.Get());
        }

        public void Dispose()
        {
            _context.Values.RemoveRange(_context.Values.ToList());
        }
    }
}
