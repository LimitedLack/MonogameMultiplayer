using System;

namespace Monogame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("1) Client");
            Console.WriteLine("2) Server");

            int selection = int.Parse(Console.ReadKey().KeyChar.ToString());
            Console.Clear();

            if (selection == 1)
            {
                Client.CreateClient();
            }
            else if (selection == 2)
            {
                Server.CreateServer();
            }
        }
    }
}
