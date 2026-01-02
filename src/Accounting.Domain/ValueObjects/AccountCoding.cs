using System.Text.RegularExpressions;

namespace Accounting.Domain.ValueObjects;

public sealed record AccountCoding
{
    public string Value { get; }

    public string[] Segments => Value.Split('-');

    private AccountCoding() { }

    public AccountCoding(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("کد حسابداری نمی‌تواند خالی باشد.");

        var segments = value.Split('-');
        Validate(segments);

        Value = value;
    }

    public static AccountCoding CreateFromSegments(IEnumerable<string> segments)
    {
        if (segments == null) throw new ArgumentNullException(nameof(segments));
        // آرایه را میگیرد و با خط تیره بهم میچسباند تا فرمت استاندارد داخل برنامه ساخته شود
        return new AccountCoding(string.Join("-", segments));
    }

    public static AccountCoding Create(string value) => new(value);


    private static void Validate(string[] segments)
    {
        if (segments.Length < 3)
            throw new ArgumentException("کد حسابداری باید حداقل شامل ۳ بخش باشد.");

        foreach (var segment in segments)
        {
            if (!Regex.IsMatch(segment, @"^\d{4,8}$"))
                throw new ArgumentException($"بخش '{segment}' نامعتبر است. هر بخش باید ۴ تا ۸ رقم باشد.");
        }
    }

    public override string ToString() => Value;


    public static implicit operator string(AccountCoding coding) => coding.Value;
    public static explicit operator AccountCoding(string value) => new(value);
}