using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Cfg = BlackFraude.Properties.Settings;


namespace BlackFraude {
    
    class BlackFraude {



        static List<string> url;
        static Thread kabum, terab, pichau;
        static int iteracao, iK, iT, iP;
        static Dictionary<int, string> achados;
        static List<Regra> regras;
        

        

        public static void Init() {

            url = new List<string>(Cfg.Default.urls.Split(';'));
            url.RemoveAt(url.Count-1);
            iteracao = iK = iP = iT = 0;
            achados = new Dictionary<int, string>();
            kabum = new Thread(Kabum);
            terab = new Thread(Terabyte);
            pichau = new Thread(Pichau);
            regras = Regra.Converter(Cfg.Default.regras);


        }

        public static void Config() {
            Config:
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"> Congurações\t[V]oltar\t[S]alvar\n\n-> [R]egras ({regras.Count})\n-> S[i]tes ({url.Count})\n-> [D]elay: {Cfg.Default.delay}ms\n-> [B]eepar: {Cfg.Default.beepar}\n-> [T]urbo: {Cfg.Default.modoTurbo}\n-> [P]anico: {Cfg.Default.panico}\n-> Resetar [c]onfigurações\n\n> ");
            switch (Console.ReadLine().ToLower()) {
                case "v": return;
                case "s":
                    Console.WriteLine("\n\nSalvando... ");
                    try {
                        string urlsave = "", regrassave = "";
                        foreach (string s in url) {
                            urlsave += s.Trim() + ";";
                        }
                        foreach (Regra r in regras) {
                            regrassave += r.ToString().Trim() + ";";
                        }


                        Cfg.Default.urls = urlsave;
                        Cfg.Default.regras = regrassave;

                        Cfg.Default.Save();
                    } catch (Exception e) {
                        Console.WriteLine("Erro  " + e.Message);
                        Console.ReadKey();

                    }
                    break;
                case "r":
                regras:

                    HUDRegras();
                    switch (Console.ReadLine().ToLower()) {
                        case "a":
                            Regra regra = new Regra();
                            Console.Write("\nPalavra-chave: ");
                            regra.Palavra = Console.ReadLine();
                            Console.Write("\nPreço mínimo: ");
                            regra.Preco = Convert.ToSingle(Console.ReadLine());
                            regras.Add(regra);



                            goto regras;
                        case "r":
                            Console.Write("Digite o número ou V para voltar\n> ");
                            string txt;
                            txt = Console.ReadLine().ToLower();
                            
                            
                            if (txt == "v")
                                goto regras;

                            try {
                                regras.RemoveAt(Convert.ToInt32(txt) - 1);
                            } catch {

                            }
                            
                            HUDRegras();
                            goto case "r";
                        case "v":
                            break;
                        default:
                            goto regras;

                    }

                    break;
                case "i":
                sites:
                    HUDSites();
                    switch (Console.ReadLine().ToLower()) {
                        case "a":
                            Console.Write("\nPalavra-chave: ");
                            url.Add(Console.ReadLine());
                            
                            goto sites;
                        case "r":
                            Console.Write("\nDigite o número ou V para voltar\n> ");
                            string txt = Console.ReadLine().ToLower();
                            if (txt == "v")
                                goto sites;
                            try {
                                url.RemoveAt(Convert.ToInt32(txt) - 1);
                            } catch {

                            }
                            
                            HUDSites();
                            goto case "r";
                        case "v":
                            break;
                        default:
                            goto sites;

                    }
                    break;
                case "d":
                    Console.Write("\n\nNovo delay\n> ");
                    Cfg.Default.delay = Convert.ToInt32(Console.ReadLine());
                    break;
                case "b":
                    Cfg.Default.beepar = !Cfg.Default.beepar;
                    break;
                case "t":
                    Cfg.Default.modoTurbo = !Cfg.Default.modoTurbo;
                    break;
                case "p":
                    Cfg.Default.panico = !Cfg.Default.panico;
                    break;
                case "c":
                    Cfg.Default.Reset();
                    break;
                    

            }
            
            
            Thread.Sleep(500);
            goto Config;
            void HUDSites() {
                Console.Clear();
                Console.WriteLine("> Configrações > Sites\n\n-------------------------------");
                int p = 1;
                foreach (string s in url) {
                    Console.WriteLine(p++ + ". " + s);
                }
                Console.WriteLine("-------------------------------\n" + (p-1) + " sites");
                Console.WriteLine("\n\n[A]dicionar\t[R]emover\t[V]oltar");
            }
            void HUDRegras() {
                Console.Clear();
                Console.WriteLine("> Configrações > Regras\n\n-------------------------------");
                int p = 1;
                foreach (Regra r in regras) {
                    Console.WriteLine(p++ + ". " + r.ToString());
                }
                Console.WriteLine("-------------------------------\n" + regras.Count + " regras");
                Console.WriteLine("\n\n[A]dicionar\t[R]emover\t[V]oltar");
            }
        }

        public static void HUD() {
            
            Console.Clear();

            Console.Write($"ITERAÇÃO {iteracao}\t{iK} k\t{iT} t\t{iP} i\n-----------------------------------------------\n");

            if (achados.Count == 0) {
                Console.WriteLine("Nenhum achado ainda :(");
            } else {
                achados.Values.ToList<string>().ForEach(delegate (string item){

                    Console.WriteLine(item);

                });
            }
            Console.WriteLine("-----------------------------------------------");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("Para sair aperte Enter\n"+achados.Count+" achados. Buscando...");
        }
        public static void Main() {
            Main:
            try {
                Init();
            } catch {
                Console.WriteLine("Erro ao ler arquivo de configuração, não vou dizer o que é, te vira\nQuer resetar? (S/N)\n> ");
                if (Console.ReadLine().ToLower().Equals("s")) {
                    try {
                        Cfg.Default.Reset();
                        goto Main;
                    } catch {
                        Console.WriteLine("Outro erro... me feche e delete você mesmo, é o config.ini\n");
                        _ = Console.ReadLine();
                    }
                }
                
            }

            int erros = 0;

        menu:
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("1 - Iniciar\n2 - Configurar\n0 - Sair\n\n> ");
            switch (Console.ReadLine()) {
                case "1" :
                    if(regras.Count == 0) {
                        Console.WriteLine("Não há regras, adcione-as");
                        goto case "2";
                    }
                    HUD();

                    
                    kabum.Start();
                    //terab.Start();
                    pichau.Start();

                    break;

                case "2":
                    Config();
                    goto menu;

                case "0":

                    Environment.Exit(0);
                    return;

                default:
                    Console.WriteLine("Existem somente as opções 1, 2 e 0");
                    erros++;
                    if(erros > 4) {
                        Console.Write("Você é retardado?! (S/N)\n> ");
                        _ = Console.ReadLine();
                    }
                    goto menu;
                    
                    
            }

            
            cuidado:
            Console.Read();
            if(achados.Count != 0) {
                Console.WriteLine("Há coisas achadas, vai sair mesmo? (S/N)");
                if (Console.ReadLine().ToLower() == "s")
                    Environment.Exit(0);
                else
                    goto cuidado;
            }
            return;

        }



        public static void Terabyte() {
            //commerce_columns_item_caption
            foreach (string s in url) {
                if (s.Contains("terabyteshop.com.br")) {
                    if (Cfg.Default.modoTurbo)
                        new Thread(TerabytePesquisa(s)).Start();
                    else TerabytePesquisa(s);
                }

            }

            ThreadStart TerabytePesquisa(string urlT) {
            pesquisa:
                WebClient webclient = new WebClient();
                webclient.Headers.Add("user-agent", "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36");
                webclient.Headers.Add("accept", "text/html, application/xhtml + xml, application/xml; q = 0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                webclient.Headers.Add("accept-encoding","gzip, deflate, br");
                webclient.Headers.Add("cookie", "__cfduid=d2f957cea06a13000b97d3a2aea48aed21605567797; PHPSESSID=ofogn6rvvoa94h4j4o2qdro586; _tnt=rgmfXjawvBuNofxxdzVjZwq6qLf0dWXw; _tnd=1606242765081; _tnwc=s=m|m=i|a=|d=; __cf_bm=f7b486c0e5c86568934b3cc8674118133a040111-1606243681-1800-AeXRbdjcaB9YSnkQfcSN80A+aN56Tamoj2CFWK68jxMMO8VgwLrPldo6ApmTJghC8+xwdpPcIe1XITWwKlxrHY+5GBtoOq9i1F5pzOZc33DtAu+tUE2ThfgoJ4HYI0I6kw==");
                webclient.Headers.Add("accept-language","pt-BR,pt; q=0.9, en-US; q=0.8, en; q=0.7");


                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

               



                string str = "";
                try {
                    webclient.DownloadFile("https://terabyteshop.com.br", "./test.html");
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                }

    
                //doc.UseCookies = true;
                //doc.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36";


                //var driver = new OpenQA.Selenium.Firefox.FirefoxDriver();
                //driver.Navigate().GoToUrl(urlT);
                //Console.WriteLine(str);



                var doc = new HtmlWeb();
                doc.UserAgent = "Teste";
                var down = doc.Load(urlT);
                var comments = down.DocumentNode.SelectNodes("//div[@class=\"commerce_columns_item_caption\"]");

                foreach (HtmlNode c in comments) {
                    Console.WriteLine(c.InnerHtml);
                }

                Console.WriteLine();
                Console.ReadLine();
                goto pesquisa;
            }

            

        }

        public static void Kabum() {
            
            
            for(int i = 0; i < url.Count; i++) {
                if (url[i].Contains("kabum.com.br")) {
                    if (Cfg.Default.modoTurbo)
                        new Thread(KabumPesquisa(url[i])).Start();
                    else
                        KabumPesquisa(url[i]);
                }
            }




            ThreadStart KabumPesquisa(string urlK) {
                
                pesquisa:
                WebClient webclient = new WebClient();
                webclient.Headers.Add("ContentType", "application/xhtml+xml");
                webclient.Headers.Add("user-agent", "Mozilla / 5.0(Windows; U; MSIE 9.0; Windows NT 9.0; pt - BR)");
                string[] linha = webclient.DownloadString(urlK).Split('\n');

                foreach (string l in linha) {
                    if (l.Contains("const listagemDados")) {
                        var serializer = new JavaScriptSerializer();
                        string jsonString = (l.Replace("const listagemDados = ", ""));

                        dynamic jsonObject = serializer.Deserialize<dynamic>(jsonString);


                        foreach (dynamic x in jsonObject) {
                            string nome = Convert.ToString(x["nome"]);
                            float preco = Convert.ToSingle(x["preco_desconto"]);
                            string link = "https://www.kabum.com.br/" + Convert.ToString(x["link_descricao"]);
                            if (Bate(nome, preco)) {
                                int hash = Convert.ToString(link + preco).GetHashCode();
                                achados[hash] = $"{ DateTime.Now.Hour}h {DateTime.Now.Minute}m\n{nome}\nR$ {preco}\n{link}\n";
                                HUD();
                                Beepar();
                            }
                            continue;





                        }




                    }
                }
                iteracao++;
                iK++;
                HUD();
                Thread.Sleep(Cfg.Default.delay);
                goto pesquisa;
            }
        }

        public static void Pichau() {
            

            for (int i = 0; i < url.Count; i++) {
                if (url[i].Contains("pichau.com.br")) {
                    if (Properties.Settings.Default.modoTurbo)
                        new Thread(PichauPesquisa(url[i])).Start();
                    else
                        PichauPesquisa(url[i]);
                }
            }

           

            ThreadStart PichauPesquisa(string urlP) {
                pesquisa:
                WebClient webclient = new WebClient();
                webclient.Headers.Add("ContentType", "application/xhtml+xml");
                webclient.Headers.Add("user-agent", "Mozilla / 5.0(Windows; U; MSIE 9.0; Windows NT 9.0; pt - BR)");

                string[] linha = webclient.DownloadString(urlP).Split('\n');

                foreach (string l in linha) {
                    if (l.Contains("var dlObjects =")) {
                        var serializer = new JavaScriptSerializer();
                        string jsonString = (l.Replace("var dlObjects = ", "")).Replace(";", "");
                        dynamic jsonObject = serializer.Deserialize<dynamic>(jsonString);


                        foreach (dynamic paginas in jsonObject) {

                            dynamic itens = paginas["ecommerce"]["impressions"];

                            foreach (dynamic x in itens) {
                                string nome = Convert.ToString(x["name"]);

                                float preco = Convert.ToSingle(((string)x["price"]).Replace(".", ",")) * 0.88f;
                                string posicao = Convert.ToString(x["position"]);
                                Console.WriteLine(preco);


                                if (Bate(nome, preco)) {
                                    
                                    int hash = Convert.ToString(posicao + preco).GetHashCode();
                                    achados[hash] = $"{ DateTime.Now.Hour}h {DateTime.Now.Minute}m\n{nome}\nR$ {preco}\nPosição: {posicao}\n";
                                    HUD();
                                    Beepar();
                                }




                            }



                        }




                    }
                }
                iP++;
                iteracao++;
                HUD();
                Thread.Sleep(Properties.Settings.Default.delay);
                goto pesquisa;
            }
        }
        

        private static void Beepar() {
            for (int i = 0; i < 2; i++) {
                Console.Beep(250, 500);
                Console.Beep(500, 1000);
            }
            Console.Beep(500, 100);
        }

        private static bool Bate(string produto, float valor) {
            foreach (Regra r in regras) {
                if (r.Bate(produto, valor) == true)
                    return true;

            }

            return false;
        }
    }
    
}
