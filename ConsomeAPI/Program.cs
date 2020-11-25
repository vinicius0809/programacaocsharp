using API.Models;
using ConsomeAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace ConsomeAPI
{
    class Program
    {
        private static readonly string nomePasta = ConfigurationManager.AppSettings["NomePasta"];
        private static readonly string nomeArquivoMoeda = nomePasta + "DadosMoeda.csv";
        private static readonly string nomeArquivoCotacao = nomePasta + "DadosCotacao.csv";

        static void Main(string[] args)
        {
            while (true)
            {
                var inicio = DateTime.Now;
                Console.WriteLine("Iniciando consulta na API");
                string url = @"https://localhost:44356/api/Fila";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("API retornou um objeto. Tratando...");
                        string resposta = reader.ReadToEnd();
                        APIMoeda moeda = JsonConvert.DeserializeObject<APIMoeda>(resposta);
                        DateTime dataIniApi = Convert.ToDateTime(moeda.DataInicio);
                        DateTime dataFimApi = Convert.ToDateTime(moeda.DataFim);

                        IList<Moeda> moedasArquivo = ConverteMoedasCsv(nomeArquivoMoeda);
                        IList<Moeda> moedasFiltradas = moedasArquivo
                                                        .Where(x => x.IdMoeda.Equals(moeda.Moeda)
                                                        && x.DataReferencia >= dataIniApi
                                                        && x.DataReferencia <= dataFimApi)
                                                        .ToList();

                        IList<Cotacao> cotacoesArquivo = ConverteCotacoesCsv(nomeArquivoCotacao);

                        EnumCotacao enumCotacao = (EnumCotacao)Enum.Parse(typeof(EnumCotacao), moeda.Moeda);

                        IList<Cotacao> cotacoesFiltradas = cotacoesArquivo
                                                            .Where(x => x.CodCotacao == enumCotacao
                                                             && x.DataCotacao >= dataIniApi
                                                             && x.DataCotacao <= dataFimApi)
                                                             .ToList();
                        DateTime agora = DateTime.Now;
                        string nomeArquivo = nomePasta + string.Format("Resultado_{0}{1}{2}_{3}{4}{5}.csv",
                            AdicionaZeroEsquerda(agora.Year),
                            AdicionaZeroEsquerda(agora.Month),
                            AdicionaZeroEsquerda(agora.Day),
                            AdicionaZeroEsquerda(agora.Hour),
                            AdicionaZeroEsquerda(agora.Minute),
                            AdicionaZeroEsquerda(agora.Second));

                        using (var sw = File.CreateText(nomeArquivo))
                        {
                            sw.WriteLine("ID_MOEDA;DATA_REF;VL_COTACAO");
                            foreach (var cotacao in cotacoesFiltradas.OrderBy(x => x.DataCotacao))
                            {
                                sw.WriteLine(string.Format("{0};{1};{2}", cotacao.CodCotacao.ToString(), cotacao.DataCotacao.Date.ToString("yyyy-MM-dd"), cotacao.ValorCotacao));
                            }
                        }

                    }
                    else
                    {
                        Console.WriteLine("API não retornou nenhum objeto.");
                    }
                }

                Console.WriteLine("Fim do ciclo. Tempo total: " + (DateTime.Now - inicio).TotalSeconds + " segundos. Próximo ciclo em 2 minutos...");
                Thread.Sleep(120000);
            }
        }

        private static string AdicionaZeroEsquerda(int numero)
        {
            return numero < 10 ? "0" + numero : numero.ToString();
        }

        private static IList<Cotacao> ConverteCotacoesCsv(string nomeArquivoCotacao)
        {
            var retorno = new List<Cotacao>();

            if (File.Exists(nomeArquivoCotacao))
            {
                string textoCsv = File.ReadAllText(nomeArquivoCotacao);
                string[] textoCsvDividido = textoCsv.Split('\n');

                for (int i = 1; i < textoCsvDividido.Length; i++)
                {
                    if (!string.IsNullOrEmpty(textoCsvDividido[i]))
                    {
                        string[] splitLinha = textoCsvDividido[i].Split(';');
                        string valorCotacao = splitLinha[0];
                        string codCotacao = splitLinha[1];
                        string dataReferencia = splitLinha[2].Replace("\r", string.Empty);
                        var cotacao = new Cotacao()
                        {
                            ValorCotacao = Convert.ToDouble(valorCotacao),
                            DataCotacao = Convert.ToDateTime(dataReferencia),
                            CodCotacao = (EnumCotacao)Convert.ToInt16(codCotacao)
                        };

                        retorno.Add(cotacao);
                    }
                }
            }

            return retorno;
        }

        private static IList<Moeda> ConverteMoedasCsv(string nomeArquivoMoeda)
        {
            var retorno = new List<Moeda>();

            if (File.Exists(nomeArquivoMoeda))
            {
                string textoCsv = File.ReadAllText(nomeArquivoMoeda);
                string[] textoCsvDividido = textoCsv.Split('\n');

                for (int i = 1; i < textoCsvDividido.Length; i++)
                {
                    string[] splitLinha = textoCsvDividido[i].Split(';');
                    string idMoeda = splitLinha[0];
                    string dataReferencia = splitLinha[1].Replace("\r", string.Empty);
                    var moeda = new Moeda() { IdMoeda = idMoeda, DataReferencia = Convert.ToDateTime(dataReferencia) };

                    retorno.Add(moeda);
                }

            }

            return retorno;
        }
    }
}
