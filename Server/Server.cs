using ComLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //client in server
    class Client
    {
        public string nick;
        public string url;
        public IClient client;
    }

    class Server
    {

        private ArrayList playersAlive;
        private ArrayList playersDead;
        private ArrayList coins;
        private ArrayList monsters;
        private ArrayList walls;
        private ArrayList inputs;
        private int MSECROUND = 10; //game speed [communication refresh time]

        public Server()
        {

        }

        public void run()
        {
            while (true)
            {
                System.Threading.Thread.Sleep(MSECROUND);
                UpdateBoard();
                ResetInputsReceived();
            }
        }

        public void receiveInputs()
        {
            //TODO receber inputs dos clientes e atualizar parametros Inputs
        }

        private void ResetInputsReceived()
        {
            inputs = new ArrayList();
        }

        private void UpdateBoard()
        {
            Move();
            CheckDeadPlayers();
            CheckCoinsRetrieved();
            CheckGameEnd();

        }

        private void CheckGameEnd()
        {
            throw new NotImplementedException();
        }

        private void CheckCoinsRetrieved()
        {
            throw new NotImplementedException();
        }

        private void CheckDeadPlayers()
        {
            throw new NotImplementedException();
        }

        private void Move()
        {
            throw new NotImplementedException();
        }
    }
}
