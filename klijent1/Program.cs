using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace _4uNizu_konzola
{

    class Program
    {

        public int pobjednik = 0; //odreduje pobjednika
        public int potez = 1; //koji je igrac na potezu
        public int[,] pozicija2d = new int[8, 10]; //smjestaj znaka u 2d matrici
        public int[] visina_stupca = new int[10]; //za provjeru koliko je elemenata zapisano u nekom stupcu
        public string igrac1 = "0"; //simbol za prvog igraca
        public string igrac2 = "X"; //simbol za drugog igraca

        public void OcistiKonzolu()
        {
            string spejsovi = new String('\n', 5);
            Console.WriteLine(spejsovi);
        }

        public void OcistiPozadinu()
        {  //resetiranje cjelokupnog sadržaja unutar okvira na pocetak
            for (int red = 0; red < 8; red++)
            {
                visina_stupca[red] = 0;
                for (int stupac = 0; stupac < 10; stupac++)
                {
                    pozicija2d[red, stupac] = 0;
                }
            }
        }

        public void UcitajOkvir()
        { //ispis okvira igre
            Console.WriteLine("____________");
            for (int red = 0; red < 8; red++)
            {
                string trenutni_red = "";
                for (int stupac = 0; stupac < 10; stupac++)
                {
                    switch (pozicija2d[red, stupac])
                    { //stavlja X ili 0 ovisno o tome koji je igrac na potezu
                        case 1:
                            trenutni_red += igrac1;
                            break;
                        case 2:
                            trenutni_red += igrac2;
                            break;
                        default:
                            trenutni_red += " ";
                            break;
                    }
                }
                Console.WriteLine("|" + trenutni_red + "|");
            }
            Console.WriteLine("************" + Environment.NewLine);
            Console.WriteLine("*0123456789*" + Environment.NewLine);
        }

        public void DodajJedan(int igrac, int stupac)
        {  //dodaje unos igraca u odgovarajuci stupac
            visina_stupca[stupac]++;
            pozicija2d[8 - visina_stupca[stupac], stupac] = igrac;
        }

        public void SljedeciIgrac()
        { //promjena igrača
            bool promjena = false;
            if (potez == 1 && !promjena)
            {
                potez = 2;
                promjena = true;
            }
            if (potez == 2 && !promjena)
            {
                potez = 1;
                promjena = true;
            }
        }

        public bool StupacPun(int stupac)
        {  //provjerava ukoliko je odredeni stupac pun
            return (visina_stupca[stupac] >= 8);
        }

        public bool ProvjeraPobjednika()
        {
            //provjera ukoliko je neki od igraca slozio 4 u nizu
            for (int red = 0; red < 8; red += 1)
            {
                for (int stupac = 0; stupac < 10; stupac += 1)
                {
                    if (pozicija2d[red, stupac] != 0)
                    {
                        // vodoravno (lijevo-desno)
                        if (stupac <= 6)
                        {
                            if (pozicija2d[red, stupac] == pozicija2d[red, stupac + 1] && pozicija2d[red, stupac] == pozicija2d[red, stupac + 2] && pozicija2d[red, stupac] == pozicija2d[red, stupac + 3])
                            {
                                pobjednik = pozicija2d[red, stupac];
                            }
                        }
                        // okomito (gore/dolje)
                        if (red <= 4)
                        {
                            if (pozicija2d[red, stupac] == pozicija2d[red + 1, stupac] && pozicija2d[red, stupac] == pozicija2d[red + 2, stupac] && pozicija2d[red, stupac] == pozicija2d[red + 3, stupac])
                            {
                                pobjednik = pozicija2d[red, stupac];
                            }
                        }
                        //dijagonalno (dolje-desno/gore-lijevo)
                        if (red <= 4 && stupac <= 6)
                        {
                            if (pozicija2d[red, stupac] == pozicija2d[red + 1, stupac + 1] && pozicija2d[red, stupac] == pozicija2d[red + 2, stupac + 2] && pozicija2d[red, stupac] == pozicija2d[red + 3, stupac + 3])
                            {
                                pobjednik = pozicija2d[red, stupac];
                            }
                        }
                        //dijagonalno(dolje-lijevo/gore-desno)
                        if (red >= 4 && stupac <= 6)
                        {
                            if (pozicija2d[red, stupac] == pozicija2d[red - 1, stupac + 1] && pozicija2d[red, stupac] == pozicija2d[red - 2, stupac + 2] && pozicija2d[red, stupac] == pozicija2d[red - 3, stupac + 3])
                            {
                                pobjednik = pozicija2d[red, stupac];
                            }
                        }
                    }
                }
            }
            return (pobjednik != 0);
        }

        //dohvacanje poteza drugog klijenta kako bi znali na osnovu njega iscrtati polje
        static int PrimiPotezDrugog(Socket sok)
        {
            byte[] bytes = new byte[1024];
            sok.Receive(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        //slanje poteza klijenta
        static void PosaljiPotez(Socket sok, int potez)
        {
            byte[] bytes = BitConverter.GetBytes(potez);
            sok.Send(bytes);

        }
        static void Main(string[] args)
        {
            byte[] bytes = new byte[1024];
            try
            {

                //postavljanje ip adrese i porta 11000
                IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                //kreiranje TCP socketa
                Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);


                //spajanje soketa sa odredištem
                try
                {

                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    //slanje poruke serveru
                    byte[] msg2 = Encoding.ASCII.GetBytes("Igrac 1 spojen na server!!");
                    int bytesSent = sender.Send(msg2);

                    //primanje poruke od servera
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}", Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    //dohvati redni broj igraca kako bismo poslije mogli odrediti koji igrac igras
                    sender.Receive(bytes);
                    Console.WriteLine("Vi ste igrac {0}", BitConverter.ToInt32(bytes, 0));
                    int redniBroj = BitConverter.ToInt32(bytes, 0);
                    

                    Program objekt = new Program();
                    bool ponavljaj = true; //ponavljanje igre
                    objekt.OcistiKonzolu(); //pozivi funkcija
                    objekt.OcistiPozadinu();
                    objekt.potez = 1;

                    
                    while (ponavljaj)
                    {
                        
                        //određivanje igraca prema rednom broju
                        if (redniBroj == 1)
                        {
                            objekt.SljedeciIgrac();

                            objekt.UcitajOkvir();
                            Console.WriteLine("Vi ste na redu" + Environment.NewLine);
                            int unos = 800;
                            while (unos < 0 || unos > 9 || objekt.StupacPun(unos))
                            {
                                Console.WriteLine("Upisite broj: ");
                                unos = Convert.ToInt32(Console.ReadLine());

                            }

                            PosaljiPotez(sender, unos);
                            objekt.DodajJedan(objekt.potez, unos);
                            redniBroj = 2;


                        }
                        //nakon svakog poteza moze biti pobjednik pa to moramo provjeriti
                        if (objekt.ProvjeraPobjednika())
                        {
                           
                            PosaljiPotez(sender, 150);
                            objekt.UcitajOkvir();
                            break;
                        }
                        if (redniBroj == 2)
                        {
                            objekt.SljedeciIgrac();
                            objekt.UcitajOkvir();
                            Console.WriteLine("Drugi igrac je na redu" + Environment.NewLine);


                            objekt.DodajJedan(objekt.potez, PrimiPotezDrugog(sender));
                            redniBroj = 1;
                        }

                        
                        if (objekt.ProvjeraPobjednika())
                        {
                            
                            PosaljiPotez(sender, 150);
                            objekt.UcitajOkvir();
                            break;
                        }
                    }

                    //provjera pobjednika igre i ispis na zaslon
                    if (objekt.pobjednik==1) {
                        Console.WriteLine("Pobjedio je igrac 2!" + Environment.NewLine);
                        Console.ReadLine();

                    }
                    if (objekt.pobjednik == 2)
                    {
                        Console.WriteLine("Pobjedio je igrac 1!" + Environment.NewLine);
                        Console.ReadLine();
                    }
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
