using System.Globalization;
using System.Linq;
using System.Numerics;

namespace Mandelbrot;

[Serializable]
public readonly struct BigFloat : IComparable, IComparable<BigFloat>, IEquatable<BigFloat>
{
    public readonly BigInteger Numerator;
    public readonly BigInteger Denominator;

    public static BigFloat One => new(BigInteger.One);

    public static BigFloat Zero => new(BigInteger.Zero);

    public static BigFloat MinusOne => new(BigInteger.MinusOne);

    public static BigFloat OneHalf => new(BigInteger.One, 2);

    public int Sign
    {
        get
        {
            BigInteger bigInteger = Numerator;
            int sign1 = bigInteger.Sign;
            bigInteger = Denominator;
            int sign2 = bigInteger.Sign;
            switch (sign1 + sign2)
            {
                case -2:
                case 2:
                    return 1;
                case 0:
                    return -1;
                default:
                    return 0;
            }
        }
    }

    private BigFloat(string value)
    {
        BigFloat bigFloat = Parse(value);
        Numerator = bigFloat.Numerator;
        Denominator = bigFloat.Denominator;
    }

    public BigFloat(BigInteger numerator, BigInteger denominator)
    {
        Numerator = numerator;
        Denominator = !(denominator == 0L) ? denominator : throw new ArgumentException("denominator equals 0");
    }

    public BigFloat(BigInteger value)
    {
        Numerator = value;
        Denominator = BigInteger.One;
    }

    public BigFloat(BigFloat value)
    {
        if (value.Equals(null))
        {
            Numerator = BigInteger.Zero;
            Denominator = BigInteger.One;
        }
        else
        {
            Numerator = value.Numerator;
            Denominator = value.Denominator;
        }
    }

    public BigFloat(ulong value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(long value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(uint value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(int value)
        : this(new BigInteger(value))
    {
    }

    public BigFloat(float value)
        : this(value.ToString("N99"))
    {
    }

    public BigFloat(double value)
        : this(value.ToString("N99"))
    {
    }

    public BigFloat(decimal value)
        : this(value.ToString("N99"))
    {
    }

    public static BigFloat Add(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator + other.Numerator * value.Denominator,
            value.Denominator * other.Denominator);
    }

    public static BigFloat Subtract(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator - other.Numerator * value.Denominator,
            value.Denominator * other.Denominator);
    }

    public static BigFloat Multiply(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return new BigFloat(value.Numerator * other.Numerator, value.Denominator * other.Denominator);
    }

    public static BigFloat Divide(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        if (other.Numerator == 0L)
            throw new DivideByZeroException(nameof(other));
        return new BigFloat(value.Numerator * other.Denominator, value.Denominator * other.Numerator);
    }

    public static BigFloat Remainder(BigFloat value, BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        return value - Floor(value / other) * other;
    }

    public static BigFloat DivideRemainder(BigFloat value, BigFloat other, out BigFloat remainder)
    {
        value = Divide(value, other);
        remainder = Remainder(value, other);
        return value;
    }

    public static BigFloat Pow(BigFloat value, int exponent)
    {
        if (value.Numerator.IsZero)
            return value;
        if (exponent >= 0)
            return new BigFloat(BigInteger.Pow(value.Numerator, exponent), BigInteger.Pow(value.Denominator, exponent));
        BigInteger numerator = value.Numerator;
        return new BigFloat(BigInteger.Pow(value.Denominator, -exponent), BigInteger.Pow(numerator, -exponent));
    }

    public static BigFloat Abs(BigFloat value)
    {
        return new BigFloat(BigInteger.Abs(value.Numerator), value.Denominator);
    }

    public static BigFloat Negate(BigFloat value)
    {
        return new BigFloat(BigInteger.Negate(value.Numerator), value.Denominator);
    }

    public static BigFloat Inverse(BigFloat value)
    {
        return new BigFloat(value.Denominator, value.Numerator);
    }

    public static BigFloat Increment(BigFloat value)
    {
        return new BigFloat(value.Numerator + value.Denominator, value.Denominator);
    }

    public static BigFloat Decrement(BigFloat value)
    {
        return new BigFloat(value.Numerator - value.Denominator, value.Denominator);
    }

    public static BigFloat Ceil(BigFloat value)
    {
        BigInteger numerator = value.Numerator;
        return Factor(new BigFloat(
            !(numerator < 0L)
                ? numerator + (value.Denominator - BigInteger.Remainder(numerator, value.Denominator))
                : numerator - BigInteger.Remainder(numerator, value.Denominator), value.Denominator));
    }

    public static BigFloat Floor(BigFloat value)
    {
        BigInteger numerator = value.Numerator;
        return Factor(new BigFloat(
            !(numerator < 0L)
                ? numerator - BigInteger.Remainder(numerator, value.Denominator)
                : numerator + (value.Denominator - BigInteger.Remainder(numerator, value.Denominator)),
            value.Denominator));
    }

    public static BigFloat Round(BigFloat value)
    {
        return Decimals(value).CompareTo(OneHalf) >= 0 ? Ceil(value) : Floor(value);
    }

    public static BigFloat Truncate(BigFloat value)
    {
        BigInteger numerator = value.Numerator;
        return Factor(new BigFloat(numerator - BigInteger.Remainder(numerator, value.Denominator), value.Denominator));
    }

    public static BigFloat Decimals(BigFloat value)
    {
        return new BigFloat(BigInteger.Remainder(value.Numerator, value.Denominator), value.Denominator);
    }

    public static BigFloat ShiftDecimalLeft(BigFloat value, int shift)
    {
        return shift < 0
            ? ShiftDecimalRight(value, -shift)
            : new BigFloat(value.Numerator * BigInteger.Pow(10, shift), value.Denominator);
    }

    public static BigFloat ShiftDecimalRight(BigFloat value, int shift)
    {
        if (shift < 0)
            return ShiftDecimalLeft(value, -shift);
        BigInteger denominator = value.Denominator * BigInteger.Pow(10, shift);
        return new BigFloat(value.Numerator, denominator);
    }

    public static BigFloat Sqrt(BigFloat value)
    {
        return Divide(Math.Pow(10.0, BigInteger.Log10(value.Numerator) / 2.0),
            Math.Pow(10.0, BigInteger.Log10(value.Denominator) / 2.0));
    }

    public static double Log10(BigFloat value)
    {
        return BigInteger.Log10(value.Numerator) - BigInteger.Log10(value.Denominator);
    }

    public static double Log(BigFloat value, double baseValue)
    {
        return BigInteger.Log(value.Numerator, baseValue) - BigInteger.Log(value.Numerator, baseValue);
    }

    public static BigFloat Factor(BigFloat value)
    {
        if (value.Denominator == 1L)
            return value;
        BigInteger bigInteger = BigInteger.GreatestCommonDivisor(value.Numerator, value.Denominator);
        return new BigFloat(value.Numerator / bigInteger, value.Denominator / bigInteger);
    }

    public new static bool Equals(object? left, object? right)
    {
        if (left == null && right == null)
            return true;
        return left != null && right != null && !(left.GetType() != right.GetType()) &&
               ((BigInteger) left).Equals((BigInteger) right);
    }

    public static string ToString(BigFloat value)
    {
        return value.ToString();
    }

    public static BigFloat Parse(string value)
    {
        value = value != null ? value.Trim() : throw new ArgumentNullException(nameof(value));
        NumberFormatInfo numberFormat = Thread.CurrentThread.CurrentCulture.NumberFormat;
        value = value.Replace(numberFormat.NumberGroupSeparator, "");
        int num = value.IndexOf(numberFormat.NumberDecimalSeparator, StringComparison.Ordinal);
        value = value.Replace(numberFormat.NumberDecimalSeparator, "");
        return num < 0
            ? Factor(BigInteger.Parse(value))
            : Factor(new BigFloat(BigInteger.Parse(value), BigInteger.Pow(10, value.Length - num)));
    }

    public static bool TryParse(string value, out BigFloat result)
    {
        try
        {
            result = Parse(value);
            return true;
        }
        catch (ArgumentNullException)
        {
            result = new BigFloat();
            return false;
        }
        catch (FormatException)
        {
            result = new BigFloat();
            return false;
        }
    }

    public static int Compare(BigFloat left, BigFloat right)
    {
        if (object.Equals(left, null))
            throw new ArgumentNullException(nameof(left));
        return !Equals(right, null)
            ? new BigFloat(left).CompareTo(right)
            : throw new ArgumentNullException(nameof(right));
    }

    public override string ToString()
    {
        return ToString(100);
    }

    public string ToString(int precision, bool trailingZeros = false)
    {
        BigFloat bigFloat = Factor(this);
        NumberFormatInfo numberFormat = Thread.CurrentThread.CurrentCulture.NumberFormat;
        BigInteger bigInteger1 = BigInteger.DivRem(bigFloat.Numerator, bigFloat.Denominator, out BigInteger remainder);
        if ((remainder == 0L) & trailingZeros)
            return bigInteger1 + numberFormat.NumberDecimalSeparator + "0";
        if (remainder == 0L)
            return bigInteger1.ToString();
        BigInteger bigInteger2 = bigFloat.Numerator * BigInteger.Pow(10, precision) / bigFloat.Denominator;
        if ((bigInteger2 == 0L) & trailingZeros)
            return bigInteger1 + numberFormat.NumberDecimalSeparator + "0";
        if (bigInteger2 == 0L)
            return bigInteger1.ToString();
        StringBuilder stringBuilder = new StringBuilder();
        while (precision-- > 0)
        {
            stringBuilder.Append(bigInteger2 % 10);
            bigInteger2 /= 10;
        }

        string str = bigInteger1 + numberFormat.NumberDecimalSeparator +
                     new string(stringBuilder.ToString().Reverse().ToArray());
        if (trailingZeros)
            return str;
        return str.TrimEnd('0');
    }

    public string ToMixString()
    {
        BigFloat bigFloat = Factor(this);
        BigInteger bigInteger = BigInteger.DivRem(bigFloat.Numerator, bigFloat.Denominator, out BigInteger remainder);
        if (remainder == 0L)
            return bigInteger.ToString();
        return bigInteger + ", " + remainder + "/" + bigFloat.Denominator;
    }

    public string ToRationalString()
    {
        BigFloat bigFloat = Factor(this);
        BigInteger bigInteger = bigFloat.Numerator;
        string str1 = bigInteger.ToString();
        bigInteger = bigFloat.Denominator;
        string str2 = bigInteger.ToString();
        return str1 + " / " + str2;
    }

    public int CompareTo(BigFloat other)
    {
        if (object.Equals(other, null))
            throw new ArgumentNullException(nameof(other));
        BigInteger numerator1 = Numerator;
        BigInteger numerator2 = other.Numerator;
        BigInteger denominator = other.Denominator;
        return BigInteger.Compare(numerator1 * denominator, numerator2 * Denominator);
    }

    public int CompareTo(object? obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));
        if (!(obj is BigFloat other))
            throw new ArgumentException("obj is not a BigFloat");
        return CompareTo(other);
    }

    public override bool Equals(object? obj)
    {
        return obj != null && !(GetType() != obj.GetType()) && Numerator == ((BigFloat) obj).Numerator &&
               Denominator == ((BigFloat) obj).Denominator;
    }

    public bool Equals(BigFloat other)
    {
        return other.Numerator * Denominator == Numerator * other.Denominator;
    }

    public override int GetHashCode()
    {
        return (Numerator, Denominator).GetHashCode();
    }

    public static BigFloat operator -(BigFloat value)
    {
        return Negate(value);
    }

    public static BigFloat operator -(BigFloat left, BigFloat right)
    {
        return Subtract(left, right);
    }

    public static BigFloat operator --(BigFloat value)
    {
        return Decrement(value);
    }

    public static BigFloat operator +(BigFloat left, BigFloat right)
    {
        return Add(left, right);
    }

    public static BigFloat operator +(BigFloat value)
    {
        return Abs(value);
    }

    public static BigFloat operator ++(BigFloat value)
    {
        return Increment(value);
    }

    public static BigFloat operator %(BigFloat left, BigFloat right)
    {
        return Remainder(left, right);
    }

    public static BigFloat operator *(BigFloat left, BigFloat right)
    {
        return Multiply(left, right);
    }

    public static BigFloat operator /(BigFloat left, BigFloat right)
    {
        return Divide(left, right);
    }

    public static BigFloat operator >> (BigFloat value, int shift)
    {
        return ShiftDecimalRight(value, shift);
    }

    public static BigFloat operator <<(BigFloat value, int shift)
    {
        return ShiftDecimalLeft(value, shift);
    }

    public static BigFloat operator ^(BigFloat left, int right)
    {
        return Pow(left, right);
    }

    public static BigFloat operator ~(BigFloat value)
    {
        return Inverse(value);
    }

    public static bool operator !=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) != 0;
    }

    public static bool operator ==(BigFloat left, BigFloat right)
    {
        return Compare(left, right) == 0;
    }

    public static bool operator <(BigFloat left, BigFloat right)
    {
        return Compare(left, right) < 0;
    }

    public static bool operator <=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) <= 0;
    }

    public static bool operator >(BigFloat left, BigFloat right)
    {
        return Compare(left, right) > 0;
    }

    public static bool operator >=(BigFloat left, BigFloat right)
    {
        return Compare(left, right) >= 0;
    }

    public static bool operator true(BigFloat value)
    {
        return value != 0;
    }

    public static bool operator false(BigFloat value)
    {
        return value == 0;
    }

    public static explicit operator decimal(BigFloat value)
    {
        if (decimal.MinValue > value)
            throw new OverflowException("value is less than decimal.MinValue.");
        if (decimal.MaxValue < value)
            throw new OverflowException("value is greater than decimal.MaxValue.");
        return (decimal) value.Numerator / (decimal) value.Denominator;
    }

    public static explicit operator double(BigFloat value)
    {
        if (double.MinValue > value)
            throw new OverflowException("value is less than double.MinValue.");
        if (double.MaxValue < value)
            throw new OverflowException("value is greater than double.MaxValue.");
        return (double) value.Numerator / (double) value.Denominator;
    }

    public static explicit operator float(BigFloat value)
    {
        if (float.MinValue > value)
            throw new OverflowException("value is less than float.MinValue.");
        if (float.MaxValue < value)
            throw new OverflowException("value is greater than float.MaxValue.");
        return (float) value.Numerator / (float) value.Denominator;
    }

    public static implicit operator BigFloat(byte value)
    {
        return new BigFloat((uint) value);
    }

    public static implicit operator BigFloat(sbyte value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(short value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(ushort value)
    {
        return new BigFloat((uint) value);
    }

    public static implicit operator BigFloat(int value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(long value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(uint value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(ulong value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(decimal value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(double value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(float value)
    {
        return new BigFloat(value);
    }

    public static implicit operator BigFloat(BigInteger value)
    {
        return new BigFloat(value);
    }

    public static explicit operator BigFloat(string value)
    {
        return new BigFloat(value);
    }
}