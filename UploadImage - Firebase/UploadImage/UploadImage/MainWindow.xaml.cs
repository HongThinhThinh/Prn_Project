using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Firebase.Storage; // Dùng Firebase SDK cho .NET (cần cài đặt qua NuGet)

namespace UploadImage
{
    public partial class MainWindow : Window
    {
        private bool isCapturing = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            isCapturing = true;

            // Lấy kích thước của màn hình chính
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // Chụp màn hình
            var bitmap = new Bitmap((int)screenWidth, (int)screenHeight);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }

            // Hiển thị ảnh trên Image control
            var bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            imgScreen.Source = bitmapSource;
            imgScreen.Visibility = Visibility.Visible;
        }

        private async void btnCapture_Click(object sender, RoutedEventArgs e)
        {
            if (!isCapturing)
            {
                MessageBox.Show("Capturing is not started.");
                return;
            }

            try
            {
                // Lấy kích thước của màn hình chính
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                // Chụp màn hình
                var bitmap = new Bitmap((int)screenWidth, (int)screenHeight);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
                }

                // Thêm thông tin vào ảnh
                await AddTextToImage(bitmap);

                // Xác định đường dẫn đến thư mục Data
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string dataFolderPath = Path.Combine(exePath, "Data");

                // Tạo thư mục Data nếu chưa tồn tại
                if (!Directory.Exists(dataFolderPath))
                {
                    Directory.CreateDirectory(dataFolderPath);
                }

                // Lưu ảnh vào thư mục Data
                string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
                string filePath = Path.Combine(dataFolderPath, fileName);

                bitmap.Save(filePath, ImageFormat.Png);
                MessageBox.Show($"Screenshot saved as {filePath}");

                // Kiểm tra nếu người dùng chọn checkbox "Send to Firebase"
                if (chkSendFirebase.IsChecked == true)
                {
                    await UploadToFirebase(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            isCapturing = false;
            imgScreen.Visibility = Visibility.Collapsed;
            MessageBox.Show("Stopped capturing.");
        }

        private async Task AddTextToImage(Bitmap bitmap)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Thiết lập font và brush
                var fontFamily = new FontFamily("Arial");
                var font = new Font(fontFamily, 12, System.Drawing.FontStyle.Bold); // 
                var brush = Brushes.Red; // Định nghĩa brush để vẽ chữ
                // Thông tin để thêm vào ảnh
                string dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ip = await GetPublicIPAddressAsync();
                string dimensions = $"{bitmap.Width}x{bitmap.Height}";

                // Khoảng cách giữa các dòng văn bản
                float lineSpacing = 20f; // Khoảng cách giữa các dòng

                // Tọa độ bắt đầu
                float x = 10f; // Khoảng cách từ cạnh trái
                float y = bitmap.Height - 60f; // Bắt đầu từ đáy ảnh, có khoảng cách từ cạnh dưới

                // Vẽ văn bản lên ảnh với khoảng cách giữa các dòng
                graphics.DrawString($"DateTime: {dateTime}", font, brush, new PointF(x, y));
                y += lineSpacing;
                graphics.DrawString($"IP: {ip}", font, brush, new PointF(x, y));
                y += lineSpacing;
                graphics.DrawString($"Dimensions: {dimensions}", font, brush, new PointF(x, y));
            }
        }

        private async Task<string> GetPublicIPAddressAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Gửi yêu cầu GET đến dịch vụ API để lấy IP công cộng
                    string ip = await client.GetStringAsync("https://api.ipify.org?format=text");
                    return ip;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving public IP: " + ex.Message);
                return "N/A";
            }
        }

        private async Task UploadToFirebase(string filePath)
        {
            try
            {
                var stream = File.Open(filePath, FileMode.Open);

                // Thiết lập kết nối với Firebase Storage
                var firebaseStorage = new FirebaseStorage("abi-dynamic.appspot.com");

                // Tải file lên Firebase
                var uploadTask = await firebaseStorage
                    .Child("images")   // Thư mục lưu trữ trên Firebase
                    .Child(Path.GetFileName(filePath))  // Tên file
                    .PutAsync(stream);

                // Lấy URL của ảnh đã tải lên
                string downloadUrl = uploadTask;  // await ở đây

                MessageBox.Show($"Image uploaded to Firebase: {downloadUrl}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading to Firebase: " + ex.Message);
            }
        }
    }
}
