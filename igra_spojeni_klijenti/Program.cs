using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SynchronousSocketListener
{

    
    public static string data = null;

    //metoda koja sluzi za spremanje soketa od klijenta 1 i klijenta 2 u listu koja se poslije prosljedjuje u dretvu
    public static void Igra(object soketi)
    {
        byte[] bytes = new Byte[1024];
        List<Socket> soketiLista = (List <Socket>)soketi;
        while (true)
        {

            soketiLista[0].Receive(bytes);
            if (BitConverter.ToInt32(bytes, 0) == 150) break;
            soketiLista[1].Send(bytes);
            soketiLista[1].Receive(bytes);
            if (BitConverter.ToInt32(bytes, 0) == 150) break;
            soketiLista[0].Send(bytes);
        }
        soketiLista[0].Shutdown(SocketShutdown.Both);
        soketiLista[1].Shutdown(SocketShutdown.Both);
        soketiLista[0].Close();
        soketiLista[1].Close();

    }
    public static void StartListening()
    {
        //polje bajtova za dohvacanje
        byte[] bytes = new Byte[1024];

        //incijaliziranje servera
        IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
        IPAddress ipAddress = ipHostInfo.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        
        //otvoren soket za prihvacanje zahtjeva preko TCP-a
        Socket listener = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

        
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            
            while (true)
            {
                Console.WriteLine("Waiting for a connection...");

                //odsluskuj soket
                Socket handler = listener.Accept();
                data = null;

                //primi bajtove iz soketa i pretvori ih u string
                int bytesRec = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    
                Console.WriteLine("Text received : {0}", data);

               
                byte[] msg = Encoding.ASCII.GetBytes(data);
                //slanje poruke klijentu 1 da je spojen
                handler.Send(msg);
                
                //----------------------------------------------------
                Console.WriteLine("Waiting for a connection...");

                //odsluskuj soket
                Socket handler2 = listener.Accept();
                data = null;

                //primi bajtove iz soketa i pretvori ih u string
                int bytesRec2 = handler2.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec2);

                Console.WriteLine("Text received : {0}", data);

                byte[] msg2 = Encoding.ASCII.GetBytes(data);

                //slanje poruke klijentu 2 da je spojen
                handler2.Send(msg2);

                //posalji bitove za redni broj
                handler.Send(BitConverter.GetBytes(1));
                handler2.Send(BitConverter.GetBytes(2));

                //stvaranje liste handlera
                List<Socket> soketiLista = new List<Socket>();
                soketiLista.Add(handler);
                soketiLista.Add(handler2);

                //dretva za posluživanje više klijenata
                Thread dretva = new Thread(new ParameterizedThreadStart(Igra));
                dretva.Start(soketiLista);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static int Main(String[] args)
    {
        StartListening();
        return 0;
    }
}
