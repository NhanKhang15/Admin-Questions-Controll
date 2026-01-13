# Hướng dẫn chạy dự án Admin_Question_Controll

Dự án này là một ứng dụng web **ASP.NET Core 9.0** sử dụng **SQL Server**.

## 1. Yêu cầu hệ thống (Prerequisites)

Để chạy dự án này, bạn cần cài đặt:
- **.NET SDK 9.0**: [Tải về tại đây](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (Express hoặc Developer): [Tải về tại đây](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- **SQL Server Management Studio (SSMS)** (để chạy script database): [Tải về tại đây](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

## 2. Cài đặt Cơ sở dữ liệu (Database Setup)

Dự án yêu cầu một cơ sở dữ liệu tên là `Floria_2`.

1. Mở **SQL Server Management Studio (SSMS)** và kết nối đến SQL Server của bạn (thường là `.` hoặc `localhost`).
2. Mở file `SQLQuery1.sql` (nằm ở thư mục gốc của dự án `e:\Admin_Question_Controll\SQLQuery1.sql`).
3. Chạy toàn bộ script (nhấn F5 hoặc nút Execute) để tạo database, bảng và dữ liệu mẫu.

> **Lưu ý**: Script sẽ tạo user admin mặc định:
> - **Email**: `admin@example.com`
> - **Password**: (Đã mã hóa, nếu bạn không biết mật khẩu này, bạn có thể cần tạo hash mới hoặc tạo user mới trong bảng Users).

## 3. Cấu hình Kết nối (Configuration)

Kiểm tra file cấu hình tại `Admintask/appsettings.json`. Chuỗi kết nối mặc định là:

```json
"ConnectionStrings": {
    "FloriaDb": "Server=.,1433;Database=Floria_2;User Id=sa;Password=1234567;Encrypt=True;TrustServerCertificate=True;"
}
```

Nếu cấu hình SQL Server của bạn khác (ví dụ: dùng Windows Authentication hoặc user/pass khác), hãy sửa lại chuỗi kết nối này.
- Nếu dùng **Windows Authentication**, chuỗi kết nối nên là:
  `"Server=.;Database=Floria_2;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"`

## 4. Chạy Ứng dụng

1. Mở terminal (CMD, PowerShell, hoặc Git Bash) tại thư mục `e:\Admin_Question_Controll\Admintask`.
2. Chạy lệnh sau để khôi phục các thư viện và chạy ứng dụng:

```powershell
dotnet run
```

3. Sau khi ứng dụng khởi động thành công, terminal sẽ hiển thị đường dẫn truy cập, thường là `http://localhost:5xxx`.
4. Mở trình duyệt và truy cập địa chỉ đó.

## 5. Cấu trúc Dự án

- **Admintask**: Thư mục chứa mã nguồn chính (ASP.NET Core MVC).
  - **Controllers**: Chứa logic xử lý.
  - **Views**: Chứa giao diện (.cshtml).
  - **Models/ViewModels**: Định nghĩa dữ liệu.
  - **Program.cs**: Điểm khởi chạy ứng dụng.
- **SQLQuery1.sql**: Script khởi tạo cơ sở dữ liệu.
