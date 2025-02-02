using Business_Logic;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ZXing.SkiaSharp;
using ZXing;
using BarcodeFormat = ZXing.BarcodeFormat;
using ZXing.Common;
using Microsoft.Maui.Graphics;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;
using System.Text.Json;

namespace Nail_Salon_Mobile_App;

public partial class MainPage : ContentPage
{
    private ImageSource qrCodeImageSource;
    private byte[] qrCodeBytes;

    public class QrCodeData
    {
        public Customer Customer { get; set; }
        public Service Service { get; set; }
    }

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnCreateCustomerClicked(object sender, EventArgs e)
    {
        try
        {
            string fullName = FullNameEntry.Text;
            DateTime birthDate = BirthDatePicker.Date;
            decimal phoneNumber = decimal.Parse(PhoneNumberEntry.Text);
            string selectedServiceName = ServicePicker.SelectedItem?.ToString() ?? "Не указано";
            DateTime appointmentDate = AppointmentDatePicker.Date;
            TimeSpan appointmentTime = AppointmentTimePicker.Time;
            DateTime appointmentDateTime = appointmentDate.Add(appointmentTime);

            var customer = new Customer
            {
                CustomerFullName = fullName,
                CustomerBirthDate = birthDate,
                CustomerPhoneNumber = phoneNumber,
                CustomerIsNew = true 
            };

            var service = new Service
            {
                ServiceName = selectedServiceName,
                ServicePrice = 100, 
                ServiceExecutionTime = TimeSpan.FromHours(1) 
            };

            bool isEmployeeAvailable = true; 

            bool isCustomerCreated = true; 

            customer.IsRecordingSuccessful(isCustomerCreated, isEmployeeAvailable);

            var qrCodeData = new VisitLogs
            {
                Customer = customer,
                Service = service,
                StartDateTime = appointmentDateTime,
                EndTime = appointmentDateTime.TimeOfDay.Add(service.ServiceExecutionTime),
                Price = service.GetDiscountedPrice(customer.CustomerIsNew) 
            };

            qrCodeBytes = GenerateQRCode(qrCodeData);
            qrCodeImageSource = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));

            QrCodeImage.Source = qrCodeImageSource;
            await DisplayAlert("Успех", "QR-код с данными клиента сгенерирован!", "OK");
        }
        catch (ZXing.FormatException)
        {
            await DisplayAlert("Ошибка", "Неверный формат даты или номера телефона.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }

    private byte[] GenerateQRCode(VisitLogs qrCodeData)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(qrCodeData);
        var qrCodeWriter = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions
            {
                Width = 300,
                Height = 300,
                Margin = 10
            }
        };

        var pixelData = qrCodeWriter.Write(json);

        using (var stream = new MemoryStream())
        {
            using (var bitmap = new SkiaSharp.SKBitmap(pixelData.Width, pixelData.Height))
            {
                for (int y = 0; y < pixelData.Height; y++)
                {
                    for (int x = 0; x < pixelData.Width; x++)
                    {
                        var pixelIndex = (y * pixelData.Width + x) * 4;
                        var color = new SkiaSharp.SKColor(pixelData.Pixels[pixelIndex + 2], pixelData.Pixels[pixelIndex + 1], pixelData.Pixels[pixelIndex], pixelData.Pixels[pixelIndex + 3]);
                        bitmap.SetPixel(x, y, color);
                    }
                }
                using (var image = bitmap.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    image.SaveTo(stream);
                }
            }
            return stream.ToArray();
        }
    }

    private async void OnSaveQrCodeClicked(object sender, EventArgs e)
    {
        if (qrCodeBytes == null)
        {
            await DisplayAlert("Ошибка", "Сначала создайте QR-код.", "OK");
            return;
        }

        try
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string folderPath = Path.Combine(documentsPath, "NailSalonQRCodes"); 

            Directory.CreateDirectory(folderPath); 

            string uniqueFileName = Guid.NewGuid().ToString() + ".png";
            string filePath = Path.Combine(folderPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(qrCodeBytes, 0, qrCodeBytes.Length);
            }
            await DisplayAlert("Успех", $"QR-код сохранен в {filePath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка при сохранении: {ex.Message}", "OK");
        }
    }

    private async void OnRecognizeQrCodeClicked(object sender, EventArgs e)
    {
        try
        {
            var fileResult = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите изображение QR-кода",
                FileTypes = FilePickerFileType.Images
            });

            if (fileResult != null)
            {
                using var stream = await fileResult.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                using var bitmap = SKBitmap.Decode(imageBytes);
                var barcodeReader = new BarcodeReader();
                var decodeResult = barcodeReader.Decode(bitmap); 

                if (decodeResult != null)
                {
                    string decodedJson = decodeResult.Text;
                    try
                    {
                        var qrCodeData = JsonSerializer.Deserialize<VisitLogs>(decodedJson);
                        await DisplayAlert("Распознанный QR-код", $"Клиент: {qrCodeData.Customer.CustomerFullName}, Услуга: {qrCodeData.Service.ServiceName}, Дата/Время: {qrCodeData.StartDateTime}", "OK");
                    }
                    catch (JsonException ex)
                    {
                        await DisplayAlert("Ошибка", $"Ошибка при десериализации JSON: {ex.Message}", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Ошибка", "QR-код не распознан.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Произошла ошибка: {ex.Message}", "OK");
        }
    }



    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            Resources["BackgroundColorLight"] = Color.FromArgb("#3C0A1D"); // Темно-розовый фон
            Resources["TextColorLight"] = Color.FromArgb("#FF6F91"); // Темно-розовый текст
            Resources["EntryBackgroundLight"] = Color.FromArgb("#4A1A2D"); // Темный фон для Entry
            Resources["EntryTextColorLight"] = Colors.White; // Белый текст для Entry
            Resources["ButtonBackgroundLight"] = Color.FromArgb("#FF6F91"); // Темно-розовая кнопка
            Resources["ButtonTextColorLight"] = Colors.Black; // Черный текст для кнопки
            QrCodeImage.BackgroundColor = Color.FromArgb("#4A1A2D"); // Темный фон для QR-кода
        }
        else
        {
            Resources["BackgroundColorLight"] = Color.FromArgb("#FFF0F6"); // Светло-розовый фон
            Resources["TextColorLight"] = Color.FromArgb("#D81B60"); // Светло-розовый текст
            Resources["EntryBackgroundLight"] = Colors.White; // Белый фон для Entry
            Resources["EntryTextColorLight"] = Colors.Black; // Черный текст для Entry
            Resources["ButtonBackgroundLight"] = Color.FromArgb("#D81B60"); // Светло-розовая кнопка
            Resources["ButtonTextColorLight"] = Colors.White; // Белый текст для кнопки
            QrCodeImage.BackgroundColor = Color.FromArgb("#F8BBD0"); // Светлый фон для QR-кода
        }
    }
}
