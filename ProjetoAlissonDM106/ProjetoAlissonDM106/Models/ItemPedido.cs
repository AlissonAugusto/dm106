using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoAlissonDM106.Models
{
    public class ItemPedido
    {
        public int Id { get; set; }

        public int quantidadeProdutos { get; set; }

        //	Foreign	Key
        public int ProdutosId { get; set; }

        public int PedidoId { get; set; }

        //	Navigation	property
        public virtual Produtos Produtos { get; set; }
    }
}