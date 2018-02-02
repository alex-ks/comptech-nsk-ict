using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace example
{
    public enum ExecutorStatus
    {
        Idle,
        Running,
        Stopped,
        Error
    }

    public class Executor
    {
        public ExecutorStatus Status { get; private set; } = ExecutorStatus.Idle;
        private class InputParameters
        {
            public string ScriptSource { get; set; } 
            public string[] Parameters { get; set; }
            public Action<string[]> CallBack { get; set; }
        }
        private Semaphore semaphore;
        BufferBlock<InputParameters> queue = new BufferBlock<InputParameters>();
        private string ArgumentsToFiles(string[] arguments, string scriptSource, string id)
        {
            string pathTemp = Path.Combine(Path.GetTempPath(),id);
            Directory.CreateDirectory(pathTemp); // индивидуальная папка
            int count = arguments.Length;
            for (int i = 0; i < count; i++)
            {
                string pathToArgument = Path.Combine(pathTemp, $"{i}input.txt"); 
                File.WriteAllText(pathToArgument, arguments[i]);
            }
            File.WriteAllText(Path.Combine(pathTemp, "source.py"), scriptSource);
            return pathTemp;
        }
        
        private string[] FilesToOutputs(string path)
        {
            
            string[] files = Directory.GetFiles(path, "*output.txt");
            string[] result = new string[files.Length];
            for( int i=0;i< files.Length;i++)
            {
                using (FileStream fs = File.OpenRead(files[i]))
                {
                    // преобразуем строку в байты
                    byte[] array = new byte[fs.Length];
                    // считываем данные
                    fs.Read(array, 0, array.Length);
                    // декодируем байты в строку
                    result[i] = System.Text.Encoding.Default.GetString(array);
                }

            }
            return result;
       
        }
        public Executor(int n)
        {
            Status = ExecutorStatus.Idle;
            semaphore = new Semaphore(n, n);
            Loop();
        }

        public void Add(string name, string[] parameters, Action<string[]> callBack)
        {
            queue.Post(new InputParameters()
            {
                ScriptSource = name,
                Parameters = parameters,
                CallBack = callBack
            });
        }

        public async void Loop()
        {
            Status = ExecutorStatus.Running;

            while (await queue.OutputAvailableAsync()) 
            {
                InputParameters tmp = queue.Receive(); 
                
                semaphore.WaitOne();

                Action taskingOperation = () =>
                {
                    if (tmp.CallBack != null)
                    {
                        string id = Guid.NewGuid().ToString();
                        var path = ArgumentsToFiles(tmp.Parameters, tmp.ScriptSource, id);
                        OperationRun(path, tmp.CallBack);
                        string[] result = FilesToOutputs(path);
                        tmp.CallBack(result);
                        Directory.Delete(path, true);
                    }
                    semaphore.Release();
                };
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
               Task.Run(taskingOperation);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            }
        }

        private void OperationRun(string inputPath, Action<string[]> callBack)
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = Path.Combine(inputPath, "source.py") + " " + inputPath,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };        

            try
            {
                Process process = Process.Start(start);
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception("Ошибка при выполнении скрипта");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                callBack(null);
                //Console.Error;
            }
        }
    }
}
