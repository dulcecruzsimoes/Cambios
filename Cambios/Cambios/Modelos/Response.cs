namespace Cambios.Modelos
{
    // A classe Response tem como objectivo a criação de um modelo de dados que me guarda respostas
    public class Response
    {
        // Propriedades

        public bool IsSuccess { get; set; }

        public string Message { get; set; }

        public object Result { get; set; }
    }
}
