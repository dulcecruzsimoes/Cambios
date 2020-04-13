namespace Cambios.Servicos
{
    using Modelos;
    using System.Net;

    public class NetworkService
    {
        // Esta classe vai apenas ter um método, em que se verifica se existe ou não ligação à internet

        public Response CheckConnection()
        {
            // Para ver se existe ou não ligação faz-se um ping ao servidor da google

            var client = new WebClient();

            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSuccess = true

                    };
                }

            }
            catch 
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Verifique a sua ligação à internet"
                }; 
            }            
        }
    }
}
