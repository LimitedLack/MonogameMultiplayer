using System;
using System.Collections.Generic;
using System.Text;

namespace Monogame
{
    class Client
    {
        public static void CreateClient()
        {
            Console.Clear();
            Console.WriteLine("Client Creator");
            Console.WriteLine();
            Console.WriteLine("Please enter an IP to connect to (do not include port):");
            string selectedIP = Console.ReadLine();
            Console.WriteLine("Please enter the port of the server:");
            int selectedPort = int.Parse(Console.ReadLine());



            Console.Clear();
            Console.WriteLine("Client Creator");
            Console.WriteLine();
            Console.WriteLine("Selected IP: " + selectedIP);
            Console.WriteLine("Selected Port: " + selectedPort.ToString());
            Console.WriteLine();
            Console.WriteLine("1) Login as Player 1");
            Console.WriteLine("2) Login as Player 2");
            Console.WriteLine("3) Login as Viewer");

            int selection = int.Parse(Console.ReadKey().KeyChar.ToString());

            switch (selection)
            {
                case 1:
                    StartClient(selectedIP, selectedPort, 1);
                    break;
                case 2:
                    StartClient(selectedIP, selectedPort, 2);
                    break;
                case 3:
                    StartClient(selectedIP, selectedPort, 3);
                    break;
            }
        }

        public static void StartClient(string IPAddress, int port, int loginSelection)
        {
            using (var game = new Game1(IPAddress, port, loginSelection))
                game.Run();
        }
    }
}
