using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;

namespace CompTech.Ict.Sample
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
            public byte[] ScriptSource { get; set; }
            public string[] Parameters { get; set; }
            public Action<string[]> CallBack { get; set; }
        }
        private Semaphore semaphore;
        BufferBlock<InputParameters> queue = new BufferBlock<InputParameters>();
        private string ArgumentsToFiles(string[] arguments, byte[] scriptSource, string id)
        {
            string pathTemp = Path.GetTempPath() + id;
            Directory.CreateDirectory(pathTemp); // индивидуальная папка
            int count = arguments.Length;
            for (int i = 0; i < count; i++)
            {
                string pathToArgument = pathTemp + "\\" + i.ToString() + "input.txt";
                byte[] array = System.Text.Encoding.Default.GetBytes(arguments[i]);
                using (FileStream fs = new FileStream(pathToArgument, FileMode.Create))
                    fs.Write(array, 0, array.Length);
            }
            using (FileStream fs = new FileStream(pathTemp + "\\" + "source.py", FileMode.Create))
                fs.Write(scriptSource, 0, scriptSource.Length);

            return pathTemp;
        }

        private string[] FilesToArguments(string path)
        {

            string[] files = Directory.GetFiles(path, "*output.txt");
            string[] result = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
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
            semaphore = new Semaphore(n, n);
            Status = ExecutorStatus.Idle;
        }

        public void Add(byte[] name, string[] parameters, Action<string[]> callBack) // по одному
        {
            queue.Post(new InputParameters()
            {
                ScriptSource = name,
                Parameters = parameters,
                CallBack = callBack
            });
        }

        public async Task Loop()
        {
            Status = ExecutorStatus.Running;

            while (true)
            {

                while (await queue.OutputAvailableAsync())
                {
                    InputParameters tmp = queue.Receive();
                    string id = Guid.NewGuid().ToString();
                    semaphore.WaitOne();

                    Action taskingOperation = () =>
                    {
                        var path = ArgumentsToFiles(tmp.Parameters, tmp.ScriptSource, id);
                        OperationRun(path, tmp.CallBack);
                        string[] result = FilesToArguments(path);
                        tmp.CallBack(result);
                        Directory.Delete(path, true);
                        semaphore.Release();
                    };
#pragma warning disable CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                    Task.Run(taskingOperation);
#pragma warning restore CS4014 // Так как этот вызов не ожидается, выполнение существующего метода продолжается до завершения вызова
                }


            }


        }

        private void OperationRun(string inputPath, Action<string[]> callBack)
        {
            ProcessStartInfo start = new ProcessStartInfo("python");
            start.Arguments = inputPath + "\\source.py " + inputPath;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            string tmpResult = "";
            try
            {
                Process process = Process.Start(start);
                using (StreamReader reader = process.StandardOutput)
                    tmpResult = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                callBack(null);
            }
        }

    }
}


