using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetoAlissonDM106.Models
{
    public class Pedido
    {
        public Pedido()
        {
            this.OrderItems = new HashSet<ItemPedido>();
        }

        public int Id { get; set; }

        public string userEmail { get; set; }

        public string dataPedido { get; set; }

        public string dataEntrega { get; set; }

        public string status { get; set; }

        public decimal precoTotal { get; set; }

        public decimal pesoTotal { get; set; }

        public decimal precoFrete { get; set; }

        public virtual ICollection<ItemPedido> OrderItems { get; set; }
    }
}