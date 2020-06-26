using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Evan.Dynamic.Attributes;

namespace Evan.Dynamic.Test
{
    public class Program
    {
        public static async Task Main()
        {
            var p = new Program();

            dynamic m = DynamicObject.CreateProxy(new TestModel());
            var proxyType = (Type)m.GetType();

            m.run_proxy<Program>();
            m.Run0_1<Program>();

            // m.Run0_2(ref p); // DynamicMethod는 포인터를 지원하지 않음.
            var Run0_2 = proxyType.GetMethod("Run0_2").MakeGenericMethod(typeof(Program));
            var Run0_2_Arg = new object[] { p };
            Run0_2!.Invoke(m, Run0_2_Arg);

            m.Run1();

            m.Run2();

            await m.Run3();

            await m.Run3_1();

            Console.WriteLine(m.Run4());

            Console.WriteLine(await m.Run5());

            Console.WriteLine(await m.Run6());

            await foreach (var v in (IAsyncEnumerable<int>)m.Run7())
                Console.WriteLine($"Run7: {v}");

            await foreach (var v in await (Task<IAsyncEnumerable<int>>)m.Run7_1())
                Console.WriteLine($"Run7_1: {v}");

            await foreach (var v in (IAsyncEnumerable<int>)m.Run8())
                Console.WriteLine($"Run8: {v}");

            var console = (IConsole)m;
            console.Write("Explicit interface method test\n");
            console.WriteLine("Implicit interface method test");

            var console2 = (IConsole2)m;
            console2.Write("Explicit interface method test\n");
            console2.WriteLine("Implicit interface method test");
        }
    }

    public interface IConsole
    {
        void Write(object o);

        void WriteLine(object o);
    }

    public interface IConsole2
    {
        void Write(object o);

        void WriteLine(object o);
    }

    public class TestModel4 : IConsole, IConsole2
    {
        private readonly TestModel _object;
        
        void IConsole.Write(object o)
        {
            ((IConsole)_object).Write(o);
        }

        void IConsole2.WriteLine(object o)
        {
            _object.WriteLine(o);
        }

        void IConsole2.Write(object o)
        {
            ((IConsole2)_object).Write(o);
        }

        void IConsole.WriteLine(object o)
        {
            _object.WriteLine(o);
        }
    }

    public class TestModel : IConsole, IConsole2
    {
        [ProxyMethodName("run_proxy")]
        public void Run0<T>() where T : Program
        {
            Console.WriteLine(typeof(T));
        }

        public T Run0_1<T>()
        {
            return default;
        }

        public ref T Run0_2<T>(ref T t)
        {
            t = default;
            return ref t;
        }

        public void Run1()
        {
            Console.WriteLine("Run1");
        }

        public async void Run2()
        {
            await Task.Delay(0);
            Console.WriteLine("Run2");
        }

        public Task Run3()
        {
            Console.WriteLine("Run3");
            return Task.CompletedTask;
        }

        public async Task Run3_1()
        {
            await Task.Delay(0);
            Console.WriteLine("Run3_1");
        }

        public int Run4()
        {
            return 4;
        }

        public Task<int> Run5()
        {
            return Task.FromResult(5);
        }

        public async Task<int> Run6()
        {
            await Task.Delay(0);
            return 6;
        }

        public IAsyncEnumerable<int> Run7()
        {
            return Impl();

            static async IAsyncEnumerable<int> Impl()
            {
                await Task.Delay(0);
                yield return 7;
            }
        }

        public Task<IAsyncEnumerable<int>> Run7_1()
        {
            return Task.FromResult(Impl());

            static async IAsyncEnumerable<int> Impl()
            {
                await Task.Delay(0);
                yield return 7;
            }
        }

        public async IAsyncEnumerable<int> Run8()
        {
            await Task.Delay(0);
            yield return 8;
        }

        void IConsole.Write(object o)
        {
            Console.Write($"Console: {o}");
        }

        void IConsole2.Write(object o)
        {
            Console.Write($"Console2: {o}");
        }

        public void WriteLine(object o)
        {
            Console.WriteLine($"Console?: {o}");
        }
    }
}
