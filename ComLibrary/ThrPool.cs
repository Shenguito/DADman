using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ComLibrary
{
    
    public delegate void ThrWork();

    public class ThrPool
    {
        private CircularBuffer<ThrWork> buf;
        private Thread[] pool;

        public ThrPool(int thrNum, int bufSize)
        {
            buf = new CircularBuffer<ThrWork>(bufSize);
            pool = new Thread[thrNum];
            for(int i=0; i<pool.Length; i++)
            {
                //ThreadStart
                pool[i] = new Thread(new ThreadStart(consomeExec));
                pool[i].Start();
            }
        }

        public void AssyncInvoke(ThrWork action)
        {
            buf.Produce(action);
            Console.WriteLine("Submitted action");
        }

        private void consomeExec()
        {
            while (true)
            {
                ThrWork tw = buf.Consume();
                tw();
            }
        }

        private class CircularBuffer<T>
        {
            private T[] buffer;
            private int size;
            private int busy;
            private int insertionBuffer;
            private int removalBuffer;
            public CircularBuffer(int size)
            {
                buffer = new T[size];
                this.size = size;
                busy = 0;
                insertionBuffer = 0;
                removalBuffer = 0;
            }
            public void Produce(T o)
            {
                lock (this)
                {
                    while (busy == size)
                    {
                        Monitor.Wait(this);
                    }
                    buffer[insertionBuffer] = o;
                    insertionBuffer = ++insertionBuffer % size;

                    busy++;
                    Console.WriteLine("Produce");
                    if (busy == 1)
                    {
                        Monitor.Pulse(this);
                    }
                }
            }
            public T Consume()
            {
                T o;
                lock (this)
                {
                    while (busy == 0)
                    {
                        Monitor.Wait(this);
                    }
                    o = buffer[removalBuffer];
                    buffer[removalBuffer] = default(T);
                    removalBuffer = ++removalBuffer % size;
                    busy--;
                    Console.WriteLine("Consume");
                    if (busy == size - 1)
                    {
                        Monitor.Pulse(this);
                    }
                }
                return o;
            }
            public string toString()
            {
                string s = "";
                lock (this)
                {
                    for (int i = 0; i < size; i++)
                    {
                        s += buffer[i].ToString() + " ,";
                    }
                }
                return s;
            }
        }
    }


    public class A
    {
        private int _id;

        public A(int id)
        {
            _id = id;
        }

        public void DoWorkA()
        {
            Console.WriteLine("A-{0}", _id);
        }
    }


    public class B
    {
        private int _id;

        public B(int id)
        {
            _id = id;
        }

        public void DoWorkB()
        {
            Console.WriteLine("B-{0}", _id);
        }
    }
    
}
