using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackFraude {
    class Regra {
        private string palavra;
        public  string Palavra {
            get => palavra;
            set => palavra = value.ToLower().Trim();}
        public float Preco { get; set; }

        public Regra() {

        }

        public Regra(string palavra, float preco) {
            this.Palavra = palavra.ToLower();
            this.Preco = preco;
        }

        public bool Bate(string produto, float valor) {
            produto = produto.ToLower();
            if(valor <= this.Preco) {
                string[] tags = Palavra.Split(',');
                foreach (string t in tags) {
                    string[] tags2 = t.Split(' ');
                    if (t.Contains(",")) { //Regras com e

                        foreach (string t2 in tags2) {
                            if (!produto.Contains(t2)) {
                                break;
                            }
                        }
                        return true;
                    
                    } else {
                        if (produto.Contains(t))
                            return true;
                    }

                    
                    
                }
            }
            return false;
        }

        public override string ToString() {
            
            return Palavra + " ; " + Preco;
        }

        public static List<Regra> Converter(string regras) {
            string[] split = regras.Split(';');
            List<Regra> lista = new List<Regra>();
            for (int i = 0; i < split.Length-1; i+=2) {
                
                lista.Add(new Regra(split[i], Convert.ToSingle(split[i + 1])));
            }

            return lista;
        }
    }
}
