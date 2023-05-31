using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Web_Application_Paypal.Models.Paypal_Order;
using Web_Application_Paypal.Models.Paypal_Solicitudes;
using Amount = Web_Application_Paypal.Models.Paypal_Order.Amount;
using Breakdown = Web_Application_Paypal.Models.Paypal_Order.Breakdown;
using Item = Web_Application_Paypal.Models.Paypal_Order.Item;
using ItemTotal = Web_Application_Paypal.Models.Paypal_Order.ItemTotal;
using PurchaseUnit = Web_Application_Paypal.Models.Paypal_Order.PurchaseUnit;
using UnitAmount = Web_Application_Paypal.Models.Paypal_Order.UnitAmount;

namespace Web_Application_Paypal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }





        public async  Task<ActionResult> About()
        {
            string token = Request.QueryString["token"];

            ViewBag.Message = "Your application description page.";
            ViewData["Status"] = true;

            bool status = false;
            string respuesta = string.Empty;
            using (var cliente = new HttpClient())
            {
                string user = "AT6MFYD64Ivz_qiZBW5gb_vZiJd049hownBVRru97LLCxyVuXs1ckyr3Wlylzf7GXC2He2Udp5qzo2fZ";   //cliente id
                string clave = "EEeg4gAFTeh6Tl_OGRLeFOFNEr7-BR99usYcQp-jRnaCazkIOwj_knCp77LcEq_GYAuNZIkmZ2b0LHyT";  //secret key

                cliente.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");  //base de la url (url de prueba)

                var authToken = Encoding.ASCII.GetBytes($"{user}:{clave}");    //definir la autenticacion usario mas clave
                cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken)); //agregar al header el token de autenticacion    

                var data = new StringContent("{}", Encoding.UTF8, "application/json");

                HttpResponseMessage response = await cliente.PostAsync($"/v2/checkout/orders/{token}/capture", data);

                status = response.IsSuccessStatusCode;
                ViewData["Status"] = status;
                if (status)
                {
                    respuesta = response.Content.ReadAsStringAsync().Result;
                    PaypalSolicitud objecto = JsonConvert.DeserializeObject<PaypalSolicitud>(respuesta);
                    ViewData["IdTransaccion"] = objecto.purchase_units[0].payments.captures[0].id;
                }
            }
            return View();
        }





        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }




        #region enviar orden a paypal 
        [HttpPost]
        public async Task<JsonResult> Paypal(string nombre, string precio)
        {
            nombre = nombre.Trim();
            precio = precio.Trim();
            PurchaseUnit push = new PurchaseUnit();

            bool status = false;
            string respuesta = string.Empty;

            using (var cliente = new HttpClient())
            {
                string user = "AT6MFYD64Ivz_qiZBW5gb_vZiJd049hownBVRru97LLCxyVuXs1ckyr3Wlylzf7GXC2He2Udp5qzo2fZ";   //cliente id
                string clave = "EEeg4gAFTeh6Tl_OGRLeFOFNEr7-BR99usYcQp-jRnaCazkIOwj_knCp77LcEq_GYAuNZIkmZ2b0LHyT";  //secret key

                cliente.BaseAddress = new Uri("https://api-m.sandbox.paypal.com");  //base de la url (url de prueba)

                var authToken = Encoding.ASCII.GetBytes($"{user}:{clave}");    //definir la autenticacion usario mas clave
                cliente.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken)); //agregar al header el token de autenticacion    



                //objecto que se enviara en forma de json
                PaypalOrder order = new PaypalOrder
                {
                    intent = "CAPTURE",
                    purchase_units = new List<PurchaseUnit>
                    {
                            new PurchaseUnit
                            {
                                items = new List<Item>
                                {
                                    new Item
                                    {
                                        name = nombre,
                                        description = nombre,
                                        quantity = "1",
                                        unit_amount = new UnitAmount
                                        {
                                            currency_code = "USD",
                                            value = precio
                                        }
                                    },
                                   
                                },
                    amount = new Amount
                    {
                        currency_code = "USD",
                        value = precio,
                        breakdown = new Breakdown
                        {
                            item_total = new ItemTotal
                            {
                                currency_code = "USD",
                                value = precio
                            }
                        }
                    }
                    }
                    },
                    application_context = new ApplicationContext
                    {
                        return_url = "https://localhost:44336/Home/About",  //cuando se aprobo la solicitud
                        cancel_url = "https://localhost:44336/"             //cuando cancela la operacion
                    }   
                };



                var json = JsonConvert.SerializeObject(order);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await cliente.PostAsync("/v2/checkout/orders", data);

                status = response.IsSuccessStatusCode;

                if (status) 
                {
                    respuesta = response.Content.ReadAsStringAsync().Result;

                }

                return Json(new { status = status, respuesta = respuesta }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
}