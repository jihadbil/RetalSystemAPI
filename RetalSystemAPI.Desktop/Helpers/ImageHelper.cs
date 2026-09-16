using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RetalSystemAPI.Desktop.Helpers;

/// <summary>
/// فئة مساعدة لتحميل وعرض الصور من الخادم المحلي أو الإنترنت في WPF مع دعم التخزين المؤقت وتجاوز شهادات SSL المحلية.
/// </summary>
public static class ImageHelper
{
    public static readonly DependencyProperty SourceUrlProperty =
        DependencyProperty.RegisterAttached(
            "SourceUrl",
            typeof(string),
            typeof(ImageHelper),
            new PropertyMetadata(null, OnSourceUrlChanged));

    public static string? GetSourceUrl(DependencyObject obj) => (string?)obj.GetValue(SourceUrlProperty);
    public static void SetSourceUrl(DependencyObject obj, string? value) => obj.SetValue(SourceUrlProperty, value);

    private static readonly ConcurrentDictionary<string, BitmapSource> _cache = new();
    private static readonly HttpClient _httpClient = new(new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
    }) { Timeout = TimeSpan.FromSeconds(15) };

    private static async void OnSourceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Image image) return;

        string? rawUrl = e.NewValue as string;
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            image.Source = null;
            return;
        }

        string url = rawUrl.Trim();

        // 1. ملف محلي موجود على القرص
        if (File.Exists(url))
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(url, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                image.Source = bmp;
                return;
            }
            catch
            {
                image.Source = null;
                return;
            }
        }

        // 2. توحيد المسار النسبي أو الكامل للخادم
        string fullUrl = url;
        if (fullUrl.StartsWith("/"))
        {
            fullUrl = $"https://localhost:7226{fullUrl}";
        }
        else if (!fullUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
                 !fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            fullUrl = $"https://localhost:7226/{fullUrl.TrimStart('/')}";
        }

        // 3. التحقق من الذاكرة المؤقتة (Cache)
        if (_cache.TryGetValue(fullUrl, out var cached))
        {
            image.Source = cached;
            return;
        }

        image.Source = null;

        // 4. تحميل الصورة في الخلفية وفك تشفيرها
        try
        {
            var bytes = await _httpClient.GetByteArrayAsync(fullUrl);
            if (bytes != null && bytes.Length > 0)
            {
                var bmp = new BitmapImage();
                using (var ms = new MemoryStream(bytes))
                {
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                }
                bmp.Freeze();
                _cache[fullUrl] = bmp;

                if (string.Equals(GetSourceUrl(image)?.Trim(), url, StringComparison.OrdinalIgnoreCase))
                {
                    image.Source = bmp;
                }
            }
        }
        catch
        {
            // في حال فشل HTTPS، المحاولة عبر HTTP المحلي
            if (fullUrl.StartsWith("https://localhost:7226", StringComparison.OrdinalIgnoreCase))
            {
                string httpFallback = fullUrl.Replace("https://localhost:7226", "http://localhost:5005");
                try
                {
                    var bytes = await _httpClient.GetByteArrayAsync(httpFallback);
                    if (bytes != null && bytes.Length > 0)
                    {
                        var bmp = new BitmapImage();
                        using (var ms = new MemoryStream(bytes))
                        {
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.StreamSource = ms;
                            bmp.EndInit();
                        }
                        bmp.Freeze();
                        _cache[fullUrl] = bmp;

                        if (string.Equals(GetSourceUrl(image)?.Trim(), url, StringComparison.OrdinalIgnoreCase))
                        {
                            image.Source = bmp;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
