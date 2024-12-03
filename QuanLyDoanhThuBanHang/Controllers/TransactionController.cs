using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesManagementApp.Data;
using SalesManagementApp.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagementApp.Controllers
{
    public class TransactionController : Controller
    {
        private readonly SalesContext _context;

        public TransactionController(SalesContext context)
        {
            _context = context;
        }
        public IActionResult Home()
        {
            // Trang chủ không cần dữ liệu từ cơ sở dữ liệu, chỉ cần hiển thị các chức năng
            return View();
        }

        // Hiển thị danh sách giao dịch
        public async Task<IActionResult> Index(string search, string status, DateTime? startDate, DateTime? endDate)
        {
            var transactions = _context.Transactions.AsQueryable();

            // Tìm kiếm theo khách hàng hoặc sản phẩm
            if (!string.IsNullOrEmpty(search))
            {
                transactions = transactions.Where(t => t.Customer.Contains(search) || t.Product.Contains(search));
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                transactions = transactions.Where(t => t.Status == status);
            }

            // Lọc theo khoảng ngày
            if (startDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate <= endDate.Value);
            }

            var transactionList = await transactions.ToListAsync();
            return View(transactionList);
        }

        // Hiển thị form sửa giao dịch
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return View(transaction);
        }

        // Xử lý khi người dùng gửi form sửa giao dịch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Customer, Product, Status, TransactionDate, Quantity, UnitPrice, Discount")] Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(transaction);
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
        // Phương thức Xoá giao dịch
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Sau khi xóa, quay lại trang danh sách giao dịch
        }// Phương thức Xoá giao dịch
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));  // Sau khi xóa, quay lại trang danh sách giao dịch
        }


        // Nhập giao dịch mới (GET)
        public IActionResult Create()
        {
            var transaction = new Transaction
            {
                TransactionDate = DateTime.Now, // Ngày giao dịch mặc định là hôm nay
                Customer = string.Empty,
                Product = string.Empty,
                Quantity = 1,
                UnitPrice = 0,
                Discount = 0,
                Status = "Đang xử lý"
            };
            return View(transaction);
        }

        // Nhập giao dịch mới (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create([Bind("TransactionCode,TransactionDate,Customer,Product,Quantity,UnitPrice,Discount,Status")] Transaction transaction)
        {
            // Kiểm tra xem mã giao dịch có tồn tại trong cơ sở dữ liệu không
            var existingTransaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionCode == transaction.TransactionCode);

            if (existingTransaction != null)
            {
                // Nếu mã giao dịch đã tồn tại, thêm lỗi vào ModelState
                ModelState.AddModelError("TransactionCode", "Mã giao dịch này đã tồn tại. Vui lòng nhập mã giao dịch khác.");
            }

            // Nếu ModelState vẫn hợp lệ (không có lỗi)
            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));  // Quay lại danh sách sau khi tạo
            }

            // Nếu có lỗi (bao gồm lỗi trùng mã giao dịch), quay lại form nhập giao dịch mới
            return View(transaction);
        }


        public IActionResult Details(int id)
        {
            // Tìm giao dịch theo id
            var transaction = _context.Transactions
                .Where(t => t.Id == id)
                .FirstOrDefault();

            if (transaction == null)
            {
                return NotFound(); // Nếu không tìm thấy giao dịch
            }

            return View(transaction); // Trả về view chi tiết với model là giao dịch
        }

        // Quản lý doanh thu bán hàng
        public async Task<IActionResult> Revenue(int? year, int? month, int? quarter)
        {
            var transactions = _context.Transactions.AsQueryable();
            transactions = transactions.Where(t => t.Status == "Thành công"); //lọc giao dịch để chỉ hiện các giao dịch đã thanh toán

            // Lọc theo năm nếu có
            if (year.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate.Year == year.Value);
            }

            // Lọc theo tháng nếu có
            if (month.HasValue)
            {
                transactions = transactions.Where(t => t.TransactionDate.Month == month.Value);
            }

            // Lọc theo quý nếu có
            if (quarter.HasValue)
            {
                transactions = transactions.Where(t =>
                    (quarter == 1 && t.TransactionDate.Month >= 1 && t.TransactionDate.Month <= 3) ||
                    (quarter == 2 && t.TransactionDate.Month >= 4 && t.TransactionDate.Month <= 6) ||
                    (quarter == 3 && t.TransactionDate.Month >= 7 && t.TransactionDate.Month <= 9) ||
                    (quarter == 4 && t.TransactionDate.Month >= 10 && t.TransactionDate.Month <= 12));
            }

            // Lấy danh sách giao dịch đã lọc
            var transactionsList = await transactions.ToListAsync();

            // Tính tổng doanh thu và tổng chiết khấu
            // Tính tổng doanh thu = Tổng thành tiền của tất cả giao dịch
            var totalRevenue = transactionsList.Sum(t => t.Total);  // Tính tổng thành tiền

            // Tính tổng chiết khấu (số lượng x đơn giá - tổng doanh thu)
            var totalDiscount = transactionsList.Sum(t => (t.Quantity * t.UnitPrice) - t.Total);


            // Gửi dữ liệu vào ViewBag để hiển thị trong View
            ViewBag.TotalRevenue = totalRevenue.ToString("C");  // Hiển thị tổng doanh thu dưới dạng tiền tệ
            ViewBag.TotalDiscount = totalDiscount.ToString("C");  // Hiển thị tổng chiết khấu dưới dạng tiền tệ

            // Trả về danh sách giao dịch đã lọc cho view
            return View(transactionsList);
        }


    }
}
