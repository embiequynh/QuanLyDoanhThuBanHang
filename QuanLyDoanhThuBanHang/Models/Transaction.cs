namespace SalesManagementApp.Models
{
    public class Transaction
    {
        public int Id { get; set; } // Mã giao dịch
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; } // Ngày giao dịch
        public string Customer { get; set; } // Tên khách hàng
        public string Product { get; set; } // Tên sản phẩm
        public int Quantity { get; set; } // Số lượng
        public decimal UnitPrice { get; set; } // Đơn giá
        public decimal Discount { get; set; } // Chiết khấu (từ 0 - 1)
        public decimal Total => Quantity * UnitPrice * (1 - Discount); // Thành tiền
        public string Status { get; set; } // Trạng thái ("Thành công", "Đang xử lý", "Đã hủy")
    }
}
