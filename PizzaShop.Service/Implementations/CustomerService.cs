using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PizzaShop.Entity.Constants;
using PizzaShop.Entity.ViewModels;
using PizzaShop.Repository.Interfaces;
using PizzaShop.Service.Interfaces;

namespace PizzaShop.Service.Implementations;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IWebHostEnvironment _webHost;
    public CustomerService(ICustomerRepository customerRepository, IWebHostEnvironment webHost)
    {
        _customerRepository = customerRepository;
        _webHost = webHost;
    }

    public async Task<CustomerPageViewModel> GetCustomerList(string searchString, string sortOrder, int pageIndex, int pageSize, string dateRange, DateOnly? fromDate, DateOnly? toDate)
    {
        var (customers, count) = await _customerRepository.GetCustomerList(searchString, sortOrder, pageIndex, pageSize, dateRange, fromDate, toDate);
        var filterCustomers = customers.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        var model = new CustomerPageViewModel
        {
            CustomerList = filterCustomers,
            TotalCustomers = count,
            PageIndex = pageIndex,
            PageSize = pageSize,
            SearchString = searchString,
            SortOrder = sortOrder,
            DateRange = dateRange,
            FromDate = fromDate,
            ToDate = toDate,
            TotalPage = (int)Math.Ceiling(count / (double)pageSize)
        };
        return model;
    }

    public async Task<FileContentResult> ExportCustomersExcel(string searchString, string sortOrder, int pageIndex, int pageSize, string dateRange, DateOnly? fromDate, DateOnly? toDate)
    {
        var (customers, count) = await _customerRepository.GetCustomerList(searchString, sortOrder, pageIndex, pageSize, dateRange, fromDate, toDate);
        if (count == 0)
            return null!;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("CustomerList");



            worksheet.Cells[9, 1, 9, 16].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[9, 1, 9, 16].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 96, 152));

            worksheet.Cells[2, 1, 3, 2].Merge = true;
            worksheet.Cells[2, 1, 3, 2].Value = "Account";
            worksheet.Cells[2, 1, 3, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 1, 3, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[2, 1, 3, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[2, 1, 3, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 96, 152));
            worksheet.Cells[2, 1, 3, 2].Style.Font.Color.SetColor(System.Drawing.Color.White);

            worksheet.Cells[2, 3, 3, 6].Merge = true;
            // worksheet.Cells[2, 3, 3, 6].Value = ;
            worksheet.Cells[2, 3, 3, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 3, 3, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[2, 3, 3, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[2, 3, 3, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
            worksheet.Cells[2, 3, 3, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);
            worksheet.Cells[2, 3, 3, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[2, 8, 3, 9].Merge = true;
            worksheet.Cells[2, 8, 3, 9].Value = "Search Text:";
            worksheet.Cells[2, 8, 3, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 8, 3, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[2, 8, 3, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[2, 8, 3, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 96, 152));
            worksheet.Cells[2, 8, 3, 9].Style.Font.Color.SetColor(System.Drawing.Color.White);

            worksheet.Cells[2, 10, 3, 13].Merge = true;
            worksheet.Cells[2, 10, 3, 13].Value = searchString;
            worksheet.Cells[2, 10, 3, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[2, 10, 3, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[2, 10, 3, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[2, 10, 3, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
            worksheet.Cells[2, 10, 3, 13].Style.Font.Color.SetColor(System.Drawing.Color.Black);
            worksheet.Cells[2, 10, 3, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[5, 1, 6, 2].Merge = true;
            worksheet.Cells[5, 1, 6, 2].Value = "Date:";
            worksheet.Cells[5, 1, 6, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[5, 1, 6, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[5, 1, 6, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, 1, 6, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 96, 152));
            worksheet.Cells[5, 1, 6, 2].Style.Font.Color.SetColor(System.Drawing.Color.White);

            worksheet.Cells[5, 3, 6, 6].Merge = true;
            worksheet.Cells[5, 3, 6, 6].Value = dateRange;
            worksheet.Cells[5, 3, 6, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[5, 3, 6, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[5, 3, 6, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, 3, 6, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
            worksheet.Cells[5, 3, 6, 6].Style.Font.Color.SetColor(System.Drawing.Color.Black);
            worksheet.Cells[5, 3, 6, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells[5, 8, 6, 9].Merge = true;
            worksheet.Cells[5, 8, 6, 9].Value = "No of Records:";
            worksheet.Cells[5, 8, 6, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[5, 8, 6, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[5, 8, 6, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, 8, 6, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 96, 152));
            worksheet.Cells[5, 8, 6, 9].Style.Font.Color.SetColor(System.Drawing.Color.White);

            worksheet.Cells[5, 10, 6, 13].Merge = true;
            worksheet.Cells[5, 10, 6, 13].Value = count;
            worksheet.Cells[5, 10, 6, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[5, 10, 6, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells[5, 10, 6, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[5, 10, 6, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
            worksheet.Cells[5, 10, 6, 13].Style.Font.Color.SetColor(System.Drawing.Color.Black);
            worksheet.Cells[5, 10, 6, 13].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            string path = _webHost.WebRootPath;
            string imagePath = Path.Combine(path, "images", "logos", "pizzashop_logo.png");
            if (System.IO.File.Exists(imagePath))
            {
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    var ExcelImage = worksheet.Drawings.AddPicture("logo", stream);
                    ExcelImage.SetPosition(1, 0, 14, 0);
                    ExcelImage.SetSize(150, 100);
                }
            }



            // worksheet.Cells[9, 1, 9, 1].Merge = true;
            worksheet.Cells[9, 1].Value = "Id";
            worksheet.Cells[9, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[9, 2, 9, 4].Merge = true;
            worksheet.Cells[9, 2, 9, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 2, 9, 4].Value = "Name";

            worksheet.Cells[9, 5, 9, 7].Merge = true;
            worksheet.Cells[9, 5, 9, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 5, 9, 7].Value = "Email";

            worksheet.Cells[9, 8, 9, 10].Merge = true;
            worksheet.Cells[9, 8, 9, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 8, 9, 10].Value = "Date";

            worksheet.Cells[9, 11, 9, 12].Merge = true;
            worksheet.Cells[9, 11, 9, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 11, 9, 12].Value = "Mobile Number";

            worksheet.Cells[9, 13, 9, 14].Merge = true;
            worksheet.Cells[9, 13, 9, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells[9, 13, 9, 14].Value = "Total Order";

            int row = 10;
            if (customers.Count > 0)
            {
                foreach (var o in customers)
                {
                    worksheet.Cells[row, 1, row, 1].Value = o.Id;
                    worksheet.Cells[row, 1, row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells[row, 2, row, 4].Merge = true;
                    worksheet.Cells[row, 2, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 2, row, 4].Value = o.Name;

                    worksheet.Cells[row, 5, row, 7].Merge = true;
                    worksheet.Cells[row, 5, row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 5, row, 7].Value = o.Email;

                    worksheet.Cells[row, 8, row, 10].Merge = true;
                    worksheet.Cells[row, 8, row, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 8, row, 10].Value = DateOnly.FromDateTime((DateTime)o.CreatedAt);

                    worksheet.Cells[row, 11, row, 12].Merge = true;
                    worksheet.Cells[row, 11, row, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 11, row, 12].Value = o.Phone;


                    worksheet.Cells[row, 13, row, 14].Merge = true;
                    worksheet.Cells[row, 13, row, 14].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[row, 13, row, 14].Value = o.TotalOrders;
                    row++;
                }
            }
            else
            {
                worksheet.Cells[10, 1, 10, 16].Merge = true;
                worksheet.Cells[10, 1, 10, 16].Value = "No Record Found";
                worksheet.Cells[10, 1, 10, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            }

            // Auto-fit columns for better readability
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            // Convert the package to a byte array
            var fileBytes = package.GetAsByteArray();
            return new FileContentResult(
                fileBytes, //Excel File data in Byte Array
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" //Excel Sheet Mime Type
            )
            {
                FileDownloadName = "Customers.xlsx" //File Name
            };
        }
    }

    public async Task<CustomerDetailsViewModel> GetCustomerDetails(int id)
    {
        try
        {
            var customer = await _customerRepository.CustomerDetails(id);
            var customerDetails = new CustomerDetailsViewModel
            {
                Name = customer.Name,
                MobileNumber = customer.Phone,
                MaxOrder = customer.Orders.Max(o => o.TotalAmount),
                AvgBill = Math.Round((decimal)customer.Orders.Average(o => o.TotalAmount)!, 2),
                CommingSince = customer.Orders.Min(o => o.CreatedAt),
                Visits = customer.Orders.Count(c => c.CustomerId == customer.Id),
                CustomerOrders = customer.Orders.Select(o => new CustomerOrderDetails
                {
                    OrderDate = DateOnly.FromDateTime(o.CreatedAt.GetValueOrDefault()),
                    OrderType = "Dinein",
                    Payment = o.Invoices.FirstOrDefault() != null ? o.Invoices.FirstOrDefault()?.Payments.FirstOrDefault()!.PaymentMethod! : 0,
                    items = o.OrderedItems.Count(i => i.OrderId == o.Id),
                    Amount = o.OrderedItems.Sum(t => t.TotalAmount)
                }).ToList(),
            };
            return customerDetails;
        }
        catch (Exception)
        {
            return null!;
        }
    }

    public async Task<bool> UpdateCustomerDetails(int id, string name, string MobileNumber, int userId)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerById(id);
            customer.Phone = MobileNumber;
            customer.Name = name;
            customer.ModifiedBy = userId;
            customer.ModifiedAt = DateTime.UtcNow;

            var updatedCustomer = await _customerRepository.UpdateCustomer(customer);
            if (customer != null)
                return true;
            return false;
        }
        catch (Exception e)
        {
            return false;
        }

    }
}