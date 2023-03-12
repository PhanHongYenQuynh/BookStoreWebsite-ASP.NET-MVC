using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BookGrotto.Momo
{
    public class HandleMomoResponse
    {

        /* LƯU ĐƠN HÀNG KHI THANH TOÁN THÀNH CÔNG
         * 1. Lấy tất cả sản phẩm trong giỏ hàng
         * 2. Tạo mới Order -> thêm order vào db
         * 3. Lấy Order từ DB -> Chuyễn nhưng sản phẩm trong giỏ hàng qua Order
         * 4. Xóa chi tiết giỏ hàng
         * 5. Lưu tất cả nhưng thay đỗi
         * 
         * Gửi mail về người dùng khi đặt hàng thành côngi
         */

        public static string saveOrderByMomoPayment(string amount, string orderID)
        {

            int paymentMethodID = 1;


            return "success";
        }
    }
}