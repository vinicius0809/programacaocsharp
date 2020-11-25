using API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API.Controllers
{
    public class FilaController : ApiController
    {
        private static readonly string nomePasta = ConfigurationManager.AppSettings["NomePasta"];
        private static readonly string nomeArquivoFila = nomePasta + "Fila.txt";
        // GET api/fila
        public IHttpActionResult Get()
        {
            IHttpActionResult retorno;

            try
            {
                bool arquivoFilaExiste = File.Exists(nomeArquivoFila);
                IList<APIMoeda> fila = arquivoFilaExiste
                    ? JsonConvert.DeserializeObject<IList<APIMoeda>>(File.ReadAllText(nomeArquivoFila))
                    : null;

                if (fila != null && fila.Any())
                {
                    APIMoeda itemRetorno = fila.LastOrDefault();
                    fila.Remove(itemRetorno);

                    retorno = this.MontaResposta(HttpStatusCode.OK, itemRetorno);

                    this.AtualizaArquivoFila(fila);
                }
                else
                {
                    throw new FileNotFoundException("Nenhum item na fila!");
                }
            }
            catch (FileNotFoundException ex)
            {
                retorno = this.MontaResposta(HttpStatusCode.NoContent, ex.Message);
            }
            catch (Exception ex)
            {
                retorno = this.MontaResposta(HttpStatusCode.InternalServerError, ex.Message);
            }

            return retorno;
        }

        // POST api/fila
        public IHttpActionResult Post([FromBody]IList<APIMoeda> jsonEntrada)
        {
            IHttpActionResult retorno;

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Algum campo preenchido incorretamente!");
                }

                foreach (var item in jsonEntrada)
                {
                    if (Convert.ToDateTime(item.DataFim) < Convert.ToDateTime(item.DataInicio))
                    {
                        throw new ArgumentException("Algum objeto com Data Fim menor que Data Início!");
                    }
                }

                string textoFila = File.Exists(nomeArquivoFila) ? File.ReadAllText(nomeArquivoFila) : string.Empty;

                IList<APIMoeda> fila = string.IsNullOrEmpty(textoFila)
                    ? new List<APIMoeda>()
                    : JsonConvert.DeserializeObject<IList<APIMoeda>>(textoFila);

                foreach (var item in jsonEntrada)
                {
                    fila.Add(item);
                }

                this.AtualizaArquivoFila(fila);

                retorno = this.MontaResposta(HttpStatusCode.Created, jsonEntrada.Count + " item(s) adicionado(s) com sucesso!");
            }
            catch (ArgumentException ex)
            {
                retorno = this.MontaResposta(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                retorno = this.MontaResposta(HttpStatusCode.InternalServerError, ex.Message);
            }

            return retorno;
        }

        private void AtualizaArquivoFila(IList<APIMoeda> fila)
        {
            string textoFila = JsonConvert.SerializeObject(fila);

            File.WriteAllText(nomeArquivoFila, textoFila);
        }

        private IHttpActionResult MontaResposta(HttpStatusCode status, object obj)
        {
            var response = Request.CreateResponse(status, obj);
            return ResponseMessage(response);
        }
    }
}
