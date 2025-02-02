using Business_Logic;
using Data_Access;
using ZXing.SkiaSharp;
using ZXing;
using BarcodeFormat = ZXing.BarcodeFormat;
using ZXing.Common;
using SkiaSharp;
using System.Text.Json;

namespace Nail_Salon_Mobile_App;

public partial class MainPage : ContentPage
{
    private Database _database;

    private ImageSource _qrCodeImageSource;
    private byte[] _qrCodeBytes;

    public class QrCodeData
    {
        public Customer Customer { get; set; }
        public Service Service { get; set; }
    }

    public MainPage()
    {
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "customers.db3");
        _database = new Database(dbPath);
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

            var existingCustomer = await _database.GetCustomerByDetailsAsync(fullName, phoneNumber);

            if (existingCustomer == null)
            {
                existingCustomer = new Customer
                {
                    CustomerFullName = fullName,
                    CustomerBirthDate = birthDate,
                    CustomerPhoneNumber = phoneNumber,
                    CustomerIsNew = true
                };

                await _database.SaveCustomerAsync(existingCustomer);
            } 
            else
            {
                existingCustomer.CustomerIsNew = false;
            }

            var service = await _database.GetServicesAsync(); 
            var selectedService = service.FirstOrDefault(s => s.ServiceName == selectedServiceName);
            if (selectedService == null)
            {
                await DisplayAlert("Ошибка", "Услуга не найдена в базе данных.", "OK");
                return;
            }

            var visitLog = new VisitLogs
            {
                CustomerId = existingCustomer.Id, 
                ServiceId = selectedService.Id, 
                StartDateTime = appointmentDateTime,
                EndTime = appointmentDateTime.Add(selectedService.ServiceExecutionTime),
                Price = selectedService.GetDiscountedPrice(existingCustomer.CustomerIsNew)
            };

            await _database.SaveVisitLogAsync(visitLog);

            _qrCodeBytes = GenerateQRCode(visitLog);
            _qrCodeImageSource = ImageSource.FromStream(() => new MemoryStream(_qrCodeBytes));

            QrCodeImage.Source = _qrCodeImageSource;
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

    private async Task InitializeServicesAsync()
    {
        var services = new List<Service>
    {
        new Service { ServiceName = "Маникюр", ServicePrice = 100, ServiceExecutionTime = TimeSpan.FromHours(1) },
        new Service { ServiceName = "Педикюр", ServicePrice = 150, ServiceExecutionTime = TimeSpan.FromHours(1.5) },
        new Service { ServiceName = "Наращивание", ServicePrice = 200, ServiceExecutionTime = TimeSpan.FromHours(2) }
    };

        foreach (var service in services)
        {
            var existingService = await _database.GetServicesAsync(); 
            var serviceExists = existingService.FirstOrDefault(s => s.ServiceName == service.ServiceName);
            if (serviceExists == null)
            {
                await _database.SaveServiceAsync(service);
            }
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
        if (_qrCodeBytes == null)
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
                await fileStream.WriteAsync(_qrCodeBytes, 0, _qrCodeBytes.Length);
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

                        var customer = await _database.GetCustomerByIdAsync(qrCodeData.CustomerId);
                        var service = await _database.GetServiceByIdAsync(qrCodeData.ServiceId);

                        await DisplayAlert("Распознанный QR-код",
                            $"Клиент: {customer?.CustomerFullName}, Услуга: {service?.ServiceName}, Дата/Время: {qrCodeData.StartDateTime}, Стоимость: {qrCodeData.Price}",
                            "OK");
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

    private async Task LoadServicesAsync()
    {
        var services = await _database.GetServicesAsync();
        ServicePicker.ItemsSource = services.Select(s => s.ServiceName).ToList();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //await InitializeServicesAsync();
        await LoadServicesAsync();

        if (Application.Current.RequestedTheme == AppTheme.Dark)
        {
            Resources["BackgroundColorLight"] = Color.FromArgb("#3C0A1D"); // Темно-розовый фон
            Resources["TextColorLight"] = Color.FromArgb("#FF6F91"); // Темно-розовый текст
            Resources["EntryBackgroundLight"] = Color.FromArgb("#4A1A2D"); // Темный фон для Entry
            Resources["EntryTextColorLight"] = Colors.White; // Белый текст для Entry
            Resources["ButtonBackgroundLight"] = Color.FromArgb("#FF6F91"); // Темно-розовая кнопка
            Resources["ButtonTextColorLight"] = Colors.Black; // Черный текст для кнопки
        }
        else
        {
            Resources["BackgroundColorLight"] = Color.FromArgb("#FFF0F6"); // Светло-розовый фон
            Resources["TextColorLight"] = Color.FromArgb("#D81B60"); // Светло-розовый текст
            Resources["EntryBackgroundLight"] = Colors.White; // Белый фон для Entry
            Resources["EntryTextColorLight"] = Colors.Black; // Черный текст для Entry
            Resources["ButtonBackgroundLight"] = Color.FromArgb("#D81B60"); // Светло-розовая кнопка
            Resources["ButtonTextColorLight"] = Colors.White; // Белый текст для кнопки
        }
    }
}
