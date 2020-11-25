using System;

namespace ConsomeAPI.Models
{
    public class Cotacao
    {
        public double ValorCotacao { get; set; }

        public EnumCotacao CodCotacao { get; set; }

        public DateTime DataCotacao { get; set; }
    }
}
