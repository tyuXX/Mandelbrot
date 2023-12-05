using System.Numerics;

namespace Mandelbrot.BigIntegerRenderer;

public struct BigDecimal : IComparable, IComparable<BigDecimal>
{
    private static readonly bool AlwaysTruncate = false;
    private static readonly int Precision = 16;

    private BigInteger Mantissa { get; set; }
    private int Exponent { get; set; }

    private BigDecimal(BigInteger mantissa, int exponent)
        : this()
    {
        Mantissa = mantissa;
        Exponent = exponent;

        if (AlwaysTruncate) Truncate();
    }
    public void Normalize()
    {
        if (Mantissa.IsZero)
        {
            Exponent = 0;
        }
        else
        {
            BigInteger remainder = 0;
            while (remainder == 0)
            {
                BigInteger shortened = BigInteger.DivRem(Mantissa, 10, out remainder);
                if (remainder == 0)
                {
                    Mantissa = shortened;
                    Exponent++;
                }
            }
        }
    }
    public void Truncate()
    {
        int numDigits = NumberOfDigits(Mantissa);
        if (numDigits > Precision)
        {
            int powTen = numDigits - Precision;
            Mantissa /= BigInteger.Pow(10, powTen);
            Exponent += powTen;
        }
    }

    private static int NumberOfDigits(BigInteger value)
    {
        return (int) BigInteger.Log10(BigInteger.Abs(value)) + 1;
    }

    #region Conversions

    public static implicit operator BigDecimal(int value)
    {
        return new BigDecimal(value, 0);
    }

    public static implicit operator BigDecimal(double value)
    {
        BigInteger mantissa = (BigInteger) value;
        int exponent = 0;
        double scaleFactor = 1;
        while (Math.Abs(value * scaleFactor - (double) mantissa) > 0)
        {
            exponent -= 1;
            scaleFactor *= 10;
            mantissa = (BigInteger) (value * scaleFactor);
        }

        return new BigDecimal(mantissa, exponent);
    }

    public static explicit operator double(BigDecimal value)
    {
        return (double) value.Mantissa * Math.Pow(10, value.Exponent);
    }

    public static explicit operator float(BigDecimal value)
    {
        return Convert.ToSingle((double) value);
    }

    public static explicit operator int(BigDecimal value)
    {
        return (int) (value.Mantissa * BigInteger.Pow(10, value.Exponent));
    }

    public static explicit operator uint(BigDecimal value)
    {
        return (uint) (value.Mantissa * BigInteger.Pow(10, value.Exponent));
    }

    #endregion

    #region Operators

    public static BigDecimal operator +(BigDecimal value)
    {
        return value;
    }

    public void Zero()
    {
        Mantissa = BigInteger.Zero;
        Exponent = 0;
    }


    public void Add(BigDecimal value)
    {
        if (Exponent > value.Exponent)
        {
            Mantissa = AlignExponent(this, value) + value.Mantissa;
            Exponent = value.Exponent;
        }
        else if (Exponent < value.Exponent)
        {
            Mantissa = AlignExponent(value, this) + Mantissa;
        }
        else
        {
            Mantissa += value.Mantissa;
        }
    }

    public void Multiply(BigDecimal value)
    {
        Mantissa *= value.Mantissa;
        Exponent += value.Exponent;
    }

    public void Divide(BigDecimal divisor)
    {
        int exponentChange = Precision - (NumberOfDigits(Mantissa) - NumberOfDigits(divisor.Mantissa));
        if (exponentChange < 0) exponentChange = 0;
        Mantissa *= BigInteger.Pow(10, exponentChange);
        Mantissa /= divisor.Mantissa;
        Exponent -= divisor.Exponent + exponentChange;
    }


    public static BigDecimal operator -(BigDecimal value)
    {
        value.Mantissa *= -1;
        return value;
    }

    public static BigDecimal operator ++(BigDecimal value)
    {
        return value + 1;
    }

    public static BigDecimal operator --(BigDecimal value)
    {
        return value - 1;
    }

    public static BigDecimal operator +(BigDecimal left, BigDecimal right)
    {
        return Add(left, right);
    }

    public static BigDecimal operator -(BigDecimal left, BigDecimal right)
    {
        return Add(left, -right);
    }

    private static BigDecimal Add(BigDecimal left, BigDecimal right)
    {
        return left.Exponent > right.Exponent
            ? new BigDecimal(AlignExponent(left, right) + right.Mantissa, right.Exponent)
            : new BigDecimal(AlignExponent(right, left) + left.Mantissa, left.Exponent);
    }

    public static BigDecimal operator *(BigDecimal left, BigDecimal right)
    {
        return new BigDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
    }

    public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
    {
        int exponentChange = Precision - (NumberOfDigits(dividend.Mantissa) - NumberOfDigits(divisor.Mantissa));
        if (exponentChange < 0) exponentChange = 0;
        dividend.Mantissa *= BigInteger.Pow(10, exponentChange);
        return new BigDecimal(dividend.Mantissa / divisor.Mantissa,
            dividend.Exponent - divisor.Exponent - exponentChange);
    }

    public static bool operator ==(BigDecimal left, BigDecimal right)
    {
        return left.Exponent == right.Exponent && left.Mantissa == right.Mantissa;
    }

    public static bool operator !=(BigDecimal left, BigDecimal right)
    {
        return left.Exponent != right.Exponent || left.Mantissa != right.Mantissa;
    }

    public static bool operator <(BigDecimal left, BigDecimal right)
    {
        return left.Exponent > right.Exponent
            ? AlignExponent(left, right) < right.Mantissa
            : left.Mantissa < AlignExponent(right, left);
    }

    public static bool operator >(BigDecimal left, BigDecimal right)
    {
        return left.Exponent > right.Exponent
            ? AlignExponent(left, right) > right.Mantissa
            : left.Mantissa > AlignExponent(right, left);
    }

    public static bool operator <=(BigDecimal left, BigDecimal right)
    {
        return left.Exponent > right.Exponent
            ? AlignExponent(left, right) <= right.Mantissa
            : left.Mantissa <= AlignExponent(right, left);
    }

    public static bool operator >=(BigDecimal left, BigDecimal right)
    {
        return left.Exponent > right.Exponent
            ? AlignExponent(left, right) >= right.Mantissa
            : left.Mantissa >= AlignExponent(right, left);
    }
    
    private static BigInteger AlignExponent(BigDecimal value, BigDecimal reference)
    {
        BigInteger b = value.Mantissa * BigInteger.Pow(10, value.Exponent - reference.Exponent);
        return b;
    }

    #endregion

    public override string ToString()
    {
        return string.Concat(Mantissa.ToString(), "E", Exponent);
    }

    private bool Equals(BigDecimal other)
    {
        return other.Mantissa.Equals(Mantissa) && other.Exponent == Exponent;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is BigDecimal @decimal && Equals(@decimal);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (Mantissa.GetHashCode() * 397) ^ Exponent;
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj is not BigDecimal @decimal) throw new ArgumentException();
        return CompareTo(@decimal);
    }

    public int CompareTo(BigDecimal other)
    {
        return this < other ? -1 : this > other ? 1 : 0;
    }
}