using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace CompTech.Ict.Executor
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
            string pathTemp = Path.Combine(Path.GetTempPath(), id);
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
            string[] result;
            if (files != null)
            {
                result = new string[files.Length];
                for (int i = 0; i < files.Length; i++)
                    result[i] = File.ReadAllText(files[i]);
            }
            else
                return null;
            return result;
        }
        public Executor(int n)
        {
            semaphore = new Semaphore(n, n);
            Status = ExecutorStatus.Idle;
            Loop();
        }

        public void Add(string name, string[] parameters, Action<string[]> callBack) // по одному
        {
            queue.Post(new InputParameters()
            {
                ScriptSource = name,
                Parameters = parameters,
                CallBack = callBack
            });
        }
        public void EndOfData()
        {
            queue.Complete();
        }

        private async void Loop()
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
                        try
                        {
                            OperationRun(path);
                            string[] result = FilesToOutputs(path);
                            tmp.CallBack(result);
                        }
                        catch(Exception ex)
                        {
                            Console.Error.WriteLine(ex.Message);
                            tmp.CallBack(null);
                        }
                        Directory.Delete(path, true);
                    }
                    semaphore.Release();
                };
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                Task.Run(taskingOperation);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
            }
        }

        private void OperationRun(string inputPath)
        {
            ProcessStartInfo start = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = Path.Combine(inputPath, "source.py") + " " + inputPath,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            Process process = new Process();
            process = Process.Start(start);
            process.WaitForExit();
            if (process.ExitCode != 0)
                throw new Exception($"Ошибка в исполнении скрипта: {process.ExitCode}");
          
        }
    }
}
