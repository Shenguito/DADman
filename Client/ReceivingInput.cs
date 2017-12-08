using ComLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public delegate void ThrWork();

    public class ReceivingInput
    {
        private CircularBuffer buf;
        private Thread[] pool;

        public ReceivingInput()
        {
            buf = new CircularBuffer();
            pool = new Thread[3];
            for (int i = 0; i < 3; i++)
            {
                //ThreadStart, this thread may have at most 25 threads
                pool[i] = new Thread(new ThreadStart(consomeExec));
                pool[i].Start();
            }
        }

        //Add list
        public void AssyncInvoke(Movement move, ref TextBox textBox)
        {
            buf.addMove(move);
            textBox.AppendText("\r\nSubmitted action");
            Console.WriteLine("Submitted action");
        }

        private void consomeExec()
        {
            while (true)
            {
                //RETRIEVING
                Movement move= buf.retrieveMove();
            }
        }

        private class CircularBuffer
        {
            private List<Movement> listMove;
            public CircularBuffer()
            {
                listMove = new List<Movement>();
            }
            //add to list
            public void addMove(Movement move)
            {
                lock (this)
                {
                    listMove.Add(move);
                    Console.WriteLine("List added move: "+move.move);
                    Monitor.Pulse(this);
                }
            }
            //retrieve from list
            public Movement retrieveMove()
            {
                Movement move=null;
                lock (this)
                {
                    while (listMove.Count == 0)
                    {
                        Monitor.Wait(this);
                    }
                    move= listMove[0];
                    listMove.RemoveAt(0);
                    Console.WriteLine("List removed move: " + move.move);
                }
                return move;
            }
        }
    }
    
    public class Worker
    {
        private int _id;

        public Worker(int id)
        {
            _id = id;
        }
        //do work
        public void DoWorkA()
        {
            Console.WriteLine("Worker-{0}", _id);
        }
    }
    /*
    //ThrPool(thread, task);
    ThrPool tpool = new ThrPool(5, 10);
            //ThrWork work = null;
            for (int i = 0; i< 5; i++)
            {
                A a = new A(i);
    tpool.AssyncInvoke(new ThrWork(a.DoWorkA));
                B b = new B(i);
    tpool.AssyncInvoke(new ThrWork(b.DoWorkB));
            }
    Console.ReadLine();
    */
}
