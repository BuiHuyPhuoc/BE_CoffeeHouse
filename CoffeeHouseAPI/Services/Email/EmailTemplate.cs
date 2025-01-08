using Microsoft.Identity.Client;

namespace CoffeeHouseAPI.Services.Email
{
    public static class EMAIL_TEMPLATE
    {
        public static string SendOtpTemplate(string otpString)
        {
            return string.Format(@"
                <!DOCTYPE html>
                <html>
                <head>
                <title>Verify account</title>
                </head>
                <body>
                <p>Trân trọng cảm ơn đã sử dụng dịch vụ của Coffee House.</p>
                <p><strong>Kính gửi khách hàng.</strong></p>
                <p>Đường dẫn xác nhận tài khoản: {0}</p>
                <i>Đây là email tự động từ hệ thống Coffee House, vui lòng không phản hồi.</i>
                </body>
                </html>
            ", otpString);
        }

        public static string SendOtpForgotPasswordTemplate(string otpString, DateTime expire)
        {
            return string.Format(@"
                <!DOCTYPE html>
                <html>
                <head>
                <title>Page Title</title>
                </head>
                <body>
                <p><strong>Kính gửi khách hàng.</strong></p>
                <p>Vui lòng không chia sẽ mã OTP cho bất kỳ ai.</p>
                <p>Mã OTP xác nhận đổi mật khẩu là: <strong style=""color: black;"">{0}</strong></p>
                <p>Hạn sử dụng cho OTP xác nhận đổi mật khẩu là: <strong style=""color: black;"">{1}</strong></p>
                <i>Đây là email tự động từ hệ thống Coffee House, vui lòng không phản hồi.</i>
                <p>Trân trọng cảm ơn đã sử dụng dịch vụ của Coffee House.</p>
                </body>
                </html>
                ", otpString, expire.ToString("dd/MM/yyyy HH:mm:ss"));
        }
    }
}
