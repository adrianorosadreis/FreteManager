namespace FreteManager.Models
{
    public class FreteModels
    {
        public class PacoteFrete
        {
            public decimal Altura { get; set; }
            public decimal Largura { get; set; }
            public decimal Comprimento { get; set; }
            public decimal Peso { get; set; }
            public int Quantidade { get; set; } = 1;
        }

        public class ParametrosFrete
        {
            public string CepOrigem { get; set; }
            public string CepDestino { get; set; }
            public decimal ValorDeclarado { get; set; }
            public List<PacoteFrete> Pacotes { get; set; } = new List<PacoteFrete>();
        }
    }
}
