using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;
using System.Threading;
using System.Linq;
using CompTech.Ict.Executor;

namespace CompTech.Ict.ExecutorTest
{
    public class ExecutorTest
    {
        string script = File.ReadAllText(@"C:\Users\Admin\Source\Repos\Winter School\CompTech.Ict\src\CompTech.Ict.Sample\first.py");
        private Executor.Executor _executor;
        public ExecutorTest()
        {
            _executor = new Executor.Executor(4);
        }

        [Fact]
        public void PassingTest()
        {
            string[,] result = new string[2, 2] { 
                                                    { "30", "60" },
                                                    { "100", "200"}
                                               };
            string[,] RecieveResult = new string[2, 2];
            // как правильно оформить?
            _executor.Add(
                    script,
                    new string[] { 10.ToString() }, 
                    (s) => 
                    {
                        RecieveResult[0, 0] = s[0];
                        RecieveResult[0, 1] = s[1];
                    }
            );
            _executor.Add(
                    script, 
                    new string[]
                    {
                        20.ToString(),
                        25.ToString()
                    },
                    (s) => 
                    {
                        RecieveResult[1, 0] = s[0];
                        RecieveResult[1, 1] = s[1];
                    }
            );
            _executor.EndOfData();
            Thread.Sleep(1000);
        
            Assert.Equal(result, RecieveResult);
        }

        [Fact]
        public void PassingTestWithTwoThread()
        {
            string[,] result = new string[10, 2];
            string[,] recieveResult = new string[10, 2];
            foreach (var i in Enumerable.Range(0, 10))
            {
                int k = i + 1;
                int outputValue = 10 + 2 * (k * 5 + k * 10);
                result[i, 0] = outputValue.ToString();
                result[i, 1] = (2 * outputValue).ToString();
                _executor.Add(
                       script,
                       new string[]
                       {
                           (k*5).ToString(),
                           (k*10).ToString()
                       },
                       (s) =>
                       {
                           recieveResult[i, 0] = s[0];
                           recieveResult[i, 1] = s[1]; 
                       }
                       
                );
            }

            _executor.EndOfData();
            Thread.Sleep(5000);

            Assert.Equal(result, recieveResult);
        }
    }
}
