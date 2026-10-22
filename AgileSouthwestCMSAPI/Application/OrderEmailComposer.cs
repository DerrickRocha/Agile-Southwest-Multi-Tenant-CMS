using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Domain.Mail;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgileSouthwestCMSAPI.Application;

public class OrderEmailComposer(CmsDbContext db, IOptions<EmailOptions> options) : IOrderEmailComposer
{
    private static readonly CultureInfo UsCulture = CultureInfo.GetCultureInfo("en-US");
    private readonly EmailOptions _options = options.Value;

    public async Task<EmailMessage> ComposeAsync(EmailOutboxMessage message, CancellationToken ct)
    {
        // The worker has no tenant context, so scope by TenantId explicitly.
        var order = await db.Orders
                        .AsNoTracking()
                        .Include(o => o.Tenant)
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.Id == message.OrderId && o.TenantId == message.TenantId, ct)
                    ?? throw new InvalidOperationException(
                        $"Order {message.OrderId} was not found for outbox message {message.Id}.");

        return message.Type switch
        {
            EmailType.OrderReceived => BuildCustomerEmail(
                order,
                heading: "We received your order",
                intro: "Thanks for your order. We'll send another email once your payment is confirmed."),

            EmailType.OrderConfirmed => BuildCustomerEmail(
                order,
                heading: "Your order is confirmed",
                intro: "Thanks! We received your payment and we're getting your order ready."),

            EmailType.MerchantNewOrder => BuildMerchantEmail(
                order,
                "New order",
                "A new order has been placed."
            ),

            _ => throw new ArgumentOutOfRangeException(nameof(message), message.Type, "Unknown email type.")
        };
    }

    private static EmailMessage BuildCustomerEmail(Order order, string heading, string intro)
    {
        var tenantName = order.Tenant.Name;

        var subject = OneLine($"{heading}: {order.OrderNumber} ({tenantName})");
        return new EmailMessage(
            To: order.CustomerEmail,
            Subject: subject,
            HtmlBody: BuildHtml(order, heading, intro),
            TextBody: BuildText(order, heading, intro));
    }

    private EmailMessage BuildMerchantEmail(Order order, string heading, string intro)
    {
        var tenantName = order.Tenant.Name;

        var subject = OneLine($"New order from {order.CustomerEmail}: {order.OrderNumber} ({tenantName})");
        return new EmailMessage(
            To: _options.FromAddress,
            Subject: subject,
            HtmlBody: BuildHtml(order, heading, intro),
            TextBody: BuildText(order, heading, intro));
    }

    // ------------------------------------------------------------------ HTML

    private static string BuildHtml(Order order, string heading, string intro)
    {
        var rows = new StringBuilder();
        foreach (var item in order.OrderItems.OrderBy(i => i.Id))
        {
            var options = DescribeOptions(item.OptionDetails);
            rows.Append($"""
                         <tr>
                           <td style="padding:8px 0;border-bottom:1px solid #eeeeee;">
                             {Enc(item.ProductName)} &times; {item.Quantity}
                             {(options.Length > 0 ? $"<br /><span style=\"color:#666666;font-size:13px;\">{Enc(options)}</span>" : "")}
                           </td>
                           <td align="right" style="padding:8px 0;border-bottom:1px solid #eeeeee;white-space:nowrap;">
                             {Enc(Money(item.TotalPriceCents))}
                           </td>
                         </tr>
                         """);
        }

        var totals = new StringBuilder();
        AppendTotalRow(totals, "Subtotal", order.SubtotalCents);
        if (order.DiscountCents + order.CouponDiscountCents > 0)
            AppendTotalRow(totals, "Discount", -(order.DiscountCents + order.CouponDiscountCents));
        AppendTotalRow(totals, "Tax", order.TaxCents);
        AppendTotalRow(totals, "Shipping", order.ShippingCents);
        AppendTotalRow(totals, "Total", order.TotalCents, bold: true);

        var shipping = order.ShippingRateId is null
            ? ""
            : $"""
               <p style="margin:20px 0 4px 0;"><strong>Shipping to</strong></p>
               <p style="margin:0;line-height:1.5;">{Enc(ShippingAddressLines(order)).Replace("\n", "<br />")}</p>
               """;

        return $"""
                <!DOCTYPE html>
                <html lang="en">
                <body style="margin:0;padding:24px;background-color:#f4f5f7;font-family:Arial,Helvetica,sans-serif;color:#222222;">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                    <tr>
                      <td align="center">
                        <table role="presentation" width="600" cellpadding="0" cellspacing="0"
                               style="max-width:600px;background-color:#ffffff;border-radius:6px;padding:24px;">
                          <tr>
                            <td>
                              <h2 style="margin:0 0 16px 0;font-size:20px;">{Enc(heading)}</h2>
                              <p style="margin:0 0 12px 0;">Hi {Enc(order.CustomerFirstName)},</p>
                              <p style="margin:0 0 16px 0;">{Enc(intro)}</p>
                              <p style="margin:0 0 16px 0;"><strong>Order number:</strong> {Enc(order.OrderNumber)}</p>
                              <table role="presentation" width="100%" cellpadding="0" cellspacing="0">
                                {rows}
                              </table>
                              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin-top:12px;">
                                {totals}
                              </table>
                              {shipping}
                              <p style="margin:24px 0 0 0;color:#666666;font-size:13px;">{Enc(order.Tenant.Name)}</p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
    }

    private static void AppendTotalRow(StringBuilder sb, string label, int cents, bool bold = false)
    {
        var weight = bold ? "font-weight:bold;" : "";
        sb.Append($"""
                   <tr>
                     <td style="padding:4px 0;{weight}">{Enc(label)}</td>
                     <td align="right" style="padding:4px 0;{weight}">{Enc(Money(cents))}</td>
                   </tr>
                   """);
    }

    // ------------------------------------------------------------------ plain text

    private static string BuildText(Order order, string heading, string intro)
    {
        var sb = new StringBuilder();
        sb.AppendLine(heading);
        sb.AppendLine();
        sb.AppendLine($"Hi {order.CustomerFirstName},");
        sb.AppendLine();
        sb.AppendLine(intro);
        sb.AppendLine();
        sb.AppendLine($"Order number: {order.OrderNumber}");
        sb.AppendLine();

        foreach (var item in order.OrderItems.OrderBy(i => i.Id))
        {
            var options = DescribeOptions(item.OptionDetails);
            sb.AppendLine($"{item.ProductName} x {item.Quantity}  {Money(item.TotalPriceCents)}");
            if (options.Length > 0) sb.AppendLine($"  {options}");
        }

        sb.AppendLine();
        sb.AppendLine($"Subtotal: {Money(order.SubtotalCents)}");
        if (order.DiscountCents + order.CouponDiscountCents > 0)
            sb.AppendLine($"Discount: {Money(-(order.DiscountCents + order.CouponDiscountCents))}");
        sb.AppendLine($"Tax: {Money(order.TaxCents)}");
        sb.AppendLine($"Shipping: {Money(order.ShippingCents)}");
        sb.AppendLine($"Total: {Money(order.TotalCents)}");

        if (order.ShippingRateId is not null)
        {
            sb.AppendLine();
            sb.AppendLine("Shipping to:");
            sb.AppendLine(ShippingAddressLines(order));
        }

        sb.AppendLine();
        sb.AppendLine(order.Tenant.Name);
        return sb.ToString();
    }

    // ------------------------------------------------------------------ helpers

    private static string ShippingAddressLines(Order order)
    {
        var lines = new List<string> { order.ShippingAddressLine1 };
        if (!string.IsNullOrWhiteSpace(order.ShippingAddressLine2)) lines.Add(order.ShippingAddressLine2);

        var cityLine = string.Join(", ", new[] { order.ShippingCity, order.ShippingState }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
        lines.Add($"{cityLine} {order.ShippingPostalCode}".Trim());
        lines.Add(order.ShippingCountry);

        return string.Join("\n", lines);
    }

    private static string DescribeOptions(string? json)
    {
        if (string.IsNullOrEmpty(json)) return "";
        try
        {
            var options = JsonSerializer.Deserialize<List<OrderItemOptionSnapshot>>(json) ?? [];
            return string.Join(", ", options.Select(o => $"{o.OptionName}: {o.ChoiceName}"));
        }
        catch (JsonException)
        {
            return ""; // a bad snapshot shouldn't stop the email
        }
    }

    private static string Money(int cents) => (cents / 100m).ToString("C", UsCulture);

    private static string Enc(string? value) => WebUtility.HtmlEncode(value ?? "");

    // Subjects must be a single line.
    private static string OneLine(string value) =>
        value.Replace("\r", " ").Replace("\n", " ").Trim();
}