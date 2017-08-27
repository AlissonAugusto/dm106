using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ProjetoAlissonDM106.Models;
using System.Diagnostics;
using ProjetoAlissonDM106.br.com.correios.ws;
using ProjetoAlissonDM106.CRMClient;

namespace ProjetoAlissonDM106.Controllers
{
    [RoutePrefix("api/Pedidos")]
    public class PedidosController : ApiController
    {
        private ProjetoAlissonDM106Context db = new ProjetoAlissonDM106Context();

        // GET: api/Pedidos
        [Authorize(Roles = "ADMIN")]
        public IQueryable<Pedido> GetPedidoes()
        {
            return db.Pedidoes;
        }

        /*
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("cep")]
        public IHttpActionResult ObtemCEP()
        {
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);
            if (customer != null)
            {
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha	ao	consultar	o	CRM");
            }
        }*/

        // GET: api/Pedidos/calcularFrete?id={id}&cepDestino={cepDestino}	
        [ResponseType(typeof(string))]
        [HttpGet]
        [Route("calcularFrete")]
        [Authorize]
        public IHttpActionResult calcularFrete(int id, string cepDestino)
        {
            Pedido pedido = db.Pedidoes.Find(id);
            if (pedido == null)
            {
                return BadRequest("O pedido não existe.");
            }
            else if (User.Identity.Name.Equals(pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                if (pedido.status.Equals("Novo"))
                {
                    string frete;
                    decimal larguraTotal = 0;
                    decimal comprimentoTotal = 0;
                    decimal alturaTotal = 0;
                    decimal diametroTotal = 0;

                    CalcPrecoPrazoWS correios = new CalcPrecoPrazoWS();
                    foreach (ItemPedido itemPedido in pedido.OrderItems)
                    {
                        pedido.pesoTotal += itemPedido.Produtos.peso;
                        pedido.precoTotal += itemPedido.Produtos.preco;
                        larguraTotal += itemPedido.Produtos.largura;
                        comprimentoTotal = itemPedido.Produtos.comprimento;
                        alturaTotal = itemPedido.Produtos.altura;
                        diametroTotal = itemPedido.Produtos.diamentro;
                    }
                    //Foi usado um cep de origem fixo pois não consegui utilizar o serviço CRM
                    cResultado resultado = correios.CalcPrecoPrazo("", "", "40010", "37540000", cepDestino, pedido.pesoTotal.ToString(), 1, comprimentoTotal, alturaTotal, larguraTotal, diametroTotal, "N", pedido.precoTotal, "S");
                    pedido.precoFrete = decimal.Parse(resultado.Servicos[0].Valor);
                    if (resultado.Servicos[0].Erro.Equals("0"))
                    {
                        frete = "Valor	do	frete:	" + resultado.Servicos[0].Valor + "	-	Prazo	de	entrega:	" + resultado.Servicos[0].PrazoEntrega + "	dia(s)";
                        return Ok(frete);
                    }
                    else
                    {
                        return BadRequest("Código	do	erro:	" + resultado.Servicos[0].Erro + "-" + resultado.Servicos[0].MsgErro);
                    }
                }
                else
                {
                    return BadRequest("Pedido com status diferente de 'novo'.");
                }
            }
            else
            {
                return BadRequest("Usuario não autorizado.");
            }
        }

        // GET: api/Pedidos/byEmail? email = { email }
        [ResponseType(typeof(Pedido))]
        [HttpGet]
        [Route("byEmail")]
        [Authorize]
        public IHttpActionResult GetPedidoByEmail(string email)
        {
            var pedidos = db.Pedidoes.Where(p => p.userEmail == email);
            if (pedidos == null)
            {
                return NotFound();
            }
            else if (User.Identity.Name.Equals(email) || User.IsInRole("ADMIN"))
            {
                return Ok(pedidos);
            }
            else
            {
                return BadRequest("Usuario não autorizado.");
            }
        }

        //GET: api/Pedidos/fecharPedido? id = { id }
        [ResponseType(typeof(Pedido))]
        [HttpGet]
        [Route("fecharPedido")]
        [Authorize]
        public IHttpActionResult fecharPedido(int id)
        {
            Pedido pedido = db.Pedidoes.Find(id);
            if (pedido == null)
            {
                return BadRequest("O pedido não existe.");
            }
            else if (User.Identity.Name.Equals(pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                pedido.status = "fechado";
                db.Entry(pedido).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PedidoExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Ok(pedido);
            }
            else
            {
                return BadRequest("Usuario não autorizado.");
            }
        }

        // GET: api/Pedidos/5
        [ResponseType(typeof(Pedido))]
        [Authorize]
        public IHttpActionResult GetPedido(int id)
        {
            Pedido pedido = db.Pedidoes.Find(id);
            if (pedido == null)
            {
                return BadRequest("Pedido não encontrado.");
            }
            else if (User.Identity.Name.Equals(pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                return Ok(pedido);
            }
            else
            {
                return BadRequest("Usuario não autorizado.");
            }

        }

        // PUT: api/Pedidos/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutPedido(int id, Pedido pedido)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != pedido.Id)
            {
                return BadRequest();
            }

            db.Entry(pedido).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Pedidos
        [ResponseType(typeof(Pedido))]
        [Authorize]
        public IHttpActionResult PostPedido(Pedido pedido)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            pedido.status = "novo";
            pedido.pesoTotal = 0;
            pedido.precoFrete = 0;
            pedido.precoTotal = 0;
            pedido.dataPedido = DateTime.Today.ToString("dd/MM/yyyy");
            db.Pedidoes.Add(pedido);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = pedido.Id }, pedido);
        }

        // DELETE: api/Pedidos/5
        [ResponseType(typeof(Pedido))]
        [Authorize]
        public IHttpActionResult DeletePedido(int id)
        {
            Pedido pedido = db.Pedidoes.Find(id);
            if (pedido == null)
            {
                return BadRequest("O pedido não existe.");
            }
            else if (User.Identity.Name.Equals(pedido.userEmail) || User.IsInRole("ADMIN"))
            {
                db.Pedidoes.Remove(pedido);
                db.SaveChanges();

                return Ok(pedido);
            }
            else
            {
                return BadRequest("Usuario não autorizado.");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PedidoExists(int id)
        {
            return db.Pedidoes.Count(e => e.Id == id) > 0;
        }
    }
}