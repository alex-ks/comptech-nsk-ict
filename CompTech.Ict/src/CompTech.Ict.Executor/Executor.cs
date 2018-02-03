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
            string[] result = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
                result[i] = File.ReadAllText(files[i]);
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
                        OperationRun(path, tmp.CallBack);
                        string[] result = FilesToOutputs(path);
                        Console.WriteLine(result[0]);
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

            Process process = new Process();
            try
            {
                process = Process.Start(start);
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Console.Error.WriteLine("Ошибка в исполнении скрипта: {0}", process.ExitCode);
                    callBack(null);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                callBack(null);
            }
        }
    }
}
