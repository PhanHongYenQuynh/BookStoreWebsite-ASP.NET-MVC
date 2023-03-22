using BookGrotto.Models;
using BookGrotto.Models.EF;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Management;
using System.Web.Mvc;
using PayPal.Api;
using CKFinder.Connector;
using BookGrotto.VNPay;
using Util = BookGrotto.VNPay.Util;
using System.ComponentModel.DataAnnotations;
using Microsoft.Ajax.Utilities;
using BookGrotto.Momo;
using Newtonsoft.Json.Linq;
using System.Security.Policy;

namespace BookGrotto.Controllers
{
    class GetInfo
    {
        public static string Email;
        public static Models.EF.Order OrderInfo;
    }
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: ShoppingCart
        public ActionResult Index()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }

        public ActionResult CheckOut()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                ViewBag.CheckCart = cart;
            }
            return View();
        }
        public ActionResult CheckOutSuccess(OrderViewModel req)
        {

            return View();
        }

        public ActionResult Partial_Item_ThanhToan()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult Partial_Item_Cart()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null && cart.Items.Any())
            {
                return PartialView(cart.Items);
            }
            return PartialView();
        }
        public ActionResult ShowCount()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (cart != null)
            {
                return Json(new { Count = cart.Items.Count }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Partial_CheckOut()
        {
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut(OrderViewModel req)
        {

            var code = new { Success = false, Code = -1 };
            if (ModelState.IsValid)
            {
                ShoppingCart cart = (ShoppingCart)Session["cart"];
                if (cart != null)
                {
                    
                    var order = new BookGrotto.Models.EF.Order();
                    order.CustomerName = req.CustomerName;
                    order.Phone = req.Phone;
                    order.Address = req.Address;
                    order.Email = req.Email;
                    cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        Price = x.Price,
                    }));
                    order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
                    order.TypePayment = req.TypePayment;
                    order.CreatedDate = DateTime.Now;
                    order.ModifiedDate = DateTime.Now;
                    order.CreatedBy = req.Phone;
                    GetInfo.Email = req.Email;
                    GetInfo.OrderInfo = order;
                    Random rd = new Random();
                    order.Code = "DH" + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9) + rd.Next(0, 9);
                    // gui mail khach hang

                    if (order.TypePayment == 3)
                    {
                        return RedirectToAction("PaymentWithPaypal");
                        //tạo đơn hàng Paypal
                    }
                    if (order.TypePayment == 4)
                    {
                        return RedirectToAction("PaymentVNPay");
                        //tạo hàng VNPay
                    }
                    if (order.TypePayment == 5)
                    {
                        return RedirectToAction("PaymentMomo");
                        //tạo hàng VNPay
                    }
                    SendMail(GetInfo.Email, cart, order);
                    return RedirectToAction("CheckOutSuccess");
                }
            }
            return Json(code);
        }
        private void SendMail(string email, ShoppingCart cart, Models.EF.Order order)
        {
            List<Product> products = db.Products.ToList();
            List<OrderDetail> orderDetails = order.OrderDetails.ToList();
            foreach (var itemProduct in products)
            {
                foreach (var itemOrderDetail in orderDetails)
                {
                    if (itemProduct.Id == itemOrderDetail.ProductId)
                    {
                        itemProduct.Quantity -= itemOrderDetail.Quantity;
                        db.Products.Attach(itemProduct);
                        db.Entry(itemProduct).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            db.Orders.Add(order);
            db.SaveChanges();
            var strSanPham = "";
            var thanhtien = decimal.Zero;
            var TongTien = decimal.Zero;
            foreach (var sp in cart.Items)
            {
                strSanPham += "<tr>";
                strSanPham += "<td>" + sp.ProductName + "</td>";
                strSanPham += "<td>" + sp.Quantity + "</td>";
                strSanPham += "<td>" + BookGrotto.Common.Common.FormatNumber(sp.TotalPrice) + "</td>";
                strSanPham += "</tr>";
                thanhtien += sp.Quantity * sp.Price;
            }
            TongTien = thanhtien;
            string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
            contentCustomer = contentCustomer.Replace("{{MaDon}}", order.Code);
            contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
            contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
            contentCustomer = contentCustomer.Replace("{{TenKhachHang}}","Tên Người Nhận: " + order.CustomerName);
            contentCustomer = contentCustomer.Replace("{{Phone}}", "Số Điện Thoại: " + order.Phone);
            contentCustomer = contentCustomer.Replace("{{Email}}", "Email: " + email);
            contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", "Địa Chỉ Nhận Hàng: " + order.Address);
            if (order.TypePayment == 1)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: COD");
            }
            if (order.TypePayment == 2)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Chuyển Khoản");
            }
            if (order.TypePayment == 3)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: PayPal");
            }
            if (order.TypePayment == 4)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: VNPay");
            }
            if (order.TypePayment == 5)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Momo");
            }
            contentCustomer = contentCustomer.Replace("{{ThanhTien}}", BookGrotto.Common.Common.FormatNumber(thanhtien, 0));
            contentCustomer = contentCustomer.Replace("{{TongTien}}", BookGrotto.Common.Common.FormatNumber(TongTien, 0));
            BookGrotto.Common.Common.SendMail("BookGrotto", "Đơn hàng #" + order.Code, contentCustomer.ToString(), email);

            string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
            contentAdmin = contentAdmin.Replace("{{MaDon}}", order.Code);
            contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
            contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
            contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", "Tên Người Nhận: " + order.CustomerName);
            contentAdmin = contentAdmin.Replace("{{Phone}}", "Số Điện Thoại: " + order.Phone);
            contentAdmin = contentAdmin.Replace("{{Email}}", "Emaili: " + email);
            contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", "Địa Chỉ Nhận Hàng: " + order.Address);
            if (order.TypePayment == 1)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: COD");
            }
            if (order.TypePayment == 2)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Chuyển Khoản");
            }
            if (order.TypePayment == 3)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: PayPal");
            }
            if (order.TypePayment == 4)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: VNPay");
            }
            if (order.TypePayment == 5)
            {
                contentCustomer = contentCustomer.Replace("{{HinhThucThanhToan}}", "Hình Thức Thanh Toán: Momo");
            }
            contentAdmin = contentAdmin.Replace("{{ThanhTien}}", BookGrotto.Common.Common.FormatNumber(thanhtien, 0));
            contentAdmin = contentAdmin.Replace("{{TongTien}}", BookGrotto.Common.Common.FormatNumber(TongTien, 0));
            BookGrotto.Common.Common.SendMail("BookGrotto", "Đơn hàng mới #" + order.Code, contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
            cart.ClearCart();
        }
        public ActionResult PaymentMomo()
        {
            var order = new BookGrotto.Models.EF.Order();
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
            }));
            order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
            //request params need to request to MoMo system
            string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
            string partnerCode = "MOMOJMUD20220907";
            string accessKey = "4P1sX4jWK4RhExaX";
            string serectkey = "FcnI5hgWY9fkaf5rNRNrR8Ugrq7LaRCw";
            string orderInfo = "Thanh toán đơn hàng - BookGrotto";
            string returnUrl = "https://localhost:44303/ShoppingCart/ConfirmPaymentClient";
            string notifyurl = "https://dashboard.ngrok.com/events/subscriptions"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

            string amount = ((int)order.TotalAmount).ToString();
            string orderid = DateTime.Now.Ticks.ToString();//mã đơn hàng
            string requestId = DateTime.Now.Ticks.ToString();
            string extraData = "";

            //Before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;

            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);

            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };

            string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

            JObject jmessage = JObject.Parse(responseFromMomo);
            return Redirect(jmessage.GetValue("payUrl").ToString());
        }

        //Khi thanh toán xong ở cổng thanh toán Momo, Momo sẽ trả về một số thông tin, trong đó có errorCode để check thông tin thanh toán
        //errorCode = 0 : thanh toán thành công (Request.QueryString["errorCode"])
        //Tham khảo bảng mã lỗi tại: https://developers.momo.vn/#/docs/aio/?id=b%e1%ba%a3ng-m%c3%a3-l%e1%bb%97i
        public ActionResult ConfirmPaymentClient(Result result)
        {
            //lấy kết quả Momo trả về và hiển thị thông báo cho người dùng (có thể lấy dữ liệu ở đây cập nhật xuống db)
            string rMessage = result.message;
            string rOrderId = result.orderId;
            string rErrorCode = result.errorCode;
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            if (rErrorCode == "0")
            {

                //Thanh toán thành công
                SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
                return View("CheckOutSuccess");
            }
            else
            {
                return View("CheckOutFailure");
            }
        }
        [HttpPost]
        public ActionResult AddToCart(int id, int quantity)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            var db = new ApplicationDbContext();
            var checkProduct = db.Products.FirstOrDefault(x => x.Id == id);
            if (checkProduct != null)
            {
                ShoppingCart cart = (ShoppingCart)Session["Cart"];
                if (cart == null)
                {
                    cart = new ShoppingCart();
                }
                ShoppingCartItem item = new ShoppingCartItem
                {
                    ProductId = checkProduct.Id,
                    ProductName = checkProduct.Title,
                    CategoryName = checkProduct.ProductCategory.Title,
                    Alias = checkProduct.Alias,
                    Quantity = quantity
                };
                if (checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault) != null)
                {
                    item.ProductImg = checkProduct.ProductImage.FirstOrDefault(x => x.IsDefault).Image;
                }
                item.Price = checkProduct.Price;
                if (checkProduct.PriceScale > 0)
                {
                    item.Price = (decimal)checkProduct.PriceScale;
                }
                item.TotalPrice = item.Quantity * item.Price;
                cart.AddToCart(item, quantity);
                Session["Cart"] = cart;
                code = new { Success = true, msg = "Thêm sản phẩm thành công!", code = 1, Count = cart.Items.Count };
            }
            return Json(code);
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1, Count = 0 };
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                var checkProduct = cart.Items.FirstOrDefault(x => x.ProductId == id);
                if (checkProduct != null)
                {
                    cart.Remove(id);
                    code = new { Success = true, msg = "", code = 1, Count = cart.Items.Count };
                }
            }
            return Json(code);
        }

        [HttpPost]
        public ActionResult DeleteAll()
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.ClearCart();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            ShoppingCart cart = (ShoppingCart)Session["Cart"];
            if (cart != null)
            {
                cart.UpdateQuantity(id, quantity);
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        // Work with Paypal Payment
        private Payment payment;

        //Create a payment using an APIContext
        private Payment CreatedPayment(APIContext apiContext, string redirectUrl)
        {
            var listItems = new ItemList() { items = new List<Item>() };
            ShoppingCart shoppingCart = Session["cart"] as ShoppingCart;

            List<ShoppingCartItem> listCarts = shoppingCart.Items;

            foreach (var cart in listCarts)
            {
                listItems.items.Add(new Item()
                {
                    name = cart.ProductName,
                    currency = "USD",
                    price = cart.Price.ToString(),
                    quantity = cart.Quantity.ToString(),
                    sku = "sku"
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            //Do the configuration RedirectURLS here with redirectURLs object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl,
                return_url = redirectUrl,
            };

            //Create details object
            var details = new Details()
            {
                tax = "1",
                shipping = "2",
                subtotal = listCarts.Sum(x => x.Quantity * x.Price).ToString()
            };

            // Create amount object
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping) + Convert.ToDouble(details.subtotal)).ToString(),//tax + shipping + subtotal
                details = details
            };

            //Create transaction
            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Grotto Testing transaction description",
                invoice_number = Convert.ToString((new Random()).Next(1000000000)),
                amount = amount,
                item_list = listItems
            });

            payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            return payment.Create(apiContext);
        }

        //Create ExecutePayment method
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId,
            };
            payment = new Payment() { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        //Create PaymentWithPayPal method
        public ActionResult PaymentWithPaypal()
        {
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            //Gettings context from the paypal bases on clientId and secret for payment
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/ShoppingCart/PaymentWithPaypal?";
                    var guid = Convert.ToString((new Random()).Next(1000000000));
                    var createdPayment = CreatedPayment(apiContext, baseURI + "guid=" + guid);

                    //Get links returned from paypal response to create call funciton
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = string.Empty;

                    while (links.MoveNext())
                    {
                        Links link = links.Current;
                        if (link.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            paypalRedirectUrl = link.href;
                        }
                    }
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // this one will be executed when we have received all the payment params from previous call
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        Session.Remove("cart");
                        return View("CheckOutFailure");

                    }
                }
            }
            catch (Exception ex)
            {
                PaypalLogger.Log("Error: " + ex.Message);
                Session.Remove("cart");
                return View("CheckOutFailure");
                ;
            }
            Session.Remove("cart");
            SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
            return View("CheckOutSuccess");


        }
        public ActionResult PaymentVNPay()
        {
            string url = ConfigurationManager.AppSettings["Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["HashSecret"];
            var order = new BookGrotto.Models.EF.Order();
            ShoppingCart cart = (ShoppingCart)Session["cart"];
            cart.Items.ForEach(x => order.OrderDetails.Add(new OrderDetail
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
            }));
            order.TotalAmount = cart.Items.Sum(x => (x.Price * x.Quantity));
            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
            pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
            pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
            pay.AddRequestData("vnp_Amount", ((int)order.TotalAmount * 100).ToString()); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
            pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
            pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
            pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
            pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang sach - BookGrotto"); //Thông tin mô tả nội dung thanh toán
            pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
            pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }
        public ActionResult PaymentVNPConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?
                var order = new BookGrotto.Models.EF.Order();
                ShoppingCart cart = (ShoppingCart)Session["cart"];
                if (checkSignature)
                {
                    ViewBag.Code = vnp_ResponseCode;
                    if (vnp_ResponseCode == "00")
                    {

                        //Thanh toán thành công
                        SendMail(GetInfo.Email, cart, GetInfo.OrderInfo);
                        ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    }
                    if (vnp_ResponseCode == "24")
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Giao dịch không thành công do: Khách hàng hủy giao dịch";
                    }

                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                }
            }

            return View();
        }
    }
}