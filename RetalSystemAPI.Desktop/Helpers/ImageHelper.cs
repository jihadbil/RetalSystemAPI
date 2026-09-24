using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RetalSystemAPI.Desktop.Helpers;

/// <summary>
/// فئة مساعدة لتحميل وعرض الصور المحلية أو صور خادم النظام عبر HTTPS مع تخزين مؤقت محدود.
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
    private static readonly ConcurrentDictionary<string, DateTimeOffset> _failedUrls = new();
    private static readonly ConditionalWeakTable<Image, CancellationTokenSource> _pendingLoads = new();
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly TimeSpan FailedUrlRetryDelay = TimeSpan.FromMinutes(2);
    private const int MaxCachedImages = 500;
    private const int MaxImageBytes = 10 * 1024 * 1024;

    private static async void OnSourceUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Image image) return;

        if (_pendingLoads.TryGetValue(image, out var previousLoad))
        {
            previousLoad.Cancel();
            previousLoad.Dispose();
            _pendingLoads.Remove(image);
        }

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
        if (!TryResolveRemoteUrl(url, out var fullUrl))
        {
            image.Source = null;
            return;
        }

        // 3. التحقق من الذاكرة المؤقتة (Cache) أو قائمة الروابط الفاشلة لتفادي الانتظار المتكرر
        if (_failedUrls.TryGetValue(fullUrl, out var failedAt))
        {
            if (DateTimeOffset.UtcNow - failedAt < FailedUrlRetryDelay)
            {
                image.Source = null;
                return;
            }

            _failedUrls.TryRemove(fullUrl, out _);
        }

        if (_cache.TryGetValue(fullUrl, out var cached))
        {
            image.Source = cached;
            return;
        }

        image.Source = null;
        var loadCancellation = new CancellationTokenSource();
        _pendingLoads.Add(image, loadCancellation);

        // 4. تحميل الصورة في الخلفية وفك تشفيرها
        try
        {
            using var response = await _httpClient.GetAsync(
                fullUrl,
                HttpCompletionOption.ResponseHeadersRead,
                loadCancellation.Token);
            response.EnsureSuccessStatusCode();

            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType is not null && !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("The remote resource is not an image.");
            }

            if (response.Content.Headers.ContentLength > MaxImageBytes)
            {
                throw new InvalidDataException("The remote image exceeds the permitted size.");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(loadCancellation.Token);
            using var imageStream = new MemoryStream();
            var buffer = new byte[81_920];
            while (true)
            {
                var bytesRead = await responseStream.ReadAsync(buffer, loadCancellation.Token);
                if (bytesRead == 0) break;
                if (imageStream.Length + bytesRead > MaxImageBytes)
                {
                    throw new InvalidDataException("The remote image exceeds the permitted size.");
                }

                await imageStream.WriteAsync(buffer.AsMemory(0, bytesRead), loadCancellation.Token);
            }

            if (imageStream.Length > 0)
            {
                imageStream.Position = 0;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = imageStream;
                bmp.EndInit();
                bmp.Freeze();
                TrimCacheIfNeeded();
                _cache[fullUrl] = bmp;

                if (string.Equals(GetSourceUrl(image)?.Trim(), url, StringComparison.OrdinalIgnoreCase))
                {
                    image.Source = bmp;
                }
                return;
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch
        {
            _failedUrls[fullUrl] = DateTimeOffset.UtcNow;
        }
        finally
        {
            if (_pendingLoads.TryGetValue(image, out var currentLoad) &&
                ReferenceEquals(currentLoad, loadCancellation))
            {
                _pendingLoads.Remove(image);
            }

            loadCancellation.Dispose();
        }
    }

    private static bool TryResolveRemoteUrl(string value, out string resolvedUrl)
    {
        resolvedUrl = string.Empty;
        var configuredApiUrl = Environment.GetEnvironmentVariable("RETAL_API_BASE_URL")
            ?? "https://localhost:7226/api/";

        if (!Uri.TryCreate(configuredApiUrl, UriKind.Absolute, out var apiBaseUri) ||
            apiBaseUri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        var allowedOrigin = new Uri(apiBaseUri.GetLeftPart(UriPartial.Authority));
        Uri candidate;
        if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri))
        {
            candidate = absoluteUri;
        }
        else
        {
            candidate = new Uri(allowedOrigin, value.TrimStart('/'));
        }

        if (candidate.Scheme != Uri.UriSchemeHttps ||
            !string.Equals(candidate.Host, allowedOrigin.Host, StringComparison.OrdinalIgnoreCase) ||
            candidate.Port != allowedOrigin.Port)
        {
            return false;
        }

        resolvedUrl = candidate.AbsoluteUri;
        return true;
    }

    private static void TrimCacheIfNeeded()
    {
        if (_cache.Count < MaxCachedImages) return;

        foreach (var key in _cache.Keys.Take(Math.Max(1, MaxCachedImages / 10)))
        {
            _cache.TryRemove(key, out _);
        }
    }
}
