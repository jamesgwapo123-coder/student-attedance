using AttendanceSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Configure SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=attendance.db"));

// Configure Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// Bind Kestrel to all interfaces and use middleware to restrict access to local network ranges only.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5274);
});

var app = builder.Build();

// Auto-create database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

var allowedCidrs = builder.Configuration.GetSection("LocalNetwork:AllowedCidrs").Get<string[]>() ?? new[]
{
    "127.0.0.1/32",
    "::1/128",
    "10.0.0.0/8",
    "172.16.0.0/12",
    "192.168.0.0/16"
};

app.Use(async (context, next) =>
{
    var remoteIp = context.Connection.RemoteIpAddress;
    if (remoteIp is null || !IsAllowedLocalNetwork(remoteIp, allowedCidrs))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Access denied. This application is only available from the local network.");
        return;
    }

    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

static bool IsAllowedLocalNetwork(IPAddress remoteIp, string[] allowedCidrs)
{
    foreach (var cidr in allowedCidrs)
    {
        if (TryParseCidr(cidr, out var networkAddress, out var prefixLength) && IsIpInCidr(remoteIp, networkAddress, prefixLength))
        {
            return true;
        }
    }

    return false;
}

static bool TryParseCidr(string cidr, out IPAddress networkAddress, out int prefixLength)
{
    networkAddress = IPAddress.None;
    prefixLength = 0;

    var parts = cidr.Split('/', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0 || parts.Length > 2)
    {
        return false;
    }

    if (!IPAddress.TryParse(parts[0], out networkAddress))
    {
        return false;
    }

    if (parts.Length == 1)
    {
        prefixLength = networkAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 32 : 128;
        return true;
    }

    if (!int.TryParse(parts[1], out prefixLength))
    {
        return false;
    }

    return true;
}

static bool IsIpInCidr(IPAddress address, IPAddress networkAddress, int prefixLength)
{
    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && networkAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
        address = address.MapToIPv4();
    }
    else if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && networkAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
    {
        networkAddress = networkAddress.MapToIPv4();
    }

    var addressBytes = address.GetAddressBytes();
    var networkBytes = networkAddress.GetAddressBytes();

    if (addressBytes.Length != networkBytes.Length)
    {
        return false;
    }

    var maskBytes = new byte[addressBytes.Length];
    for (var i = 0; i < maskBytes.Length; i++)
    {
        var bits = Math.Min(8, Math.Max(0, prefixLength - i * 8));
        maskBytes[i] = bits switch
        {
            >= 8 => 0xFF,
            > 0 => (byte)(0xFF << (8 - bits)),
            _ => (byte)0
        };
    }

    for (var i = 0; i < addressBytes.Length; i++)
    {
        if ((addressBytes[i] & maskBytes[i]) != (networkBytes[i] & maskBytes[i]))
        {
            return false;
        }
    }

    return true;
}
