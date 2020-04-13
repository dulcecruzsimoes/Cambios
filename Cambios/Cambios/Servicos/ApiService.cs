namespace Cambios.Servicos
{
    using Modelos;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    public class ApiService // Esta classe só vai ter um método
    {

        // Este método será um método assincrono (será criada uma thread para realizar esta tarefa)
        // Este método tem como parâmetros o endereço da api e do seu controlador
        // O méstodo devolve um objecto do tipo Response

        public async Task<Response> GetRates(string urlBase, string controller)
        {

            // Tudo que envolve comunicação com base de dados deverá estar asssegurado com um try... catch

            try
            {
                // 1º Criar uma ligação de internet
                var client = new HttpClient();

                // 2º Passar o endereço da base de dados
                client.BaseAddress = new Uri(urlBase);

                // 3º Guardar a resposta do controlador numa variavel
                var response = await client.GetAsync(controller);

                // 4º Guardar a resposta numa variável
                var result = await response.Content.ReadAsStringAsync();

                // Se tiver corrido algum erro, vamos enviar para fora um objecto do tipo Response com a propriedade IsSuccess igual a false e a Message com o valor do resultado
                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                // Se tiver corrido tudo bem vamos enviar para fora um objecto do tipo Response com as taxas

                else 
                {
                    var rates = JsonConvert.DeserializeObject<List<Rate>>(result);

                    return new Response
                    {
                        IsSuccess = true,
                        Result = rates
                    };
                }

            }
            catch (Exception ex)
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }



        }
    }
}
